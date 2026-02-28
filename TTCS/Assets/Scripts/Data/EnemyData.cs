using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.AI;

namespace TTCS.Data
{
    /// <summary>
    /// 🟢 Dev B - Enemy ScriptableObject
    /// Dữ liệu kẻ địch dạng ScriptableObject cho Unity Inspector.
    ///
    /// Tạo asset: Project panel → Right-click → Create → TTCS → Enemy Data
    /// Đặt trong: Assets/ScriptableObjects/Enemies/
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewEnemyData",
        menuName = "TTCS/Enemy Data",
        order    = 12)]
    public class EnemyData : ScriptableObject
    {
        [Header("Metadata")]
        public string id;
        public string nameKey;
        [Tooltip("common / elite / boss")]
        public string enemyType = "common";

        [Header("Base Stats")]
        public int   baseHP  = 500;
        public int   baseATK = 80;
        public int   baseDEF = 50;
        public int   baseSPD = 90;
        [Range(0f, 1f)]
        public float baseCritRate = 0.05f;
        [Range(0f, 1f)]
        public float baseResist   = 0f;

        [Header("Skills")]
        [Tooltip("Danh sách skill ID của enemy này")]
        public List<string> skillIds = new List<string>();

        [Header("AI")]
        [Tooltip("AIBehavior asset quy định hành vi của enemy này. Tạo tại: Create → TTCS → Combat → AI Behavior")]
        public AIBehavior aiBehavior;

        [Header("Rewards")]
        public int rewardGold = 10;
        public int rewardXP   = 20;

        [Header("Visual (Placeholder)")]
        [Tooltip("Sprites placeholder — phần visual hoàn thiện ở Sprint 2")]
        public Sprite bodySprite;
    }
}
