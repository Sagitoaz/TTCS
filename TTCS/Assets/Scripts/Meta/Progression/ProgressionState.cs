namespace TTCS.Meta.Progression
{
    public readonly struct ChapterState
    {
        public string ChapterId { get; }
        public bool IsUnlocked { get; }

        public ChapterState(string chapterId, bool isUnlocked)
        {
            ChapterId = chapterId;
            IsUnlocked = isUnlocked;
        }
    }

    public readonly struct LevelState
    {
        public string LevelId { get; }
        public bool IsUnlocked { get; }
        public bool IsCleared { get; }
        public int Stars { get; }
        public int BestScore { get; }

        public LevelState(string levelId, bool isUnlocked, bool isCleared, int stars, int bestScore)
        {
            LevelId = levelId;
            IsUnlocked = isUnlocked;
            IsCleared = isCleared;
            Stars = stars;
            BestScore = bestScore;
        }
    }

    public readonly struct UnlockResult
    {
        public bool AnyUnlocked { get; }
        public int ChaptersUnlocked { get; }
        public int LevelsUnlocked { get; }

        public UnlockResult(bool anyUnlocked, int chaptersUnlocked, int levelsUnlocked)
        {
            AnyUnlocked = anyUnlocked;
            ChaptersUnlocked = chaptersUnlocked;
            LevelsUnlocked = levelsUnlocked;
        }
    }
}
