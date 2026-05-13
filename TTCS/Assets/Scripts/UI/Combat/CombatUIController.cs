using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TTCS.Combat.Entities;
using TTCS.Core.Events;
using TTCS.Core.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;
using TTCS.Combat.Timing;
using TTCS.Combat.Managers;
using TTCS.Flow;
using TTCS.Flow.Inventory;
using TTCS.Flow.LevelSelect;
using TTCS.Data;
using TTCS.Meta;
using TTCS.Meta.Inventory;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Combat UI Controller
    /// Singleton root của toàn bộ UI combat.
    /// Giữ references đến tất cả sub-panels và làm cầu nối với engine events.
    ///
    /// Setup trong Unity:
    ///   1. Đặt CombatUIController trên Canvas root GameObject.
    ///   2. Gán tất cả SerializeField references.
    ///   3. Gọi Initialize() từ CombatSceneManager sau khi EntityFactory tạo xong entities.
    ///
    /// Developer B gọi:
    ///   CombatUIController.Instance.RegisterEntityPosition(entityId, transform)
    ///   để ActionResultDisplay biết vị trí world của entity.
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static CombatUIController _instance;
        public  static CombatUIController Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                EventBus.Instance?.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
                EventBus.Instance?.Unsubscribe<TurnStartedEvent>(OnPlayerTurnStarted);
            }

            // Safety: avoid leaving the game paused if this UI is destroyed.
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
        }

        // ─── Sub-Panel References ─────────────────────────────────────────
        [Header("Sub-Panels")]
        [SerializeField] private BattleHUD            _battleHUD;
        [SerializeField] private SkillButtonPanel     _skillButtonPanel;
        [SerializeField] private TurnOrderDisplay     _turnOrderDisplay;
        [SerializeField] private ActionResultDisplay  _actionResultDisplay;
        [SerializeField] private TimingFeedbackUI     _timingFeedbackUI;
        [SerializeField] private SkillCastPortraitPanel _skillCastPortraitPanel;

        [Header("Pause")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _pauseContinueButton;
        [SerializeField] private Button _pauseRetryButton;
        [SerializeField] private Button _pauseExitButton;

        [Header("Combat Item UI")]
        [SerializeField] private InventoryItemCellView _combatItemCellPrefab;

        [Header("Result Screen")]
        [SerializeField] private GameObject          _resultPanel;
        [SerializeField] private Button              _resultButton;
        [SerializeField] private TextMeshProUGUI     _resultButtonLabel;
        [SerializeField] private ResultCardController _resultCardController;
        [SerializeField] private string              _victoryButtonLabel = "Continue";
        [SerializeField] private string              _defeatButtonLabel = "Retry";

        [Header("Result Actions")]
        [SerializeField] private Button _backHomeButton;

        [Header("Victory Rewards (Icon + Text)")]
        [SerializeField] private RectTransform _victoryRewardContent;
        [SerializeField] private LevelRewardItemView _victoryRewardItemViewPrefab;
        [SerializeField] private float _victoryRewardSpacing = 14f;
        [SerializeField] private float _victoryRewardPaddingLeftRight = 18f;

        [Header("Result Team Progress")]
        [SerializeField] private RectTransform _resultTeamProgressContent;
        [SerializeField] private CombatResultCharacterProgressView _resultTeamProgressItemPrefab;

        private readonly List<GameObject> _spawnedVictoryRewardViews = new List<GameObject>();
        private readonly List<GameObject> _spawnedResultTeamProgressViews = new List<GameObject>();

        private bool _lastVictory;
        private string _lastLevelId;

        private bool _isPaused;
        private float _previousTimeScale = 1f;
        private Button _combatItemButton;
        private GameObject _combatItemPanel;
        private RectTransform _combatItemListContent;
        private TMP_Text _combatItemNameText;
        private TMP_Text _combatItemDescriptionText;
        private TMP_Text _combatItemInfoText;
        private Button _combatItemUseButton;
        private Button _combatItemCancelButton;
        private readonly List<GameObject> _spawnedCombatItemViews = new List<GameObject>();
        private string _selectedCombatItemId;

        // ─── Entity Position Registry ─────────────────────────────────────
        /// <summary>Mapping entityId → World Transform (set bởi Dev B CharacterView).</summary>
        private readonly Dictionary<string, Transform> _entityTransforms = new();

        /// <summary>Mapping entityId → CombatEntity (để query HP).</summary>
        private readonly Dictionary<string, CombatEntity> _entityMap = new();

        /// <summary>Cache danh sách enemy để re-initialize SkillButtonPanel khi đổi lượt player.</summary>
        private List<CombatEntity> _enemyList = new();

        /// <summary>Cache danh sách player ally để target single_ally chính xác.</summary>
        private List<CombatEntity> _allyList = new();

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Khởi tạo toàn bộ UI system.
        /// Gọi từ CombatSceneManager.InitializeCombat() sau khi entities được tạo.
        /// </summary>
        /// <param name="playerTeam">Danh sách character của player</param>
        /// <param name="enemyTeam">Danh sách enemy</param>
        public void Initialize(List<CombatEntity> playerTeam, List<CombatEntity> enemyTeam)
        {
            Log("CombatUIController: Initializing UI...", LogCategory.UI);

            EnsureCombatPanelsActive();
            EnsureCombatItemUI();
            HideCombatItemPanel();

            // Build entity map
            _entityMap.Clear();
            foreach (var e in playerTeam)  if (e != null) _entityMap[e.ID] = e;
            foreach (var e in enemyTeam)   if (e != null) _entityMap[e.ID] = e;

            // Register entity names với TurnOrderDisplay
            foreach (var entity in playerTeam)
                if (entity != null) _turnOrderDisplay?.RegisterEntity(entity);
            foreach (var entity in enemyTeam)
                if (entity != null) _turnOrderDisplay?.RegisterEntity(entity);

            // BattleHUD
            _battleHUD?.InitializeSlots(playerTeam, enemyTeam);

            // Cache enemy list để dùng khi re-init skill panel
            _enemyList = enemyTeam ?? new List<CombatEntity>();
            _allyList = new List<CombatEntity>(playerTeam ?? new List<CombatEntity>());

            // SkillButtonPanel — khởi tạo cho player đầu tiên còn sống
            var firstPlayer = playerTeam.Find(p => p != null && !p.IsDead);
            if (firstPlayer != null)
            {
                var character = firstPlayer as TTCS.Combat.Entities.Character;
                _skillButtonPanel?.Initialize(
                    firstPlayer.ID,
                    character?.SkillIds ?? new List<string>(),
                    _enemyList,
                    _allyList
                );
            }

            // Ẩn result panel
            _resultPanel?.SetActive(false);

            // Pause panel default state
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(false);
            }

            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveAllListeners();
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (_pauseContinueButton != null)
            {
                _pauseContinueButton.onClick.RemoveAllListeners();
                _pauseContinueButton.onClick.AddListener(OnPauseContinueClicked);
            }

            if (_pauseRetryButton != null)
            {
                _pauseRetryButton.onClick.RemoveAllListeners();
                _pauseRetryButton.onClick.AddListener(OnPauseRetryClicked);
            }

            if (_pauseExitButton != null)
            {
                _pauseExitButton.onClick.RemoveAllListeners();
                _pauseExitButton.onClick.AddListener(OnPauseExitClicked);
            }

            SetPaused(false);
            RefreshCombatItemButtonState();
            TimingSystem.Instance.OnTimingResult += ShowTimingResult;

            EventBus.Instance.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Instance.Subscribe<TurnStartedEvent>(OnPlayerTurnStarted);

            Log("CombatUIController: UI initialized.", LogCategory.UI);
        }

        /// <summary>
        /// Update cached enemy list khi wave progression xảy ra (gọi từ CombatFlowController).
        /// </summary>
        public void UpdateEnemyList(List<CombatEntity> newEnemyTeam)
        {
                // Remove old enemy entries from entity map
                foreach (var oldEnemy in _enemyList)
                {
                    if (oldEnemy != null && _entityMap.ContainsKey(oldEnemy.ID))
                        _entityMap.Remove(oldEnemy.ID);
                }

                _enemyList = newEnemyTeam ?? new List<CombatEntity>();
            
                // Add new enemies to entity map
                foreach (var entity in _enemyList)
                {
                    if (entity != null)
                    {
                        _entityMap[entity.ID] = entity;
                        _turnOrderDisplay?.RegisterEntity(entity);
                    }
                }

            // Update BattleHUD slots with new enemies
            _battleHUD?.UpdateEnemySlots(_enemyList);

            // Update SkillButtonPanel's enemy list for target selection
            _skillButtonPanel?.UpdateEnemies(_enemyList);

                Log($"CombatUIController: Updated enemy list to {_enemyList.Count} entities. Entity map now has {_entityMap.Count} total entries.", LogCategory.UI);
        }

        private void EnsureCombatPanelsActive()
        {
            if (_battleHUD != null) _battleHUD.gameObject.SetActive(true);
            if (_skillButtonPanel != null) _skillButtonPanel.gameObject.SetActive(true);
            if (_turnOrderDisplay != null) _turnOrderDisplay.gameObject.SetActive(true);
            if (_actionResultDisplay != null) _actionResultDisplay.gameObject.SetActive(true);
            if (_timingFeedbackUI != null) _timingFeedbackUI.gameObject.SetActive(true);
            if (_skillCastPortraitPanel != null) _skillCastPortraitPanel.gameObject.SetActive(true);
            if (_resultPanel != null) _resultPanel.SetActive(false);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Entity Position Registry

        /// <summary>
        /// Đăng ký Transform của entity để ActionResultDisplay biết vị trí spawn text.
        /// Gọi từ CharacterView / EnemyView (Developer B) khi spawn xong prefab.
        /// </summary>
        public void RegisterEntityPosition(string entityId, Transform worldTransform)
        {
            if (worldTransform == null) return;
            _entityTransforms[entityId] = worldTransform;
        }

        /// <summary>Trả về world position của entity (Vector3.zero nếu chưa đăng ký).</summary>
        public Vector3 GetEntityWorldPos(string entityId)
        {
            return _entityTransforms.TryGetValue(entityId, out var t) && t != null
                ? t.position
                : Vector3.zero;
        }

        /// <summary>HP percent hiện tại của entity (0–1). Dùng bởi BattleHUD.</summary>
        public float GetEntityHPPercent(string entityId)
        {
            return _entityMap.TryGetValue(entityId, out var entity) && entity != null
                ? entity.HPPercent
                : 0f;
        }

        public void BeginEnemyTargetingHUD(List<string> candidateEnemyIds, string currentTargetId)
        {
            _battleHUD?.BeginEnemyTargeting(candidateEnemyIds, currentTargetId);
        }

        public void UpdateEnemyTargetingHUD(string currentTargetId)
        {
            _battleHUD?.UpdateEnemyTargeting(currentTargetId);
        }

        public void EndEnemyTargetingHUD()
        {
            _battleHUD?.EndEnemyTargeting();
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Timing Feedback

        /// <summary>Hiển thị kết quả timing. Gọi từ TimingSystem.</summary>
        public void ShowTimingResult(TimingGrade grade)
        {
            _timingFeedbackUI?.ShowResult(grade);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Turn Routing

        /// <summary>
        /// Khi lượt của bất kỳ player character nào bắt đầu, re-initialize SkillButtonPanel
        /// với đúng skill của nhân vật đó.
        /// </summary>
        private void OnPlayerTurnStarted(TurnStartedEvent e)
        {
            if (!_entityMap.TryGetValue(e.EntityId, out var entity)) return;
            var character = entity as TTCS.Combat.Entities.Character;
            if (character == null)
            {
                // Lượt của enemy — ẩn panel
                _skillButtonPanel?.Hide();
                _battleHUD?.EndEnemyTargeting();
                HideCombatItemPanel();
                RefreshCombatItemButtonState();
                return;
            }

            _skillButtonPanel?.Initialize(character.ID, character.SkillIds, _enemyList, _allyList);
            _skillButtonPanel?.ShowForTurn();
            RefreshCombatItemButtonState();
        }

        private void EnsureCombatItemUI()
        {
            if (_combatItemButton != null && _combatItemPanel != null)
            {
                return;
            }

            var root = transform.parent as RectTransform;
            if (root == null)
            {
                return;
            }

            _combatItemButton = CreateActionButton(root, "ItemButton", "Item", new Vector2(-300f, 90f), OnCombatItemButtonClicked);
            _combatItemPanel = CreateItemPanel(root);
            HideCombatItemPanel();
        }

        private Button CreateActionButton(RectTransform parent, string objectName, string label, Vector2 anchoredPosition, Action onClick)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(180f, 56f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.16f, 0.23f, 0.96f);

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            button.colors = colors;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            CreateButtonLabel(buttonObject.transform, label);
            return button;
        }

        private GameObject CreateItemPanel(RectTransform parent)
        {
            var panelObject = new GameObject("CombatItemPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panelObject.transform.SetParent(parent, false);

            var panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(840f, 460f);

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);

            var layoutRoot = new GameObject("Layout", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            layoutRoot.transform.SetParent(panelObject.transform, false);
            var layoutRect = layoutRoot.GetComponent<RectTransform>();
            layoutRect.anchorMin = new Vector2(0f, 0f);
            layoutRect.anchorMax = new Vector2(1f, 1f);
            layoutRect.offsetMin = new Vector2(20f, 20f);
            layoutRect.offsetMax = new Vector2(-20f, -20f);

            var horizontal = layoutRoot.GetComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 20f;
            horizontal.childForceExpandHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childControlHeight = true;
            horizontal.childControlWidth = true;

            var listPanel = CreatePanelSection(layoutRoot.transform, "ListSection", 320f);
            var detailPanel = CreatePanelSection(layoutRoot.transform, "DetailSection", -1f);

            CreateTextBlock(listPanel.transform, "Title", "Items", 28, TextAlignmentOptions.TopLeft, Color.white, 16f, 16f, 16f, 34f);

            var scrollObject = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            scrollObject.transform.SetParent(listPanel.transform, false);
            var scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0f, 0f);
            scrollRectTransform.anchorMax = new Vector2(1f, 1f);
            scrollRectTransform.offsetMin = new Vector2(16f, 16f);
            scrollRectTransform.offsetMax = new Vector2(-16f, -52f);

            var scrollImage = scrollObject.GetComponent<Image>();
            scrollImage.color = new Color(1f, 1f, 1f, 0.04f);
            scrollObject.GetComponent<Mask>().showMaskGraphic = false;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(scrollObject.transform, false);
            _combatItemListContent = contentObject.GetComponent<RectTransform>();
            _combatItemListContent.anchorMin = new Vector2(0f, 1f);
            _combatItemListContent.anchorMax = new Vector2(1f, 1f);
            _combatItemListContent.pivot = new Vector2(0.5f, 1f);
            _combatItemListContent.anchoredPosition = Vector2.zero;
            _combatItemListContent.sizeDelta = new Vector2(0f, 0f);

            var listLayout = contentObject.GetComponent<VerticalLayoutGroup>();
            listLayout.spacing = 10f;
            listLayout.padding = new RectOffset(0, 0, 0, 0);
            listLayout.childControlHeight = false;
            listLayout.childControlWidth = true;
            listLayout.childForceExpandHeight = false;
            listLayout.childForceExpandWidth = true;

            var fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = scrollRectTransform;
            scrollRect.content = _combatItemListContent;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            _combatItemNameText = CreateTextBlock(detailPanel.transform, "ItemName", "Select an item", 30, TextAlignmentOptions.TopLeft, Color.white, 18f, 18f, 18f, 40f);
            _combatItemDescriptionText = CreateTextBlock(detailPanel.transform, "Description", "Choose an item to preview its combat effect.", 22, TextAlignmentOptions.TopLeft, new Color(0.9f, 0.95f, 1f, 0.95f), 18f, 76f, 18f, 120f);
            _combatItemInfoText = CreateTextBlock(detailPanel.transform, "Info", string.Empty, 20, TextAlignmentOptions.TopLeft, new Color(0.55f, 0.88f, 1f, 1f), 18f, 208f, 18f, 120f);

            _combatItemUseButton = CreateActionButton(detailPanel.transform as RectTransform, "UseButton", "Use", new Vector2(-18f, 18f), OnCombatItemUseClicked);
            var useRect = _combatItemUseButton.GetComponent<RectTransform>();
            useRect.anchorMin = new Vector2(1f, 0f);
            useRect.anchorMax = new Vector2(1f, 0f);
            useRect.pivot = new Vector2(1f, 0f);
            useRect.sizeDelta = new Vector2(160f, 52f);

            _combatItemCancelButton = CreateActionButton(detailPanel.transform as RectTransform, "CancelButton", "Cancel", new Vector2(-194f, 18f), HideCombatItemPanel);
            var cancelRect = _combatItemCancelButton.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(1f, 0f);
            cancelRect.anchorMax = new Vector2(1f, 0f);
            cancelRect.pivot = new Vector2(1f, 0f);
            cancelRect.sizeDelta = new Vector2(160f, 52f);

            return panelObject;
        }

        private GameObject CreatePanelSection(Transform parent, string objectName, float preferredWidth)
        {
            var section = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            section.transform.SetParent(parent, false);

            var image = section.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.05f);

            var layoutElement = section.GetComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
            }
            else
            {
                layoutElement.flexibleWidth = 1f;
            }

            return section;
        }

        private void CreateButtonLabel(Transform parent, string value)
        {
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = 26;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableAutoSizing = false;
            text.font = TMP_Settings.defaultFontAsset;
            text.raycastTarget = false;
        }

        private TMP_Text CreateTextBlock(Transform parent, string objectName, string value, int fontSize, TextAlignmentOptions alignment, Color color, float left, float top, float right, float height)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.enableAutoSizing = false;
            text.font = TMP_Settings.defaultFontAsset;
            text.raycastTarget = false;
            return text;
        }

        private void OnCombatItemButtonClicked()
        {
            var flow = CombatFlowController.Instance;
            if (_combatItemPanel == null || flow == null || !flow.IsPlayerTurn())
            {
                return;
            }

            RebuildCombatItemList();
            _combatItemPanel.SetActive(true);
            RefreshCombatItemButtonState();
        }

        private void HideCombatItemPanel()
        {
            if (_combatItemPanel != null)
            {
                _combatItemPanel.SetActive(false);
            }

            _selectedCombatItemId = null;
        }

        private void RebuildCombatItemList()
        {
            ClearSpawned(_spawnedCombatItemViews);
            _selectedCombatItemId = null;

            if (_combatItemListContent == null || _combatItemCellPrefab == null)
            {
                SetCombatItemDetail(null, "Combat item UI is not configured.");
                return;
            }

            var actor = CombatFlowController.Instance?.GetCurrentActor() as Character;
            var inventory = GetInventoryService();
            var items = inventory?.GetItems();
            if (actor == null || items == null || items.Count == 0)
            {
                SetCombatItemDetail(null, "No combat item available.");
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var stack = items[i];
                var itemData = DataManager.Instance?.LoadItem(stack.itemId);
                if (itemData == null)
                {
                    continue;
                }

                if (!inventory.CanUseItem(stack.itemId, "combat"))
                {
                    continue;
                }

                var cell = Instantiate(_combatItemCellPrefab, _combatItemListContent);
                var layout = cell.gameObject.GetComponent<LayoutElement>() ?? cell.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 96f;
                layout.minHeight = 96f;
                layout.flexibleWidth = 1f;

                var icon = LoadResourceSprite(itemData.iconPath);
                cell.Bind(icon, stack.quantity, itemData.rarity, stack.itemId, () => OnCombatItemSelected(stack.itemId));
                _spawnedCombatItemViews.Add(cell.gameObject);
            }

            if (_spawnedCombatItemViews.Count == 0)
            {
                SetCombatItemDetail(null, "No consumable can be used right now.");
                return;
            }

            var firstCell = _spawnedCombatItemViews[0].GetComponent<InventoryItemCellView>();
            OnCombatItemSelected(firstCell != null ? firstCell.ItemId : null);
        }

        private void OnCombatItemSelected(string itemId)
        {
            _selectedCombatItemId = itemId;
            for (var i = 0; i < _spawnedCombatItemViews.Count; i++)
            {
                var cell = _spawnedCombatItemViews[i].GetComponent<InventoryItemCellView>();
                if (cell != null)
                {
                    cell.SetSelected(string.Equals(cell.ItemId, itemId, StringComparison.Ordinal));
                }
            }

            SetCombatItemDetail(itemId, null);
        }

        private void SetCombatItemDetail(string itemId, string fallbackMessage)
        {
            var actor = CombatFlowController.Instance?.GetCurrentActor() as Character;
            var itemData = !string.IsNullOrWhiteSpace(itemId) ? DataManager.Instance?.LoadItem(itemId) : null;

            if (_combatItemNameText != null)
            {
                _combatItemNameText.text = itemData != null ? itemData.nameKey : "Select an item";
            }

            if (_combatItemDescriptionText != null)
            {
                _combatItemDescriptionText.text = itemData != null
                    ? BuildCombatItemDescription(itemData)
                    : "Choose an item from the list.";
            }

            var previewText = string.Empty;
            var canUse = actor != null && itemData != null && TryBuildCombatItemPreview(actor, itemData, out previewText);
            if (!canUse && string.IsNullOrWhiteSpace(previewText))
            {
                previewText = fallbackMessage ?? "This item cannot be used right now.";
            }

            if (_combatItemInfoText != null)
            {
                _combatItemInfoText.text = previewText;
            }

            if (_combatItemUseButton != null)
            {
                _combatItemUseButton.interactable = canUse;
            }
        }

        private static string BuildCombatItemDescription(ItemDataModel itemData)
        {
            if (itemData == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(itemData.description))
            {
                return itemData.description;
            }

            return $"Effect: {itemData.effectType} (+{Mathf.Max(itemData.effectAmount, itemData.healAmount)})";
        }

        private bool TryBuildCombatItemPreview(Character actor, ItemDataModel itemData, out string previewText)
        {
            previewText = string.Empty;
            if (actor == null || itemData == null)
            {
                return false;
            }

            var amount = Mathf.Max(itemData.effectAmount, itemData.healAmount);
            var effectType = (itemData.effectType ?? string.Empty).Trim().ToLowerInvariant();

            switch (effectType)
            {
                case "heal_hp":
                    if (actor.Health.CurrentHP >= actor.Health.MaxHP)
                    {
                        previewText = "HP is already full.";
                        return false;
                    }

                    previewText = $"After use: restore {amount} HP to the acting character and end this turn.";
                    return amount > 0;

                case "restore_energy":
                case "restore_mana":
                    var skillManager = SkillManager.Instance;
                    if (skillManager == null)
                    {
                        previewText = "Mana system is unavailable.";
                        return false;
                    }

                    if (skillManager.GetMana(actor.ID) >= skillManager.GetMaxMana(actor.ID))
                    {
                        previewText = "Mana is already full.";
                        return false;
                    }

                    previewText = $"After use: restore {amount} mana to the acting character and end this turn.";
                    return amount > 0;

                default:
                    previewText = $"Unsupported combat effect: {itemData.effectType}";
                    return false;
            }
        }

        private void OnCombatItemUseClicked()
        {
            if (string.IsNullOrWhiteSpace(_selectedCombatItemId))
            {
                return;
            }

            CombatFlowController.Instance?.SubmitPlayerItemUse(_selectedCombatItemId);
            if (_combatItemButton != null)
            {
                _combatItemButton.interactable = false;
            }
            HideCombatItemPanel();
            RefreshCombatItemButtonState();
        }

        private void RefreshCombatItemButtonState()
        {
            if (_combatItemButton == null)
            {
                return;
            }

            var flow = CombatFlowController.Instance;
            var actor = flow?.GetCurrentActor() as Character;
            var inventory = GetInventoryService();
            var canShow = flow != null
                && flow.IsPlayerTurn()
                && actor != null
                && HasAnyCombatItem(actor, inventory);

            _combatItemButton.gameObject.SetActive(actor != null);
            _combatItemButton.interactable = canShow;
        }

        private bool HasAnyCombatItem(Character actor, IInventoryService inventory)
        {
            var items = inventory?.GetItems();
            if (actor == null || items == null)
            {
                return false;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var itemData = DataManager.Instance?.LoadItem(items[i].itemId);
                if (itemData == null || !inventory.CanUseItem(items[i].itemId, "combat"))
                {
                    continue;
                }

                if (TryBuildCombatItemPreview(actor, itemData, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private static IInventoryService GetInventoryService()
        {
            var hub = MetaServiceHub.Instance;
            hub?.EnsureInitialized();
            return hub?.InventoryService;
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Combat End

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (_resultPanel == null) return;

            SetPaused(false);
            HideCombatItemPanel();
            RefreshCombatItemButtonState();

            _resultPanel.SetActive(true);
            ApplyResultVisual(e.Victory);

            _lastVictory = e.Victory;
            _lastLevelId = ResolveLevelIdForResult();

            WireResultButtons();
            RefreshVictoryRewards(e.Victory);
            RefreshResultTeamProgress();

            // Wire nút result lần đầu (tránh duplicate listener)
            if (_resultButton != null)
            {
                _resultButton.onClick.RemoveAllListeners();
                _resultButton.onClick.AddListener(OnResultButtonClicked);
            }

            if (_backHomeButton != null)
            {
                _backHomeButton.gameObject.SetActive(true);
                _backHomeButton.onClick.RemoveAllListeners();
                _backHomeButton.onClick.AddListener(OnBackHomeClicked);
            }

            // Fade in result panel
            var cg = _resultPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            }
        }

        private void ApplyResultVisual(bool victory)
        {
            _resultCardController?.ShowResult(victory);

            if (_resultButtonLabel != null)
                _resultButtonLabel.text = victory ? _victoryButtonLabel : _defeatButtonLabel;
        }

        private void WireResultButtons()
        {
            if (_resultButton != null)
            {
                _resultButton.gameObject.SetActive(true);
            }
        }

        private string ResolveLevelIdForResult()
        {
            if (!string.IsNullOrWhiteSpace(FlowRuntimeContext.SelectedLevelId))
            {
                return FlowRuntimeContext.SelectedLevelId;
            }

            var stage = CombatSceneManager.Instance?.GetCurrentStageData();
            var stageId = stage?.id ?? string.Empty;
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return string.Empty;
            }

            return DataManager.Instance?.ResolveLevelIdByStageId(stageId) ?? stageId;
        }

        private void RefreshVictoryRewards(bool victory)
        {
            ClearSpawned(_spawnedVictoryRewardViews);

            if (_victoryRewardContent == null || _victoryRewardItemViewPrefab == null)
            {
                return;
            }

            if (!victory)
            {
                return;
            }

            if (CombatSceneManager.Instance == null || !CombatSceneManager.Instance.TryGetLastGrantedRewards(out var gold, out var exp, out var items))
            {
                return;
            }

            var rewards = BuildRewardDisplayListFromGranted(gold, exp, items);
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                var view = Instantiate(_victoryRewardItemViewPrefab, _victoryRewardContent);
                _spawnedVictoryRewardViews.Add(view.gameObject);
                view.Bind(reward.Rarity, reward.Icon, reward.Label);
            }
        }

        private void RefreshResultTeamProgress()
        {
            ClearSpawned(_spawnedResultTeamProgressViews);

            if (_resultTeamProgressContent == null || _resultTeamProgressItemPrefab == null)
            {
                return;
            }

            List<string> ids = null;
            var snapshot = FlowRuntimeContext.SelectedLineupSnapshot;
            if (snapshot != null && snapshot.Count > 0)
            {
                ids = new List<string>(snapshot);
            }

            if (ids == null || ids.Count == 0)
            {
                var team = CombatFlowController.Instance?.GetPlayerTeam();
                if (team != null)
                {
                    ids = new List<string>();
                    for (var i = 0; i < team.Count; i++)
                    {
                        var id = team[i]?.CharacterId;
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            ids.Add(id);
                        }
                    }
                }
            }

            if (ids == null || ids.Count == 0)
            {
                return;
            }

            var spawned = new HashSet<string>();
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (!spawned.Add(id))
                {
                    continue;
                }

                var view = Instantiate(_resultTeamProgressItemPrefab, _resultTeamProgressContent);
                _spawnedResultTeamProgressViews.Add(view.gameObject);
                view.Bind(id);
            }
        }

        private void OnPauseClicked()
        {
            SetPaused(!_isPaused);
        }

        private void OnPauseContinueClicked()
        {
            SetPaused(false);
        }

        private void OnPauseRetryClicked()
        {
            SetPaused(false);
            OnRetryClicked();
        }

        private void OnPauseExitClicked()
        {
            SetPaused(false);

            var flow = FlowController.Instance;
            if (flow != null)
            {
                flow.OpenLevelSelect(FlowRuntimeContext.SelectedChapterId);
                return;
            }

            // Fallback
            SceneManager.LoadScene(0);
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;

            if (_pausePanel != null)
            {
                _pausePanel.SetActive(paused);
            }

            if (paused)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                return;
            }

            Time.timeScale = _previousTimeScale <= 0f ? 1f : _previousTimeScale;
        }

        private static void ClearSpawned(List<GameObject> targets)
        {
            if (targets == null)
            {
                return;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    Destroy(targets[i]);
                }
            }

            targets.Clear();
        }

        private static void ResizeHorizontalContent(RectTransform content, RectTransform prefabRect, int count, float spacing, float paddingLeftRight)
        {
            if (content == null)
            {
                return;
            }

            var itemWidth = prefabRect != null && prefabRect.rect.width > 0f ? prefabRect.rect.width : 140f;
            var effectiveCount = Mathf.Max(count, 0);
            var totalWidth = paddingLeftRight + effectiveCount * itemWidth + Mathf.Max(0, effectiveCount - 1) * spacing;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(totalWidth, 0f));
        }

        private static Sprite LoadResourceSprite(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return null;
            }

            var path = rawPath.Replace("\\", "/").Trim();
            if (path.StartsWith("Assets/Resources/", System.StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("Assets/Resources/".Length);
            }
            else if (path.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("Resources/".Length);
            }

            path = System.IO.Path.ChangeExtension(path, null)?.Replace("\\", "/") ?? path;
            return Resources.Load<Sprite>(path);
        }

        private static List<RewardDisplay> BuildRewardDisplayListFromGranted(int gold, int exp, IReadOnlyList<StageRewardItem> items)
        {
            var displays = new List<RewardDisplay>();

            if (exp > 0)
            {
                displays.Add(new RewardDisplay
                {
                    Label = $"Exp x{exp}",
                    Icon = Resources.Load<Sprite>("UI/Reward/icon_exp"),
                    Rarity = "common"
                });
            }

            if (gold > 0)
            {
                displays.Add(new RewardDisplay
                {
                    Label = $"Gold x{gold}",
                    Icon = Resources.Load<Sprite>("UI/Reward/icon_gold"),
                    Rarity = "common"
                });
            }

            if (items != null)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null || string.IsNullOrWhiteSpace(item.id))
                    {
                        continue;
                    }

                    var itemData = DataManager.Instance?.LoadItem(item.id);
                    displays.Add(new RewardDisplay
                    {
                        Label = $"{item.id} x{Mathf.Max(1, item.amount)}",
                        Icon = LoadResourceSprite(itemData?.iconPath),
                        Rarity = string.IsNullOrWhiteSpace(itemData?.rarity) ? "common" : itemData.rarity
                    });
                }
            }

            return displays;
        }

        private sealed class RewardDisplay
        {
            public string Label;
            public Sprite Icon;
            public string Rarity;
        }

        private void OnBackHomeClicked()
        {
            var flow = FlowController.Instance;
            if (flow != null)
            {
                flow.OpenMainMenu();
                return;
            }

            // Fallback
            SceneManager.LoadScene(0);
        }

        private void OnContinueClicked()
        {
            var flow = FlowController.Instance;
            if (flow != null)
            {
                // Rewards/progression are applied by combat flow. Continue simply returns to level select.
                flow.OpenLevelSelect(FlowRuntimeContext.SelectedChapterId);
                return;
            }

            // Fallback: return to level select if present, otherwise reload.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnRetryClicked()
        {
            var flow = FlowController.Instance;
            if (flow != null && !string.IsNullOrWhiteSpace(_lastLevelId))
            {
                var lineup = FlowRuntimeContext.SelectedLineupSnapshot;
                if (lineup == null)
                {
                    lineup = new List<string>();
                }

                flow.EnterCombat(_lastLevelId, lineup);
                return;
            }

            // Fallback: reload combat scene.
            var scene = SceneManager.GetActiveScene();
            var transition = FlowController.Instance != null
                ? FlowController.Instance.GetComponent<SceneTransitionController>()
                : null;
            if (transition != null)
            {
                transition.LoadScene(scene.name);
                return;
            }

            SceneManager.LoadScene(scene.buildIndex);
        }

        /// <summary>
        /// Xử lý nút trên result screen — reload scene hiện tại (Sprint 2).
        /// Ưu tiên đi qua FlowController/SceneTransitionController để có transition nhất quán.
        /// </summary>
        private void OnResultButtonClicked()
        {
            // Legacy single-button result screen fallback.
            if (_lastVictory)
            {
                OnContinueClicked();
            }
            else
            {
                OnRetryClicked();
            }
        }

        #endregion
    }
}
