using System;
using System.Collections.Generic;
using System.Linq;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Data;
using TTCS.Debugging;
using UnityEngine;

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

            // Inventory: return old item (if any), consume new item.
            if (!string.IsNullOrWhiteSpace(currentlyEquipped))
            {
                AdjustInventoryQuantity(save, currentlyEquipped, +1);
            }

            AdjustInventoryQuantity(save, itemId, -1);

            // Stats: adjust current HP/Mana by delta between accessories.
            ApplyAccessoryDeltaToCurrentResources(save, characterId, oldAccessoryItemId: currentlyEquipped, newAccessoryItemId: itemId);

            save.SetEquippedAccessory(characterId, itemId);

            PersistSave();

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

            // Inventory: return equipped item back to inventory.
            AdjustInventoryQuantity(save, equipped, +1);

            // Stats: remove accessory bonuses from current HP/Mana.
            ApplyAccessoryDeltaToCurrentResources(save, characterId, oldAccessoryItemId: equipped, newAccessoryItemId: string.Empty);

            save.SetEquippedAccessory(characterId, string.Empty);

            PersistSave();

            DebugLogger.Log(
                $"[InventoryService] UnequipAccessory character='{characterId}' oldItem='{equipped}'",
                DebugLogger.LogCategory.Save);

            return UseItemResult.Ok(0, $"Unequipped accessory from {characterId}.");
        }

        private void PersistSave()
        {
            var slot = _saveManager.ActiveSlotIndex >= 0 ? _saveManager.ActiveSlotIndex : 0;
            _saveManager.Save(slot);
        }

        private static void AdjustInventoryQuantity(SaveData save, string itemId, int delta)
        {
            if (save == null || string.IsNullOrWhiteSpace(itemId) || delta == 0)
            {
                return;
            }

            var stack = save.inventoryItems.FirstOrDefault(i => string.Equals(i.itemId, itemId, StringComparison.Ordinal));
            if (stack == null)
            {
                if (delta > 0)
                {
                    save.inventoryItems.Add(new SaveItemStack { itemId = itemId, quantity = delta });
                }

                return;
            }

            stack.quantity += delta;
            if (stack.quantity <= 0)
            {
                save.inventoryItems.Remove(stack);
            }
        }

        private static void ApplyAccessoryDeltaToCurrentResources(SaveData save, string characterId, string oldAccessoryItemId, string newAccessoryItemId)
        {
            if (save == null || string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            var level = Mathf.Max(1, save.GetCharacterLevel(characterId));
            var characterData = DataManager.Instance?.LoadCharacter(characterId);
            if (characterData == null)
            {
                return;
            }

            var baseMaxHp = EstimateMaxHp(characterData, level);
            var baseMaxMana = EstimateMaxMana(characterData, level);

            var oldItem = !string.IsNullOrWhiteSpace(oldAccessoryItemId) ? DataManager.Instance?.LoadItem(oldAccessoryItemId) : null;
            var newItem = !string.IsNullOrWhiteSpace(newAccessoryItemId) ? DataManager.Instance?.LoadItem(newAccessoryItemId) : null;

            var oldHpBonus = GetFlatBonus(oldItem, "HP");
            var newHpBonus = GetFlatBonus(newItem, "HP");
            var hpDelta = newHpBonus - oldHpBonus;

            var oldManaBonus = GetFlatBonus(oldItem, "MANA");
            var newManaBonus = GetFlatBonus(newItem, "MANA");
            var manaDelta = newManaBonus - oldManaBonus;

            var oldMaxHp = Mathf.Max(1, baseMaxHp + oldHpBonus);
            var newMaxHp = Mathf.Max(1, baseMaxHp + newHpBonus);
            var oldMaxMana = Mathf.Max(0, baseMaxMana + oldManaBonus);
            var newMaxMana = Mathf.Max(0, baseMaxMana + newManaBonus);

            var currentHp = save.GetCharacterCurrentHp(characterId, oldMaxHp);
            var currentMana = save.GetCharacterCurrentMana(characterId, oldMaxMana);

            var nextHp = Mathf.Clamp(currentHp + hpDelta, 0, newMaxHp);
            var nextMana = Mathf.Clamp(currentMana + manaDelta, 0, newMaxMana);

            save.SetCharacterCurrentHp(characterId, nextHp);
            save.SetCharacterCurrentMana(characterId, nextMana);
        }

        private static int GetFlatBonus(ItemDataModel item, string statKey)
        {
            if (item == null)
            {
                return 0;
            }

            var targetKey = AccessoryStatUtility.NormalizeStatKey(statKey);
            var bonuses = AccessoryStatUtility.GetBonuses(item);
            var total = 0;
            for (var i = 0; i < bonuses.Count; i++)
            {
                if (string.Equals(bonuses[i].StatKey, targetKey, StringComparison.OrdinalIgnoreCase))
                {
                    total += bonuses[i].Amount;
                }
            }

            return total;
        }

        private static int EstimateMaxHp(CharacterDataModel data, int level)
        {
            var baseHp = Mathf.Max(1, data.baseStats?.hp ?? 1);
            var perLevel = Mathf.Max(0, data.growthCurve?.hpPerLevel ?? 0);
            return Mathf.Max(1, baseHp + Mathf.Max(0, level - 1) * perLevel);
        }

        private static int EstimateMaxMana(CharacterDataModel data, int level)
        {
            var maxSkillCost = 0;
            if (data.skills != null)
            {
                for (var i = 0; i < data.skills.Count; i++)
                {
                    var skill = DataManager.Instance?.LoadSkill(data.skills[i]);
                    var mana = skill?.cost?.mana ?? 0;
                    if (mana > maxSkillCost)
                    {
                        maxSkillCost = mana;
                    }
                }
            }

            return Mathf.Max(20, maxSkillCost * 3 + Mathf.Max(1, level) * 5);
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
