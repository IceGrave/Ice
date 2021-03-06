namespace Ydis.Model.MemoryReading.GameStateStructures
{
    /// <summary>
    /// Class <c>GDLoadedLevel</c> represents the level object in the game
    /// </summary>
    public class GDLoadedLevel
    {
        /// <summary>
        /// Boolean true if the level is actually on going, and not loading
        /// </summary>
        public bool IsRunning { get; set; }
        /// <summary>
        /// Boolean true if the current session is on a copy
        /// </summary>
        public bool IsTestmode { get; set; }
        /// <summary>
        /// Boolean true if the level is currently being played in practice mode
        /// </summary>
        public bool IsPractice { get; set; }
        /// <summary>
        /// Current attempt
        /// </summary>
        public int AttemptNumber { get; set; }
        /// <summary>
        /// Length of the level, used to compute current %
        /// </summary>
        public float PhysicalLength { get; set; }
        /// <summary>
        /// Position on the X-axis at which the player object respawns
        /// </summary>
        public float StartPosition { get; set; }
        /// <summary>
        /// Position on the X-axis at which the player object currently respawns in practice
        /// </summary>
        public float PracticeStartPosition { get; set; }

        public GDLoadedLevel(bool isRunning, bool isTestmode, bool isPractice, int attemptNumber, float physicalLength, float startPosition, float practiceStartPosition)
        {
            IsRunning = isRunning;
            IsTestmode = isTestmode;
            IsPractice = isPractice;
            AttemptNumber = attemptNumber;
            PhysicalLength = physicalLength;
            StartPosition = startPosition;
            PracticeStartPosition = practiceStartPosition;
        }
    }
}
