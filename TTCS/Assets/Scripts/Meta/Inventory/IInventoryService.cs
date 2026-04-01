using System.Collections.Generic;

namespace TTCS.Meta
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
    }

    /// <summary>
    /// A stack of items in inventory.
    /// </summary>
    public class ItemStack
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Result of using an item.
    /// </summary>
    public class UseItemResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int QuantityUsed { get; set; }
    }
}
