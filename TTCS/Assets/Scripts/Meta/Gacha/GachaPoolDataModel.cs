using System;
using System.Collections.Generic;

namespace TTCS.Meta.Gacha
{
    [Serializable]
    public class GachaPoolDataModel
    {
        public string id;
        public string nameKey;
        public int pityThreshold = 10;
        public List<GachaPoolEntry> entries = new List<GachaPoolEntry>();
    }

    [Serializable]
    public class GachaPoolEntry
    {
        public string rewardId;
        public string rewardType;
        public int weight;
        public bool isRare;
        public int amount = 1;
    }
}
