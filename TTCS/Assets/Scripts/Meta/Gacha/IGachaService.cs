using System.Collections.Generic;

namespace TTCS.Meta.Gacha
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
        public GachaPoolInfo GetPoolInfo(string poolId);

        /// <summary>
        /// Performs a gacha draw in a pool.
        /// Returns roll result with items/characters.
        /// </summary>
        public GachaRollResult Roll(string poolId, int count);

        /// <summary>
        /// Applies a roll result to the player's inventory.
        /// Updates roster, items, and pity state.
        /// </summary>
        public void ApplyRollResult(GachaRollResult result);
    }

   
    
    
}
