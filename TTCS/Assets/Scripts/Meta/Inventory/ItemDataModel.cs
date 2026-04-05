using System;

namespace TTCS.Meta.Inventory
{
    [Serializable]
    public class ItemDataModel
    {
        public string id;
        public string nameKey;
        public string itemType;
        public string iconPath;
        public string effectType;
        public int effectAmount;
        public bool usableOutsideCombat;
        public bool stackable;
        public int maxStack = 99;
        // Legacy field: keep for compatibility with old data/logic.
        public int healAmount;
    }
}
