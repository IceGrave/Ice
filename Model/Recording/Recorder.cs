﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whydoisuck.Model.DataStructures;
using Whydoisuck.Model.MemoryReading;
using Whydoisuck.Model.MemoryReading.GameStateStructures;

namespace Whydoisuck.DataSaving
{
    /// <summary>
    /// Saves data on the disk based on what happens in the game
    /// </summary>
    public class Recorder
    {
        /// <summary>
        /// Current session on the currently played level.
        /// </summary>
        public Session CurrentSession { get; set; }

        /// <summary>
        /// Delegate for callbacks when the current session changes
        /// </summary>
        /// <param name="session">The current session</param>
        public delegate void SessionCallback(Session session);
        /// <summary>
        /// Delegare for callbacks when a new attempt is created
        /// </summary>
        /// <param name="attempt">The created attempt</param>
        public delegate void AttemptCallback(Attempt attempt);

        /// <summary>
        /// Event invoked when a new session is initialized
        /// </summary>
        public event SessionCallback OnNewCurrentSessionInitialized;
        /// <summary>
        /// Event invoked when the current session is saved
        /// </summary>
        public event SessionCallback OnQuitCurrentSession;
        /// <summary>
        /// Event invoked when a new attempt is created
        /// </summary>
        public event AttemptCallback OnAttemptAdded;

        private SessionManager Manager { get; set; }
        private Attempt CurrentAttempt { get; set; }

        public Recorder()
        {
            GameWatcher.OnLevelEntered += CreateNewSession;
            GameWatcher.OnLevelStarted += UpdateCurrentSession;
            GameWatcher.OnLevelExited += PopSaveCurrentSession;
            GameWatcher.OnPlayerSpawns += CreateNewAttempt;
            GameWatcher.OnPlayerDies += PopSaveLosingAttempt;
            GameWatcher.OnPlayerRestarts += PopSaveLosingAttempt;
            GameWatcher.OnPlayerWins += PopSaveWinningAttempt;
        }

        /// <summary>
        /// Starts saving data based on what happens in game.
        /// </summary>
        public void StartRecording()
        {
            Manager = SerializationManager.DeserializeSessionManager();
            GameWatcher.StartWatching();
        }

        /// <summary>
        /// Stops saving data.
        /// </summary>
        public void StopRecording()
        {
            GameWatcher.StopWatching();
            SerializationManager.SerializeSessionManager(Manager);
        }

        // Called when entering a level, ensure a session is created before an attempt needs to be saved
        // However, while its metadata is fully loaded, the level is not
        // Therefore stuff like the level length, the start position etc. is updated when the level is fully loaded and not in this function
        private void CreateNewSession(GDLevelMetadata level)
        {
            CurrentSession = new Session(DateTime.Now);
        }

        // Update values for the current session, is called when the level is fully loaded
        private void UpdateCurrentSession(GameState state)
        {
            CreateSessionIfNotExists(state);
            CurrentSession.Level = new Level(state);
            CurrentSession.IsCopyRun = state.LoadedLevel.IsTestmode;
            CurrentSession.StartPercent = 100 * state.LoadedLevel.StartPosition / state.LoadedLevel.PhysicalLength;
            OnNewCurrentSessionInitialized?.Invoke(CurrentSession);
        }

        //Saves current session and removes it from the recorder.
        private void PopSaveCurrentSession(GDLevelMetadata level)
        {
            //Don't save if :
            //  -no session were created (= software launched while playing a level, and no attempts have been played before exiting)
            //  -The current level is unknown (= The level was left before it finished loading)
            //  -There are not attempts in the session (= useless data)
            if (CurrentSession == null || CurrentSession.Level == null || CurrentSession.Attempts.Count == 0) return;
            Manager.SaveSession(CurrentSession);
            SerializationManager.SerializeSessionManager(Manager);

            OnQuitCurrentSession?.Invoke(CurrentSession);
            CurrentSession = null;
            CurrentAttempt = null;
        }

        // Creates an attempt
        private void CreateNewAttempt(GameState state)
        {
            CurrentAttempt = new Attempt(state.LoadedLevel.AttemptNumber, DateTime.Now);
        }

        // Saves a losing attempt in the current session, and remove current attempt from recorder
        private void PopSaveLosingAttempt(GameState state)
        {
            CreateSessionIfNotExists(state);
            CreateAttemptIfNotExists(state);
            CurrentAttempt.EndPercent = 100 * state.PlayerObject.XPosition / state.LoadedLevel.PhysicalLength;
            CurrentAttempt.Duration = DateTime.Now - CurrentAttempt.StartTime;
            CurrentSession.AddAttempt(CurrentAttempt);
            OnAttemptAdded?.Invoke(CurrentAttempt);
            CurrentAttempt = null;
        }

        // Saves a winning attempt in the current session, and remove current attempt from recorder
        private void PopSaveWinningAttempt(GameState state)
        {
            CreateSessionIfNotExists(state);
            CreateAttemptIfNotExists(state);
            CurrentAttempt.EndPercent = 100;
            CurrentAttempt.Duration = DateTime.Now - CurrentAttempt.StartTime;
            CurrentSession.AddAttempt(CurrentAttempt);
            OnAttemptAdded?.Invoke(CurrentAttempt);
            CurrentAttempt = null;
        }

        //Creates a session if there is no current session and initialize known values
        private void CreateSessionIfNotExists(GameState state)
        {
            if (CurrentSession != null) return;
            CurrentSession = new Session(DateTime.Now);

            if (state == null || state.LevelMetadata == null || state.LoadedLevel == null) return;
            CurrentSession.Level = new Level(state);
            CurrentSession.IsCopyRun = state.LoadedLevel.IsTestmode;
            CurrentSession.StartPercent = 100 * state.LoadedLevel.StartPosition / state.LoadedLevel.PhysicalLength;
            OnNewCurrentSessionInitialized?.Invoke(CurrentSession);
        }

        //Creates an attempt if there is no current attempt and initialize known values
        private void CreateAttemptIfNotExists(GameState state)
        {
            if (CurrentAttempt != null) return;
            CurrentAttempt = new Attempt(state.LoadedLevel.AttemptNumber, DateTime.Now);

            if (state.LoadedLevel == null) return;
            CurrentAttempt.Number = state.LoadedLevel.AttemptNumber;
        }
    }
}
