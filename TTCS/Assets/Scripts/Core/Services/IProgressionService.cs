using System.Collections.Generic;

namespace TTCS.Core
{
    /// <summary>
    /// Manages progression state: chapters, levels, unlock rules.
    /// Locked interface from Sprint 3 Phase 1 Kickoff.
    /// </summary>
    public interface IProgressionService
    {
        /// <summary>
        /// Gets the state of a chapter (unlocked, star count, etc.).
        /// </summary>
        ChapterState GetChapterState(string chapterId);

        /// <summary>
        /// Gets the state of a level within a chapter.
        /// </summary>
        LevelState GetLevelState(string levelId);

        /// <summary>
        /// Checks if player can enter a level (is unlocked, prerequisites met).
        /// </summary>
        bool CanEnterLevel(string levelId);

        /// <summary>
        /// Marks a level as completed and saves star/score.
        /// </summary>
        void MarkLevelCompleted(string levelId, int stars, int score);

        /// <summary>
        /// Attempts to unlock next content based on progression rules.
        /// Returns result with unlocked chapters/levels.
        /// </summary>
        UnlockResult TryUnlockNextContent();
    }

    /// <summary>
    /// State of a chapter.
    /// </summary>
    public class ChapterState
    {
        public string ChapterId { get; set; }
        public bool Unlocked { get; set; }
        public int CompletedLevels { get; set; }
        public int TotalStars { get; set; }
    }

    /// <summary>
    /// State of a single level.
    /// </summary>
    public class LevelState
    {
        public string LevelId { get; set; }
        public bool Unlocked { get; set; }
        public bool Cleared { get; set; }
        public int BestStars { get; set; }
        public int BestScore { get; set; }
    }

    /// <summary>
    /// Result of unlock attempt.
    /// </summary>
    public class UnlockResult
    {
        public bool Success { get; set; }
        public List<string> NewlyUnlockedChapters { get; set; } = new List<string>();
        public List<string> NewlyUnlockedLevels { get; set; } = new List<string>();
        public string Message { get; set; }
    }
}
