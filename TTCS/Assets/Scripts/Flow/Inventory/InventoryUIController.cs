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
        [SerializeField] private GameObject _itemRowPrefab;

        [Header("Detail")]
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemDescriptionText;
        [SerializeField] private TMP_Text _itemQuantityText;
        [SerializeField] private Button _useItemButton;
        [SerializeField] private TMP_Text _feedbackText;

        [Header("Actions")]
        [SerializeField] private Button _backButton;
        [SerializeField] private int _useQuantity = 1;

        public IInventoryService InventoryService { get; set; }

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        private ItemStack _selectedStack;

        private void Start()
        {
            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();
            InventoryService ??= hub?.InventoryService;

            if (_useItemButton != null)
            {
                _useItemButton.onClick.AddListener(UseSelectedItem);
            }

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

        public void OnUseItemConfirmed(string itemId, int quantity)
        {
            if (InventoryService == null)
            {
                ShowFeedback("InventoryService is not available.");
                return;
            }

            var result = InventoryService.UseItem(itemId, quantity, "menu");
            ShowFeedback(result.Message);
            DisplayItems();
        }

        public void Back()
        {
            FlowController.Instance.OpenMainMenu();
        }

        private void UseSelectedItem()
        {
            if (_selectedStack == null)
            {
                ShowFeedback("Please select an item first.");
                return;
            }

            OnUseItemConfirmed(_selectedStack.itemId, Math.Max(1, _useQuantity));
        }

        private void SpawnRow(ItemStack stack)
        {
            if (_itemListRoot == null || _itemRowPrefab == null)
            {
                return;
            }

            var row = Instantiate(_itemRowPrefab, _itemListRoot);
            _spawnedRows.Add(row);

            var button = row.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemClicked(stack.itemId));
            }

            var label = row.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                var itemData = DataManager.Instance?.LoadItem(stack.itemId);
                var displayName = itemData?.nameKey ?? stack.itemId;
                label.text = $"{displayName} x{stack.quantity}";
            }
        }

        private void SetDetail(ItemStack stack)
        {
            var hasSelection = stack != null;
            var itemData = hasSelection ? DataManager.Instance?.LoadItem(stack.itemId) : null;

            if (_itemNameText != null)
            {
                _itemNameText.text = hasSelection ? (itemData?.nameKey ?? stack.itemId) : "-";
            }

            if (_itemDescriptionText != null)
            {
                if (!hasSelection)
                {
                    _itemDescriptionText.text = "Select an item to view details.";
                }
                else
                {
                    _itemDescriptionText.text = $"Type: {itemData?.itemType ?? "unknown"}\nEffect: {itemData?.effectType ?? "none"} (+{itemData?.effectAmount ?? 0})";
                }
            }

            if (_itemQuantityText != null)
            {
                _itemQuantityText.text = hasSelection ? $"Qty: {stack.quantity}" : "Qty: -";
            }

            if (_useItemButton != null)
            {
                _useItemButton.interactable = hasSelection && InventoryService != null && InventoryService.CanUseItem(stack.itemId, "menu");
            }
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
    }
}
