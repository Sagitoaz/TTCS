namespace TTCS.Meta.Gacha
{
    public interface IGachaService
    {
        GachaPoolInfo GetPoolInfo(string poolId);
        GachaRollResult Roll(string poolId, int count);
        void ApplyRollResult(GachaRollResult result);
    }
}
