using UnityEngine;

namespace TTCS.Combat.AI
{
    /// <summary>
    /// 🔵 Dev A - AI Behavior Profile (ScriptableObject)
    /// Cấu hình hành vi AI cho từng loại enemy (hoặc có thể dùng cho unit thân thiện AI-controlled).
    ///
    /// Để tạo asset trong Unity:
    ///   Right-click trong Project panel → Create → TTCS → Combat → AI Behavior
    ///
    /// Mỗi enemy nên có 1 AIBehavior asset riêng (ví dụ: GoblinAI, DarkKnightAI).
    /// Tham chiếu từ: enemy JSON field "aiBehaviorAsset" (Sprint 2) hoặc gán trực tiếp trong Inspector.
    ///
    /// Decision Priority Order (từ cao đến thấp):
    ///   1. Heal nếu HP &lt; hpThresholdHeal và có heal skill
    ///   2. Strong attack nếu target HP &lt; hpThresholdAggressive
    ///   3. Special skill nếu cooldown ready (theo skillPreferences)
    ///   4. Basic attack (fallback)
    /// </summary>
    [CreateAssetMenu(
        fileName = "New AIBehavior",
        menuName = "TTCS/Combat/AI Behavior",
        order = 20)]
    public class AIBehavior : ScriptableObject
    {
        [Header("Profile Info")]
        [Tooltip("Tên profile AI — dùng để debug")]
        public string profileName = "Default AI";

        [Header("Behavior Thresholds")]
        [Range(0f, 1f)]
        [Tooltip("Khi HP% thấp hơn ngưỡng này → AI sẽ ưu tiên dùng skill heal (nếu có)")]
        public float hpThresholdHeal = 0.30f;

        [Range(0f, 1f)]
        [Tooltip("Khi target HP% thấp hơn ngưỡng này → AI dùng đòn mạnh nhất để kết liễu")]
        public float hpThresholdAggressive = 0.50f;

        [Header("Personality")]
        [Range(0f, 1f)]
        [Tooltip("0 = thụ động (ít tấn công), 1 = hung hãn (luôn tấn công mạnh nhất)")]
        public float aggression = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("0 = liều mạng, 1 = thận trọng (ưu tiên tự bảo vệ)")]
        public float defensiveness = 0.5f;

        [Header("Skill Configuration")]
        [Tooltip("Skill ID dùng để tấn công thường (fallback cuối cùng)")]
        public string basicAttackSkillId = "";

        [Tooltip("Skill ID dùng để tấn công mạnh nhất")]
        public string strongAttackSkillId = "";

        [Tooltip("Skill ID dùng để heal bản thân hoặc đồng đội")]
        public string healSkillId = "";

        [Tooltip("Danh sách skill ID theo thứ tự ưu tiên (AI thử từ đầu đến cuối)")]
        public string[] skillPreferences = new string[0];

        [Header("Target Preference")]
        [Tooltip("Loại target ưu tiên: 'lowest_hp', 'random_enemy', 'single_enemy'")]
        public string preferredTargetRule = "lowest_hp";

        // ─── Utility Methods ──────────────────────────────────────────

        /// <summary>
        /// Kiểm tra AI profile có hợp lệ không (có ít nhất basicAttackSkillId).
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(basicAttackSkillId))
            {
                Debug.LogWarning($"[AIBehavior] '{profileName}': basicAttackSkillId chưa được set!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Trả về danh sách skill theo thứ tự ưu tiên để AI thử theo thứ tự.
        /// Đưa healSkillId lên đầu nếu điều kiện heal được đáp ứng.
        /// </summary>
        /// <param name="includeHealFirst">True nếu HP thấp và nên ưu tiên heal</param>
        public string[] GetPrioritizedSkills(bool includeHealFirst = false)
        {
            var result = new System.Collections.Generic.List<string>();

            // Nếu nên heal, đưa heal skill lên đầu
            if (includeHealFirst && !string.IsNullOrEmpty(healSkillId))
                result.Add(healSkillId);

            // Thêm strong attack nếu aggressive
            if (!string.IsNullOrEmpty(strongAttackSkillId) && aggression >= 0.5f)
                result.Add(strongAttackSkillId);

            // Thêm skill preferences
            foreach (string skillId in skillPreferences)
            {
                if (!string.IsNullOrEmpty(skillId) && !result.Contains(skillId))
                    result.Add(skillId);
            }

            // Fallback: basic attack
            if (!string.IsNullOrEmpty(basicAttackSkillId) && !result.Contains(basicAttackSkillId))
                result.Add(basicAttackSkillId);

            return result.ToArray();
        }
    }
}
