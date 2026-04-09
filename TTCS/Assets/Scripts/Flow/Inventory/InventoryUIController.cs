using System;
using System.Collections.Generic;
using TMPro;
using TTCS.Core.Data;
using TTCS.Meta;
using TTCS.Meta.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace TTCS.Flow.Inventory
{
    /// <summary>
    /// Day 6 - Inventory scene UI controller.
    /// Handles item list, detail panel and use-item flow.
    /// </summary>
    public class InventoryUIController : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform _itemListRoot;
        [SerializeField] private InventoryItemCellView _itemCellPrefab;

        [Header("Detail")]
        [SerializeField] private Image _detailIconImage;
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemRarityText;
        [SerializeField] private TMP_Text _itemDescriptionText;
        [SerializeField] private Image _detailFrameImageA;
        [SerializeField] private Image _detailFrameImageB;
        [SerializeField] private Image _detailBorderImage;
        [SerializeField] private Image _detailGlowImage;
        [SerializeField] private Image _accessoryBorderImage;
        [SerializeField] private Image _accessoryGlowImage;
        [SerializeField] private GameObject _accessoryStatRoot;
        [SerializeField] private TMP_Text _accessoryStatTitleText;
        [SerializeField] private Transform _accessoryStatLineRoot;
        [SerializeField] private InventoryAccessoryStatLineView _accessoryStatLinePrefab;
        [SerializeField] private TMP_Text _accessoryStatText;
        [SerializeField] private TMP_Text _feedbackText;

        [Header("Actions")]
        [SerializeField] private Button _backButton;

        public IInventoryService InventoryService { get; set; }

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        private readonly List<GameObject> _spawnedStatLines = new List<GameObject>();
        private ItemStack _selectedStack;

        private void Start()
        {
            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();
            InventoryService ??= hub?.InventoryService;

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(Back);
            }

            DisplayItems();
        }

        public void DisplayItems()
        {
            ClearRows();
            _selectedStack = null;
            SetDetail(null);

            var items = InventoryService?.GetItems();
            if (items == null || items.Count == 0)
            {
                ShowFeedback("Inventory is empty.");
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var stack = items[i];
                SpawnRow(stack);
            }

            ShowFeedback(string.Empty);
        }

        public void OnItemClicked(string itemId)
        {
            var items = InventoryService?.GetItems();
            if (items == null)
            {
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i].itemId, itemId, StringComparison.Ordinal))
                {
                    _selectedStack = items[i];
                    SetDetail(_selectedStack);
                    return;
                }
            }

            ShowFeedback($"Item not found: {itemId}");
        }

        public void Back()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private void SpawnRow(ItemStack stack)
        {
            if (_itemListRoot == null || _itemCellPrefab == null)
            {
                return;
            }

            var cell = Instantiate(_itemCellPrefab, _itemListRoot);
            _spawnedRows.Add(cell.gameObject);

            var itemData = DataManager.Instance?.LoadItem(stack.itemId);
            var borderColor = GetRarityColor(itemData?.rarity);
            var icon = LoadItemIcon(itemData?.iconPath);
            cell.Bind(icon, stack.quantity, borderColor, () => OnItemClicked(stack.itemId));
        }

        private void SetDetail(ItemStack stack)
        {
            var hasSelection = stack != null;
            var itemData = hasSelection ? DataManager.Instance?.LoadItem(stack.itemId) : null;
            var rarity = itemData?.rarity ?? "R";
            var rarityColor = GetRarityColor(rarity);

            if (_itemNameText != null)
            {
                _itemNameText.text = hasSelection ? (itemData?.nameKey ?? stack.itemId) : "-";
            }

            if (_itemRarityText != null)
            {
                _itemRarityText.text = hasSelection ? rarity : "-";
                ApplyRarityTextStyle(_itemRarityText, rarity);
            }

            if (_itemDescriptionText != null)
            {
                if (!hasSelection)
                {
                    _itemDescriptionText.text = "Select an item to view details.";
                }
                else
                {
                    var description = !string.IsNullOrWhiteSpace(itemData?.description)
                        ? itemData.description
                        : $"Type: {itemData?.itemType ?? "unknown"}\nEffect: {itemData?.effectType ?? "none"} (+{itemData?.effectAmount ?? 0})";
                    _itemDescriptionText.text = description;
                }
            }

            if (_detailIconImage != null)
            {
                var icon = hasSelection ? LoadItemIcon(itemData?.iconPath) : null;
                _detailIconImage.sprite = icon;
                _detailIconImage.color = icon == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }

            if (_detailFrameImageA != null)
            {
                _detailFrameImageA.color = hasSelection ? rarityColor : Color.white;
            }

            if (_detailFrameImageB != null)
            {
                _detailFrameImageB.color = hasSelection ? rarityColor : Color.white;
            }

            if(_detailBorderImage != null)
            {
                _detailBorderImage.color = hasSelection ? rarityColor: Color.white;
            }
            
            if (_detailGlowImage != null)
            {
                _detailGlowImage.color = hasSelection ? rarityColor : Color.white;
            }
            if (_accessoryBorderImage != null)
            {
                _accessoryBorderImage.color = hasSelection ? rarityColor : Color.white;
            }
            if(_accessoryGlowImage != null)
            {
                _accessoryGlowImage.color = hasSelection ? rarityColor: Color.white;            
            }

            var isAccessory = hasSelection && string.Equals(itemData?.itemType, "accessory", StringComparison.OrdinalIgnoreCase);

            if (_accessoryStatTitleText != null)
            {
                ApplyRarityTextStyle(_accessoryStatTitleText, hasSelection ? rarity : string.Empty);
            }

            var statLineCount = RebuildAccessoryStatLines(itemData, isAccessory);
            var hasStatContent = isAccessory && (statLineCount > 0 || !string.IsNullOrWhiteSpace(itemData?.statDescription));

            if (_accessoryStatRoot != null)
            {
                _accessoryStatRoot.SetActive(hasStatContent);
            }

            if (_accessoryStatText != null)
            {
                _accessoryStatText.text = isAccessory
                    ? (string.IsNullOrWhiteSpace(itemData?.statDescription) ? "No stat description." : itemData.statDescription)
                    : string.Empty;
            }

            UpdateDetailContentOrder(hasStatContent);

        }

        private int RebuildAccessoryStatLines(ItemDataModel itemData, bool isAccessory)
        {
            ClearAccessoryStatLines();
            if (!isAccessory || _accessoryStatLineRoot == null || _accessoryStatLinePrefab == null)
            {
                return 0;
            }

            var bonuses = AccessoryStatUtility.GetBonuses(itemData);
            for (var i = 0; i < bonuses.Count; i++)
            {
                var bonus = bonuses[i];
                var line = Instantiate(_accessoryStatLinePrefab, _accessoryStatLineRoot);
                _spawnedStatLines.Add(line.gameObject);

                var iconPath = string.IsNullOrWhiteSpace(bonus.IconPath)
                    ? AccessoryStatUtility.GetDefaultIconPath(bonus.StatKey)
                    : bonus.IconPath;

                var icon = LoadResourceSprite(iconPath);
                line.Bind(AccessoryStatUtility.GetDisplayName(bonus.StatKey), icon, bonus.Amount);
            }

            return bonuses.Count;
        }

        private void UpdateDetailContentOrder(bool hasStatContent)
        {
            if (_itemDescriptionText == null || _accessoryStatRoot == null)
            {
                return;
            }

            var descriptionTransform = _itemDescriptionText.transform;
            var statTransform = _accessoryStatRoot.transform;

            if (descriptionTransform.parent == null || descriptionTransform.parent != statTransform.parent)
            {
                return;
            }

            if (hasStatContent)
            {
                var first = Math.Min(descriptionTransform.GetSiblingIndex(), statTransform.GetSiblingIndex());
                statTransform.SetSiblingIndex(first);
                descriptionTransform.SetSiblingIndex(first + 1);
            }
            else
            {
                descriptionTransform.SetAsFirstSibling();
            }
        }

        private static Sprite LoadItemIcon(string iconPath)
        {
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                return null;
            }

            return Resources.Load<Sprite>(iconPath);
        }

        private static Sprite LoadResourceSprite(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            return Resources.Load<Sprite>(resourcePath);
        }

        private static Color GetRarityColor(string rarity)
        {
            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#FFD700");
            }

            if (string.Equals(rarity, "SR", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#FF007F");
            }

            if (string.Equals(rarity, "R", StringComparison.OrdinalIgnoreCase))
            {
                return HexToColor("#00F0FF");
            }

            return Color.white;
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

            if (string.Equals(rarity, "SSR", StringComparison.OrdinalIgnoreCase))
            {
                // SSR: Gradient color - top corners #FFFFB3FF, bottom corners #FFB300FF
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
            var normalized = hex.Replace("#", string.Empty);
            if (normalized.Length == 6)
            {
                normalized += "FF";
            }

            if (int.TryParse(normalized, System.Globalization.NumberStyles.HexNumber, null, out var result))
            {
                var r = (byte)((result >> 24) & 0xFF);
                var g = (byte)((result >> 16) & 0xFF);
                var b = (byte)((result >> 8) & 0xFF);
                var a = (byte)(result & 0xFF);
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }

            return Color.white;
        }

        private void ShowFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
            }
        }

        private void ClearRows()
        {
            for (var i = 0; i < _spawnedRows.Count; i++)
            {
                if (_spawnedRows[i] != null)
                {
                    Destroy(_spawnedRows[i]);
                }
            }

            _spawnedRows.Clear();
        }

        private void ClearAccessoryStatLines()
        {
            for (var i = 0; i < _spawnedStatLines.Count; i++)
            {
                if (_spawnedStatLines[i] != null)
                {
                    Destroy(_spawnedStatLines[i]);
                }
            }

            _spawnedStatLines.Clear();
        }
    }
}
