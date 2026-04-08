using System;
using System.Collections.Generic;

namespace TTCS.Meta.Inventory
{
    [Serializable]
    public class ItemDataModel
    {
        public string id;
        public string nameKey;
        public string description;
        public string itemType;
        public string rarity;
        public string iconPath;
        public string effectType;
        public int effectAmount;
        public bool usableOutsideCombat;
        public bool equippable;
        public string equipSlot;
        public string statDescription;
        public List<AccessoryStatBonusData> statBonuses = new List<AccessoryStatBonusData>();
        public bool stackable;
        public int maxStack = 99;
        // Legacy field: keep for compatibility with old data/logic.
        public int healAmount;
    }

    [Serializable]
    public class AccessoryStatBonusData
    {
        public string statKey;
        public int amount;
        public string iconPath;
    }
}
