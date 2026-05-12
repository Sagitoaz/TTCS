using System;
using System.Collections.Generic;

namespace TTCS.Data
{
    /// <summary>
    /// 💜 Shared - JSON-serializable model cho skill data.
    /// Dùng để deserialize từ Assets/Data/Skills/*.json
    /// </summary>
    [Serializable]
    public class SkillDataModel
    {
        public string id;
        public string nameKey;
        public string description;
        public string type;         // "attack", "heal", "buff", "debuff"
        public SkillTargetRule targetRule;
        public SkillCost cost;
        public SkillDamage damage;
        public List<SkillEffect> effects;
        public SkillTiming timing;
        public SkillActionCost actionCost;
        public SkillVisual visual;
    }

    [Serializable]
    public class SkillTargetRule
    {
        public string type;         // "single_enemy", "all_enemies", "single_ally", "self"
        public int count;
        public List<string> filter;
        public bool canTargetSelf;
    }

    [Serializable]
    public class SkillCost
    {
        public int mana;
        public int cooldown;
        public int limitPerFight;   // -1 = unlimited
    }

    [Serializable]
    public class SkillDamage
    {
        public string formula;      // e.g. "ATK * 1.5"
        public string element;
        public bool canCrit;
        public float ignoreDefense; // 0.0 = no ignore, 1.0 = full ignore
    }

    [Serializable]
    public class SkillEffect
    {
        public string type;         // "damage", "burn", "heal", "shield", etc.
        public string value;
        public string element;
        public float chance;        // 0.0 - 1.0
        public int duration;        // turns, 0 if instant
    }

    [Serializable]
    public class SkillTiming
    {
        public bool hasTimingWindow;
        public List<SkillTimingWindow> windows;
    }

    [Serializable]
    public class SkillTimingWindow
    {
        public float startMs;
        public float endMs;
    }

    [Serializable]
    public class SkillActionCost
    {
        public int timelineUnits;   // Timeline cost sau khi dùng skill
        public string description;
    }

    [Serializable]
    public class SkillVisual
    {
        public string attackStyle;  // "melee" or "ranged" (optional)
        public string animation;
        public float cameraShake;
        public float freeFrame;     // optional hit-stop duration (seconds)
    }
}
