using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Combat.Timing;
using TTCS.Core.Data;
using TTCS.Data;
using TTCS.Debugging;
using TTCS.UI.Combat;
using static TTCS.Debugging.DebugLogger;
using TTCS.Core.Events;
using UnityEngine.InputSystem;

namespace TTCS.Combat.Managers
{
    /// <summary>
    /// 🔵 Dev A - Combat Scene Manager
    /// Điều phối khởi tạo toàn bộ combat scene: entities, UI, audio, bridge.
    /// Thay thế CombatTestLoader cho production use.
    ///
    /// Pipeline:
    ///   InitializeCombat(stage) →
    ///     DataManager load stage →
    ///     EntityFactory tạo entities →
    ///     CombatBridge setup (Dev B đăng ký views) →
    ///     CombatUIController.Initialize() →
    ///     CombatFlowController.StartBattle()
    ///
    /// Setup trong Unity:
    ///   1. Thêm CombatSceneManager vào CombatScene.
    ///   2. Gán [SerializeField] references.
    ///   3. Gọi InitializeCombat(stageId) từ scene loader / menu.
    /// </summary>
    public class CombatSceneManager : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static CombatSceneManager _instance;
        public static CombatSceneManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─── Inspector ────────────────────────────────────────────────────
        [Header("Default Stage (for testing)")]
        [SerializeField] private string _defaultStageId = "stage_01_tutorial";
        [SerializeField] private int _defaultSeed = 0;
        [SerializeField] private bool _autoStartOnPlay = false;

        [Header("Default Party (for testing)")]
        [SerializeField]
        private List<string> _defaultPartyIds = new List<string>
        {
            "char_warrior",
            "char_mage"
        };

        // ─── Runtime State ────────────────────────────────────────────────
        private List<Character> _playerTeam = new();
        private List<Enemy> _enemyTeam = new();
        private StageDataModel _currentStage;

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Start()
        {

            if (_autoStartOnPlay)
                StartCoroutine(InitializeCombat(_defaultStageId, _defaultPartyIds, _defaultSeed));
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Entry point chính: khởi tạo toàn bộ combat từ stageId + partyIds.
        /// </summary>
        /// <param name="stageId">ID của stage (loads từ DataManager)</param>
        /// <param name="partyCharacterIds">Danh sách character ID của player</param>
        /// <param name="seed">RNG seed (0 = random)</param>
        public IEnumerator InitializeCombat(string stageId, List<string> partyCharacterIds, int seed = 0)
        {
            Log($"CombatSceneManager: Initializing stage '{stageId}'...", LogCategory.Combat);

            // ─── 1. Load stage data ───────────────────────────────────────
            yield return null; // Frame gap cho DataManager init nếu cần

            _currentStage = DataManager.Instance?.LoadStage(stageId);
            if (_currentStage == null)
            {
                Log($"CombatSceneManager: Stage '{stageId}' không tìm thấy — dùng wave rỗng.", LogCategory.Combat);
            }

            // ─── 2. Tạo entities ──────────────────────────────────────────
            _playerTeam = EntityFactory.CreateParty(partyCharacterIds);
            if (_playerTeam == null || _playerTeam.Count == 0)
            {
                Log("CombatSceneManager: Party rỗng!", LogCategory.Combat);
                yield break;
            }

            // Lấy wave đầu tiên từ stage, hoặc dùng default enemies
            var enemyIds = GetFirstWaveEnemyIds(_currentStage);
            _enemyTeam = EntityFactory.CreateWave(enemyIds);

            Log($"CombatSceneManager: {_playerTeam.Count} players, {_enemyTeam.Count} enemies.", LogCategory.Combat);

            // ─── 3. Pre-register entities vào SkillManager ───────────────
            // Cần làm trước khi HUD init để BattleHUD đọc được mana đúng (không bị 0)
            var allies = _playerTeam.Cast<CombatEntity>().ToList();
            var enemies = _enemyTeam.Cast<CombatEntity>().ToList();
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.ResetCombat();
                foreach (var e in allies.Concat(enemies))
                    SkillManager.Instance.RegisterEntity(e.ID);
            }

            // ─── 4. Khởi tạo CombatUIController ──────────────────────────
            CombatUIController.Instance?.Initialize(allies, enemies);

            // ─── 4. Chờ một frame để Developer B spawn CharacterViews ─────
            // Developer B sẽ spawn views và gọi CombatBridge.RegisterView() trong Start()
            // CombatSceneManager chờ để đảm bảo tất cả views đã đăng ký
            yield return null;

            // ─── 5. Start battle ──────────────────────────────────────────
            CombatFlowController.Instance?.StartBattle(_playerTeam, _enemyTeam, seed);

            Log("CombatSceneManager: Combat started.", LogCategory.Combat);
        }

        /// <summary>
        /// Overload đơn giản — dùng party mặc định từ inspector.
        /// </summary>
        public void StartDefaultCombat()
        {
            StartCoroutine(InitializeCombat(_defaultStageId, _defaultPartyIds, _defaultSeed));
        }

        /// <summary>
        /// Gọi khi combat kết thúc (từ CombatFlowController hoặc event).
        /// Hiển thị result và chuẩn bị transition.
        /// </summary>
        public void OnCombatComplete(bool victory)
        {
            Log($"CombatSceneManager: Combat complete — Victory={victory}.", LogCategory.Combat);
            // CombatUIController đã lắng nghe CombatEndedEvent và hiển thị result panel
            // Future: scene transition, reward screen, etc.
        }

        void Update()
        {
            if (Keyboard.current[Key.H].wasPressedThisFrame)
            {
                // Gọi TakeDamage() trực tiếp: HealthComponent cập nhật _currentHP rồi mới publish event
                // → BattleHUD đọc GetEntityHPPercent() sẽ thấy giá trị đã thay đổi
                CombatEntity target = _playerTeam.Find(e => e.ID == "char_warrior");
                if (target == null) target = _enemyTeam.Find(e => e.ID == "char_warrior");
                target?.Health.TakeDamage(500, "enemy_goblin");
            }
            if (Keyboard.current[Key.Y].wasPressedThisFrame)
            {
                CombatEntity target = _playerTeam.Find(e => e.ID == "char_warrior");
                if (target == null) target = _enemyTeam.Find(e => e.ID == "char_warrior");
                target?.Health.Heal(300, "char_mage");
            }
            if (Keyboard.current[Key.V].wasPressedThisFrame)
                EventBus.Instance.Publish(new TTCS.Core.Events.CombatEndedEvent(victory: true));

            // ── Week 2 Timing Tests ──────────────────────────────────────
            // T: Mở timing window 2 giây (sau đó nhấn Space để register input)
            if (Keyboard.current[Key.T].wasPressedThisFrame)
            {
                var window = TTCS.Combat.Timing.TimingWindow.CreateDefault(Time.time, 2.0f);
                
                TTCS.Combat.Timing.TimingSystem.Instance?.OpenWindow(window);
            }
            // F/G/M: Test trực tiếp TimingFeedbackUI (không cần qua TimingSystem)
            if (Keyboard.current[Key.F].wasPressedThisFrame)
                CombatUIController.Instance?.ShowTimingResult(TTCS.UI.Combat.TimingGrade.Perfect);
            if (Keyboard.current[Key.G].wasPressedThisFrame)
                CombatUIController.Instance?.ShowTimingResult(TTCS.UI.Combat.TimingGrade.Good);
            if (Keyboard.current[Key.M].wasPressedThisFrame)
                CombatUIController.Instance?.ShowTimingResult(TTCS.UI.Combat.TimingGrade.Miss);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Helpers

        public List<string> GetFirstWaveEnemyIds(StageDataModel stage)
        {
            if (stage?.encounters == null || stage.encounters.Count == 0)
            {
                // Fallback cho testing
                return new List<string> { "enemy_goblin", "enemy_goblin" };
            }

            var firstWave = stage.encounters[0];
            if (firstWave?.enemies == null || firstWave.enemies.Count == 0)
                return new List<string> { "enemy_goblin" };

            var ids = new List<string>();
            foreach (var e in firstWave.enemies)
                if (!string.IsNullOrEmpty(e?.enemyId))
                    ids.Add(e.enemyId);

            return ids;
        }


        #endregion
    }
}
