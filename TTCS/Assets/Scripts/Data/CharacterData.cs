using System.Collections.Generic;
using UnityEngine;

namespace TTCS.Data
{
    /// <summary>
    /// 🟢 Dev B - Character ScriptableObject
    /// Dữ liệu nhân vật dạng ScriptableObject cho Unity Inspector.
    ///
    /// Tạo asset: Project panel → Right-click → Create → TTCS → Character Data
    /// Đặt trong: Assets/ScriptableObjects/Characters/
    ///
    /// Mối quan hệ với JSON:
    ///   CharacterData (SO) = bản "editor-side" của CharacterDataModel (JSON).
    ///   DataManager load runtime từ JSON; SO dùng cho Inspector / prototyping nhanh.
    /// </summary>
    [CreateAssetMenu(
        fileName  = "NewCharacterData",
        menuName  = "TTCS/Character Data",
        order     = 10)]
    public class CharacterData : ScriptableObject
    {
        [Header("Metadata")]
        [Tooltip("ID duy nhất, khớp với field id trong JSON")]
        public string id;

        [Tooltip("Key nội bộ cho localization, hoặc dùng thẳng làm tên")]
        public string nameKey;

        [Tooltip("SSR / SR / R")]
        public string rarity = "R";

        [Tooltip("Attacker / Defender / Support")]
        public string roleTag;

        [Tooltip("Faction")]
        public string factionTag;

        [Header("Base Stats")]
        public int   baseHP  = 1000;
        public int   baseATK = 100;
        public int   baseDEF = 80;
        public int   baseSPD = 100;
        [Range(0f, 1f)]
        public float baseCritRate = 0.05f;
        [Range(0f, 1f)]
        public float baseResist   = 0f;

        [Header("Skills")]
        [Tooltip("Danh sách skill ID gắn cho nhân vật này (khớp id trong SkillData / skill JSON)")]
        public List<string> skillIds = new List<string>();

        [Header("Visual (Placeholder)")]
        [Tooltip("Sprites placeholder — phần visual hoàn thiện ở Sprint 2")]
        public Sprite bodySprite;
        public Sprite headSprite;
        public Sprite weaponSprite;
    }
}
