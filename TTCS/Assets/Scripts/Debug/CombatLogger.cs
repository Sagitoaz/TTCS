using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat
{
    /// <summary>
    /// 🔵 Dev A - Combat Logger
    /// Logger chuyên dụng cho hệ thống combat.
    /// Wraps DebugLogger với format chuẩn cho combat logs.
    ///
    /// Format: [Turn X] Actor: Y | Action: Z | Result: W
    ///
    /// Usage:
    ///   CombatLogger.LogAction(3, "char_warrior", "skill_warrior_slash", "Hit enemy_bandit for 240 damage");
    ///   CombatLogger.LogTurnStart(3, "char_warrior");
    ///   CombatLogger.LogDamage(3, "char_warrior", "enemy_bandit", 240, isCrit: true);
    ///   CombatLogger.LogHeal(3, "char_mage", "char_warrior", 500);
    ///   CombatLogger.LogStatus(3, "char_warrior", "BURN", "applied");
    ///   CombatLogger.LogCombatResult(true, turnCount: 8);
    /// </summary>
    public static class CombatLogger
    {
        // ─── Core Format ──────────────────────────────────────────────

        /// <summary>
        /// Log một hành động chiến đấu với format đầy đủ.
        /// Format: [Turn X] Actor: Y | Action: Z | Result: W
        /// </summary>
        public static void LogAction(int turnNumber, string actorId, string actionId, string result)
        {
            string message = $"[Turn {turnNumber}] Actor: {actorId} | Action: {actionId} | Result: {result}";
            DebugLogger.Log(message, LogCategory.Combat);
        }

        // ─── Turn Events ──────────────────────────────────────────────

        /// <summary>Log khi lượt mới bắt đầu</summary>
        public static void LogTurnStart(int turnNumber, string actorId)
        {
            string message = $"[Turn {turnNumber}] Actor: {actorId} | Action: TURN_START | Result: Turn begins";
            DebugLogger.Log(message, LogCategory.Combat);
        }

        /// <summary>Log khi lượt kết thúc với chi phí hành động</summary>
        public static void LogTurnEnd(int turnNumber, string actorId, int actionCost)
        {
            string message = $"[Turn {turnNumber}] Actor: {actorId} | Action: TURN_END | Result: Cost={actionCost}";
            DebugLogger.Log(message, LogCategory.Combat);
        }

        // ─── Damage Events ────────────────────────────────────────────

        /// <summary>
        /// Log damage gây ra cho target.
        /// </summary>
        /// <param name="turnNumber">Số lượt hiện tại</param>
        /// <param name="actorId">Entity gây damage</param>
        /// <param name="targetId">Entity nhận damage</param>
        /// <param name="damageAmount">Lượng damage</param>
        /// <param name="isCrit">Có phải crit không</param>
        /// <param name="isBlocked">Có phải đòn bị block không</param>
        public static void LogDamage(
            int turnNumber,
            string actorId,
            string targetId,
            int damageAmount,
            bool isCrit = false,
            bool isBlocked = false)
        {
            string critTag = isCrit ? " [CRIT]" : "";
            string blockTag = isBlocked ? " [BLOCKED]" : "";
            string result = $"Dealt {damageAmount} DMG to {targetId}{critTag}{blockTag}";
            LogAction(turnNumber, actorId, "ATTACK", result);
        }

        /// <summary>
        /// Log heal thực hiện lên target.
        /// </summary>
        public static void LogHeal(int turnNumber, string actorId, string targetId, int healAmount)
        {
            string result = $"Healed {targetId} for {healAmount} HP";
            LogAction(turnNumber, actorId, "HEAL", result);
        }

        // ─── Skill Events ─────────────────────────────────────────────

        /// <summary>
        /// Log sử dụng skill.
        /// </summary>
        /// <param name="turnNumber">Số lượt</param>
        /// <param name="actorId">Entity sử dụng skill</param>
        /// <param name="skillId">ID của skill</param>
        /// <param name="targetIds">Danh sách target (dùng string.Join để nối)</param>
        public static void LogSkillUse(int turnNumber, string actorId, string skillId, params string[] targetIds)
        {
            string targets = targetIds.Length > 0 ? string.Join(", ", targetIds) : "none";
            string result = $"Targets: [{targets}]";
            LogAction(turnNumber, actorId, skillId, result);
        }

        /// <summary>
        /// Log skill không thực hiện được (cooldown, mana, etc.)
        /// </summary>
        public static void LogSkillFailed(int turnNumber, string actorId, string skillId, string reason)
        {
            string result = $"FAILED — {reason}";
            LogAction(turnNumber, actorId, skillId, result);
        }

        // ─── Status Effect Events ─────────────────────────────────────

        /// <summary>
        /// Log status effect được áp dụng hoặc kết thúc.
        /// </summary>
        /// <param name="turnNumber">Số lượt</param>
        /// <param name="targetId">Entity bị ảnh hưởng</param>
        /// <param name="statusName">Tên status (BURN, POISON, STUN...)</param>
        /// <param name="action">Trạng thái: "applied", "expired", "tick" (phát damage mỗi lượt), "resisted"</param>
        /// <param name="value">Giá trị liên quan (damage tick, stacks...) — 0 nếu không có</param>
        public static void LogStatus(int turnNumber, string targetId, string statusName, string action, int value = 0)
        {
            string valueStr = value > 0 ? $" ({value})" : "";
            string result = $"Status [{statusName}] {action}{valueStr} on {targetId}";
            DebugLogger.Log($"[Turn {turnNumber}] Actor: SYSTEM | Action: STATUS | Result: {result}", LogCategory.Combat);
        }

        // ─── Entity State Events ──────────────────────────────────────

        /// <summary>Log khi entity chết</summary>
        public static void LogDeath(int turnNumber, string entityId)
        {
            string message = $"[Turn {turnNumber}] Actor: SYSTEM | Action: DEATH | Result: {entityId} has been defeated";
            DebugLogger.LogWarning(message, LogCategory.Combat);
        }

        /// <summary>Log khi entity hồi phục (revive)</summary>
        public static void LogRevive(int turnNumber, string actorId, string targetId, int hpRestored)
        {
            string result = $"Revived {targetId} with {hpRestored} HP";
            LogAction(turnNumber, actorId, "REVIVE", result);
        }

        // ─── Combat State Events ──────────────────────────────────────

        /// <summary>Log khi combat bắt đầu</summary>
        public static void LogCombatStart(int seed, int entityCount)
        {
            string message = $"[Turn 0] Actor: SYSTEM | Action: COMBAT_START | Result: {entityCount} entities, seed={seed}";
            DebugLogger.Log(message, LogCategory.Combat);
        }

        /// <summary>Log kết quả combat</summary>
        /// <param name="victory">True nếu thắng</param>
        /// <param name="turnCount">Tổng số lượt đã diễn ra</param>
        public static void LogCombatResult(bool victory, int turnCount)
        {
            string outcome = victory ? "VICTORY" : "DEFEAT";
            string message = $"[Turn {turnCount}] Actor: SYSTEM | Action: COMBAT_END | Result: {outcome} after {turnCount} turns";

            if (victory)
                DebugLogger.Log(message, LogCategory.Combat);
            else
                DebugLogger.LogWarning(message, LogCategory.Combat);
        }

        /// <summary>Log wave bắt đầu trong stage</summary>
        public static void LogWaveStart(int waveNumber, int enemyCount)
        {
            string message = $"[Turn -] Actor: SYSTEM | Action: WAVE_START | Result: Wave {waveNumber} begins with {enemyCount} enemies";
            DebugLogger.Log(message, LogCategory.Combat);
        }

        // ─── Timeline Events ──────────────────────────────────────────

        /// <summary>
        /// Log trạng thái timeline (dùng để debug thứ tự lượt).
        /// </summary>
        public static void LogTimeline(int turnNumber, System.Collections.Generic.List<string> upcomingActors)
        {
            if (upcomingActors == null || upcomingActors.Count == 0)
            {
                DebugLogger.Log($"[Turn {turnNumber}] Timeline: (empty)", LogCategory.Combat);
                return;
            }

            string order = string.Join(" → ", upcomingActors);
            DebugLogger.Log($"[Turn {turnNumber}] Actor: SYSTEM | Action: TIMELINE | Result: {order}", LogCategory.Combat);
        }

        // ─── Error / Warning Helpers ──────────────────────────────────

        /// <summary>Log lỗi combat (lỗi logic, invalid state...)</summary>
        public static void LogError(int turnNumber, string context, string errorMessage)
        {
            string message = $"[Turn {turnNumber}] Actor: SYSTEM | Action: ERROR | Result: [{context}] {errorMessage}";
            DebugLogger.LogError(message, LogCategory.Combat);
        }
    }
}
