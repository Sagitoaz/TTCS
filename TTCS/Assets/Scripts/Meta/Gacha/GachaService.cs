using System;
using System.Collections.Generic;
using System.Linq;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Core.Utilities;
using TTCS.Debugging;
using TTCS.Meta.Inventory;

namespace TTCS.Meta.Gacha
{
    public sealed class GachaService : IGachaService
    {
        private const string SharedPityKey = "__shared__";
        private readonly SaveManager _saveManager;
        private readonly IInventoryService _inventoryService;

        public GachaService(SaveManager saveManager, IInventoryService inventoryService)
        {
            _saveManager = saveManager;
            _inventoryService = inventoryService;
        }

        public GachaPoolInfo GetPoolInfo(string poolId)
        {
            var save = _saveManager.CurrentSave;
            if (save == null || string.IsNullOrWhiteSpace(poolId))
            {
                return new GachaPoolInfo(poolId, 0, 10);
            }

            var pity = save.GetPityCount(SharedPityKey);
            var pool = DataManager.Instance?.LoadGachaPool(poolId);
            var threshold = pool?.pityThreshold > 0 ? pool.pityThreshold : 10;
            return new GachaPoolInfo(poolId, pity, threshold);
        }

        public GachaRollResult Roll(string poolId, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Roll count must be > 0.");
            }

            var save = _saveManager.CurrentSave;
            var pool = DataManager.Instance?.LoadGachaPool(poolId);
            var result = new GachaRollResult { PoolId = poolId };

            if (save == null || pool == null || pool.entries == null || pool.entries.Count == 0)
            {
                return result;
            }

            var rollCost = count >= 10
                ? Math.Max(0, pool.rollCostTen)
                : Math.Max(0, pool.rollCostSingle) * count;

            if (save.gold < rollCost)
            {
                DebugLogger.LogWarning(
                    $"[GachaService] Not enough gold. Need={rollCost}, Current={save.gold}",
                    DebugLogger.LogCategory.Save);
                return result;
            }

            save.gold -= rollCost;

            var pityCount = save.GetPityCount(SharedPityKey);
            for (var i = 0; i < count; i++)
            {
                var shouldForceRare = pityCount + 1 >= Math.Max(1, pool.pityThreshold);
                var entry = shouldForceRare
                    ? RollRareEntry(pool.entries) ?? RollEntry(pool.entries)
                    : RollEntry(pool.entries);

                if (entry == null)
                {
                    continue;
                }

                result.Rewards.Add(new GachaRollReward
                {
                    RewardId = entry.rewardId,
                    RewardType = entry.rewardType,
                    Amount = Math.Max(1, entry.amount),
                    IsRare = entry.isRare
                });

                pityCount = entry.isRare ? 0 : pityCount + 1;
            }

            result.PityAfterRoll = pityCount;
            save.SetPityCount(SharedPityKey, pityCount);

            if (_saveManager.ActiveSlotIndex >= 0)
            {
                _saveManager.Save(_saveManager.ActiveSlotIndex);
            }

            DebugLogger.Log(
                $"[GachaService] Roll pool='{poolId}' count={count} rewards={result.Rewards.Count} pityAfter={result.PityAfterRoll} goldLeft={save.gold}",
                DebugLogger.LogCategory.Save);

            return result;
        }

        public void ApplyRollResult(GachaRollResult result)
        {
            if (result == null || result.Rewards.Count == 0)
            {
                return;
            }

            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                return;
            }

            foreach (var reward in result.Rewards)
            {
                if (string.Equals(reward.RewardType, "character", StringComparison.OrdinalIgnoreCase))
                {
                    if (!save.unlockedCharacters.Contains(reward.RewardId))
                    {
                        save.unlockedCharacters.Add(reward.RewardId);
                        continue;
                    }

                    // Duplicate character -> convert to gacha currency based on rarity.
                    var rarity = DataManager.Instance?.LoadCharacter(reward.RewardId)?.metadata?.rarity;
                    var converted = GetDuplicateCurrencyByRarity(rarity);
                    save.gold += converted;
                    reward.IsDuplicateConverted = true;
                    reward.ConvertedCurrencyAmount = converted;
                }
                else
                {
                    _inventoryService.AddItem(reward.RewardId, Math.Max(1, reward.Amount));
                }
            }

            if (_saveManager.ActiveSlotIndex >= 0)
            {
                _saveManager.Save(_saveManager.ActiveSlotIndex);
            }

            DebugLogger.Log(
                $"[GachaService] ApplyRollResult pool='{result.PoolId}' rewards={result.Rewards.Count}",
                DebugLogger.LogCategory.Save);
        }

        private static int GetDuplicateCurrencyByRarity(string rarity)
        {
            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase))
            {
                return 3500;
            }

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                return 2000;
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                return 800;
            }

            return 300;
        }

        private static GachaPoolEntry RollEntry(List<GachaPoolEntry> entries)
        {
            var totalWeight = entries.Sum(e => Math.Max(0, e.weight));
            if (totalWeight <= 0)
            {
                return null;
            }

            var random = RNGService.Instance.Range(0, totalWeight);
            var cursor = 0;
            foreach (var entry in entries)
            {
                cursor += Math.Max(0, entry.weight);
                if (random < cursor)
                {
                    return entry;
                }
            }

            return entries[entries.Count - 1];
        }

        private static GachaPoolEntry RollRareEntry(List<GachaPoolEntry> entries)
        {
            var rareEntries = entries.Where(e => e.isRare).ToList();
            return rareEntries.Count == 0 ? null : RollEntry(rareEntries);
        }
    }
}
