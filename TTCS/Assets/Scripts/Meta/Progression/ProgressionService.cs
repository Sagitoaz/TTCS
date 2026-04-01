using System;
using System.Linq;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Debugging;

namespace TTCS.Meta.Progression
{
    public sealed class ProgressionService : IProgressionService
    {
        private readonly SaveManager _saveManager;

        public ProgressionService(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public ChapterState GetChapterState(string chapterId)
        {
            var save = _saveManager.CurrentSave;
            var unlocked = save != null && save.unlockedChapters.Contains(chapterId);
            return new ChapterState(chapterId, unlocked);
        }

        public LevelState GetLevelState(string levelId)
        {
            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return new LevelState(levelId, false, false, 0, 0);
            }

            var progress = save.GetLevelProgress(levelId);
            var isUnlocked = save.unlockedLevels.Contains(levelId) || progress.isCleared;
            return new LevelState(levelId, isUnlocked, progress.isCleared, progress.stars, progress.bestScore);
        }

        public bool CanEnterLevel(string levelId)
        {
            var levelState = GetLevelState(levelId);
            if (levelState.IsUnlocked)
            {
                return true;
            }

            var levelData = DataManager.Instance?.LoadLevel(levelId);
            if (levelData == null)
            {
                return false;
            }

            var save = _saveManager.CurrentSave;
            if (save == null || !save.unlockedChapters.Contains(levelData.chapterId))
            {
                return false;
            }

            if (levelData.requiredStarsToUnlock <= 0)
            {
                return true;
            }

            return save.GetTotalStarsInChapter(levelData.chapterId) >= levelData.requiredStarsToUnlock;
        }

        public void MarkLevelCompleted(string levelId, int stars, int score)
        {
            var save = _saveManager.CurrentSave;
            if (save == null || string.IsNullOrWhiteSpace(levelId))
            {
                return;
            }

            stars = Clamp(stars, 0, 3);
            var current = save.GetLevelProgress(levelId);
            var merged = new SaveLevelProgress
            {
                levelId = levelId,
                isCleared = true,
                stars = Math.Max(current.stars, stars),
                bestScore = Math.Max(current.bestScore, score)
            };

            save.SetLevelProgress(merged);
            if (!save.unlockedLevels.Contains(levelId))
            {
                save.unlockedLevels.Add(levelId);
            }

            var unlockResult = TryUnlockNextContent();

            DebugLogger.Log(
                $"[ProgressionService] MarkLevelCompleted levelId='{levelId}' stars={stars} score={score} unlockChanged={unlockResult.AnyUnlocked}",
                DebugLogger.LogCategory.Save);
        }

        public UnlockResult TryUnlockNextContent()
        {
            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return new UnlockResult(false, 0, 0);
            }

            var chaptersUnlocked = 0;
            var levelsUnlocked = 0;

            var chapters = DataManager.Instance?.GetAllChapters();
            if (chapters != null)
            {
                foreach (var chapter in chapters)
                {
                    if (chapter == null || string.IsNullOrWhiteSpace(chapter.id))
                    {
                        continue;
                    }

                    if (save.unlockedChapters.Contains(chapter.id))
                    {
                        continue;
                    }

                    if (chapter.order <= 1)
                    {
                        save.unlockedChapters.Add(chapter.id);
                        chaptersUnlocked++;
                        continue;
                    }

                    if (chapter.requiredTotalStars > 0 && save.GetTotalStars() >= chapter.requiredTotalStars)
                    {
                        save.unlockedChapters.Add(chapter.id);
                        chaptersUnlocked++;
                    }
                }
            }

            var levels = DataManager.Instance?.GetAllLevels();
            if (levels != null)
            {
                foreach (var level in levels)
                {
                    if (level == null || string.IsNullOrWhiteSpace(level.id))
                    {
                        continue;
                    }

                    if (save.unlockedLevels.Contains(level.id))
                    {
                        continue;
                    }

                    if (!save.unlockedChapters.Contains(level.chapterId))
                    {
                        continue;
                    }

                    if (level.requiredStarsToUnlock > 0 &&
                        save.GetTotalStarsInChapter(level.chapterId) < level.requiredStarsToUnlock)
                    {
                        continue;
                    }

                    save.unlockedLevels.Add(level.id);
                    levelsUnlocked++;
                }
            }

            return new UnlockResult(
                anyUnlocked: chaptersUnlocked > 0 || levelsUnlocked > 0,
                chaptersUnlocked: chaptersUnlocked,
                levelsUnlocked: levelsUnlocked);
        }

        public bool IsTutorialCompleted()
        {
            return _saveManager.CurrentSave?.tutorialCompleted ?? false;
        }

        public void MarkTutorialCompleted()
        {
            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return;
            }

            save.tutorialCompleted = true;
            DebugLogger.Log("[ProgressionService] Tutorial completed flag set.", DebugLogger.LogCategory.Save);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
