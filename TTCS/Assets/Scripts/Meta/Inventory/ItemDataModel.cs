using System;

namespace TTCS.Meta.Inventory
{
    [Serializable]
    public class ItemDataModel
    {
        public string id;
        public string nameKey;
        public string itemType;
        public bool usableOutsideCombat;
        public bool stackable;
        public int maxStack = 99;
        public int healAmount;
    }
}
