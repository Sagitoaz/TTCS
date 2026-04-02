using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Data;
using TTCS.Meta;
using TTCS.Meta.Team;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.TeamFormation
{
    /// <summary>
    /// Day 4 team setup UI controller.
    /// Handles 3-slot lineup selection with picker/filter/confirm flow.
    /// </summary>
    public class TeamFormationUIController : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button[] _slotButtons;
        [SerializeField] private SlotView[] _slotViews;

        [Header("Panels")]
        [SerializeField] private GameObject _teamPanel;
        [SerializeField] private GameObject _pickerPanel;

        [Header("Picker")]
        [SerializeField] private Transform _pickerListRoot;
        [SerializeField] private TeamFormationPickerCellView _pickerItemPrefab;
        [SerializeField] private Button _pickerConfirmButton;
        [SerializeField] private Button _pickerCloseButton;

        [Header("Picker Filters (optional)")]
        [SerializeField] private TMP_Dropdown _sortDropdown;
        [SerializeField] private TMP_Dropdown _roleDropdown;

        [Header("Quick Info (left panel in picker)")]
        [SerializeField] private TextMeshProUGUI _quickInfoNameText;
        [SerializeField] private TextMeshProUGUI _quickInfoRoleText;
        [SerializeField] private TextMeshProUGUI _quickInfoRarityText;
        [SerializeField] private TextMeshProUGUI _quickInfoElementText;
        [SerializeField] private Image _quickInfoPortrait;
        [SerializeField] private Transform _quickInfoSkillRoot;
        [SerializeField] private TeamFormationSkillQuickItemView _quickInfoSkillItemPrefab;

        [Serializable]
        public sealed class SlotView
        {
            public GameObject selectedHighlight;
            public Image portrait;
            public TextMeshProUGUI nameText;
            public Slider hpSlider;
            public TextMeshProUGUI hpText;
            public TextMeshProUGUI levelText;
        }

        private enum SortMode
        {
            LevelAsc = 0,
            LevelDesc = 1,
            RarityAsc = 2,
            RarityDesc = 3
        }

        private sealed class CandidateInfo
        {
            public string CharacterId;
            public string Name;
            public string Role;
            public string Rarity;
            public string PortraitPath;
            public int Level;
            public int CurrentHp;
            public int RarityRank;
            public CharacterDataModel Data;
        }

        private readonly string[] _slotCharacterIds = new string[3];
        private readonly List<TeamFormationPickerCellView> _spawnedPickerCells = new List<TeamFormationPickerCellView>();
        private readonly List<TeamFormationSkillQuickItemView> _spawnedSkillQuickItems = new List<TeamFormationSkillQuickItemView>();
        private ITeamService _teamService;
        private SaveData _saveData;
        private DataManager _dataManager;

        private int _activeSlotIndex = -1;
        private string _highlightedPickerCharacterId;
        private SortMode _sortMode = SortMode.LevelDesc;
        private string _roleFilter = "All";
        private List<CandidateInfo> _lastCandidates = new List<CandidateInfo>();

        private void Start()
        {
            _teamService = MetaServiceHub.Instance?.TeamService;
            if (_teamService == null)
            {
                Debug.LogWarning("[TeamFormation] TeamService not found, save will be skipped.");
            }

            if (_slotButtons != null)
            {
                for (var i = 0; i < _slotButtons.Length; i++)
                {
                    var idx = i;
                    if (_slotButtons[i] != null)
                    {
                        _slotButtons[i].onClick.AddListener(() => OnSlotClicked(idx));
                    }
                }
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_pickerConfirmButton != null)
            {
                _pickerConfirmButton.onClick.AddListener(OnPickerConfirmClicked);
            }

            if (_pickerCloseButton != null)
            {
                _pickerCloseButton.onClick.AddListener(ClosePicker);
            }

            if (_sortDropdown != null)
            {
                _sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);
            }

            if (_roleDropdown != null)
            {
                _roleDropdown.onValueChanged.AddListener(OnRoleDropdownChanged);
            }

            InitializeDropdownOptions();

            ResolveSaveData();
            ResolveDataManager();
            LoadInitialLineup();
            ShowTeamPanel();
            RefreshSlotViews();
            RefreshSlotHighlights();
        }

        public void OnSlotClicked(int slotIndex)
        {
               Debug.Log($"[TeamFormation] OnSlotClicked called with slotIndex={slotIndex}");
           
            if (slotIndex < 0 || slotIndex >= _slotCharacterIds.Length)
            {
                   Debug.LogWarning($"[TeamFormation] Invalid slot index {slotIndex}");
                return;
            }

            _activeSlotIndex = slotIndex;
            _highlightedPickerCharacterId = _slotCharacterIds[_activeSlotIndex];
               Debug.Log($"[TeamFormation] Active slot: {slotIndex}, highlighted character: {_highlightedPickerCharacterId}");
           
            ShowPickerPanel();
            RefreshSlotHighlights();
            RebuildPickerList();
        }

        public void SetSortByLevelAsc()
        {
            _sortMode = SortMode.LevelAsc;
            RebuildPickerList();
        }

        public void SetSortByLevelDesc()
        {
            _sortMode = SortMode.LevelDesc;
            RebuildPickerList();
        }

        public void SetSortByRarityAsc()
        {
            _sortMode = SortMode.RarityAsc;
            RebuildPickerList();
        }

        public void SetSortByRarityDesc()
        {
            _sortMode = SortMode.RarityDesc;
            RebuildPickerList();
        }

        public void SetRoleFilterAll()
        {
            _roleFilter = "All";
            RebuildPickerList();
        }

        public void SetRoleFilter(string roleTag)
        {
            _roleFilter = string.IsNullOrWhiteSpace(roleTag) ? "All" : roleTag;
            RebuildPickerList();
        }

        public void ClosePicker()
        {
            _highlightedPickerCharacterId = null;
            ShowTeamPanel();
            RefreshSlotHighlights();
        }

        private void OnPickerCharacterClicked(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId) || _activeSlotIndex < 0)
            {
                return;
            }

            // Toggle highlight only. Team changes happen only after confirm.
            if (string.Equals(_highlightedPickerCharacterId, characterId, StringComparison.Ordinal))
            {
                _highlightedPickerCharacterId = null;
                RefreshPickerCellHighlights();
                UpdateQuickInfo(null);
                return;
            }

            _highlightedPickerCharacterId = characterId;
            RefreshPickerCellHighlights();
            UpdateQuickInfo(FindCandidateByCharacterId(characterId));
        }

        private void OnPickerConfirmClicked()
        {
            if (_activeSlotIndex < 0 || _activeSlotIndex >= _slotCharacterIds.Length)
            {
                return;
            }

            // Confirm with no highlighted candidate means unequip active slot.
            if (string.IsNullOrWhiteSpace(_highlightedPickerCharacterId))
            {
                if (!string.IsNullOrWhiteSpace(_slotCharacterIds[_activeSlotIndex]))
                {
                    _slotCharacterIds[_activeSlotIndex] = string.Empty;
                    PersistLineup();
                    RefreshSlotViews();
                }

                ClosePicker();
                return;
            }

            for (var i = 0; i < _slotCharacterIds.Length; i++)
            {
                if (i != _activeSlotIndex && string.Equals(_slotCharacterIds[i], _highlightedPickerCharacterId, StringComparison.Ordinal))
                {
                    _slotCharacterIds[i] = string.Empty;
                }
            }

            _slotCharacterIds[_activeSlotIndex] = _highlightedPickerCharacterId;
            PersistLineup();
            RefreshSlotViews();
            ClosePicker();
        }

        private void OnBackClicked()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private void OnSortDropdownChanged(int index)
        {
            _sortMode = index switch
            {
                0 => SortMode.LevelAsc,
                1 => SortMode.LevelDesc,
                2 => SortMode.RarityAsc,
                3 => SortMode.RarityDesc,
                _ => SortMode.LevelDesc
            };

            RebuildPickerList();
        }

        private void OnRoleDropdownChanged(int index)
        {
            if (_roleDropdown == null || _roleDropdown.options == null || index < 0 || index >= _roleDropdown.options.Count)
            {
                return;
            }

            _roleFilter = _roleDropdown.options[index].text;
            RebuildPickerList();
        }

        private void InitializeDropdownOptions()
        {
            if (_sortDropdown != null)
            {
                _sortDropdown.ClearOptions();
                _sortDropdown.AddOptions(new List<string>
                {
                    "Level Asc",
                    "Level Desc",
                    "Rarity Asc",
                    "Rarity Desc"
                });
                _sortDropdown.SetValueWithoutNotify(1);
                _sortMode = SortMode.LevelDesc;
            }

            if (_roleDropdown != null)
            {
                var roleOptions = BuildRoleFilterOptions();
                _roleDropdown.ClearOptions();
                _roleDropdown.AddOptions(roleOptions);
                _roleDropdown.SetValueWithoutNotify(0);
                _roleFilter = "All";
            }
        }

        private List<string> BuildRoleFilterOptions()
        {
            var options = new List<string> { "All" };
            var roleSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var allCharacters = ResolveDataManager()?.GetAllCharacters();
            if (allCharacters != null)
            {
                foreach (var character in allCharacters)
                {
                    var role = character?.metadata?.roleTag;
                    if (string.IsNullOrWhiteSpace(role))
                    {
                        continue;
                    }

                    if (roleSet.Add(role))
                    {
                        options.Add(role);
                    }
                }
            }

            return options;
        }

        private void LoadInitialLineup()
        {
            var existing = _teamService?.GetCurrentLineup();
            if (existing == null || existing.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _slotCharacterIds.Length && i < existing.Count; i++)
            {
                _slotCharacterIds[i] = existing[i];
            }
        }

        private void PersistLineup()
        {
            if (_teamService == null)
            {
                return;
            }

            var lineup = _slotCharacterIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (lineup.Count == 0)
            {
                return;
            }

            var result = _teamService.ValidateLineup(lineup);
            if (!result.IsValid)
            {
                Debug.LogWarning($"[TeamFormation] Persist lineup rejected: {result.Message}");
                return;
            }

            _teamService.SaveLineup(lineup);
            Debug.Log($"[TeamFormation] Saved lineup: {string.Join(",", lineup)}");
        }

        private void RefreshSlotViews()
        {
            for (var i = 0; i < _slotViews.Length && i < _slotCharacterIds.Length; i++)
            {
                BindSlotView(_slotViews[i], _slotCharacterIds[i], i);
            }
        }

        private void RefreshSlotHighlights()
        {
            for (var i = 0; i < _slotViews.Length; i++)
            {
                if (_slotViews[i] != null && _slotViews[i].selectedHighlight != null)
                {
                    _slotViews[i].selectedHighlight.SetActive(_activeSlotIndex == i && _pickerPanel != null && _pickerPanel.activeSelf);
                }
            }
        }

        private void BindSlotView(SlotView slotView, string characterId, int slotIndex)
        {
            if (slotView == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(characterId))
            {
                if (slotView.nameText != null)
                {
                    slotView.nameText.text = $"Slot {slotIndex + 1}: Empty";
                }

                if (slotView.hpText != null)
                {
                    slotView.hpText.text = "HP: -";
                }

                if (slotView.hpSlider != null)
                {
                    slotView.hpSlider.minValue = 0f;
                    slotView.hpSlider.maxValue = 1f;
                    slotView.hpSlider.value = 0f;
                }

                if (slotView.levelText != null)
                {
                    slotView.levelText.text = "Lv: -";
                }

                if (slotView.portrait != null)
                {
                    slotView.portrait.sprite = null;
                    slotView.portrait.color = new Color(1f, 1f, 1f, 0f);
                }

                return;
            }

            var data = ResolveDataManager()?.LoadCharacter(characterId);
            var displayName = data?.nameKey ?? characterId;
            var baseHp = Math.Max(1, data?.baseStats?.hp ?? 1000);
            var save = ResolveSaveData();
            var hp = save?.GetCharacterCurrentHp(characterId, baseHp) ?? baseHp;
            var level = save?.GetCharacterLevel(characterId) ?? Math.Max(1, data?.baseStats?.level ?? 1);

            if (slotView.nameText != null)
            {
                slotView.nameText.text = displayName;
            }

            if (slotView.hpText != null)
            {
                slotView.hpText.text = $"HP: {hp}";
            }

            if (slotView.hpSlider != null)
            {
                slotView.hpSlider.minValue = 0f;
                slotView.hpSlider.maxValue = baseHp;
                slotView.hpSlider.value = Mathf.Clamp(hp, 0, baseHp);
            }

            if (slotView.levelText != null)
            {
                slotView.levelText.text = $"Lv: {level}";
            }

            if (slotView.portrait != null)
            {
                var portraitPath = data?.visual?.portraitPath;
                var portrait = ResolveDataManager()?.LoadCharacterPortraitSprite(portraitPath);

                slotView.portrait.sprite = portrait;
                slotView.portrait.color = portrait == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
        }

        private void RebuildPickerList()
        {
            Debug.Log("[TeamFormation] RebuildPickerList called");
            
            if (_pickerPanel == null || !_pickerPanel.activeSelf)
            {
                Debug.LogWarning("[TeamFormation] Picker panel is null or not active");
                return;
            }

            ClearPickerList();
            var candidates = BuildCandidates();
            _lastCandidates = candidates;

            Debug.Log($"[TeamFormation] Found {candidates.Count} candidates");

            if (_pickerItemPrefab == null || _pickerListRoot == null)
            {
                Debug.LogWarning($"[TeamFormation] Picker list root or item prefab is missing. Root={_pickerListRoot}, Prefab={_pickerItemPrefab}");
                return;
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[TeamFormation] No candidates found to display");
                return;
            }

            foreach (var candidate in candidates)
            {
                var instance = Instantiate(_pickerItemPrefab, _pickerListRoot);
                _spawnedPickerCells.Add(instance);

                var portrait = LoadPortrait(candidate.PortraitPath);
                instance.Bind(candidate.Name, candidate.Level, candidate.Rarity, portrait);

                var pickedId = candidate.CharacterId;
                if (instance.Button != null)
                {
                    instance.Button.onClick.RemoveAllListeners();
                    instance.Button.onClick.AddListener(() => OnPickerCharacterClicked(pickedId));
                }
            }

            if (string.IsNullOrWhiteSpace(_highlightedPickerCharacterId))
            {
                var currentInSlot = _activeSlotIndex >= 0 && _activeSlotIndex < _slotCharacterIds.Length
                    ? _slotCharacterIds[_activeSlotIndex]
                    : string.Empty;

                if (!string.IsNullOrWhiteSpace(currentInSlot) && candidates.Any(c => c.CharacterId == currentInSlot))
                {
                    _highlightedPickerCharacterId = currentInSlot;
                }
                else if (candidates.Count > 0)
                {
                    _highlightedPickerCharacterId = candidates[0].CharacterId;
                }
            }

            RefreshPickerCellHighlights();
            UpdateQuickInfo(FindCandidateByCharacterId(_highlightedPickerCharacterId));
        }

        private List<CandidateInfo> BuildCandidates()
        {
            var save = ResolveSaveData();
            if (save == null)
            {
                Debug.LogError("[TeamFormation] SaveData is null");
                return new List<CandidateInfo>();
            }

            Debug.Log($"[TeamFormation] Unlocked characters count: {save.unlockedCharacters.Count}");

            var selectedInOtherSlots = new HashSet<string>();
            for (var i = 0; i < _slotCharacterIds.Length; i++)
            {
                if (i == _activeSlotIndex)
                {
                    continue;
                }

                var id = _slotCharacterIds[i];
                if (!string.IsNullOrWhiteSpace(id))
                {
                    selectedInOtherSlots.Add(id);
                }
            }

            var result = new List<CandidateInfo>();
            for (var i = 0; i < save.unlockedCharacters.Count; i++)
            {
                var characterId = save.unlockedCharacters[i];
                if (string.IsNullOrWhiteSpace(characterId))
                {
                    Debug.LogWarning($"[TeamFormation] Character ID at index {i} is empty");
                    continue;
                }

                if (selectedInOtherSlots.Contains(characterId))
                {
                    Debug.Log($"[TeamFormation] {characterId} already in another slot, skipping");
                    continue;
                }

                var data = ResolveDataManager()?.LoadCharacter(characterId);
                if (data == null)
                {
                    Debug.LogWarning($"[TeamFormation] Failed to load character data for {characterId}");
                    continue;
                }

                var baseHp = Math.Max(1, data.baseStats?.hp ?? 1000);
                var hp = save.GetCharacterCurrentHp(characterId, baseHp);
                Debug.Log($"[TeamFormation] {characterId}: HP={hp}, baseHP={baseHp}");
                
                if (hp <= 0)
                {
                    Debug.Log($"[TeamFormation] {characterId} is dead (HP <= 0), skipping");
                    continue;
                }

                var role = data.metadata?.roleTag ?? "Unknown";
                if (!IsRoleAllowed(role))
                {
                    Debug.Log($"[TeamFormation] {characterId} role {role} not allowed by filter {_roleFilter}");
                    continue;
                }

                var rarity = data.metadata?.rarity ?? "R";
                result.Add(new CandidateInfo
                {
                    CharacterId = characterId,
                    Name = string.IsNullOrWhiteSpace(data.nameKey) ? characterId : data.nameKey,
                    Role = role,
                    Rarity = rarity,
                    PortraitPath = data.visual?.portraitPath,
                    Level = save.GetCharacterLevel(characterId),
                    CurrentHp = hp,
                    RarityRank = GetRarityRank(rarity),
                    Data = data
                });
            }

            var sorted = _sortMode switch
            {
                SortMode.LevelAsc => result.OrderBy(c => c.Level).ThenBy(c => c.Name).ToList(),
                SortMode.LevelDesc => result.OrderByDescending(c => c.Level).ThenBy(c => c.Name).ToList(),
                SortMode.RarityAsc => result.OrderBy(c => c.RarityRank).ThenBy(c => c.Name).ToList(),
                SortMode.RarityDesc => result.OrderByDescending(c => c.RarityRank).ThenBy(c => c.Name).ToList(),
                _ => result
            };

            var currentSlotCharacterId = _activeSlotIndex >= 0 && _activeSlotIndex < _slotCharacterIds.Length
                ? _slotCharacterIds[_activeSlotIndex]
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(currentSlotCharacterId))
            {
                var current = sorted.FirstOrDefault(c => c.CharacterId == currentSlotCharacterId);
                if (current != null)
                {
                    sorted.Remove(current);
                    sorted.Insert(0, current);
                }
            }

            return sorted;
        }

        private bool IsRoleAllowed(string roleTag)
        {
            if (string.IsNullOrWhiteSpace(_roleFilter) || string.Equals(_roleFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(roleTag, _roleFilter, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetRarityRank(string rarity)
        {
            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase)) return 5;
            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase)) return 4;
            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase)) return 3;
            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase)) return 2;
            if (string.Equals(rarity, "N", StringComparison.OrdinalIgnoreCase)) return 1;
            return 0;
        }

        private void ClearPickerList()
        {
            for (var i = 0; i < _spawnedPickerCells.Count; i++)
            {
                if (_spawnedPickerCells[i] != null)
                {
                    Destroy(_spawnedPickerCells[i].gameObject);
                }
            }

            _spawnedPickerCells.Clear();
        }

        private void RefreshPickerCellHighlights()
        {
            for (var i = 0; i < _spawnedPickerCells.Count; i++)
            {
                if (_spawnedPickerCells[i] == null || i >= _lastCandidates.Count)
                {
                    continue;
                }

                _spawnedPickerCells[i].SetSelected(string.Equals(_lastCandidates[i].CharacterId, _highlightedPickerCharacterId, StringComparison.Ordinal));
            }
        }

        private CandidateInfo FindCandidateByCharacterId(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (var i = 0; i < _lastCandidates.Count; i++)
            {
                if (string.Equals(_lastCandidates[i].CharacterId, characterId, StringComparison.Ordinal))
                {
                    return _lastCandidates[i];
                }
            }

            return null;
        }

        private void UpdateQuickInfo(CandidateInfo candidate)
        {
            if (candidate == null)
            {
                if (_quickInfoNameText != null) _quickInfoNameText.text = "-";
                if (_quickInfoRoleText != null) _quickInfoRoleText.text = "Role: -";
                if (_quickInfoRarityText != null)
                {
                    _quickInfoRarityText.text = "-";
                    ResetQuickInfoRarityStyle();
                }
                if (_quickInfoElementText != null) _quickInfoElementText.text = "Element: -";
                if (_quickInfoPortrait != null)
                {
                    _quickInfoPortrait.sprite = null;
                    _quickInfoPortrait.color = new Color(1f, 1f, 1f, 0f);
                }

                // [SUSPENDED DAY 5] ClearQuickSkillItems();
                return;
            }

            if (_quickInfoNameText != null)
            {
                _quickInfoNameText.text = candidate.Name;
            }

            if (_quickInfoRoleText != null)
            {
                _quickInfoRoleText.text = $"Role: {candidate.Role}";
            }

            if (_quickInfoRarityText != null)
            {
                _quickInfoRarityText.text = candidate.Rarity;
                ApplyQuickInfoRarityStyle(candidate.Rarity);
            }

            if (_quickInfoElementText != null)
            {
                _quickInfoElementText.text = $"Element: {candidate.Data?.metadata?.element ?? "Unknown"}";
            }

            if (_quickInfoPortrait != null)
            {
                var sprite = LoadPortrait(candidate.PortraitPath);
                _quickInfoPortrait.sprite = sprite;
                _quickInfoPortrait.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            // [SUSPENDED DAY 5] RebuildQuickSkillItems(candidate.Data);
        }

        private void RebuildQuickSkillItems(CharacterDataModel data)
        {
            ClearQuickSkillItems();
            if (_quickInfoSkillRoot == null || _quickInfoSkillItemPrefab == null || data?.skills == null)
            {
                return;
            }

            for (var i = 0; i < data.skills.Count; i++)
            {
                var skillId = data.skills[i];
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                var dataManager = ResolveDataManager();
                var skill = dataManager?.LoadSkill(skillId);
                var iconPath = dataManager?.ResolveSkillIcon(skillId);
                var icon = string.IsNullOrWhiteSpace(iconPath) ? null : Resources.Load<Sprite>(iconPath);
                var name = skill?.nameKey ?? skillId;

                var item = Instantiate(_quickInfoSkillItemPrefab, _quickInfoSkillRoot);
                item.Bind(icon, name);
                _spawnedSkillQuickItems.Add(item);
            }
        }

        private void ClearQuickSkillItems()
        {
            for (var i = 0; i < _spawnedSkillQuickItems.Count; i++)
            {
                if (_spawnedSkillQuickItems[i] != null)
                {
                    Destroy(_spawnedSkillQuickItems[i].gameObject);
                }
            }

            _spawnedSkillQuickItems.Clear();
        }

        private void ApplyQuickInfoRarityStyle(string rarity)
        {
            if (_quickInfoRarityText == null)
            {
                return;
            }

            _quickInfoRarityText.enableVertexGradient = false;
            _quickInfoRarityText.colorGradient = default;

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                var topColor = HexToColor("#FFFFB3FF");
                var bottomColor = HexToColor("#FFB300FF");
                _quickInfoRarityText.color = HexToColor("#FFD700");
                _quickInfoRarityText.enableVertexGradient = true;
                _quickInfoRarityText.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
                return;
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                _quickInfoRarityText.color = HexToColor("#FF007F");
                return;
            }

            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
            {
                _quickInfoRarityText.color = HexToColor("#00F0FF");
                return;
            }

            _quickInfoRarityText.color = Color.white;
        }

        private void ResetQuickInfoRarityStyle()
        {
            if (_quickInfoRarityText == null)
            {
                return;
            }

            _quickInfoRarityText.enableVertexGradient = false;
            _quickInfoRarityText.colorGradient = default;
            _quickInfoRarityText.color = Color.white;
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }

            return Color.white;
        }

        private Sprite LoadPortrait(string portraitPath)
        {
            if (string.IsNullOrWhiteSpace(portraitPath))
            {
                return null;
            }

            return ResolveDataManager()?.LoadCharacterPortraitSprite(portraitPath);
        }

        private void ShowTeamPanel()
        {
            if (_teamPanel != null)
            {
                _teamPanel.SetActive(true);
            }

            if (_pickerPanel != null)
            {
                _pickerPanel.SetActive(false);
            }

            _activeSlotIndex = -1;
        }

        private void ShowPickerPanel()
        {
            if (_teamPanel != null)
            {
                _teamPanel.SetActive(false);
            }

            if (_pickerPanel != null)
            {
                _pickerPanel.SetActive(true);
            }
        }

        private SaveData ResolveSaveData()
        {
            if (_saveData != null)
            {
                return _saveData;
            }

            var saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager != null)
            {
                _saveData = TryResolveExistingSave(saveManager) ?? saveManager.EnsureCurrentSave(0);
            }

            if (_saveData == null)
            {
                var fallbackSaveManager = SaveManager.Instance;
                if (fallbackSaveManager != null)
                {
                    _saveData = TryResolveExistingSave(fallbackSaveManager) ?? fallbackSaveManager.EnsureCurrentSave(0);
                }
            }

            return _saveData;
        }

        private DataManager ResolveDataManager()
        {
            if (_dataManager != null)
            {
                return _dataManager;
            }

            _dataManager = FindFirstObjectByType<DataManager>();
            if (_dataManager != null)
            {
                return _dataManager;
            }

            var go = new GameObject("[DataManager]");
            _dataManager = go.AddComponent<DataManager>();
            return _dataManager;
        }

        private static SaveData TryResolveExistingSave(SaveManager saveManager)
        {
            if (saveManager == null)
            {
                return null;
            }

            if (saveManager.CurrentSave != null)
            {
                return saveManager.CurrentSave;
            }

            var slots = saveManager.GetAllSlots();
            if (slots == null)
            {
                return null;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].hasData)
                {
                    var loaded = saveManager.Load(slots[i].slotIndex);
                    if (loaded != null)
                    {
                        return loaded;
                    }
                }
            }

            return null;
        }
    }
}
