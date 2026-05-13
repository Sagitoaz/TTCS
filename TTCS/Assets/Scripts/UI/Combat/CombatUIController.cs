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
using TTCS.Flow.LevelSelect;
using TTCS.Data;

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
                return;
            }

            _skillButtonPanel?.Initialize(character.ID, character.SkillIds, _enemyList, _allyList);
            _skillButtonPanel?.ShowForTurn();
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Combat End

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (_resultPanel == null) return;

            SetPaused(false);

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
                ResizeHorizontalContent(_victoryRewardContent, _victoryRewardItemViewPrefab.GetComponent<RectTransform>(), 0, _victoryRewardSpacing, _victoryRewardPaddingLeftRight);
                return;
            }

            if (CombatSceneManager.Instance == null || !CombatSceneManager.Instance.TryGetLastGrantedRewards(out var gold, out var exp, out var items))
            {
                ResizeHorizontalContent(_victoryRewardContent, _victoryRewardItemViewPrefab.GetComponent<RectTransform>(), 0, _victoryRewardSpacing, _victoryRewardPaddingLeftRight);
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

            ResizeHorizontalContent(_victoryRewardContent, _victoryRewardItemViewPrefab.GetComponent<RectTransform>(), rewards.Count, _victoryRewardSpacing, _victoryRewardPaddingLeftRight);
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
