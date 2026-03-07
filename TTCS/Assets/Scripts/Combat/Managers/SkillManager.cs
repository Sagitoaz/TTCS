using System.Collections.Generic;
using UnityEngine;
using TTCS.Core;
using TTCS.Core.Events;
using TTCS.Core.Data;
using TTCS.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Managers
{
    /// <summary>
    /// 🔵 Dev A - Skill Manager
    /// Quản lý cooldown, mana cost và giới hạn dùng skill mỗi trận.
    ///
    /// Responsibilities:
    ///   - Kiểm tra entity có thể dùng skill không (CanUseSkill)
    ///   - Trừ mana và kích hoạt cooldown khi UseSkill
    ///   - Giảm cooldown sau mỗi lượt (TickCooldowns)
    ///   - Theo dõi số lần dùng skill trong trận (limitPerFight)
    ///
    /// NOTE: SkillManager chỉ quản lý RESOURCE & COOLDOWN — không tính damage.
    /// Damage/Effect được xử lý bởi Dev B (ActionResolver).
    ///
    /// Usage:
    ///   SkillManager.Instance.RegisterEntity("char_warrior", startingMana: 80, maxMana: 100);
    ///   if (SkillManager.Instance.CanUseSkill("char_warrior", "skill_warrior_slash"))
    ///   {
    ///       SkillManager.Instance.UseSkill("char_warrior", "skill_warrior_slash");
    ///       // → Dev B resolves damage/effects
    ///   }
    ///   // Sau khi lượt kết thúc:
    ///   SkillManager.Instance.TickCooldowns("char_warrior");
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────
        private static SkillManager _instance;
        public static SkillManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("[SkillManager] Instance is null — add SkillManager to the scene!");
                return _instance;
            }
        }

        // ─── Constants ────────────────────────────────────────────────
        /// <summary>Mana mặc định khi RegisterEntity không chỉ định</summary>
        private const int DEFAULT_MAX_MANA = 100;

        /// <summary>Mana ban đầu khi combat bắt đầu (% của max)</summary>
        private const float STARTING_MANA_RATIO = 0.8f; // Bắt đầu với 80% mana

        // ─── State ────────────────────────────────────────────────────
        /// <summary>Cooldown còn lại (entityId → skillId → turns remaining)</summary>
        private readonly Dictionary<string, Dictionary<string, int>> _cooldowns = new();

        /// <summary>Số lần dùng skill trong trận (entityId → skillId → count)</summary>
        private readonly Dictionary<string, Dictionary<string, int>> _usageCount = new();

        /// <summary>Mana hiện tại (entityId → currentMana)</summary>
        private readonly Dictionary<string, int> _currentMana = new();

        /// <summary>Mana tối đa (entityId → maxMana)</summary>
        private readonly Dictionary<string, int> _maxMana = new();

        // ──────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLogger.Log("SkillManager initialized.", LogCategory.Combat);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Registration

        /// <summary>
        /// Đăng ký entity vào SkillManager trước khi combat bắt đầu.
        /// </summary>
        /// <param name="entityId">ID của entity</param>
        /// <param name="maxMana">Mana tối đa (mặc định: DEFAULT_MAX_MANA = 100)</param>
        /// <param name="startingMana">Mana lúc bắt đầu, -1 = tự tính theo STARTING_MANA_RATIO</param>
        public void RegisterEntity(string entityId, int maxMana = DEFAULT_MAX_MANA, int startingMana = -1)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                DebugLogger.LogWarning("RegisterEntity: entityId rỗng — bỏ qua.", LogCategory.Combat);
                return;
            }

            int safeMax = Mathf.Max(0, maxMana);
            int safeStart = startingMana < 0
                ? Mathf.RoundToInt(safeMax * STARTING_MANA_RATIO)
                : Mathf.Clamp(startingMana, 0, safeMax);

            _maxMana[entityId] = safeMax;
            _currentMana[entityId] = safeStart;
            _cooldowns[entityId] = new Dictionary<string, int>();
            _usageCount[entityId] = new Dictionary<string, int>();

            DebugLogger.Log(
                $"Registered skill entity '{entityId}': mana={safeStart}/{safeMax}",
                LogCategory.Combat);
        }

        /// <summary>
        /// Xóa entity khỏi SkillManager (gọi khi entity chết hoặc combat kết thúc).
        /// </summary>
        public void UnregisterEntity(string entityId)
        {
            _cooldowns.Remove(entityId);
            _usageCount.Remove(entityId);
            _currentMana.Remove(entityId);
            _maxMana.Remove(entityId);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Skill Checks

        /// <summary>
        /// Kiểm tra entity có thể dùng skill không.
        /// Trả về false và log lý do nếu không dùng được.
        /// </summary>
        /// <param name="entityId">ID của entity muốn dùng skill</param>
        /// <param name="skillId">ID của skill</param>
        /// <returns>True nếu có thể dùng</returns>
        public bool CanUseSkill(string entityId, string skillId)
        {
            if (!ValidateEntity(entityId, nameof(CanUseSkill))) return false;

            SkillDataModel skill = DataManager.Instance?.LoadSkill(skillId);
            if (skill == null)
            {
                DebugLogger.LogWarning($"CanUseSkill: Skill '{skillId}' không tìm thấy trong DataManager.", LogCategory.Combat);
                return false;
            }

            return CanUseSkillInternal(entityId, skill, out _);
        }

        /// <summary>
        /// Overload nhận SkillDataModel trực tiếp (tránh phải query DataManager lại).
        /// </summary>
        public bool CanUseSkill(string entityId, SkillDataModel skill)
        {
            if (!ValidateEntity(entityId, nameof(CanUseSkill))) return false;
            if (skill == null)
            {
                DebugLogger.LogWarning($"CanUseSkill: skill là null!", LogCategory.Combat);
                return false;
            }
            return CanUseSkillInternal(entityId, skill, out _);
        }

        /// <summary>
        /// Kiểm tra nội bộ — trả về lý do từ chối qua out parameter (dùng để log).
        /// </summary>
        private bool CanUseSkillInternal(string entityId, SkillDataModel skill, out string reason)
        {
            // Kiểm tra cooldown
            if (IsOnCooldown(entityId, skill.id))
            {
                int remaining = GetCooldown(entityId, skill.id);
                reason = $"Cooldown: {remaining} turn(s) remaining";
                return false;
            }

            // Kiểm tra mana
            if (skill.cost.mana > 0 && _currentMana[entityId] < skill.cost.mana)
            {
                reason = $"Not enough mana: need {skill.cost.mana}, have {_currentMana[entityId]}";
                return false;
            }

            // Kiểm tra limitPerFight
            if (skill.cost.limitPerFight > 0)
            {
                int used = GetUsageCount(entityId, skill.id);
                if (used >= skill.cost.limitPerFight)
                {
                    reason = $"Limit reached: used {used}/{skill.cost.limitPerFight} times this fight";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Skill Execution

        /// <summary>
        /// Thực thi việc dùng skill: trừ mana, đặt cooldown, tăng usageCount.
        /// Gọi sau khi CanUseSkill trả về true.
        /// KHÔNG tính damage — đó là việc của ActionResolver (Dev B).
        /// </summary>
        /// <param name="entityId">Entity dùng skill</param>
        /// <param name="skillId">ID skill</param>
        /// <returns>True nếu dùng thành công</returns>
        public bool UseSkill(string entityId, string skillId)
        {
            if (!ValidateEntity(entityId, nameof(UseSkill))) return false;

            SkillDataModel skill = DataManager.Instance?.LoadSkill(skillId);
            if (skill == null)
            {
                DebugLogger.LogWarning($"UseSkill: Skill '{skillId}' không tìm thấy.", LogCategory.Combat);
                return false;
            }

            return UseSkillInternal(entityId, skill);
        }

        /// <summary>
        /// Overload nhận SkillDataModel trực tiếp.
        /// </summary>
        public bool UseSkill(string entityId, SkillDataModel skill)
        {
            if (!ValidateEntity(entityId, nameof(UseSkill))) return false;
            if (skill == null) return false;
            return UseSkillInternal(entityId, skill);
        }

        private bool UseSkillInternal(string entityId, SkillDataModel skill)
        {
            // Kiểm tra lần cuối trước khi commit
            if (!CanUseSkillInternal(entityId, skill, out string reason))
            {
                DebugLogger.LogWarning($"UseSkill '{skill.id}' failed for '{entityId}': {reason}", LogCategory.Combat);
                return false;
            }

            // Trừ mana
            if (skill.cost.mana > 0)
            {
                _currentMana[entityId] -= skill.cost.mana;
                _currentMana[entityId] = Mathf.Max(0, _currentMana[entityId]);
            }

            // Đặt cooldown (nếu có)
            if (skill.cost.cooldown > 0)
            {
                SetCooldown(entityId, skill.id, skill.cost.cooldown);
            }

            // Tăng usage count
            if (!_usageCount[entityId].ContainsKey(skill.id))
                _usageCount[entityId][skill.id] = 0;
            _usageCount[entityId][skill.id]++;

            DebugLogger.Log(
                $"UseSkill: '{entityId}' used '{skill.id}' — mana={_currentMana[entityId]}/{_maxMana[entityId]}, cd={skill.cost.cooldown}, used={_usageCount[entityId][skill.id]} times",
                LogCategory.Combat);

            // Phát SkillCastEvent (target sẽ được truyền vào từ caller nếu cần riêng)
            EventBus.Instance.Publish(new SkillCastEvent(entityId, skill.id));
            EventBus.Instance.Publish(new ManaChangedEvent(entityId, _currentMana[entityId], _maxMana[entityId]));

            return true;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Cooldown Management

        /// <summary>
        /// Giảm tất cả cooldown của entity xuống 1 sau khi lượt kết thúc.
        /// Gọi ở cuối mỗi lượt (sau EndTurn của TurnManager).
        /// </summary>
        /// <param name="entityId">Entity cần tick cooldown</param>
        public void TickCooldowns(string entityId)
        {
            if (!ValidateEntity(entityId, nameof(TickCooldowns))) return;

            var cooldownDict = _cooldowns[entityId];
            var keysToProcess = new List<string>(cooldownDict.Keys);

            foreach (string skillId in keysToProcess)
            {
                if (cooldownDict[skillId] > 0)
                {
                    cooldownDict[skillId]--;
                    if (cooldownDict[skillId] == 0)
                    {
                        DebugLogger.Log($"TickCooldowns: '{skillId}' is now ready for '{entityId}'.", LogCategory.Combat);
                    }
                }
            }
        }

        /// <summary>Đặt cooldown cho một skill cụ thể</summary>
        public void SetCooldown(string entityId, string skillId, int turns)
        {
            if (!_cooldowns.ContainsKey(entityId)) return;
            _cooldowns[entityId][skillId] = Mathf.Max(0, turns);
        }

        /// <summary>Trả về cooldown còn lại (số lượt)</summary>
        public int GetCooldown(string entityId, string skillId)
        {
            if (!_cooldowns.ContainsKey(entityId)) return 0;
            return _cooldowns[entityId].TryGetValue(skillId, out int cd) ? cd : 0;
        }

        /// <summary>Kiểm tra skill có đang trong cooldown không</summary>
        public bool IsOnCooldown(string entityId, string skillId)
        {
            return GetCooldown(entityId, skillId) > 0;
        }

        /// <summary>Reset toàn bộ cooldown của entity (dùng khi có item/skill đặc biệt)</summary>
        public void ResetAllCooldowns(string entityId)
        {
            if (!_cooldowns.ContainsKey(entityId)) return;
            _cooldowns[entityId].Clear();
            DebugLogger.Log($"All cooldowns reset for '{entityId}'.", LogCategory.Combat);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Mana Management

        /// <summary>Trả về mana hiện tại của entity</summary>
        public int GetMana(string entityId)
        {
            return _currentMana.TryGetValue(entityId, out int mana) ? mana : 0;
        }

        /// <summary>Trả về mana tối đa của entity</summary>
        public int GetMaxMana(string entityId)
        {
            return _maxMana.TryGetValue(entityId, out int max) ? max : 0;
        }

        /// <summary>
        /// Hồi mana cho entity (heat, passive, item...).
        /// </summary>
        /// <param name="entityId">ID entity</param>
        /// <param name="amount">Lượng mana hồi (+)</param>
        public void RestoreMana(string entityId, int amount)
        {
            if (!_currentMana.ContainsKey(entityId)) return;

            int max = GetMaxMana(entityId);
            _currentMana[entityId] = Mathf.Clamp(_currentMana[entityId] + amount, 0, max);

            DebugLogger.Log(
                $"RestoreMana: '{entityId}' +{amount} mana → {_currentMana[entityId]}/{max}",
                LogCategory.Combat);

            EventBus.Instance.Publish(new ManaChangedEvent(entityId, _currentMana[entityId], max));
        }

        /// <summary>
        /// Trừ mana trực tiếp (dùng cho effect hoặc debug).
        /// </summary>
        public void DrainMana(string entityId, int amount)
        {
            if (!_currentMana.ContainsKey(entityId)) return;
            _currentMana[entityId] = Mathf.Max(0, _currentMana[entityId] - amount);
            EventBus.Instance.Publish(new ManaChangedEvent(entityId, _currentMana[entityId], GetMaxMana(entityId)));
        }

        /// <summary>Hồi đầy mana (khi combat bắt đầu wave mới hoặc item đặc biệt)</summary>
        public void FullRestoreMana(string entityId)
        {
            if (!_currentMana.ContainsKey(entityId)) return;
            _currentMana[entityId] = GetMaxMana(entityId);
            EventBus.Instance.Publish(new ManaChangedEvent(entityId, _currentMana[entityId], GetMaxMana(entityId)));
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Usage Count

        /// <summary>Trả về số lần entity đã dùng skill trong trận hiện tại</summary>
        public int GetUsageCount(string entityId, string skillId)
        {
            if (!_usageCount.ContainsKey(entityId)) return 0;
            return _usageCount[entityId].TryGetValue(skillId, out int count) ? count : 0;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Combat Reset

        /// <summary>
        /// Reset toàn bộ state combat của SkillManager.
        /// Gọi khi combat kết thúc, trước khi bắt đầu trận mới.
        /// </summary>
        public void ResetCombat()
        {
            _cooldowns.Clear();
            _usageCount.Clear();
            _currentMana.Clear();
            _maxMana.Clear();
            DebugLogger.Log("SkillManager combat state reset.", LogCategory.Combat);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────
        #region Utilities

        private bool ValidateEntity(string entityId, string callerName)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                DebugLogger.LogWarning($"{callerName}: entityId rỗng.", LogCategory.Combat);
                return false;
            }
            if (!_cooldowns.ContainsKey(entityId))
            {
                DebugLogger.LogWarning($"{callerName}: Entity '{entityId}' chưa được RegisterEntity().", LogCategory.Combat);
                return false;
            }
            return true;
        }

        #endregion
    }
}
