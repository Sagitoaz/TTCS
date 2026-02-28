using UnityEngine;
using System.Collections.Generic;

namespace TTCS.Data
{
    /// <summary>
    /// 🟢 Dev B - Skill ScriptableObject
    /// Định nghĩa kỹ năng dạng ScriptableObject cho Unity Inspector.
    ///
    /// Tạo asset: Project panel → Right-click → Create → TTCS → Skill Data
    /// Đặt trong: Assets/ScriptableObjects/Skills/
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewSkillData",
        menuName = "TTCS/Skill Data",
        order    = 11)]
    public class SkillData : ScriptableObject
    {
        [Header("Metadata")]
        [Tooltip("ID duy nhất, khớp với field id trong JSON")]
        public string id;

        [Tooltip("Tên kỹ năng (hoặc localization key)")]
        public string nameKey;

        [Tooltip("attack / heal / buff / debuff / ultimate")]
        public string type = "attack";

        [Header("Targeting")]
        [Tooltip("single_enemy / all_enemies / single_ally / all_allies / self")]
        public string targetRule = "single_enemy";

        [Header("Cost")]
        public int   manaCost    = 0;
        public int   cooldown    = 0;
        [Tooltip("Số lần tối đa dùng mỗi trận (-1 = không giới hạn)")]
        public int   limitPerFight = -1;

        [Header("Damage")]
        [Tooltip("Hệ số nhân ATK để tính sát thương (0 nếu không gây damage)")]
        public float damageMultiplier = 1.0f;

        [Tooltip("Nguyên tố của skill")]
        public string element = "Physical";

        [Header("Effects")]
        [Tooltip("Danh sách effect ID áp dụng lên target khi skill trúng")]
        public List<string> applyEffectIds = new List<string>();
        [Tooltip("Xác suất áp dụng effect (0-1). Khớp theo index với applyEffectIds")]
        public List<float>  effectChances  = new List<float>();

        [Header("Action Speed")]
        [Tooltip("80=Fast, 100=Normal, 120=Slow — ảnh hưởng đến timeline cost")]
        public int actionSpeedCost = 100;

        [Header("Description")]
        [Multiline(3)]
        public string description;
    }
}
