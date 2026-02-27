using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.AI
{
    /// <summary>
    /// Snapshot trạng thái của một entity tại thời điểm AI đưa ra quyết định.
    /// Dùng thay cho CombatEntity trực tiếp để decoupled với Dev B.
    /// Dev B sẽ tạo method ToSnapshot() trên CombatEntity khi sẵn sàng.
    /// </summary>
    [System.Serializable]
    public struct CombatEntitySnapshot
    {
        /// <summary>ID duy nhất của entity</summary>
        public string entityId;

        /// <summary>HP hiện tại (0 đến MaxHP)</summary>
        public int currentHP;

        /// <summary>HP tối đa</summary>
        public int maxHP;

        /// <summary>True nếu là đồng đội (cùng phe với AI)</summary>
        public bool isAlly;

        /// <summary>True nếu entity còn sống</summary>
        public bool IsAlive => currentHP > 0;

        /// <summary>Tỷ lệ HP còn lại (0.0 → 1.0)</summary>
        public float HPPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;

        /// <summary>Constructor tiện ích</summary>
        public CombatEntitySnapshot(string id, int hp, int maxHp, bool ally)
        {
            entityId = id;
            currentHP = hp;
            maxHP = maxHp;
            isAlly = ally;
        }
    }

    /// <summary>
    /// 🔵 Dev A - Target Selector
    /// Static utility class — chọn target dựa trên target rule type.
    ///
    /// Target rule types (từ SkillDataModel.SkillTargetRule.type):
    ///   "single_enemy"  → Enemy có HP thấp nhất
    ///   "all_enemies"   → Tất cả enemy còn sống
    ///   "single_ally"   → Ally có HP thấp nhất (không phải caster)
    ///   "all_allies"    → Tất cả ally còn sống
    ///   "lowest_hp"     → Entity (bất kể phe) có HP% thấp nhất
    ///   "highest_hp"    → Entity có HP% cao nhất
    ///   "random_enemy"  → Random 1 enemy còn sống
    ///   "self"          → Chính caster
    ///
    /// Usage:
    ///   List&lt;string&gt; targets = TargetSelector.SelectTargets(
    ///       casterId, "single_enemy", count: 1, allEntities);
    /// </summary>
    public static class TargetSelector
    {
        /// <summary>
        /// Chọn targets dựa trên targetRuleType từ SkillDataModel.
        /// </summary>
        /// <param name="casterId">ID của entity đang chọn target</param>
        /// <param name="targetRuleType">Loại target rule (từ SkillDataModel.SkillTargetRule.type)</param>
        /// <param name="count">Số target tối đa cần chọn</param>
        /// <param name="allEntities">Tất cả entity hiện có trong combat (cả 2 phe)</param>
        /// <returns>List entityId được chọn làm target</returns>
        public static List<string> SelectTargets(
            string casterId,
            string targetRuleType,
            int count,
            IEnumerable<CombatEntitySnapshot> allEntities)
        {
            var entities = allEntities?.ToList() ?? new List<CombatEntitySnapshot>();
            var caster = entities.FirstOrDefault(e => e.entityId == casterId);
            bool casterIsAlly = caster.entityId != null && caster.isAlly;

            var aliveEnemies = entities.Where(e => e.IsAlive && e.isAlly != casterIsAlly).ToList();
            var aliveAllies = entities.Where(e => e.IsAlive && e.isAlly == casterIsAlly && e.entityId != casterId).ToList();
            var aliveAll = entities.Where(e => e.IsAlive).ToList();

            switch (targetRuleType?.ToLower())
            {
                case "single_enemy":
                    return SelectLowestHP(aliveEnemies, count);

                case "all_enemies":
                    return aliveEnemies.Select(e => e.entityId).ToList();

                case "single_ally":
                    return SelectLowestHP(aliveAllies, count);

                case "all_allies":
                    return aliveAllies.Select(e => e.entityId).ToList();

                case "lowest_hp":
                    return SelectLowestHP(aliveAll, count);

                case "highest_hp":
                    return SelectHighestHP(aliveAll, count);

                case "random_enemy":
                    return SelectRandom(aliveEnemies, count);

                case "random_ally":
                    return SelectRandom(aliveAllies, count);

                case "self":
                    return new List<string> { casterId };

                default:
                    DebugLogger.LogWarning(
                        $"TargetSelector: Unknown targetRuleType '{targetRuleType}' — defaulting to single_enemy.",
                        LogCategory.AI);
                    return SelectLowestHP(aliveEnemies, 1);
            }
        }

        // ─── Selection Strategies ──────────────────────────────────────

        /// <summary>Chọn entity có HP% thấp nhất (ưu tiên bị đánh chết hoặc cần heal)</summary>
        private static List<string> SelectLowestHP(List<CombatEntitySnapshot> pool, int count)
        {
            return pool
                .OrderBy(e => e.HPPercent)
                .Take(count)
                .Select(e => e.entityId)
                .ToList();
        }

        /// <summary>Chọn entity có HP% cao nhất</summary>
        private static List<string> SelectHighestHP(List<CombatEntitySnapshot> pool, int count)
        {
            return pool
                .OrderByDescending(e => e.HPPercent)
                .Take(count)
                .Select(e => e.entityId)
                .ToList();
        }

        /// <summary>Chọn ngẫu nhiên từ pool (dùng Unity Random)</summary>
        private static List<string> SelectRandom(List<CombatEntitySnapshot> pool, int count)
        {
            if (pool.Count == 0) return new List<string>();

            // Shuffle rồi lấy count đầu
            var shuffled = pool.OrderBy(_ => Random.value).Take(count).Select(e => e.entityId).ToList();
            return shuffled;
        }

        // ─── Utility ──────────────────────────────────────────────────

        /// <summary>
        /// Kiểm tra target list có hợp lệ không (không rỗng, tất cả còn sống).
        /// </summary>
        public static bool ValidateTargets(List<string> targetIds, IEnumerable<CombatEntitySnapshot> allEntities)
        {
            if (targetIds == null || targetIds.Count == 0) return false;

            var aliveIds = new HashSet<string>(
                allEntities.Where(e => e.IsAlive).Select(e => e.entityId));

            return targetIds.All(id => aliveIds.Contains(id));
        }
    }
}
