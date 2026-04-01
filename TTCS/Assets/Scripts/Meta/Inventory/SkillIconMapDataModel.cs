using System;
using System.Collections.Generic;

namespace TTCS.Meta.Inventory
{
    [Serializable]
    public class SkillIconMapDataModel
    {
        public string schemaVersion = "1";
        public string fallbackIconPath = "Icons/skill_default";
        public List<SkillIconMapEntry> entries = new List<SkillIconMapEntry>();
    }

    [Serializable]
    public class SkillIconMapEntry
    {
        public string skillId;
        public string iconPath;
    }
}
