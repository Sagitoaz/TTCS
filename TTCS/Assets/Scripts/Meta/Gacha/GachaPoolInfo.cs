namespace TTCS.Meta.Gacha
{
    public readonly struct GachaPoolInfo
    {
        public string PoolId { get; }
        public int PityCount { get; }
        public int PityThreshold { get; }

        public GachaPoolInfo(string poolId, int pityCount, int pityThreshold)
        {
            PoolId = poolId;
            PityCount = pityCount;
            PityThreshold = pityThreshold;
        }
    }
}
