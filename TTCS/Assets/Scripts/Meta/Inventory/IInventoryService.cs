using System.Collections.Generic;

namespace TTCS.Meta.Inventory
{
    public interface IInventoryService
    {
        IReadOnlyList<ItemStack> GetItems();
        bool CanUseItem(string itemId, string targetContext);
        UseItemResult UseItem(string itemId, int quantity, string targetContext);
        void AddItem(string itemId, int quantity);
    }
}
