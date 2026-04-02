using System;
using System.IO;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Core.Data;
using TTCS.Debugging;

namespace TTCS.Core.Save
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning("[SaveManager] Instance accessed before Awake().");
                }

                return _instance;
            }
        }

        public SaveData CurrentSave { get; private set; }
        public int ActiveSlotIndex { get; private set; } = -1;

        private const int MaxSlots = 3;
        private const string SavePrefix = "ttcs_save_slot_";
        private const string SaveExt = ".json";
        private const int CurrentSchemaVersion = 1;

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

        public void NewGame()
        {
            CurrentSave = CreateDefaultSaveData();
            ActiveSlotIndex = -1;
            DebugLogger.Log("[SaveManager] New game created.", DebugLogger.LogCategory.Save);
        }

        public SaveData EnsureCurrentSave(int fallbackSlotIndex = 0)
        {
            if (CurrentSave != null)
            {
                EnsureSaveDefaults(CurrentSave);
                return CurrentSave;
            }

            if (HasSaveData(fallbackSlotIndex))
            {
                var loaded = Load(fallbackSlotIndex);
                if (loaded != null)
                {
                    return loaded;
                }
            }

            NewGame();
            Save(fallbackSlotIndex);
            ActiveSlotIndex = fallbackSlotIndex;
            return CurrentSave;
        }

        public void Save(int slotIndex = 0)
        {
            if (!ValidateSlotIndex(slotIndex))
            {
                return;
            }

            if (CurrentSave == null)
            {
                DebugLogger.LogWarning("[SaveManager] CurrentSave is null. Call NewGame() or Load() first.", DebugLogger.LogCategory.Save);
                return;
            }

            EnsureSaveDefaults(CurrentSave);
            CurrentSave.schemaVersion = CurrentSchemaVersion;
            CurrentSave.lastSavedTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            CurrentSave.isNewGame = false;

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

        public SaveData Load(int slotIndex = 0)
        {
            if (!ValidateSlotIndex(slotIndex))
            {
                return null;
            }

            string filePath = GetFilePath(slotIndex);
            if (!File.Exists(filePath))
            {
                DebugLogger.LogWarning($"[SaveManager] No save file at slot {slotIndex}.", DebugLogger.LogCategory.Save);
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    DebugLogger.LogError($"[SaveManager] Save file at slot {slotIndex} is invalid JSON.", DebugLogger.LogCategory.Save);
                    return null;
                }

                ApplyMigrations(data);
                EnsureSaveDefaults(data);

                CurrentSave = data;
                ActiveSlotIndex = slotIndex;

                // Auto-save after applying defaults to persist any new data
                Save(slotIndex);

                DebugLogger.Log($"[SaveManager] Loaded slot {slotIndex} - Level {data.playerLevel}.", DebugLogger.LogCategory.Save);
                EventBus.Instance.Publish(new GameLoadedEvent(slotIndex));

                return data;
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[SaveManager] Failed to load slot {slotIndex}: {e.Message}", DebugLogger.LogCategory.Save);
                return null;
            }
        }

        public void DeleteSave(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex))
            {
                return;
            }

            string filePath = GetFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                DebugLogger.Log($"[SaveManager] Deleted slot {slotIndex}.", DebugLogger.LogCategory.Save);

                if (ActiveSlotIndex == slotIndex)
                {
                    CurrentSave = null;
                    ActiveSlotIndex = -1;
                }
            }
        }

        public bool HasSaveData(int slotIndex)
        {
            return ValidateSlotIndex(slotIndex) && File.Exists(GetFilePath(slotIndex));
        }

        public SaveSlot GetSlotInfo(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex))
            {
                return SaveSlot.Empty(slotIndex);
            }

            if (!HasSaveData(slotIndex))
            {
                return SaveSlot.Empty(slotIndex);
            }

            try
            {
                string json = File.ReadAllText(GetFilePath(slotIndex));
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return data == null ? SaveSlot.Empty(slotIndex) : SaveSlot.FromSaveData(slotIndex, data);
            }
            catch
            {
                return SaveSlot.Empty(slotIndex);
            }
        }

        public SaveSlot[] GetAllSlots()
        {
            SaveSlot[] slots = new SaveSlot[MaxSlots];
            for (int i = 0; i < MaxSlots; i++)
            {
                slots[i] = GetSlotInfo(i);
            }

            return slots;
        }

        private string GetFilePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"{SavePrefix}{slotIndex}{SaveExt}");
        }

        private bool ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < MaxSlots)
            {
                return true;
            }

            DebugLogger.LogWarning($"[SaveManager] Invalid slot index: {slotIndex} (must be 0-{MaxSlots - 1})", DebugLogger.LogCategory.Save);
            return false;
        }

        private SaveData CreateDefaultSaveData()
        {
            var data = new SaveData
            {
                schemaVersion = CurrentSchemaVersion,
                playerLevel = 1,
                totalExp = 0,
                gold = 500,
                isNewGame = true,
                lastSavedTimestamp = "",
                tutorialCompleted = false
            };

            data.unlockedCharacters.Add("char_warrior");
            data.currentParty.Add("char_warrior");
            data.lineup.Add("char_warrior");

            data.unlockedStages.Add("stage_01_tutorial");
            data.unlockedChapters.Add("chapter_01");
            data.unlockedLevels.Add("chapter_01_level_01");

            return data;
        }

        private void ApplyMigrations(SaveData data)
        {
            if (data.schemaVersion >= CurrentSchemaVersion)
            {
                return;
            }

            // Migration v0 -> v1 (Sprint 3 meta-loop fields)
            if (data.schemaVersion <= 0)
            {
                if (data.lineup.Count == 0 && data.currentParty.Count > 0)
                {
                    data.lineup.AddRange(data.currentParty);
                }

                if (data.unlockedLevels.Count == 0 && data.unlockedStages.Count > 0)
                {
                    for (var i = 0; i < data.unlockedStages.Count; i++)
                    {
                        var stageId = data.unlockedStages[i];
                        data.unlockedLevels.Add(stageId);
                    }
                }

                if (data.unlockedChapters.Count == 0)
                {
                    data.unlockedChapters.Add("chapter_01");
                }

                data.schemaVersion = 1;
            }
        }

        private static void EnsureSaveDefaults(SaveData data)
        {
            data.unlockedCharacters ??= new System.Collections.Generic.List<string>();
            data.unlockedStages ??= new System.Collections.Generic.List<string>();
            data.currentParty ??= new System.Collections.Generic.List<string>();
            data.characterLevelKeys ??= new System.Collections.Generic.List<string>();
            data.characterLevelValues ??= new System.Collections.Generic.List<int>();
            data.characterExpKeys ??= new System.Collections.Generic.List<string>();
            data.characterExpValues ??= new System.Collections.Generic.List<int>();
            data.characterCurrentHpKeys ??= new System.Collections.Generic.List<string>();
            data.characterCurrentHpValues ??= new System.Collections.Generic.List<int>();
            data.deployedCharacters ??= new System.Collections.Generic.List<string>();
            data.unlockedChapters ??= new System.Collections.Generic.List<string>();
            data.unlockedLevels ??= new System.Collections.Generic.List<string>();
            data.lineup ??= new System.Collections.Generic.List<string>();
            data.levelProgress ??= new System.Collections.Generic.List<SaveLevelProgress>();
            data.inventoryItems ??= new System.Collections.Generic.List<SaveItemStack>();
            data.gachaPity ??= new System.Collections.Generic.List<SavePityState>();

            // Ensure all unlocked characters have level and HP set
            for (var i = 0; i < data.unlockedCharacters.Count; i++)
            {
                var characterId = data.unlockedCharacters[i];
                if (string.IsNullOrWhiteSpace(characterId))
                {
                    continue;
                }

                // Ensure character has level (default 1)
                if (!data.characterLevelKeys.Contains(characterId))
                {
                    data.SetCharacterLevel(characterId, 1);
                }

                // Ensure character has full HP based on character data
                var character = DataManager.Instance?.LoadCharacter(characterId);
                if (character != null && !data.characterCurrentHpKeys.Contains(characterId))
                {
                    var baseHp = Math.Max(1, character.baseStats?.hp ?? 1000);
                    data.SetCharacterCurrentHp(characterId, baseHp);
                }
            }

            // Ensure test characters are available for day 4 testing.
            if (!data.unlockedCharacters.Contains("char_warrior"))
            {
                data.unlockedCharacters.Add("char_warrior");
            }

            if (!data.unlockedCharacters.Contains("char_mage"))
            {
                data.unlockedCharacters.Add("char_mage");
            }

            if (!data.characterLevelKeys.Contains("char_warrior"))
            {
                data.SetCharacterLevel("char_warrior", 1);
            }

            if (!data.characterLevelKeys.Contains("char_mage"))
            {
                data.SetCharacterLevel("char_mage", 1);
            }

            if (!data.characterCurrentHpKeys.Contains("char_warrior"))
            {
                var warrior = DataManager.Instance?.LoadCharacter("char_warrior");
                var warriorHp = Math.Max(1, warrior?.baseStats?.hp ?? 3000);
                data.SetCharacterCurrentHp("char_warrior", warriorHp);
            }

            if (!data.characterCurrentHpKeys.Contains("char_mage"))
            {
                var mage = DataManager.Instance?.LoadCharacter("char_mage");
                var mageHp = Math.Max(1, mage?.baseStats?.hp ?? 2000);
                data.SetCharacterCurrentHp("char_mage", mageHp);
            }

            if (data.lineup.Count == 0 && data.currentParty.Count > 0)
            {
                data.lineup.AddRange(data.currentParty);
            }

            if (data.currentParty.Count == 0 && data.lineup.Count > 0)
            {
                data.currentParty.AddRange(data.lineup);
            }

            if (data.unlockedChapters.Count == 0)
            {
                data.unlockedChapters.Add("chapter_01");
            }
        }
    }
}
