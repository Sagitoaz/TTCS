using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TTCS.Core.Data;
using TTCS.Flow.Inventory;
using TTCS.Meta.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.CharacterCollection
{
    public sealed class AccessoryEquipPickerPanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _panelRoot;

        [Header("List")]
        [SerializeField] private Transform _listRoot;
        [SerializeField] private InventoryItemCellView _cellPrefab;

        [Header("Selection Detail")]
        [SerializeField] private GameObject _detailRoot;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private TMP_Text _descriptionText;

        [SerializeField] private Transform _statLineRoot;
        [SerializeField] private InventoryAccessoryStatLineView _statLinePrefab;

        [Header("Actions")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _feedbackText;

        private readonly List<InventoryItemCellView> _spawnedCells = new List<InventoryItemCellView>();
        private readonly List<InventoryAccessoryStatLineView> _spawnedStatLines = new List<InventoryAccessoryStatLineView>();

        private IInventoryService _inventoryService;
        private string _characterId;
        private string _selectedItemId;
        private Action _onChanged;

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(ConfirmEquip);
            }

            if (_unequipButton != null)
            {
                _unequipButton.onClick.AddListener(Unequip);
            }

            HideInternal();
        }

        public void Open(string characterId, IInventoryService inventoryService, Action onChanged)
        {
            _characterId = characterId;
            _inventoryService = inventoryService;
            _onChanged = onChanged;

            _selectedItemId = string.Empty;
            ShowFeedback(string.Empty);

            ShowInternal();
            RebuildList();

            var equipped = _inventoryService != null ? _inventoryService.GetEquippedAccessory(_characterId) : string.Empty;
            if (!string.IsNullOrWhiteSpace(equipped))
            {
                SelectItem(equipped);
            }
            else
            {
                SetDetail(null);
            }
        }

        public void Close()
        {
            HideInternal();
        }

        private void RebuildList()
        {
            ClearCells();

            if (_listRoot == null || _cellPrefab == null || _inventoryService == null)
            {
                return;
            }

            var items = _inventoryService.GetItems();
            var accessories = items
                .Where(stack => stack != null && !string.IsNullOrWhiteSpace(stack.itemId) && stack.quantity > 0)
                .Select(stack => new
                {
                    Stack = stack,
                    Data = DataManager.Instance?.LoadItem(stack.itemId)
                })
                .Where(x => x.Data != null && string.Equals(x.Data.itemType, "accessory", StringComparison.OrdinalIgnoreCase) && x.Data.equippable)
                .OrderByDescending(x => RarityWeight(x.Data.rarity))
                .ThenBy(x => x.Data.nameKey ?? x.Stack.itemId)
                .ToList();

            for (var i = 0; i < accessories.Count; i++)
            {
                var stack = accessories[i].Stack;
                var data = accessories[i].Data;
                var icon = !string.IsNullOrWhiteSpace(data.iconPath) ? Resources.Load<Sprite>(data.iconPath) : null;
                var rarity = data.rarity ?? "R";

                var cell = Instantiate(_cellPrefab, _listRoot);
                _spawnedCells.Add(cell);

                var id = stack.itemId;
                cell.Bind(icon, stack.quantity, rarity, id, () => SelectItem(id));
            }

            UpdateCellSelection();
        }

        private void SelectItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                _selectedItemId = string.Empty;
                SetDetail(null);
                UpdateCellSelection();
                return;
            }

            _selectedItemId = itemId;
            var item = DataManager.Instance?.LoadItem(itemId);
            SetDetail(item);
            UpdateCellSelection();

            ShowFeedback(string.Empty);
        }

        private void ConfirmEquip()
        {
            if (_inventoryService == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_characterId))
            {
                ShowFeedback("Character is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedItemId))
            {
                ShowFeedback("Select an accessory first.");
                return;
            }

            var result = _inventoryService.EquipAccessory(_selectedItemId, _characterId);
            ShowFeedback(result.Message);

            if (result.Success)
            {
                _onChanged?.Invoke();
                Close();
            }
        }

        private void Unequip()
        {
            if (_inventoryService == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_characterId))
            {
                ShowFeedback("Character is required.");
                return;
            }

            var result = _inventoryService.UnequipAccessory(_characterId);
            ShowFeedback(result.Message);

            if (result.Success)
            {
                _selectedItemId = string.Empty;
                _onChanged?.Invoke();
                RebuildList();
                SetDetail(null);
            }
        }

        private void SetDetail(ItemDataModel item)
        {
            var hasItem = item != null;

            if (_detailRoot != null)
            {
                _detailRoot.SetActive(hasItem);
            }

            if (!hasItem)
            {
                if (_iconImage != null)
                {
                    _iconImage.sprite = null;
                    _iconImage.color = new Color(1f, 1f, 1f, 0f);
                }

                if (_nameText != null) _nameText.text = "-";
                if (_rarityText != null) _rarityText.text = "-";
                if (_descriptionText != null) _descriptionText.text = string.Empty;

                ClearStatLines();
                if (_confirmButton != null) _confirmButton.interactable = false;
                if (_unequipButton != null) _unequipButton.interactable = true;
                return;
            }

            if (_iconImage != null)
            {
                var icon = !string.IsNullOrWhiteSpace(item.iconPath) ? Resources.Load<Sprite>(item.iconPath) : null;
                _iconImage.sprite = icon;
                _iconImage.color = icon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
                _iconImage.preserveAspect = true;
            }

            if (_nameText != null)
            {
                _nameText.text = item.nameKey ?? item.id;
            }

            if (_rarityText != null)
            {
                _rarityText.text = item.rarity ?? "R";
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = !string.IsNullOrWhiteSpace(item.description)
                    ? item.description
                    : (string.IsNullOrWhiteSpace(item.statDescription) ? string.Empty : item.statDescription);
            }

            RebuildStatLines(item);

            if (_confirmButton != null)
            {
                _confirmButton.interactable = true;
            }

            if (_unequipButton != null)
            {
                _unequipButton.interactable = true;
            }
        }

        private void RebuildStatLines(ItemDataModel item)
        {
            ClearStatLines();
            if (_statLineRoot == null || _statLinePrefab == null || item == null)
            {
                return;
            }

            var bonuses = AccessoryStatUtility.GetBonuses(item);
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                var iconPath = string.IsNullOrWhiteSpace(bonus.IconPath)
                    ? AccessoryStatUtility.GetDefaultIconPath(bonus.StatKey)
                    : bonus.IconPath;

                var icon = !string.IsNullOrWhiteSpace(iconPath) ? Resources.Load<Sprite>(iconPath) : null;

                var line = Instantiate(_statLinePrefab, _statLineRoot);
                _spawnedStatLines.Add(line);
                line.Bind(AccessoryStatUtility.GetDisplayName(bonus.StatKey), icon, bonus.Amount);
            }
        }

        private void UpdateCellSelection()
        {
            for (var i = 0; i < _spawnedCells.Count; i++)
            {
                var cell = _spawnedCells[i];
                if (cell == null)
                {
                    continue;
                }

                cell.SetSelected(string.Equals(cell.ItemId, _selectedItemId, StringComparison.Ordinal));
            }
        }

        private void ShowInternal()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        private void HideInternal()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void ClearCells()
        {
            for (var i = 0; i < _spawnedCells.Count; i++)
            {
                if (_spawnedCells[i] != null)
                {
                    Destroy(_spawnedCells[i].gameObject);
                }
            }

            _spawnedCells.Clear();
        }

        private void ClearStatLines()
        {
            for (var i = 0; i < _spawnedStatLines.Count; i++)
            {
                if (_spawnedStatLines[i] != null)
                {
                    Destroy(_spawnedStatLines[i].gameObject);
                }
            }

            _spawnedStatLines.Clear();
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message ?? string.Empty;
            }
        }

        private static int RarityWeight(string rarity)
        {
            if (string.Equals(rarity, "UR", StringComparison.OrdinalIgnoreCase)) return 4;
            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase)) return 3;
            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase)) return 2;
            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase)) return 1;
            return 0;
        }
    }
}
