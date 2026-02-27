using System;
using System.Collections.Generic;

namespace TTCS.Data
{
    /// <summary>
    /// 💜 Shared - JSON-serializable model cho character data.
    /// Dùng để deserialize từ Assets/Data/Characters/*.json
    /// Dev B sẽ nâng cấp thêm ScriptableObject wrapper sau.
    /// </summary>
    [Serializable]
    public class CharacterDataModel
    {
        public string id;
        public string nameKey;
        public string description;
        public CharacterMetadata metadata;
        public CharacterBaseStats baseStats;
        public CharacterGrowthCurve growthCurve;
        public List<string> skills;
        public CharacterPassive passive;
        public CharacterVisual visual;
        public CharacterAIHints aiHints;
    }

    [Serializable]
    public class CharacterMetadata
    {
        public string rarity;       // "SSR", "SR", "R"
        public string factionTag;
        public string roleTag;      // "Tank", "Attacker", "Support"
        public string element;
    }

    [Serializable]
    public class CharacterBaseStats
    {
        public int level;
        public int hp;
        public int atk;
        public int def;
        public int spd;
        public float crit;
        public float critDmg;
        public float resist;
    }

    [Serializable]
    public class CharacterGrowthCurve
    {
        public int hpPerLevel;
        public int atkPerLevel;
        public int defPerLevel;
        public int spdPerLevel;
    }

    [Serializable]
    public class CharacterPassive
    {
        public string id;
        public string nameKey;
        public string description;
    }

    [Serializable]
    public class CharacterVisual
    {
        public string spritePath;
        public string portraitPath;
        public string animatorController;
    }

    [Serializable]
    public class CharacterAIHints
    {
        public string priority;
        public List<string> preferredTargets;
        public float defensiveThreshold;
    }
}
