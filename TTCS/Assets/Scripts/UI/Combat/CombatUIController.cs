using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TTCS.Combat.Entities;
using TTCS.Core.Events;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;
using TTCS.Combat.Timing;

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
        }

        // ─── Sub-Panel References ─────────────────────────────────────────
        [Header("Sub-Panels")]
        [SerializeField] private BattleHUD            _battleHUD;
        [SerializeField] private SkillButtonPanel     _skillButtonPanel;
        [SerializeField] private TurnOrderDisplay     _turnOrderDisplay;
        [SerializeField] private ActionResultDisplay  _actionResultDisplay;
        [SerializeField] private TimingFeedbackUI     _timingFeedbackUI;

        [Header("Result Screen")]
        [SerializeField] private GameObject          _resultPanel;
        [SerializeField] private TextMeshProUGUI     _resultText;
        [SerializeField] private Button              _resultButton;

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

            // Build entity map
            _entityMap.Clear();
            foreach (var e in playerTeam)  if (e != null) _entityMap[e.ID] = e;
            foreach (var e in enemyTeam)   if (e != null) _entityMap[e.ID] = e;

            // Register entity names với TurnOrderDisplay
            foreach (var entity in playerTeam)
                if (entity != null) _turnOrderDisplay?.RegisterEntity(entity.ID, entity.DisplayName, isPlayer: true);
            foreach (var entity in enemyTeam)
                if (entity != null) _turnOrderDisplay?.RegisterEntity(entity.ID, entity.DisplayName, isPlayer: false);

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
            TimingSystem.Instance.OnTimingResult += ShowTimingResult;

            EventBus.Instance.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Instance.Subscribe<TurnStartedEvent>(OnPlayerTurnStarted);

            Log("CombatUIController: UI initialized.", LogCategory.UI);
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

            _resultPanel.SetActive(true);
            if (_resultText != null)
                _resultText.text = e.Victory ? "VICTORY!" : "DEFEAT...";

            // Wire nút result lần đầu (tránh duplicate listener)
            if (_resultButton != null)
            {
                _resultButton.onClick.RemoveAllListeners();
                _resultButton.onClick.AddListener(OnResultButtonClicked);
            }

            // Fade in result panel
            var cg = _resultPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
        /// Xử lý nút trên result screen — reload scene hiện tại (Sprint 2).
        /// Khi Main Menu hoàn thiện (Sprint 3+), thay bằng SceneManager.LoadScene("MainMenu").
        /// </summary>
        private void OnResultButtonClicked()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }

        #endregion
    }
}
