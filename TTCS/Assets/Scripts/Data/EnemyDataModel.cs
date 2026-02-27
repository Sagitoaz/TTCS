using System;
using System.Collections.Generic;

namespace TTCS.Data
{
    /// <summary>
    /// 💜 Shared - JSON-serializable model cho enemy data.
    /// Dùng để deserialize từ Assets/Data/Enemies/*.json
    /// </summary>
    [Serializable]
    public class EnemyDataModel
    {
        public string id;
        public string nameKey;
        public string description;
        public string type;         // "common", "elite", "boss"
        public EnemyBaseStats baseStats;
        public EnemyResistances resistances;
        public List<EnemyMove> moveSet;
        public List<EnemyPhase> phases;
        public EnemyTelegraph telegraph;
        public EnemyVisual visual;
        public EnemyRewards rewards;
    }

    [Serializable]
    public class EnemyBaseStats
    {
        public int hp;
        public int atk;
        public int def;
        public int spd;
        public float crit;
        public float resist;
    }

    [Serializable]
    public class EnemyResistances
    {
        public float physical;
        public float fire;
        public float ice;
        public float lightning;
        public float dark;
    }

    [Serializable]
    public class EnemyMove
    {
        public string skillId;
        public int weight;
        public List<EnemyMoveCondition> conditions;
    }

    [Serializable]
    public class EnemyMoveCondition
    {
        public string type;     // "hp_below", "turn_count", etc.
        public float value;
    }

    [Serializable]
    public class EnemyPhase
    {
        public int phaseId;
        public List<float> hpRange;
        public List<string> moves;
        public List<int> weights;
        public string behavior;
    }

    [Serializable]
    public class EnemyTelegraph
    {
        public string cueType;
        public float duration;
        public string animationKey;
    }

    [Serializable]
    public class EnemyVisual
    {
        public string spritePath;
        public string portraitPath;
        public string animatorController;
        public float scale;
    }

    [Serializable]
    public class EnemyRewards
    {
        public int expBase;
        public int goldBase;
        public List<EnemyDropItem> drops;
    }

    [Serializable]
    public class EnemyDropItem
    {
        public string itemId;
        public float dropChance;
        public int amount;
    }
}
