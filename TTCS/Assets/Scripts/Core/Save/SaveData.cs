using System;
using System.Collections.Generic;

namespace TTCS.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public int schemaVersion = 1;

        // Player
        public int playerLevel = 1;
        public int totalExp = 0;
        public int gold = 0;
        public bool isNewGame = true;
        public string lastSavedTimestamp = "";

        // Existing roster and stage state
        public List<string> unlockedCharacters = new List<string>();
        public List<string> unlockedStages = new List<string>();
        public List<string> currentParty = new List<string>();

        // Existing parallel-list dictionaries
        public List<string> characterLevelKeys = new List<string>();
        public List<int> characterLevelValues = new List<int>();
        public List<string> characterExpKeys = new List<string>();
        public List<int> characterExpValues = new List<int>();
        public List<string> characterCurrentHpKeys = new List<string>();
        public List<int> characterCurrentHpValues = new List<int>();
        public List<string> characterCurrentManaKeys = new List<string>();
        public List<int> characterCurrentManaValues = new List<int>();
        public List<string> deployedCharacters = new List<string>();

        public List<string> clearedStages = new List<string>();
        public List<string> stageFirstClearKeys = new List<string>();
        public List<bool> stageFirstClearValues = new List<bool>();

        // Settings
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;

        // Sprint 3 meta-loop state
        public bool tutorialCompleted = false;
        public List<string> unlockedChapters = new List<string>();
        public List<string> unlockedLevels = new List<string>();
        public List<string> lineup = new List<string>();
        public List<SaveLevelProgress> levelProgress = new List<SaveLevelProgress>();
        public List<SaveItemStack> inventoryItems = new List<SaveItemStack>();
        public List<SavePityState> gachaPity = new List<SavePityState>();

        public int GetCharacterLevel(string characterId)
        {
            int idx = characterLevelKeys.IndexOf(characterId);
            return idx >= 0 ? characterLevelValues[idx] : 1;
        }

        public int GetCharacterCurrentHp(string characterId, int defaultHp)
        {
            int idx = characterCurrentHpKeys.IndexOf(characterId);
            return idx >= 0 ? characterCurrentHpValues[idx] : defaultHp;
        }

        public void SetCharacterCurrentHp(string characterId, int hp)
        {
            int idx = characterCurrentHpKeys.IndexOf(characterId);
            if (idx >= 0)
            {
                characterCurrentHpValues[idx] = hp;
            }
            else
            {
                characterCurrentHpKeys.Add(characterId);
                characterCurrentHpValues.Add(hp);
            }
        }

        public int GetCharacterCurrentMana(string characterId, int defaultMana)
        {
            int idx = characterCurrentManaKeys.IndexOf(characterId);
            return idx >= 0 ? characterCurrentManaValues[idx] : defaultMana;
        }

        public void SetCharacterCurrentMana(string characterId, int mana)
        {
            int idx = characterCurrentManaKeys.IndexOf(characterId);
            if (idx >= 0)
            {
                characterCurrentManaValues[idx] = mana;
            }
            else
            {
                characterCurrentManaKeys.Add(characterId);
                characterCurrentManaValues.Add(mana);
            }
        }

        public bool IsCharacterDeployed(string characterId)
        {
            return deployedCharacters.Contains(characterId);
        }

        public void SetCharacterDeployed(string characterId, bool deployed)
        {
            if (deployed)
            {
                if (!deployedCharacters.Contains(characterId))
                {
                    deployedCharacters.Add(characterId);
                }

                return;
            }

            deployedCharacters.Remove(characterId);
        }

        public void SetCharacterLevel(string characterId, int level)
        {
            int idx = characterLevelKeys.IndexOf(characterId);
            if (idx >= 0)
            {
                characterLevelValues[idx] = level;
            }
            else
            {
                characterLevelKeys.Add(characterId);
                characterLevelValues.Add(level);
            }
        }

        public bool IsFirstClear(string stageId)
        {
            int idx = stageFirstClearKeys.IndexOf(stageId);
            return idx >= 0 && stageFirstClearValues[idx];
        }

        public void MarkFirstClear(string stageId)
        {
            int idx = stageFirstClearKeys.IndexOf(stageId);
            if (idx >= 0)
            {
                stageFirstClearValues[idx] = true;
            }
            else
            {
                stageFirstClearKeys.Add(stageId);
                stageFirstClearValues.Add(true);
            }
        }

        public SaveLevelProgress GetLevelProgress(string levelId)
        {
            for (var i = 0; i < levelProgress.Count; i++)
            {
                if (levelProgress[i].levelId == levelId)
                {
                    return levelProgress[i];
                }
            }

            return new SaveLevelProgress
            {
                levelId = levelId,
                isCleared = false,
                stars = 0,
                bestScore = 0
            };
        }

        public void SetLevelProgress(SaveLevelProgress progress)
        {
            for (var i = 0; i < levelProgress.Count; i++)
            {
                if (levelProgress[i].levelId == progress.levelId)
                {
                    levelProgress[i] = progress;
                    return;
                }
            }

            levelProgress.Add(progress);
        }

        public int GetTotalStarsInChapter(string chapterId)
        {
            var total = 0;
            for (var i = 0; i < levelProgress.Count; i++)
            {
                if (levelProgress[i].levelId != null && levelProgress[i].levelId.StartsWith(chapterId + "_"))
                {
                    total += levelProgress[i].stars;
                }
            }

            return total;
        }

        public int GetTotalStars()
        {
            var total = 0;
            for (var i = 0; i < levelProgress.Count; i++)
            {
                total += levelProgress[i].stars;
            }

            return total;
        }

        public int GetPityCount(string poolId)
        {
            for (var i = 0; i < gachaPity.Count; i++)
            {
                if (gachaPity[i].poolId == poolId)
                {
                    return gachaPity[i].pityCount;
                }
            }

            return 0;
        }

        public void SetPityCount(string poolId, int pityCount)
        {
            for (var i = 0; i < gachaPity.Count; i++)
            {
                if (gachaPity[i].poolId == poolId)
                {
                    gachaPity[i].pityCount = pityCount;
                    return;
                }
            }

            gachaPity.Add(new SavePityState { poolId = poolId, pityCount = pityCount });
        }
    }

    [Serializable]
    public class SaveLevelProgress
    {
        public string levelId;
        public bool isCleared;
        public int stars;
        public int bestScore;
    }

    [Serializable]
    public class SaveItemStack
    {
        public string itemId;
        public int quantity;
    }

    [Serializable]
    public class SavePityState
    {
        public string poolId;
        public int pityCount;
    }
}
