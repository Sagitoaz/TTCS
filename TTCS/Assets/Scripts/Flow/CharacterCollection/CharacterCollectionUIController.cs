using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTCS.Core.Data;
using TTCS.Core.Save;
using TTCS.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.CharacterCollection
{
    /// <summary>
    /// Day 6 - Character collection screen controller.
    /// Supports search, sorting, detail panel and feed-level action.
    /// </summary>
    public class CharacterCollectionUIController : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform _cardGridRoot;
        [SerializeField] private GameObject _characterCardPrefab;

        [Header("Search")]
        [SerializeField] private TMP_InputField _searchInput;

        [Header("Detail")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private Image _skillIconImage;

        [Header("Actions")]
        [SerializeField] private Button _feedButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _feedbackText;

        private readonly List<CharacterCardViewModel> _allCharacters = new List<CharacterCardViewModel>();
        private readonly List<GameObject> _spawnedCards = new List<GameObject>();

        private SaveManager _saveManager;
        private string _keyword = string.Empty;
        private bool _sortRareAscending = false;
        private bool _sortLevelAscending = false;
        private string _selectedCharacterId;

        private void Start()
        {
            _saveManager = SaveManager.Instance;
            _saveManager?.EnsureCurrentSave(0);

            if (_searchInput != null)
            {
                _searchInput.onValueChanged.AddListener(OnSearchChanged);
            }

            if (_feedButton != null)
            {
                _feedButton.onClick.AddListener(OnFeedCurrentCharacter);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(Back);
            }

            DisplayUnlockedCharacters();
        }

        public void DisplayUnlockedCharacters()
        {
            _allCharacters.Clear();

            var save = _saveManager?.CurrentSave;
            if (save == null)
            {
                ShowFeedback("Save is not available.");
                RenderCards();
                return;
            }

            for (var i = 0; i < save.unlockedCharacters.Count; i++)
            {
                var characterId = save.unlockedCharacters[i];
                if (string.IsNullOrWhiteSpace(characterId))
                {
                    continue;
                }

                var data = DataManager.Instance?.LoadCharacter(characterId);
                if (data == null)
                {
                    continue;
                }

                var level = save.GetCharacterLevel(characterId);
                var maxHp = ComputeMaxHp(data, level);
                var maxMana = ComputeMaxMana(data, level);
                var currentHp = save.GetCharacterCurrentHp(characterId, maxHp);
                var currentMana = save.GetCharacterCurrentMana(characterId, maxMana);

                _allCharacters.Add(new CharacterCardViewModel
                {
                    CharacterId = characterId,
                    Data = data,
                    Level = Math.Max(1, level),
                    MaxHp = maxHp,
                    MaxMana = maxMana,
                    CurrentHp = Mathf.Clamp(currentHp, 0, maxHp),
                    CurrentMana = Mathf.Clamp(currentMana, 0, maxMana)
                });
            }

            if (_allCharacters.Count > 0 && string.IsNullOrWhiteSpace(_selectedCharacterId))
            {
                _selectedCharacterId = _allCharacters[0].CharacterId;
            }

            RenderCards();
            RefreshDetail(_selectedCharacterId);
            ShowFeedback(string.Empty);
        }

        public void OnSearchChanged(string keyword)
        {
            _keyword = keyword ?? string.Empty;
            RenderCards();
        }

        public void OnSortRareChanged(bool ascending)
        {
            _sortRareAscending = ascending;
            RenderCards();
        }

        public void OnSortLevelChanged(bool ascending)
        {
            _sortLevelAscending = ascending;
            RenderCards();
        }

        public void OnCharacterCardClicked(string characterId)
        {
            _selectedCharacterId = characterId;
            RefreshDetail(characterId);
        }

        public void OnFeedButtonClicked(string characterId)
        {
            var save = _saveManager?.CurrentSave;
            if (save == null)
            {
                ShowFeedback("Save is not available.");
                return;
            }

            var vm = _allCharacters.FirstOrDefault(c => string.Equals(c.CharacterId, characterId, StringComparison.Ordinal));
            if (vm == null)
            {
                ShowFeedback("Character not found.");
                return;
            }

            var nextLevel = vm.Level + 1;
            save.SetCharacterLevel(vm.CharacterId, nextLevel);

            // Day 6 rule: level-up always restores full HP + Mana.
            var nextMaxHp = ComputeMaxHp(vm.Data, nextLevel);
            var nextMaxMana = ComputeMaxMana(vm.Data, nextLevel);
            save.SetCharacterCurrentHp(vm.CharacterId, nextMaxHp);
            save.SetCharacterCurrentMana(vm.CharacterId, nextMaxMana);

            if (_saveManager.ActiveSlotIndex >= 0)
            {
                _saveManager.Save(_saveManager.ActiveSlotIndex);
            }

            DisplayUnlockedCharacters();
            RefreshDetail(vm.CharacterId);
            ShowFeedback($"{vm.Data.nameKey ?? vm.CharacterId} reached level {nextLevel}. HP/Mana restored.");
        }

        public void Back()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private void OnFeedCurrentCharacter()
        {
            if (string.IsNullOrWhiteSpace(_selectedCharacterId))
            {
                ShowFeedback("Please choose a character first.");
                return;
            }

            OnFeedButtonClicked(_selectedCharacterId);
        }

        private void RenderCards()
        {
            ClearSpawnedCards();

            if (_cardGridRoot == null || _characterCardPrefab == null)
            {
                return;
            }

            var source = FilterAndSort();
            for (var i = 0; i < source.Count; i++)
            {
                var vm = source[i];
                var go = Instantiate(_characterCardPrefab, _cardGridRoot);
                _spawnedCards.Add(go);

                var button = go.GetComponent<Button>();
                if (button != null)
                {
                    var id = vm.CharacterId;
                    button.onClick.AddListener(() => OnCharacterCardClicked(id));
                }

                var rarity = vm.Data.metadata?.rarity ?? "R";
                SetText(go, $"{vm.Data.nameKey ?? vm.CharacterId}\nLv.{vm.Level} | {rarity}");
            }
        }

        private List<CharacterCardViewModel> FilterAndSort()
        {
            var query = _allCharacters.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_keyword))
            {
                var lowered = _keyword.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    (c.Data.nameKey ?? string.Empty).ToLowerInvariant().Contains(lowered) ||
                    c.CharacterId.ToLowerInvariant().Contains(lowered));
            }

            IOrderedEnumerable<CharacterCardViewModel> ordered = _sortRareAscending
                ? query.OrderBy(c => RarityWeight(c.Data.metadata?.rarity))
                : query.OrderByDescending(c => RarityWeight(c.Data.metadata?.rarity));

            ordered = _sortLevelAscending
                ? ordered.ThenBy(c => c.Level)
                : ordered.ThenByDescending(c => c.Level);

            return ordered.ToList();
        }

        private void RefreshDetail(string characterId)
        {
            var vm = _allCharacters.FirstOrDefault(c => string.Equals(c.CharacterId, characterId, StringComparison.Ordinal));
            if (vm == null)
            {
                SetDetailEmpty();
                return;
            }

            var rarity = vm.Data.metadata?.rarity ?? "R";
            var baseStats = vm.Data.baseStats;
            var growth = vm.Data.growthCurve;

            if (_nameText != null)
            {
                _nameText.text = vm.Data.nameKey ?? vm.CharacterId;
            }

            if (_rarityText != null)
            {
                _rarityText.text = rarity;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Level {vm.Level}";
            }

            if (_statsText != null)
            {
                var atk = ComputeScaledStat(baseStats?.atk ?? 0, growth?.atkPerLevel ?? 0, vm.Level);
                var def = ComputeScaledStat(baseStats?.def ?? 0, growth?.defPerLevel ?? 0, vm.Level);
                var spd = ComputeScaledStat(baseStats?.spd ?? 0, growth?.spdPerLevel ?? 0, vm.Level);
                _statsText.text = $"ATK {atk}  DEF {def}  SPD {spd}";
            }

            if (_hpText != null)
            {
                _hpText.text = $"HP: {vm.CurrentHp}/{vm.MaxHp}";
            }

            if (_manaText != null)
            {
                _manaText.text = $"Mana: {vm.CurrentMana}/{vm.MaxMana}";
            }

            if (_portraitImage != null)
            {
                var sprite = DataManager.Instance?.LoadCharacterPortraitSprite(vm.Data.visual?.portraitPath ?? vm.Data.visual?.spritePath);
                _portraitImage.sprite = sprite;
                _portraitImage.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_skillIconImage != null)
            {
                _skillIconImage.sprite = ResolvePrimarySkillIcon(vm.Data);
                _skillIconImage.color = _skillIconImage.sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_feedButton != null)
            {
                _feedButton.interactable = true;
            }
        }

        private void SetDetailEmpty()
        {
            if (_nameText != null)
            {
                _nameText.text = "-";
            }

            if (_rarityText != null)
            {
                _rarityText.text = "-";
            }

            if (_levelText != null)
            {
                _levelText.text = "Level -";
            }

            if (_statsText != null)
            {
                _statsText.text = "ATK -  DEF -  SPD -";
            }

            if (_hpText != null)
            {
                _hpText.text = "HP: -";
            }

            if (_manaText != null)
            {
                _manaText.text = "Mana: -";
            }

            if (_feedButton != null)
            {
                _feedButton.interactable = false;
            }
        }

        private static int RarityWeight(string rarity)
        {
            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase))
            {
                return 4;
            }

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 1;
        }

        private static int ComputeMaxHp(CharacterDataModel data, int level)
        {
            var baseValue = Math.Max(1, data.baseStats?.hp ?? 1);
            var perLevel = Math.Max(0, data.growthCurve?.hpPerLevel ?? 0);
            return ComputeScaledStat(baseValue, perLevel, level);
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

        private static int ComputeScaledStat(int baseValue, int perLevel, int level)
        {
            return Math.Max(1, baseValue + Math.Max(0, level - 1) * perLevel);
        }

        private static Sprite ResolvePrimarySkillIcon(CharacterDataModel character)
        {
            if (character?.skills == null || character.skills.Count == 0)
            {
                return null;
            }

            var skillId = character.skills[0];
            var path = DataManager.Instance?.ResolveSkillIcon(skillId);
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var resourcePath = path.Replace("\\", "/").Replace("Assets/Resources/", string.Empty).Replace("Resources/", string.Empty);
            if (resourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                resourcePath = resourcePath.Substring(0, resourcePath.Length - 4);
            }

            return Resources.Load<Sprite>(resourcePath);
        }

        private static void SetText(GameObject root, string value)
        {
            if (root == null)
            {
                return;
            }

            var tmp = root.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = value;
                return;
            }

            var legacy = root.GetComponentInChildren<Text>();
            if (legacy != null)
            {
                legacy.text = value;
            }
        }

        private void ClearSpawnedCards()
        {
            for (var i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] != null)
                {
                    Destroy(_spawnedCards[i]);
                }
            }

            _spawnedCards.Clear();
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
            }
        }

        private sealed class CharacterCardViewModel
        {
            public string CharacterId;
            public CharacterDataModel Data;
            public int Level;
            public int CurrentHp;
            public int MaxHp;
            public int CurrentMana;
            public int MaxMana;
        }
    }
}
