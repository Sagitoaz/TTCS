using System;
using System.Collections.Generic;

namespace TTCS.Core.Save
{
    /// <summary>
    /// 🔵 Dev A - Toàn bộ game state cần lưu.
    /// Tất cả fields phải là primitive hoặc List/array để JsonUtility serialize được.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // ─── Player Info ──────────────────────────────────────────────────────
        public int    playerLevel      = 1;
        public int    totalExp         = 0;
        public int    gold             = 0;
        public bool   isNewGame        = true;
        public string lastSavedTimestamp = "";

        // ─── Roster ───────────────────────────────────────────────────────────
        /// <summary>IDs của các nhân vật đã unlock</summary>
        public List<string> unlockedCharacters = new List<string>();

        /// <summary>IDs của các stage đã unlock hoặc clear</summary>
        public List<string> unlockedStages = new List<string>();

        /// <summary>IDs của 3 nhân vật trong party hiện tại</summary>
        public List<string> currentParty = new List<string>();

        // ─── Character Progress ───────────────────────────────────────────────
        // JsonUtility không serialize Dictionary → dùng parallel lists
        public List<string> characterLevelKeys   = new List<string>(); // character id
        public List<int>    characterLevelValues  = new List<int>();    // level tương ứng

        public List<string> characterExpKeys     = new List<string>();
        public List<int>    characterExpValues   = new List<int>();

        // ─── Stage Progress ───────────────────────────────────────────────────
        public List<string> clearedStages        = new List<string>();
        public List<string> stageFirstClearKeys  = new List<string>(); // stage id
        public List<bool>   stageFirstClearValues = new List<bool>();

        // ─── Settings ─────────────────────────────────────────────────────────
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>Lấy level của character. Trả về 1 nếu chưa có entry.</summary>
        public int GetCharacterLevel(string characterId)
        {
            int idx = characterLevelKeys.IndexOf(characterId);
            return idx >= 0 ? characterLevelValues[idx] : 1;
        }

        /// <summary>Set level của character.</summary>
        public void SetCharacterLevel(string characterId, int level)
        {
            int idx = characterLevelKeys.IndexOf(characterId);
            if (idx >= 0)
                characterLevelValues[idx] = level;
            else
            {
                characterLevelKeys.Add(characterId);
                characterLevelValues.Add(level);
            }
        }

        /// <summary>Kiểm tra stage đã clear lần đầu chưa.</summary>
        public bool IsFirstClear(string stageId)
        {
            int idx = stageFirstClearKeys.IndexOf(stageId);
            return idx >= 0 && stageFirstClearValues[idx];
        }

        /// <summary>Đánh dấu first clear cho stage.</summary>
        public void MarkFirstClear(string stageId)
        {
            int idx = stageFirstClearKeys.IndexOf(stageId);
            if (idx >= 0)
                stageFirstClearValues[idx] = true;
            else
            {
                stageFirstClearKeys.Add(stageId);
                stageFirstClearValues.Add(true);
            }
        }
    }
}
