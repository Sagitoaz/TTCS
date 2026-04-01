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
using TTCS.Visual;

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
            if (_instance == this)
            {
                _instance = null;
                ClearSpawnedViews();
            }
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

        [Header("Enemy Data Assets")]
        [Tooltip("Kéo các EnemyData ScriptableObject vào đây. AIBehavior sẽ được lấy từ từng asset.")]
        [SerializeField] private List<EnemyData> _enemyDataAssets = new List<EnemyData>();

        [Header("Visual Spawn")]
        [SerializeField] private bool _spawnViewsOnInitialize = true;
        [SerializeField] private List<Transform> _playerSlots = new List<Transform>();
        [SerializeField] private List<Transform> _enemySlots = new List<Transform>();

        // ─── Runtime State ────────────────────────────────────────────────
        private List<Character> _playerTeam = new();
        private List<Enemy> _enemyTeam = new();
        private StageDataModel _currentStage;
        private readonly List<GameObject> _spawnedViews = new List<GameObject>();

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

            // Gán AIBehavior từ EnemyData ScriptableObject cho từng enemy
            foreach (var enemy in _enemyTeam)
            {
                var data = _enemyDataAssets.FirstOrDefault(d => d != null && d.id == enemy.EnemyTemplateId);
                if (data != null && data.aiBehavior != null)
                    enemy.Behavior = data.aiBehavior;
            }

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
            SpawnAndRegisterViews();
            yield return null;

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
                target?.Health.TakeDamage(500, "enemy_bandit");
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
                CombatUIController.Instance?.ShowTimingResult(TimingGrade.Perfect);
            if (Keyboard.current[Key.G].wasPressedThisFrame)
                CombatUIController.Instance?.ShowTimingResult(TimingGrade.Good);
            if (Keyboard.current[Key.M].wasPressedThisFrame)
                CombatUIController.Instance?.ShowTimingResult(TimingGrade.Miss);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Helpers

        private void SpawnAndRegisterViews()
        {
            if (!_spawnViewsOnInitialize)
                return;

            ClearSpawnedViews();
            ActionAnimationController.Instance?.ClearRegistry();

            for (int i = 0; i < _playerTeam.Count; i++)
            {
                var entity = _playerTeam[i];
                if (entity == null) continue;

                var model = DataManager.Instance?.LoadCharacter(entity.CharacterId);
                var slot = ResolveSlotTransform(_playerSlots, i);
                if (slot == null)
                    Log($"CombatSceneManager: Missing Player Slot index {i}, using fallback world position.", LogCategory.Combat);
                var view = CharacterViewFactory.CreateCharacterView(model, slot);
                if (view == null) continue;

                view.EntityId = entity.ID;
                if (slot != null)
                    view.transform.localPosition = Vector3.zero;
                else
                    view.transform.position = ResolveFallbackWorldPosition(i, isPlayer: true);
                _spawnedViews.Add(view.gameObject);

                RegisterViewBindings(entity.ID, view, isPlayer: true);
            }

            for (int i = 0; i < _enemyTeam.Count; i++)
            {
                var entity = _enemyTeam[i];
                if (entity == null) continue;

                var model = DataManager.Instance?.LoadEnemy(entity.EnemyTemplateId);
                var slot = ResolveSlotTransform(_enemySlots, i);
                if (slot == null)
                    Log($"CombatSceneManager: Missing Enemy Slot index {i}, using fallback world position.", LogCategory.Combat);
                var view = CharacterViewFactory.CreateEnemyView(model, slot);
                if (view == null) continue;

                view.EntityId = entity.ID;
                if (slot != null)
                    view.transform.localPosition = Vector3.zero;
                else
                    view.transform.position = ResolveFallbackWorldPosition(i, isPlayer: false);
                _spawnedViews.Add(view.gameObject);

                RegisterViewBindings(entity.ID, view, isPlayer: false);
            }
        }

        private void RegisterViewBindings(string entityId, CharacterView view, bool isPlayer)
        {
            if (string.IsNullOrEmpty(entityId) || view == null)
                return;

            if (isPlayer)
                ActionAnimationController.Instance?.RegisterCharacterView(entityId, view);
            else if (view is EnemyView enemyView)
                ActionAnimationController.Instance?.RegisterEnemyView(entityId, enemyView);

            CombatBridge.Instance?.RegisterView(entityId, view);
            CombatUIController.Instance?.RegisterEntityPosition(entityId, view.transform);
        }

        private void ClearSpawnedViews()
        {
            for (int i = 0; i < _spawnedViews.Count; i++)
            {
                if (_spawnedViews[i] != null)
                    Destroy(_spawnedViews[i]);
            }
            _spawnedViews.Clear();
        }

        private static Transform ResolveSlotTransform(List<Transform> slots, int index)
        {
            if (slots == null || slots.Count == 0) return null;
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        private Vector3 ResolveFallbackWorldPosition(int index, bool isPlayer)
        {
            if (isPlayer)
            {
                var legacyAnchor = GameObject.Find("PlayerA");
                if (legacyAnchor != null) return legacyAnchor.transform.position + new Vector3(index * 1.6f, 0f, 0f);
                return new Vector3(-3.5f + (index * 1.6f), -1.45f, 0f);
            }

            var namedSlot = GameObject.Find($"EnemySlot_{index}");
            if (namedSlot != null) return namedSlot.transform.position;
            return new Vector3(2.4f + (index * 1.35f), 0.55f, 0f);
        }

        public List<string> GetFirstWaveEnemyIds(StageDataModel stage)
        {
            if (stage?.encounters == null || stage.encounters.Count == 0)
            {
                // Fallback cho testing
                return new List<string> { "enemy_bandit", "enemy_bandit" };
            }

            var firstWave = stage.encounters[0];
            if (firstWave?.enemies == null || firstWave.enemies.Count == 0)
                return new List<string> { "enemy_bandit" };

            var ids = new List<string>();
            foreach (var e in firstWave.enemies)
                if (!string.IsNullOrEmpty(e?.enemyId))
                    ids.Add(e.enemyId);

            return ids;
        }

        public StageDataModel GetCurrentStageData()
        {
            return _currentStage;
        }


        #endregion
    }
}
