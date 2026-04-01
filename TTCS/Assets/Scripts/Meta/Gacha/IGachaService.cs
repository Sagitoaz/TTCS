using System.Collections.Generic;

namespace TTCS.Meta
{
    /// <summary>
    /// Manages gacha draw system.
    /// Locked interface from Sprint 3 Phase 1 Kickoff.
    /// </summary>
    public interface IGachaService
    {
        /// <summary>
        /// Gets information about a gacha pool (rates, cost, etc.).
        /// </summary>
        GachaPoolInfo GetPoolInfo(string poolId);

        /// <summary>
        /// Performs a gacha draw in a pool.
        /// Returns roll result with items/characters.
        /// </summary>
        GachaRollResult Roll(string poolId, int count);

        /// <summary>
        /// Applies a roll result to the player's inventory.
        /// Updates roster, items, and pity state.
        /// </summary>
        void ApplyRollResult(GachaRollResult result);
    }

    /// <summary>
    /// Information about a gacha pool.
    /// </summary>
    public class GachaPoolInfo
    {
        public string PoolId { get; set; }
        public string Name { get; set; }
        public string CurrencyId { get; set; } // e.g., "gem" or "ticket"
        public int CostPer1 { get; set; }     // cost for 1 pull
        public int CostPer10 { get; set; }    // cost for 10 pulls
    }

    /// <summary>
    /// Result of a gacha draw.
    /// </summary>
    public class GachaRollResult
    {
        public string PoolId { get; set; }
        public int PullCount { get; set; }
        public List<GachaItem> Items { get; set; } = new List<GachaItem>();
        public int NewPityCount { get; set; }
    }

    /// <summary>
    /// A single item from a gacha roll.
    /// </summary>
    public class GachaItem
    {
        public string ItemId { get; set; }
        public string Rarity { get; set; } // e.g., "common", "rare", "legendary"
        public int Quantity { get; set; }
    }
}
