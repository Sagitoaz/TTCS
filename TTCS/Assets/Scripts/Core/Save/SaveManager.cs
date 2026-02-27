using System;
using System.IO;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Debugging;

namespace TTCS.Core.Save
{
    /// <summary>
    /// 🔵 Dev A - Save Manager Singleton.
    /// Đọc/ghi SaveData dưới dạng JSON vào Application.persistentDataPath.
    /// Hỗ trợ 3 save slots (index 0-2).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogWarning("[SaveManager] Instance accessed before Awake().");
                return _instance;
            }
        }

        /// <summary>SaveData đang active trong memory (slot đang chơi)</summary>
        public SaveData CurrentSave { get; private set; }

        /// <summary>Slot index đang được load (−1 nếu chưa load)</summary>
        public int ActiveSlotIndex { get; private set; } = -1;

        private const int   MAX_SLOTS    = 3;
        private const string SAVE_PREFIX = "ttcs_save_slot_";
        private const string SAVE_EXT    = ".json";

        // ─── Unity Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            DebugLogger.Log($"[SaveManager] Initialized. Save path: {Application.persistentDataPath}", DebugLogger.LogCategory.Save);
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Bắt đầu game mới — tạo SaveData mặc định và set làm CurrentSave.
        /// Chưa ghi file cho đến khi gọi Save().
        /// </summary>
        public void NewGame()
        {
            CurrentSave = CreateDefaultSaveData();
            ActiveSlotIndex = -1;
            DebugLogger.Log("[SaveManager] New game created.", DebugLogger.LogCategory.Save);
        }

        /// <summary>
        /// Lưu CurrentSave vào slot chỉ định.
        /// </summary>
        public void Save(int slotIndex = 0)
        {
            if (!ValidateSlotIndex(slotIndex)) return;
            if (CurrentSave == null)
            {
                DebugLogger.LogWarning("[SaveManager] CurrentSave is null. Call NewGame() or Load() first.", DebugLogger.LogCategory.Save);
                return;
            }

            CurrentSave.lastSavedTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            CurrentSave.isNewGame          = false;

            try
            {
                string json = JsonUtility.ToJson(CurrentSave, prettyPrint: true);
                File.WriteAllText(GetFilePath(slotIndex), json);

                DebugLogger.Log($"[SaveManager] Saved to slot {slotIndex}.", DebugLogger.LogCategory.Save);
                EventBus.Instance.Publish(new GameSavedEvent(slotIndex));
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[SaveManager] Failed to save slot {slotIndex}: {e.Message}", DebugLogger.LogCategory.Save);
            }
        }

        /// <summary>
        /// Load SaveData từ slot. Set làm CurrentSave và trả về.
        /// Trả về null nếu file không tồn tại hoặc lỗi parse.
        /// </summary>
        public SaveData Load(int slotIndex = 0)
        {
            if (!ValidateSlotIndex(slotIndex)) return null;

            string filePath = GetFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                DebugLogger.LogWarning($"[SaveManager] No save file at slot {slotIndex}.", DebugLogger.LogCategory.Save);
                return null;
            }

            try
            {
                string   json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                CurrentSave     = data;
                ActiveSlotIndex = slotIndex;

                DebugLogger.Log($"[SaveManager] Loaded slot {slotIndex} — Level {data.playerLevel}.", DebugLogger.LogCategory.Save);
                EventBus.Instance.Publish(new GameLoadedEvent(slotIndex));

                return data;
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[SaveManager] Failed to load slot {slotIndex}: {e.Message}", DebugLogger.LogCategory.Save);
                return null;
            }
        }

        /// <summary>Xóa file save của slot.</summary>
        public void DeleteSave(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex)) return;

            string filePath = GetFilePath(slotIndex);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                DebugLogger.Log($"[SaveManager] Deleted slot {slotIndex}.", DebugLogger.LogCategory.Save);

                if (ActiveSlotIndex == slotIndex)
                {
                    CurrentSave     = null;
                    ActiveSlotIndex = -1;
                }
            }
        }

        /// <summary>Kiểm tra slot có save file không.</summary>
        public bool HasSaveData(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex)) return false;
            return File.Exists(GetFilePath(slotIndex));
        }

        /// <summary>Lấy metadata tóm tắt của một slot để hiển thị ở UI.</summary>
        public SaveSlot GetSlotInfo(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex)) return SaveSlot.Empty(slotIndex);

            if (!HasSaveData(slotIndex)) return SaveSlot.Empty(slotIndex);

            // Load tạm để lấy metadata (không set CurrentSave)
            try
            {
                string   json = File.ReadAllText(GetFilePath(slotIndex));
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return SaveSlot.FromSaveData(slotIndex, data);
            }
            catch
            {
                return SaveSlot.Empty(slotIndex);
            }
        }

        /// <summary>Lấy info tất cả 3 slots.</summary>
        public SaveSlot[] GetAllSlots()
        {
            SaveSlot[] slots = new SaveSlot[MAX_SLOTS];
            for (int i = 0; i < MAX_SLOTS; i++)
                slots[i] = GetSlotInfo(i);
            return slots;
        }

        // ─── Private Helpers ─────────────────────────────────────────────────

        private string GetFilePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"{SAVE_PREFIX}{slotIndex}{SAVE_EXT}");
        }

        private bool ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < MAX_SLOTS) return true;
            DebugLogger.LogWarning($"[SaveManager] Invalid slot index: {slotIndex} (must be 0-{MAX_SLOTS - 1})", DebugLogger.LogCategory.Save);
            return false;
        }

        private SaveData CreateDefaultSaveData()
        {
            var data = new SaveData
            {
                playerLevel = 1,
                totalExp    = 0,
                gold        = 500,  // Starting gold
                isNewGame   = true,
                lastSavedTimestamp = ""
            };

            // Mở khóa nhân vật đầu tiên mặc định
            data.unlockedCharacters.Add("char_warrior");
            data.currentParty.Add("char_warrior");

            // Mở khóa stage đầu tiên
            data.unlockedStages.Add("stage_01_tutorial");

            return data;
        }
    }
}
