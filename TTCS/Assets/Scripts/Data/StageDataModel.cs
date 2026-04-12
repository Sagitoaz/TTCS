using System;
using System.Collections.Generic;

namespace TTCS.Data
{
    [Serializable]
    public class StageDataModel
    {
        public string id;
        public string nameKey;
        public string description;
        public int chapter;
        public int order;
        public StageRequirements requirements;
        public List<StageEncounter> encounters;
        public StageEnvironment environment;
        public StageRewards rewards;
        public StageTheme theme;
        public StageDifficulty difficulty;
    }

    [Serializable]
    public class StageRequirements
    {
        public int minLevel;
        public List<string> prerequisiteStages;
        public List<string> unlockConditions;
    }

    [Serializable]
    public class StageEncounter
    {
        public int wave;
        public List<StageEnemy> enemies;
        public float spawnDelay;
    }

    [Serializable]
    public class StageEnemy
    {
        public string enemyId;
        public int level;
        public int position;
    }

    [Serializable]
    public class StageEnvironment
    {
        public List<string> modifiers;
        public List<string> hazards;
        public string weatherEffect;
    }

    [Serializable]
    public class StageRewards
    {
        public StageRewardGroup firstClear;
        public StageRewardGroup repeatClear;
        public StageRewardRepeat repeat;
        public List<StageStarReward> stars;
    }

    [Serializable]
    public class StageRewardGroup
    {
        public int gold;
        public int exp;
        public List<StageRewardItem> items;
    }

    [Serializable]
    public class StageRewardRepeat
    {
        public int goldMin;
        public int goldMax;
        public int expMin;
        public int expMax;
    }

    [Serializable]
    public class StageStarReward
    {
        public string condition;
        public StageRewardGroup reward;
    }

    [Serializable]
    public class StageRewardItem
    {
        public string id;
        public int amount;
    }

    [Serializable]
    public class StageTheme
    {
        public string backgroundImage;
        public string musicTrack;
        public string ambientSFX;
    }

    [Serializable]
    public class StageDifficulty
    {
        public string rating;
        public int recommendedLevel;
        public int recommendedPower;
    }
}
