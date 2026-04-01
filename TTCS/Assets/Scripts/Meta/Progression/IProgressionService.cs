namespace TTCS.Meta.Progression
{
    public interface IProgressionService
    {
        ChapterState GetChapterState(string chapterId);
        LevelState GetLevelState(string levelId);
        bool CanEnterLevel(string levelId);
        void MarkLevelCompleted(string levelId, int stars, int score);
        UnlockResult TryUnlockNextContent();
        bool IsTutorialCompleted();
        void MarkTutorialCompleted();
    }
}
