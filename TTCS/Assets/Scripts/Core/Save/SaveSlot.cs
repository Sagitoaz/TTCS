using System;

namespace TTCS.Core.Save
{
    /// <summary>
    /// 🔵 Dev A - Metadata của một save slot để hiển thị ở UI chọn save.
    /// Không chứa full SaveData — chỉ thông tin tóm tắt.
    /// </summary>
    [Serializable]
    public class SaveSlot
    {
        /// <summary>Index slot (0-2)</summary>
        public int slotIndex;

        /// <summary>Slot có data không</summary>
        public bool hasData;

        /// <summary>Tên hiển thị (mặc định "Slot 1", "Slot 2", "Slot 3")</summary>
        public string displayName;

        /// <summary>Timestamp lần save gần nhất (ISO 8601)</summary>
        public string timestamp;

        /// <summary>Player level khi save</summary>
        public int playerLevel;

        /// <summary>Tổng số nhân vật đã unlock</summary>
        public int characterCount;

        /// <summary>Số stage đã clear</summary>
        public int clearedStageCount;

        // ─── Factory ─────────────────────────────────────────────────────────

        /// <summary>Tạo SaveSlot rỗng (chưa có data)</summary>
        public static SaveSlot Empty(int slotIndex)
        {
            return new SaveSlot
            {
                slotIndex        = slotIndex,
                hasData          = false,
                displayName      = $"Slot {slotIndex + 1}",
                timestamp        = "",
                playerLevel      = 0,
                characterCount   = 0,
                clearedStageCount = 0
            };
        }

        /// <summary>Tạo SaveSlot từ SaveData</summary>
        public static SaveSlot FromSaveData(int slotIndex, SaveData data)
        {
            return new SaveSlot
            {
                slotIndex         = slotIndex,
                hasData           = true,
                displayName       = $"Slot {slotIndex + 1}",
                timestamp         = data.lastSavedTimestamp,
                playerLevel       = data.playerLevel,
                characterCount    = data.unlockedCharacters?.Count ?? 0,
                clearedStageCount = data.clearedStages?.Count ?? 0
            };
        }
    }
}
