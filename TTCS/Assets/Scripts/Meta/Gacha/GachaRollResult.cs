using System.Collections.Generic;

namespace TTCS.Meta.Gacha
{
    public sealed class GachaRollResult
    {
        public string PoolId { get; set; }
        public List<GachaRollReward> Rewards { get; } = new List<GachaRollReward>();
        public int PityAfterRoll { get; set; }
    }

    public sealed class GachaRollReward
    {
        public string RewardId { get; set; }
        public string RewardType { get; set; }
        public int Amount { get; set; }
        public bool IsRare { get; set; }
    }
}
