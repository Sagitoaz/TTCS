using System;
using System.Collections.Generic;
using System.Linq;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Debugging;

namespace TTCS.Meta.Inventory
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly SaveManager _saveManager;

        public InventoryService(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public IReadOnlyList<ItemStack> GetItems()
        {
            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return Array.Empty<ItemStack>();
            }

            return save.inventoryItems
                .Select(i => new ItemStack { itemId = i.itemId, quantity = i.quantity })
                .ToList();
        }

        public bool CanUseItem(string itemId, string targetContext)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (!string.Equals(targetContext, "menu", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return false;
            }

            var itemData = DataManager.Instance?.LoadItem(itemId);
            if (itemData != null && !string.Equals(itemData.itemType, "consumable", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var stack = save.inventoryItems.FirstOrDefault(i => i.itemId == itemId);
            return stack != null && stack.quantity > 0;
        }

        public UseItemResult UseItem(string itemId, int quantity, string targetContext)
        {
            if (quantity <= 0)
            {
                return UseItemResult.Failed("Quantity must be > 0.");
            }

            if (!CanUseItem(itemId, targetContext))
            {
                return UseItemResult.Failed("Item cannot be used in this context.");
            }

            var save = _saveManager.CurrentSave;
            var stack = save.inventoryItems.First(i => i.itemId == itemId);
            if (stack.quantity < quantity)
            {
                return UseItemResult.Failed("Not enough item quantity.");
            }

            stack.quantity -= quantity;
            if (stack.quantity <= 0)
            {
                save.inventoryItems.Remove(stack);
            }

            DebugLogger.Log(
                $"[InventoryService] UseItem itemId='{itemId}' quantity={quantity} context='{targetContext}'",
                DebugLogger.LogCategory.Save);

            return UseItemResult.Ok(quantity, "Item consumed.");
        }

        public bool CanEquipAccessory(string itemId, string characterId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return false;
            }

            var itemData = DataManager.Instance?.LoadItem(itemId);
            if (itemData == null)
            {
                return false;
            }

            if (!string.Equals(itemData.itemType, "accessory", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!itemData.equippable)
            {
                return false;
            }

            if (!save.unlockedCharacters.Contains(characterId))
            {
                return false;
            }

            var stack = save.inventoryItems.FirstOrDefault(i => i.itemId == itemId);
            return stack != null && stack.quantity > 0;
        }

        public UseItemResult EquipAccessory(string itemId, string characterId)
        {
            if (!CanEquipAccessory(itemId, characterId))
            {
                return UseItemResult.Failed("Accessory cannot be equipped.");
            }

            var save = _saveManager.CurrentSave;
            var currentlyEquipped = save.GetEquippedAccessory(characterId);
            if (string.Equals(currentlyEquipped, itemId, StringComparison.Ordinal))
            {
                return UseItemResult.Ok(0, "Accessory is already equipped.");
            }

            save.SetEquippedAccessory(characterId, itemId);

            DebugLogger.Log(
                $"[InventoryService] EquipAccessory character='{characterId}' itemId='{itemId}'",
                DebugLogger.LogCategory.Save);

            return UseItemResult.Ok(0, $"Equipped {itemId} to {characterId}.");
        }

        public string GetEquippedAccessory(string characterId)
        {
            var save = _saveManager.CurrentSave;
            if (save == null || string.IsNullOrWhiteSpace(characterId))
            {
                return string.Empty;
            }

            return save.GetEquippedAccessory(characterId);
        }

        public UseItemResult UnequipAccessory(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return UseItemResult.Failed("Character is required.");
            }

            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return UseItemResult.Failed("Save is not available.");
            }

            var equipped = save.GetEquippedAccessory(characterId);
            if (string.IsNullOrWhiteSpace(equipped))
            {
                return UseItemResult.Failed("No accessory equipped.");
            }

            save.SetEquippedAccessory(characterId, string.Empty);

            DebugLogger.Log(
                $"[InventoryService] UnequipAccessory character='{characterId}' oldItem='{equipped}'",
                DebugLogger.LogCategory.Save);

            return UseItemResult.Ok(0, $"Unequipped accessory from {characterId}.");
        }

        public void AddItem(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            {
                return;
            }

            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return;
            }

            var stack = save.inventoryItems.FirstOrDefault(i => i.itemId == itemId);
            if (stack == null)
            {
                save.inventoryItems.Add(new SaveItemStack
                {
                    itemId = itemId,
                    quantity = quantity
                });
            }
            else
            {
                stack.quantity += quantity;
            }

            DebugLogger.Log(
                $"[InventoryService] AddItem itemId='{itemId}' quantity={quantity}",
                DebugLogger.LogCategory.Save);
        }

        
    }
}
