using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTCS.Data
{
    /// <summary>
    /// 🟢 Dev B - Stage ScriptableObject
    /// Cấu hình một màn chiến đấu: các wave và phần thưởng.
    ///
    /// Tạo asset: Project panel → Right-click → Create → TTCS → Stage Data
    /// Đặt trong: Assets/ScriptableObjects/Stages/
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewStageData",
        menuName = "TTCS/Stage Data",
        order    = 13)]
    public class StageData : ScriptableObject
    {
        [Header("Metadata")]
        public string id;
        public string nameKey;
        [Multiline(2)]
        public string description;

        [Header("Waves")]
        [Tooltip("Danh sách wave — mỗi wave là 1 nhóm enemy cần tiêu diệt trước khi sang wave sau")]
        public List<WaveConfig> waves = new List<WaveConfig>();

        [Header("Rewards")]
        public int rewardGold = 50;
        public int rewardXP   = 100;
        [Tooltip("Bonus reward lần đầu clear")]
        public int firstClearBonusGold = 100;
    }

    /// <summary>Cấu hình 1 wave trong stage</summary>
    [Serializable]
    public class WaveConfig
    {
        [Tooltip("Tên wave để debug")]
        public string waveName = "Wave 1";

        [Tooltip("Danh sách enemy ID sẽ spawn trong wave này")]
        public List<string> enemyIds = new List<string>();
    }
}
