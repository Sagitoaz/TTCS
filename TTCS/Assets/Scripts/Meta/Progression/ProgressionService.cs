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

            if (save.GetTotalStarsInChapter(levelData.chapterId) >= levelData.requiredStarsToUnlock)
            {
                return true;
            }

            // Fallback tạm thời khi hệ thống sao chưa hoàn thiện: mở theo tiến trình tuần tự trong chapter.
            return HasSequentialUnlockByPreviousLevel(save, levelData);
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
                        save.GetTotalStarsInChapter(level.chapterId) < level.requiredStarsToUnlock &&
                        !HasSequentialUnlockByPreviousLevel(save, level))
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

        private static bool HasSequentialUnlockByPreviousLevel(SaveData save, LevelDataModel level)
        {
            if (save == null || level == null)
            {
                return false;
            }

            if (level.order <= 1)
            {
                return true;
            }

            var allLevels = DataManager.Instance?.GetAllLevels();
            if (allLevels == null)
            {
                return false;
            }

            var previousLevel = allLevels
                .Where(l => l != null && string.Equals(l.chapterId, level.chapterId, StringComparison.Ordinal) && l.order < level.order)
                .OrderByDescending(l => l.order)
                .FirstOrDefault();

            if (previousLevel == null || string.IsNullOrWhiteSpace(previousLevel.id))
            {
                return false;
            }

            var prevProgress = save.GetLevelProgress(previousLevel.id);
            return prevProgress.isCleared || save.unlockedLevels.Contains(previousLevel.id);
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
