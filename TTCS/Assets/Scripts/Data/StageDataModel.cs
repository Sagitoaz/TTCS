using System;
using System.Collections.Generic;

namespace TTCS.Data
{
    /// <summary>
    /// 💜 Shared - JSON-serializable model cho stage data.
    /// Dùng để deserialize từ Assets/Data/Stages/*.json
    /// </summary>
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
        public StageRewardFirstClear firstClear;
        public StageRewardRepeat repeat;
    }

    [Serializable]
    public class StageRewardFirstClear
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
    public class StageRewardItem
    {
        public string id;
        public int amount;
    }
}
