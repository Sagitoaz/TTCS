using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TTCS.Combat.Entities;
using TTCS.Core.Events;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

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

            // SkillButtonPanel — khởi tạo cho player đầu tiên còn sống
            var firstPlayer = playerTeam.Find(p => p != null && !p.IsDead);
            if (firstPlayer != null)
            {
                var character = firstPlayer as TTCS.Combat.Entities.Character;
                _skillButtonPanel?.Initialize(
                    firstPlayer.ID,
                    character?.SkillIds ?? new List<string>(),
                    enemyTeam
                );
            }

            // Ẩn result panel
            _resultPanel?.SetActive(false);

            EventBus.Instance.Subscribe<CombatEndedEvent>(OnCombatEnded);

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
        #region Combat End

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (_resultPanel == null) return;

            _resultPanel.SetActive(true);
            if (_resultText != null)
                _resultText.text = e.Victory ? "VICTORY!" : "DEFEAT...";

            // Fade in result panel
            var cg = _resultPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            }
        }

        #endregion
    }
}
