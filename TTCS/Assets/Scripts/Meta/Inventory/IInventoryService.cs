using System.Collections.Generic;

namespace TTCS.Meta.Inventory
{
    /// <summary>
    /// Manages player inventory (items, consumables, etc.).
    /// Locked interface from Sprint 3 Phase 1 Kickoff.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Gets all items currently in inventory.
        /// </summary>
        IReadOnlyList<ItemStack> GetItems();

        /// <summary>
        /// Checks if an item can be used in a specific context.
        /// </summary>
        bool CanUseItem(string itemId, string targetContext);

        /// <summary>
        /// Uses an item from inventory. Returns result of the action.
        /// </summary>
        UseItemResult UseItem(string itemId, int quantity, string targetContext);

        void AddItem(string itemId, int quantity);
    }

}
