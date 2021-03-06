using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ydis.Model.DataSaving;
using Ydis.Model.DataStructures;
using Ydis.Model.MemoryReading;
using Ydis.Model.MemoryReading.GameStateStructures;

namespace Ydis.Model.Recording
{
    /// <summary>
    /// Saves data on the disk based on what happens in the game
    /// </summary>
    public class Recorder
    {
        public event Action<ISession> OnStartSession;
        public event Action<ISession> OnQuitSession;
        public event Action OnSessionAttemptsUpdated;

        /// <summary>
        /// Guessed group for the current session.
        /// </summary>
        public SessionGroup Autoguess => CurrentSession==null?null:SessionManager.Instance.FindGroupOf(CurrentSession.Level);
        private ISession CurrentSession { get; set; }

        // Current state of the recorder
        private IRecorderState _currentState;

        // Wether the recorder should fire UI updating event or not
        private bool Silent { get; set; }

        public Recorder()
        {
            GameWatcher.OnLevelEntered += LevelEntered;
            GameWatcher.OnLevelStarted += SessionStarted;
            GameWatcher.OnLevelExited += LevelExited;
            GameWatcher.OnPlayerSpawns += AttemptStarted;
            GameWatcher.OnPlayerDies += AttemptEnded;
            GameWatcher.OnPlayerRestarts += AttemptEnded;
            GameWatcher.OnPlayerWins += AttemptEnded;
            GameWatcher.OnPracticeModeStarted += SetPracticeModeState;
            GameWatcher.OnPracticeModeExited += QuitPracticeModeState;
            GameWatcher.OnNormalModeStarted += SetNewNormalModeState;
            GameWatcher.OnNormalModeExited += QuitNormalModeState;
            GameWatcher.OnGameClosed += GameClosed;
        }

        /// <summary>
        /// Starts saving data based on what happens in game.
        /// </summary>
        public void StartRecording()
        {
            GameWatcher.StartWatching();
        }

        /// <summary>
        /// Stops saving data and saves the on going session
        /// </summary>
        public void StopRecording()
        {
            GameWatcher.StopWatching();
            SessionEnded(null);
            SessionManager.Instance.Save();
        }

        /// <summary>
        /// Prevent the recorder from saving data.
        /// </summary>
        public void CrashRecorder()
        {
            //TODO prevent callbacks from being called
            GameWatcher.CancelWatchingAsync();
        }

        private void LevelEntered(GDLevelMetadata level)
        {
            SetState(new NormalRecorderState());
        }

        private void LevelExited(GameState state)
        {
            Silent = true;
            AttemptEnded(state);
            Silent = false;
            SessionEnded(state);
        }
        private void GameClosed()
        {
            SessionEnded(null);
        }


        private void SessionStarted(GameState state)
        {
            InitRecorderStateIfNeeded(state);
            _currentState?.OnSessionStarted(state);
        }

        private void SessionEnded(GameState state)
        {
            InitRecorderStateIfNeeded(state);
            _currentState?.OnSessionEnded(state);
        }

        private void AttemptStarted(GameState state)
        {
            InitRecorderStateIfNeeded(state);
            _currentState?.OnAttemptStarted(state);
        }

        private void AttemptEnded(GameState state)
        {
            InitRecorderStateIfNeeded(state);
            _currentState?.OnAttemptEnded(state);
        }

        // Makes the recorder record a normal mode session after a practice session
        private void SetNewNormalModeState(GameState state)
        {
            SetState(new NormalRecorderState());
            SessionStarted(state);
        }

        // Called when normal mode is exited (in order to save current session)
        private void QuitNormalModeState(GameState state)
        {
            InitRecorderStateIfNeeded(state);
            SessionEnded(state);
            SetState(null);
        }

        // Makes the recorder record a practice mode session
        private void SetPracticeModeState(GameState state)
        {
            SetState(new PracticeRecorderState());
            SessionStarted(state);
        }

        private void QuitPracticeModeState(GameState state)
        {
            InitRecorderStateIfNeeded(state);
            Silent = true;
            AttemptEnded(state);
            Silent = false;
            SessionEnded(state);
            SetState(null);
        }

        // Sets the recorder state
        private void SetState(IRecorderState s)
        {
            if (_currentState != null)
            {
                _currentState.OnAttemptsUpdated -= OnStateAttemptsUpdated;
                _currentState.OnSessionInitialized -= OnStateNewSession;
                _currentState.OnQuitSession -= OnStateQuitSession;
            }
            if (s != null)
            {
                s.OnAttemptsUpdated += OnStateAttemptsUpdated;
                s.OnSessionInitialized += OnStateNewSession;
                s.OnQuitSession += OnStateQuitSession;
            }
            _currentState = s;
        }

        // Used to init the current state if the recorder was started while playing a level
        private void InitRecorderStateIfNeeded(GameState gameState)
        {
            if (gameState == null || gameState.LoadedLevel == null) return;
            if (_currentState == null)
            {
                if (!gameState.LoadedLevel.IsPractice)
                {
                    SetNewNormalModeState(gameState);
                }
                else
                {
                    SetPracticeModeState(gameState);
                }
            }
        }

        // Handles the current state ending a session
        private void OnStateQuitSession(ISession s)
        {
            CurrentSession = null;
            if(!Silent) OnQuitSession?.Invoke(s);
        }

        // Handles the current state creating a session
        private void OnStateNewSession(ISession s)
        {
            CurrentSession = s;
            if (!Silent) OnStartSession?.Invoke(s);
        }

        // Handles the current state updating it's current session attempts
        private void OnStateAttemptsUpdated()
        {
            if (!Silent) OnSessionAttemptsUpdated?.Invoke();
        }
    }
}
