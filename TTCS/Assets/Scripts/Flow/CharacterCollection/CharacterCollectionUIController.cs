using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTCS.Core.Data;
using TTCS.Core.Progression;
using TTCS.Core.Save;
using TTCS.Data;
using TTCS.Flow.TeamFormation;
using TTCS.Meta;
using TTCS.Meta.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.CharacterCollection
{
    public class CharacterCollectionUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _listPanel;
        [SerializeField] private GameObject _detailPanel;

        [Header("List")]
        [SerializeField] private Transform _listRoot;
        [SerializeField] private TeamFormationPickerCellView _characterSlotPrefab;
        [SerializeField] private TMP_Text _listFeedbackText;

        [Header("List - Search")]
        [SerializeField] private TMP_InputField _searchInput;

        [Header("List - Filters")]
        [SerializeField] private TMP_Dropdown _rarityDropdown;
        [SerializeField] private TMP_Dropdown _roleDropdown;
        [SerializeField] private TMP_Dropdown _elementDropdown;

        [Header("List - Sort")]
        [SerializeField] private TMP_Dropdown _sortDropdown;

        [Header("List - Actions")]
        [SerializeField] private Button _backToMenuButton;

        [Header("Detail - Actions")]
        [SerializeField] private Button _backToListButton;

        [Header("Detail - Left")]
        [SerializeField] private Image _portraitImage;

        [Header("Detail - Right")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Slider _levelProgressSlider;
        [SerializeField] private TMP_Text _levelProgressText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private Image _roleIconImage;
        [SerializeField] private Image _elementIconImage;

        [Header("Detail - Stats")]
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private TMP_Text _atkText;
        [SerializeField] private TMP_Text _defText;
        [SerializeField] private TMP_Text _spdText;
        [SerializeField] private TMP_Text _critText;


        [Header("Detail - Skills")]
        [SerializeField] private Transform _skillRoot;
        [SerializeField] private TeamFormationSkillQuickItemView _skillItemPrefab;
        [SerializeField] private SkillDetailPanelView _skillDetailPanel;

        [Header("Detail - Equipment")]
        [SerializeField] private Button _equipAccessoryButton;
        [SerializeField] private Image _equippedAccessoryIcon;
        [SerializeField] private Sprite _emptyAccessoryIconSprite;

        [Header("Detail - Equipment - Actions")]
        [SerializeField] private GameObject _accessoryActionPanel;
        [SerializeField] private Button _accessoryActionEquipButton;
        [SerializeField] private Button _accessoryActionUnequipButton;

        [SerializeField] private AccessoryEquipPickerPanel _accessoryPicker;

        private readonly List<TeamFormationPickerCellView> _spawnedSlots = new List<TeamFormationPickerCellView>();
        private readonly List<string> _spawnedSlotCharacterIds = new List<string>();
        private readonly List<TeamFormationSkillQuickItemView> _spawnedSkillItems = new List<TeamFormationSkillQuickItemView>();
        private readonly List<CharacterEntry> _characters = new List<CharacterEntry>();

        private SaveManager _saveManager;
        private SaveData _save;
        private IInventoryService _inventoryService;

        private string _selectedCharacterId;

        private string _keyword = string.Empty;
        private string _rarityFilter = "All";
        private string _roleFilter = "All";
        private string _elementFilter = "All";

        private SortMode _sortMode = SortMode.LevelDesc;

        private enum SortMode
        {
            LevelDesc = 0,
            LevelAsc = 1
        }

        private void Start()
        {
            _saveManager = SaveManager.Instance;
            _saveManager?.EnsureCurrentSave(0);
            _save = _saveManager?.CurrentSave;

            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();
            _inventoryService = hub?.InventoryService;

            WireUiEvents();
            ShowListPanel();

            SetAccessoryActionPanelVisible(false);
        }

        public void BackToMenu()
        {
            FlowController.Instance.OpenMainMenu();
        }

        public void BackToList()
        {
            ShowListPanel();
        }

        private void WireUiEvents()
        {
            if (_backToMenuButton != null)
            {
                _backToMenuButton.onClick.AddListener(BackToMenu);
            }

            if (_backToListButton != null)
            {
                _backToListButton.onClick.AddListener(BackToList);
            }

            if (_searchInput != null)
            {
                _searchInput.onValueChanged.AddListener(OnSearchChanged);
            }

            if (_rarityDropdown != null)
            {
                _rarityDropdown.onValueChanged.AddListener(_ => OnFilterChanged());
            }

            if (_roleDropdown != null)
            {
                _roleDropdown.onValueChanged.AddListener(_ => OnFilterChanged());
            }

            if (_elementDropdown != null)
            {
                _elementDropdown.onValueChanged.AddListener(_ => OnFilterChanged());
            }

            if (_sortDropdown != null)
            {
                _sortDropdown.onValueChanged.AddListener(_ => OnFilterChanged());
            }

            if (_equipAccessoryButton != null)
            {
                _equipAccessoryButton.onClick.AddListener(OnAccessoryActionRequested);
            }

            if (_accessoryActionEquipButton != null)
            {
                _accessoryActionEquipButton.onClick.AddListener(OnAccessoryEquipOptionSelected);
            }

            if (_accessoryActionUnequipButton != null)
            {
                _accessoryActionUnequipButton.onClick.AddListener(OnAccessoryUnequipOptionSelected);
            }
        }

        private void RefreshRosterAndFilters()
        {
            _save = _saveManager?.CurrentSave;
            _characters.Clear();

            if (_save == null)
            {
                SetListFeedback("Save is not available.");
                RebuildCharacterList();
                return;
            }

            for (var i = 0; i < _save.unlockedCharacters.Count; i++)
            {
                var id = _save.unlockedCharacters[i];
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                var data = DataManager.Instance?.LoadCharacter(id);
                if (data == null)
                {
                    continue;
                }

                var level = Math.Max(1, _save.GetCharacterLevel(id));
                var exp = _save.GetCharacterExp(id);

                var computed = ComputeCharacterStats(data, level, id);
                var baseMaxMana = ComputeMaxMana(data, level);
                var maxMana = Mathf.Max(0, baseMaxMana + GetAccessoryFlatBonus(id, "MANA"));
                var currentHp = _save.GetCharacterCurrentHp(id, computed.MaxHp);
                var currentMana = _save.GetCharacterCurrentMana(id, maxMana);

                _characters.Add(new CharacterEntry
                {
                    CharacterId = id,
                    Data = data,
                    Level = level,
                    Exp = Math.Max(0, exp),
                    CurrentHp = Mathf.Clamp(currentHp, 0, computed.MaxHp),
                    MaxHp = computed.MaxHp,
                    CurrentMana = Mathf.Clamp(currentMana, 0, maxMana),
                    MaxMana = maxMana
                });
            }

            InitializeFilterDropdownOptions(preserveSelection: true);

            if (_characters.Count > 0 && string.IsNullOrWhiteSpace(_selectedCharacterId))
            {
                _selectedCharacterId = _characters[0].CharacterId;
            }

            SetListFeedback(_characters.Count == 0 ? "No unlocked characters." : string.Empty);
            RebuildCharacterList();
        }

        private void InitializeFilterDropdownOptions(bool preserveSelection)
        {
            InitializeDropdown(_rarityDropdown, BuildRarityOptions(), preserveSelection);
            InitializeDropdown(_roleDropdown, BuildDistinctOptions(_characters.Select(c => c.Data?.metadata?.roleTag), "All"), preserveSelection);
            InitializeDropdown(_elementDropdown, BuildDistinctOptions(_characters.Select(c => c.Data?.metadata?.element), "All"), preserveSelection);
            InitializeDropdown(_sortDropdown, BuildSortOptions(), preserveSelection);
        }

        private static void InitializeDropdown(TMP_Dropdown dropdown, List<string> options, bool preserveSelection)
        {
            if (dropdown == null)
            {
                return;
            }

            var previous = preserveSelection ? ReadDropdownValue(dropdown, "All") : "All";

            dropdown.ClearOptions();
            if (options == null || options.Count == 0)
            {
                dropdown.AddOptions(new List<string> { "All" });
            }
            else
            {
                dropdown.AddOptions(options);
            }

            var nextIndex = 0;
            if (preserveSelection && dropdown.options != null)
            {
                for (var i = 0; i < dropdown.options.Count; i++)
                {
                    if (string.Equals(dropdown.options[i].text, previous, StringComparison.OrdinalIgnoreCase))
                    {
                        nextIndex = i;
                        break;
                    }
                }
            }

            dropdown.value = nextIndex;
            dropdown.RefreshShownValue();
        }

        private static List<string> BuildRarityOptions()
        {
            return new List<string> { "All", "UR", "SSR", "SR", "R" };
        }

        private static List<string> BuildSortOptions()
        {
            return new List<string> { "Level DESC", "Level ASC" };
        }

        private static List<string> BuildDistinctOptions(IEnumerable<string> values, string allLabel)
        {
            var options = new List<string> { allLabel };
            if (values == null)
            {
                return options;
            }

            var distinct = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            options.AddRange(distinct);
            return options;
        }

        private void OnSearchChanged(string keyword)
        {
            _keyword = keyword ?? string.Empty;
            RebuildCharacterList();
        }

        private void OnFilterChanged()
        {
            _rarityFilter = ReadDropdownValue(_rarityDropdown, "All");
            _roleFilter = ReadDropdownValue(_roleDropdown, "All");
            _elementFilter = ReadDropdownValue(_elementDropdown, "All");

            _sortMode = ReadSortMode(_sortDropdown);

            RebuildCharacterList();
        }

        private static SortMode ReadSortMode(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return SortMode.LevelDesc;
            }

            var idx = dropdown.value;
            return idx == 1 ? SortMode.LevelAsc : SortMode.LevelDesc;
        }

        private void RebuildCharacterList()
        {
            ClearSpawnedSlots();

            if (_listRoot == null || _characterSlotPrefab == null)
            {
                return;
            }

            var filtered = ApplyFilters(_characters);
            for (var i = 0; i < filtered.Count; i++)
            {
                var entry = filtered[i];
                var instance = Instantiate(_characterSlotPrefab, _listRoot);
                _spawnedSlots.Add(instance);
                _spawnedSlotCharacterIds.Add(entry.CharacterId);

                var portrait = DataManager.Instance?.LoadCharacterPortraitSprite(entry.Data.visual?.portraitPath ?? entry.Data.visual?.spritePath);
                var rarity = entry.Data.metadata?.rarity ?? "R";
                instance.Bind(entry.Data.nameKey ?? entry.CharacterId, entry.Level, rarity, portrait);

                if (instance.Button != null)
                {
                    var id = entry.CharacterId;
                    instance.Button.onClick.RemoveAllListeners();
                    instance.Button.onClick.AddListener(() => OnCharacterSlotClicked(id));
                }

                instance.SetSelected(string.Equals(entry.CharacterId, _selectedCharacterId, StringComparison.Ordinal));
            }
        }

        private List<CharacterEntry> ApplyFilters(List<CharacterEntry> source)
        {
            if (source == null)
            {
                return new List<CharacterEntry>();
            }

            var query = source.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(_keyword))
            {
                var lowered = _keyword.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    (c.Data?.nameKey ?? string.Empty).ToLowerInvariant().Contains(lowered) ||
                    (c.CharacterId ?? string.Empty).ToLowerInvariant().Contains(lowered));
            }

            if (!string.IsNullOrWhiteSpace(_rarityFilter) && !string.Equals(_rarityFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => string.Equals(c.Data?.metadata?.rarity, _rarityFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_roleFilter) && !string.Equals(_roleFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => string.Equals(c.Data?.metadata?.roleTag, _roleFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_elementFilter) && !string.Equals(_elementFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => string.Equals(c.Data?.metadata?.element, _elementFilter, StringComparison.OrdinalIgnoreCase));
            }

            IOrderedEnumerable<CharacterEntry> ordered = _sortMode == SortMode.LevelAsc
                ? query.OrderBy(c => c.Level)
                : query.OrderByDescending(c => c.Level);

            return ordered
                .ThenBy(c => c.Data?.nameKey ?? c.CharacterId)
                .ToList();
        }

        private void OnCharacterSlotClicked(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            _selectedCharacterId = characterId;
            UpdateListSelectionHighlight();
            ShowDetailPanel(characterId);
        }

        private void UpdateListSelectionHighlight()
        {
            for (var i = 0; i < _spawnedSlots.Count; i++)
            {
                var slot = _spawnedSlots[i];
                if (slot == null)
                {
                    continue;
                }

                var id = i >= 0 && i < _spawnedSlotCharacterIds.Count ? _spawnedSlotCharacterIds[i] : string.Empty;
                slot.SetSelected(string.Equals(id, _selectedCharacterId, StringComparison.Ordinal));
            }
        }

        private void ShowListPanel()
        {
            if (_listPanel != null)
            {
                _listPanel.SetActive(true);
            }

            if (_detailPanel != null)
            {
                _detailPanel.SetActive(false);
            }

            if (_skillDetailPanel != null)
            {
                _skillDetailPanel.Hide();
            }

            SetAccessoryActionPanelVisible(false);

            RefreshRosterAndFilters();
        }

        private void ShowDetailPanel(string characterId)
        {
            if (_listPanel != null)
            {
                _listPanel.SetActive(false);
            }

            if (_detailPanel != null)
            {
                _detailPanel.SetActive(true);
            }

            SetAccessoryActionPanelVisible(false);

            RefreshDetail(characterId);
        }

        private void RefreshDetail(string characterId)
        {
            var entry = _characters.FirstOrDefault(c => string.Equals(c.CharacterId, characterId, StringComparison.Ordinal));
            if (entry == null || entry.Data == null)
            {
                ClearDetail();
                return;
            }

            var rarity = entry.Data.metadata?.rarity ?? "R";
            var role = entry.Data.metadata?.roleTag ?? string.Empty;
            var element = entry.Data.metadata?.element ?? string.Empty;

            if (_nameText != null)
            {
                _nameText.text = entry.Data.nameKey ?? entry.CharacterId;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{entry.Level}";
            }

            if (_rarityText != null)
            {
                _rarityText.text = rarity;
                ApplyRarityTextStyle(_rarityText, rarity);
            }

            SetOptionalIcon(_roleIconImage, LoadRoleIconSprite(role));
            SetOptionalIcon(_elementIconImage, LoadElementIconSprite(element));

            if (_portraitImage != null)
            {
                var portrait = DataManager.Instance?.LoadCharacterPortraitSprite(entry.Data.visual?.portraitPath ?? entry.Data.visual?.spritePath);
                _portraitImage.sprite = portrait;
                _portraitImage.color = portrait == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _portraitImage.preserveAspect = true;
            }

            var stats = ComputeCharacterStats(entry.Data, entry.Level, entry.CharacterId);
            entry.MaxHp = stats.MaxHp;
            entry.CurrentHp = Mathf.Clamp(entry.CurrentHp, 0, stats.MaxHp);

            var baseMaxMana = ComputeMaxMana(entry.Data, entry.Level);
            entry.MaxMana = Mathf.Max(0, baseMaxMana + GetAccessoryFlatBonus(entry.CharacterId, "MANA"));
            entry.CurrentMana = Mathf.Clamp(entry.CurrentMana, 0, entry.MaxMana);

            if (_hpText != null)
            {
                _hpText.text = $"HP:{entry.CurrentHp}/{stats.MaxHp}";
            }

            if (_manaText != null)
            {
                _manaText.text = $"Mana:{entry.CurrentMana}/{entry.MaxMana}";
            }

            if (_atkText != null)
            {
                _atkText.text = $"ATK:{stats.ATK}";
            }

            if (_defText != null)
            {
                _defText.text = $"DEF:{stats.DEF}";
            }

            if (_spdText != null)
            {
                _spdText.text = $"SPD:{stats.SPD}";
            }

            if (_critText != null)
            {
                _critText.text = $"Crit:{Mathf.RoundToInt(stats.CritRate * 100f)}%";
            }

            

            

            RefreshLevelProgress(entry);
            RebuildSkillPanel(entry);
            RefreshAccessorySlot(entry.CharacterId);
        }

        private void RefreshLevelProgress(CharacterEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            var expToNext = GetExpToNextLevel(entry.Level);
            expToNext = Math.Max(1, expToNext);
            var current = Math.Max(0, entry.Exp);
            current = Math.Min(current, expToNext);

            if (_levelProgressSlider != null)
            {
                _levelProgressSlider.minValue = 0;
                _levelProgressSlider.maxValue = expToNext;
                _levelProgressSlider.value = current;
            }

            if (_levelProgressText != null)
            {
                _levelProgressText.text = $"{current}/{expToNext}";
            }
        }

        private void RebuildSkillPanel(CharacterEntry entry)
        {
            ClearSpawnedSkillItems();
            if (_skillRoot == null || _skillItemPrefab == null || entry?.Data == null)
            {
                return;
            }

            if (entry.Data.skills == null)
            {
                return;
            }

            for (var i = 0; i < entry.Data.skills.Count; i++)
            {
                var skillId = entry.Data.skills[i];
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                var skill = DataManager.Instance?.LoadSkill(skillId);
                var iconPath = DataManager.Instance?.ResolveSkillIcon(skillId);
                var icon = LoadSpriteFromResourcesPath(iconPath);
                var name = skill?.nameKey ?? skillId;
                var desc = skill?.description ?? string.Empty;

                var item = Instantiate(_skillItemPrefab, _skillRoot);
                _spawnedSkillItems.Add(item);
                item.Bind(icon, name, skillId, desc);
                item.OnSkillDetailClicked = OnSkillDetailClicked;
            }
        }

        private static Sprite LoadSpriteFromResourcesPath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return null;
            }

            var resourcePath = rawPath.Replace("\\", "/").Trim();
            if (resourcePath.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = resourcePath.Substring("Assets/Resources/".Length);
            }

            if (resourcePath.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = resourcePath.Substring("Resources/".Length);
            }

            if (resourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                resourcePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                resourcePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = System.IO.Path.ChangeExtension(resourcePath, null);
            }

            return Resources.Load<Sprite>(resourcePath);
        }

        private void OnSkillDetailClicked(string skillId, string description, Vector3 anchorPosition)
        {
            if (_skillDetailPanel == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(skillId))
            {
                _skillDetailPanel.Hide();
                return;
            }

            var skill = DataManager.Instance?.LoadSkill(skillId);
            var name = skill?.nameKey ?? skillId;
            var desc = !string.IsNullOrWhiteSpace(skill?.description) ? skill.description : (description ?? string.Empty);
            var dmg = !string.IsNullOrWhiteSpace(skill?.damage?.formula) ? skill.damage.formula : "-";
            var cd = $"{Math.Max(0, skill?.cost?.cooldown ?? 0)}";
            var mana = $"{Math.Max(0, skill?.cost?.mana ?? 0)}";

            _skillDetailPanel.ShowSkillDetail(
                skillName: name,
                skillDescription: desc,
                damageText: dmg,
                cooldownText: cd,
                manaText: mana,
                targetPosition: anchorPosition);
        }

        private void OnAccessoryActionRequested()
        {
            if (string.IsNullOrWhiteSpace(_selectedCharacterId) || _inventoryService == null)
            {
                return;
            }

            if (_accessoryActionPanel == null)
            {
                OpenAccessoryPicker();
                return;
            }

            SetAccessoryActionPanelVisible(!_accessoryActionPanel.activeSelf);
        }

        private void OnAccessoryEquipOptionSelected()
        {
            SetAccessoryActionPanelVisible(false);
            OpenAccessoryPicker();
        }

        private void OnAccessoryUnequipOptionSelected()
        {
            SetAccessoryActionPanelVisible(false);

            if (string.IsNullOrWhiteSpace(_selectedCharacterId) || _inventoryService == null)
            {
                return;
            }

            _inventoryService.UnequipAccessory(_selectedCharacterId);
            RefreshRosterAndFilters();
            RefreshDetail(_selectedCharacterId);
        }

        private void OpenAccessoryPicker()
        {
            if (_accessoryPicker == null || _inventoryService == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedCharacterId))
            {
                return;
            }

            _accessoryPicker.Open(
                characterId: _selectedCharacterId,
                inventoryService: _inventoryService,
                onChanged: () =>
                {
                    RefreshRosterAndFilters();
                    RefreshDetail(_selectedCharacterId);
                });
        }

        private void SetAccessoryActionPanelVisible(bool visible)
        {
            if (_accessoryActionPanel != null)
            {
                _accessoryActionPanel.SetActive(visible);
            }
        }

        private void RefreshAccessorySlot(string characterId)
        {
            if (_inventoryService == null || string.IsNullOrWhiteSpace(characterId))
            {
                SetAccessorySlotEmpty();
                return;
            }

            var equippedItemId = _inventoryService.GetEquippedAccessory(characterId);
            if (string.IsNullOrWhiteSpace(equippedItemId))
            {
                SetAccessorySlotEmpty();
                return;
            }

            var item = DataManager.Instance?.LoadItem(equippedItemId);
            var icon = item != null && !string.IsNullOrWhiteSpace(item.iconPath) ? Resources.Load<Sprite>(item.iconPath) : null;

            if (_equippedAccessoryIcon != null)
            {
                _equippedAccessoryIcon.sprite = icon;
                _equippedAccessoryIcon.color = icon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _equippedAccessoryIcon.preserveAspect = true;
            }

           

           
        }

        private void SetAccessorySlotEmpty()
        {
            if (_equippedAccessoryIcon != null)
            {
                _equippedAccessoryIcon.sprite = _emptyAccessoryIconSprite;
                _equippedAccessoryIcon.color = _emptyAccessoryIconSprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _equippedAccessoryIcon.preserveAspect = true;
            }

            

            
        }

        private static string ReadDropdownValue(TMP_Dropdown dropdown, string fallback)
        {
            if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            {
                return fallback;
            }

            var idx = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
            var label = dropdown.options[idx]?.text;
            return string.IsNullOrWhiteSpace(label) ? fallback : label;
        }

        private static int GetExpToNextLevel(int level)
        {
            return CharacterProgression.GetExpToNextLevel(level);
        }

        private CharacterComputedStats ComputeCharacterStats(CharacterDataModel data, int level, string characterId)
        {
            level = Math.Max(1, level);

            var baseHp = Math.Max(1, data.baseStats?.hp ?? 1);
            var baseAtk = Math.Max(1, data.baseStats?.atk ?? 1);
            var baseDef = Math.Max(0, data.baseStats?.def ?? 0);
            var baseSpd = Math.Max(1, data.baseStats?.spd ?? 1);

            var hp = ComputeScaledStat(baseHp, Math.Max(0, data.growthCurve?.hpPerLevel ?? 0), level);
            var atk = ComputeScaledStat(baseAtk, Math.Max(0, data.growthCurve?.atkPerLevel ?? 0), level);
            var def = ComputeScaledStat(baseDef, Math.Max(0, data.growthCurve?.defPerLevel ?? 0), level);
            var spd = ComputeScaledStat(baseSpd, Math.Max(0, data.growthCurve?.spdPerLevel ?? 0), level);

            var crit = Mathf.Max(0f, data.baseStats?.crit ?? 0.05f);
            var res = Mathf.Max(0f, data.baseStats?.resist ?? 0f);

            ApplyAccessoryBonuses(characterId, ref hp, ref atk, ref def, ref spd, ref crit, ref res);

            return new CharacterComputedStats
            {
                MaxHp = hp,
                ATK = atk,
                DEF = def,
                SPD = spd,
                CritRate = crit,
                Resist = res
            };
        }

        private static void ApplyAccessoryBonuses(string characterId, ref int hp, ref int atk, ref int def, ref int spd, ref float crit, ref float res)
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null || string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            var accessoryItemId = save.GetEquippedAccessory(characterId);
            if (string.IsNullOrWhiteSpace(accessoryItemId))
            {
                return;
            }

            var item = DataManager.Instance?.LoadItem(accessoryItemId);
            if (item == null || !string.Equals(item.itemType, "accessory", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var bonuses = AccessoryStatUtility.GetBonuses(item);
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                switch (bonus.StatKey)
                {
                    case "HP":
                        hp += bonus.Amount;
                        break;
                    case "ATK":
                        atk += bonus.Amount;
                        break;
                    case "DEF":
                        def += bonus.Amount;
                        break;
                    case "SPD":
                        spd += bonus.Amount;
                        break;
                    case "CRIT":
                        crit += bonus.Amount * 0.01f;
                        break;
                    case "RES":
                        res += bonus.Amount * 0.01f;
                        break;
                }
            }

            hp = Mathf.Max(1, hp);
            atk = Mathf.Max(1, atk);
            def = Mathf.Max(0, def);
            spd = Mathf.Max(1, spd);
        }

        private static int ComputeScaledStat(int baseValue, int perLevel, int level)
        {
            return Math.Max(1, baseValue + Math.Max(0, level - 1) * perLevel);
        }

        private static int ComputeMaxMana(CharacterDataModel data, int level)
        {
            var maxSkillCost = 0;
            if (data.skills != null)
            {
                for (var i = 0; i < data.skills.Count; i++)
                {
                    var skill = DataManager.Instance?.LoadSkill(data.skills[i]);
                    var mana = skill?.cost?.mana ?? 0;
                    if (mana > maxSkillCost)
                    {
                        maxSkillCost = mana;
                    }
                }
            }

            return Math.Max(20, maxSkillCost * 3 + Math.Max(1, level) * 5);
        }

        private static int GetAccessoryFlatBonus(string characterId, string statKey)
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null || string.IsNullOrWhiteSpace(characterId))
            {
                return 0;
            }

            var accessoryItemId = save.GetEquippedAccessory(characterId);
            if (string.IsNullOrWhiteSpace(accessoryItemId))
            {
                return 0;
            }

            var item = DataManager.Instance?.LoadItem(accessoryItemId);
            if (item == null)
            {
                return 0;
            }

            var targetKey = AccessoryStatUtility.NormalizeStatKey(statKey);
            var bonuses = AccessoryStatUtility.GetBonuses(item);
            var total = 0;
            for (var i = 0; i < bonuses.Count; i++)
            {
                if (string.Equals(bonuses[i].StatKey, targetKey, StringComparison.OrdinalIgnoreCase))
                {
                    total += bonuses[i].Amount;
                }
            }

            return total;
        }

        private void ClearDetail()
        {
            if (_nameText != null) _nameText.text = "-";
            if (_levelText != null) _levelText.text = "Lv.-";
            if (_rarityText != null)
            {
                _rarityText.text = "-";
                ApplyRarityTextStyle(_rarityText, string.Empty);
            }
            SetOptionalIcon(_roleIconImage, null);
            SetOptionalIcon(_elementIconImage, null);
            if (_hpText != null) _hpText.text = "HP: -";
            if (_manaText != null) _manaText.text = "Mana: -";
            if (_atkText != null) _atkText.text = "-";
            if (_defText != null) _defText.text = "-";
            if (_spdText != null) _spdText.text = "-";
            if (_critText != null) _critText.text = "-";
            
            
            if (_portraitImage != null)
            {
                _portraitImage.sprite = null;
                _portraitImage.color = new Color(1f, 1f, 1f, 0f);
            }

            if (_levelProgressSlider != null)
            {
                _levelProgressSlider.minValue = 0;
                _levelProgressSlider.maxValue = 1;
                _levelProgressSlider.value = 0;
            }

            if (_levelProgressText != null)
            {
                _levelProgressText.text = "0/0";
            }

            ClearSpawnedSkillItems();
            SetAccessorySlotEmpty();

            if (_skillDetailPanel != null)
            {
                _skillDetailPanel.Hide();
            }
        }

        private static void ApplyRarityTextStyle(TMP_Text rarityText, string rarity)
        {
            if (rarityText == null)
            {
                return;
            }

            if (rarityText is TextMeshProUGUI tmp)
            {
                tmp.enableVertexGradient = false;
                tmp.colorGradient = default;
            }

            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase))
            {
                var topColor = HexToColor("#E61919");
                var bottomColor = HexToColor("#3D0000");
                rarityText.color = topColor;

                if (rarityText is TextMeshProUGUI textMesh)
                {
                    textMesh.enableVertexGradient = true;
                    textMesh.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
                }

                return;
            }

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                var topColor = HexToColor("#FFFFB3FF");
                var bottomColor = HexToColor("#FFB300FF");
                rarityText.color = HexToColor("#FFD700");

                if (rarityText is TextMeshProUGUI textMesh)
                {
                    textMesh.enableVertexGradient = true;
                    textMesh.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
                }

                return;
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                rarityText.color = HexToColor("#FF007F");
                return;
            }

            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
            {
                rarityText.color = HexToColor("#00F0FF");
                return;
            }

            rarityText.color = Color.white;
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }

            return Color.white;
        }

        private static void SetOptionalIcon(Image target, Sprite sprite)
        {
            if (target == null)
            {
                return;
            }

            target.sprite = sprite;
            target.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            target.preserveAspect = true;
        }

        private static Sprite LoadRoleIconSprite(string roleTag)
        {
            var key = NormalizeTag(roleTag);
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var candidates = new[]
            {
                $"UI/Role/icon_role_{key}",
                $"Icons/Role/icon_role_{key}",
                $"Role/{key}",
                $"UI/Role/{key}",
                $"Icons/{key}"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite LoadElementIconSprite(string elementTag)
        {
            var key = NormalizeTag(elementTag);
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var candidates = new[]
            {
                $"UI/Element/icon_element_{key}",
                $"Icons/Element/icon_element_{key}",
                $"Element/{key}",
                $"UI/Element/{key}",
                $"Icons/{key}"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(candidates[i]);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static string NormalizeTag(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }

        private void ClearSpawnedSlots()
        {
            for (var i = 0; i < _spawnedSlots.Count; i++)
            {
                if (_spawnedSlots[i] != null)
                {
                    Destroy(_spawnedSlots[i].gameObject);
                }
            }

            _spawnedSlots.Clear();
            _spawnedSlotCharacterIds.Clear();
        }

        private void ClearSpawnedSkillItems()
        {
            for (var i = 0; i < _spawnedSkillItems.Count; i++)
            {
                if (_spawnedSkillItems[i] != null)
                {
                    Destroy(_spawnedSkillItems[i].gameObject);
                }
            }

            _spawnedSkillItems.Clear();
        }

        private void SetListFeedback(string message)
        {
            if (_listFeedbackText != null)
            {
                _listFeedbackText.text = message ?? string.Empty;
            }
        }

        private sealed class CharacterEntry
        {
            public string CharacterId;
            public CharacterDataModel Data;
            public int Level;
            public int Exp;
            public int CurrentHp;
            public int MaxHp;
            public int CurrentMana;
            public int MaxMana;
        }

        private struct CharacterComputedStats
        {
            public int MaxHp;
            public int ATK;
            public int DEF;
            public int SPD;
            public float CritRate;
            public float Resist;
        }
    }
}
