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
    
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _rarityText;
        

        [SerializeField] private Transform _statLineRoot;
        [SerializeField] private InventoryAccessoryStatLineView _statLinePrefab;

        [Header("Actions")]
        [SerializeField] private Button _closeButton;
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
            var canPreselectEquipped = false;
            if (!string.IsNullOrWhiteSpace(equipped) && _inventoryService != null)
            {
                var stacks = _inventoryService.GetItems();
                for (var i = 0; i < stacks.Count; i++)
                {
                    if (stacks[i] != null && string.Equals(stacks[i].itemId, equipped, StringComparison.Ordinal) && stacks[i].quantity > 0)
                    {
                        canPreselectEquipped = true;
                        break;
                    }
                }
            }

            if (canPreselectEquipped)
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

        private void SetDetail(ItemDataModel item)
        {
            var hasItem = item != null;

            if (_detailRoot != null)
            {
                _detailRoot.SetActive(hasItem);
            }

            if (!hasItem)
            {
                

                if (_nameText != null) _nameText.text = "-";
                if (_rarityText != null)
                {
                    _rarityText.text = "-";
                    ApplyRarityTextStyle(_rarityText, string.Empty);
                }
                

                ClearStatLines();
                if (_confirmButton != null) _confirmButton.interactable = false;
                return;
            }

            

            if (_nameText != null)
            {
                _nameText.text = item.nameKey ?? item.id;
            }

            if (_rarityText != null)
            {
                var rarity = item.rarity ?? "R";
                _rarityText.text = rarity;
                ApplyRarityTextStyle(_rarityText, rarity);
            }

            

            RebuildStatLines(item);

            if (_confirmButton != null)
            {
                _confirmButton.interactable = true;
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
