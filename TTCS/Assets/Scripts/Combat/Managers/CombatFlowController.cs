using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TTCS.Combat.Actions;
using TTCS.Combat.AI;
using TTCS.Combat.Entities;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Core.Utilities;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;
// CombatLogger is in TTCS.Combat — alias to avoid confusion with Unity.Debug
using CombatLogger = TTCS.Combat.CombatLogger;
using TTCS.Combat.Timing;  // TimingSystem, TimingWindow
using TTCS.UI.Combat;      // TimingGrade

namespace TTCS.Combat.Managers
{
    /// <summary>
    /// 🟢 Dev B - Combat Flow Controller (State Machine)
    /// MonoBehaviour Singleton điều phối toàn bộ vòng chiến đấu.
    ///
    /// Combat Loop:
    ///   StartBattle → Init → [GetNextActor → StartTurn → PlayerTurn/EnemyTurn
    ///                         → ExecuteAction → EndTurn → CheckVictory] → BattleEnd
    ///
    /// Player Integration:
    ///   - Khi đến lượt player, CFC chờ UI gọi SubmitPlayerAction().
    ///   - UI gọi: CombatFlowController.Instance.SubmitPlayerAction("skill_id", targetIds)
    ///
    /// Setup trong Unity:
    ///   1. Đặt CombatFlowController GameObject vào CombatScene.
    ///   2. Gán vào TurnManager và SkillManager (cùng scene).
    ///   3. Gọi StartBattle() từ CombatSceneLoader sau khi tạo entities.
    /// </summary>
    public class CombatFlowController : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static CombatFlowController _instance;
        public static CombatFlowController Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─── Combat State ─────────────────────────────────────────────────
        private enum CombatState
        {
            Idle,
            Initializing,
            PlayerTurn,
            EnemyTurn,
            ExecutingAction,
            PostAction,
            CheckVictory,
            BattleEnd
        }

        private CombatState _state = CombatState.Idle;

        /// <summary>True khi combat đang diễn ra (không phải Idle hoặc BattleEnd)</summary>
        public bool IsBattleActive =>
            _state != CombatState.Idle && _state != CombatState.BattleEnd;

        // ─── Entity References ────────────────────────────────────────────
        private List<Character> _playerTeam = new List<Character>();
        private List<Enemy> _enemyTeam = new List<Enemy>();
        private List<CombatEntity> _allEntities = new List<CombatEntity>();

        private CombatEntity _currentActor;

        // ─── Player Input ─────────────────────────────────────────────────
        private bool _playerInputReceived;
        private string _pendingSkillId;
        private List<string> _pendingTargetIds;

        // ─── Battle Metadata ─────────────────────────────────────────────
        public int CurrentSeed { get; private set; }
        public int TurnNumber => TurnManager.Instance?.TurnCounter ?? 0;

        // ─── Timing ───────────────────────────────────────────────────────
        [Header("Turn Timing")]
        [Tooltip("Thời gian dừng sau mỗi hành động (giây) — cho UI animation")]
        [SerializeField] private float _actionDelay = 0.5f;

        [Tooltip("Thời gian dừng giữa các lượt")]
        [SerializeField] private float _betweenTurnDelay = 0.2f;

        [Tooltip("Thời gian chờ tối đa player input (giây, 0 = vô hạn)")]
        [SerializeField] private float _playerTurnTimeout = 0f;

        [Header("Guard Timing Window")]
        [Tooltip("Duration của timing window khi enemy tấn công (giây)")]
        [SerializeField] private float _guardWindowDuration = 1.5f;

        [Tooltip("Perfect threshold (ms) — input trong khoảng này = Perfect")]
        [SerializeField] private float _perfectThresholdMs = 500f;

        [Tooltip("Good threshold (ms)")]
        [SerializeField] private float _goodThresholdMs = 800f;

        // ─── Entry Point ──────────────────────────────────────────────────
        /// <summary>
        /// Bắt đầu trận chiến.
        /// Gọi từ CombatSceneLoader sau khi đã tạo entities qua EntityFactory.
        /// </summary>
        /// <param name="players">Danh sách nhân vật player (tối đa 3)</param>
        /// <param name="enemies">Danh sách enemy trong wave đầu tiên</param>
        /// <param name="seed">RNG seed; 0 = random</param>
        public void StartBattle(List<Character> players, List<Enemy> enemies, int seed = 0)
        {
            if (IsBattleActive)
            {
                Log("CombatFlowController: Battle already active — ignoring StartBattle call.", LogCategory.Combat);
                return;
            }

            if (players == null || players.Count == 0)
            {
                Log("CombatFlowController: No player characters provided!", LogCategory.Combat);
                return;
            }

            _playerTeam = players.Where(p => p != null).ToList();
            _enemyTeam = enemies?.Where(e => e != null).ToList() ?? new List<Enemy>();

            CurrentSeed = seed > 0 ? seed : UnityEngine.Random.Range(1, int.MaxValue);

            StartCoroutine(CombatLoop());
        }

        // ─── Player Input API ─────────────────────────────────────────────
        /// <summary>
        /// Gọi từ UI khi player chọn skill + target.
        /// Chỉ có hiệu lực khi đang trong trạng thái PlayerTurn.
        /// </summary>
        public void SubmitPlayerAction(string skillId, List<string> targetIds)
        {
            if (_state != CombatState.PlayerTurn)
            {
                Log("SubmitPlayerAction called outside PlayerTurn state — ignored.", LogCategory.Combat);
                return;
            }

            _pendingSkillId = skillId;
            _pendingTargetIds = new List<string>(targetIds ?? new List<string>());
            _playerInputReceived = true;
        }

        /// <summary>
        /// Skip lượt player — dùng basic attack vào enemy đầu tiên còn sống.
        /// Tiện dụng cho debug hoặc AI-assist mode.
        /// </summary>
        public void SkipPlayerTurn()
        {
            if (_state != CombatState.PlayerTurn || !(_currentActor is Character c)) return;

            string skillId = c.SkillIds.Count > 0 ? c.SkillIds[0] : "";
            var targets = GetAliveEntityIds(_enemyTeam.Cast<CombatEntity>().ToList(), max: 1);
            SubmitPlayerAction(skillId, targets);
        }

        // ─── Combat Loop Coroutine ─────────────────────────────────────────
        private IEnumerator CombatLoop()
        {
            yield return InitBattle();

            while (true)
            {
                // Check victory/defeat BEFORE getting next actor
                if (CheckVictory() || CheckDefeat()) break;

                // Get next actor from timeline
                string actorId = TurnManager.Instance?.GetNextActor();
                if (actorId == null)
                {
                    Log("CombatFlowController: GetNextActor returned null — ending battle.", LogCategory.Combat);
                    break;
                }

                _currentActor = FindEntity(actorId);
                if (_currentActor == null)
                {
                    Log($"CombatFlowController: Entity '{actorId}' not found — skipping.", LogCategory.Combat);
                    continue;
                }

                // Skip dead entity
                if (_currentActor.IsDead)
                {
                    TurnManager.Instance.RemoveEntity(actorId);
                    continue;
                }

                // ── Start Turn ─────────────────────────────────────────
                _state = _currentActor.IsPlayer ? CombatState.PlayerTurn : CombatState.EnemyTurn;
                TurnManager.Instance?.StartTurn(actorId);
                _currentActor.OnTurnStart();

                CombatLogger.LogTurnStart(TurnNumber, actorId);

                // Check stun
                bool isStunned = _currentActor.Effects.HasEffect("stun");

                int timelineCost = 100; // default Normal

                if (isStunned)
                {
                    Log($"CombatFlowController: '{actorId}' is stunned — skipping turn.", LogCategory.Combat);
                    CombatLogger.LogAction(TurnNumber, actorId, "STUN", "Skipped — stunned");
                }
                else
                {
                    // ── Execute Turn ─────────────────────────────────────
                    if (_currentActor.IsPlayer)
                        yield return PlayerTurnRoutine(_currentActor as Character);
                    else
                        yield return EnemyTurnRoutine(_currentActor as Enemy);

                    timelineCost = _lastActionCost;
                }

                // ── End Turn ───────────────────────────────────────────
                _currentActor.OnTurnEnd();
                SkillManager.Instance?.TickCooldowns(actorId);
                TurnManager.Instance?.EndTurn(actorId, timelineCost);

                CombatLogger.LogTurnEnd(TurnNumber, actorId, timelineCost);

                // Remove newly-dead entities from timeline
                CleanupDeadEntities();

                yield return new WaitForSeconds(_betweenTurnDelay);
            }

            // ── Battle End ──────────────────────────────────────────────
            bool victory = CheckVictory();
            yield return EndBattle(victory);
        }

        // ─── Battle Initialization ────────────────────────────────────────
        private IEnumerator InitBattle()
        {
            _state = CombatState.Initializing;

            // Initialize RNG seed
            RNGService.Instance.Initialize(CurrentSeed);
            Log($"CombatFlowController: Battle init with seed={CurrentSeed}", LogCategory.Combat);

            // Build combined entity list
            _allEntities.Clear();
            _allEntities.AddRange(_playerTeam.Cast<CombatEntity>());
            _allEntities.AddRange(_enemyTeam.Cast<CombatEntity>());

            // Register all entities with TurnManager
            TurnManager.Instance?.ResetCombat();
            foreach (var entity in _allEntities)
            {
                TurnManager.Instance?.RegisterEntity(entity.ID, entity.SPD);
            }

            // Register all entities with SkillManager
            SkillManager.Instance?.ResetCombat();
            foreach (var entity in _allEntities)
            {
                SkillManager.Instance?.RegisterEntity(entity.ID);
            }

            // Publish combat started event
            EventBus.Instance.Publish(new CombatStartedEvent(CurrentSeed));

            Log($"CombatFlowController: {_playerTeam.Count} players, {_enemyTeam.Count} enemies ready.",
                LogCategory.Combat);

            yield return new WaitForSeconds(0.1f);
        }

        // ─── Player Turn ──────────────────────────────────────────────────
        private int _lastActionCost = 100;
        private TimingGrade _pendingTimingGrade = TimingGrade.Miss;

        private IEnumerator PlayerTurnRoutine(Character player)
        {
            _state = CombatState.PlayerTurn;
            _playerInputReceived = false;
            _pendingSkillId = null;
            _pendingTargetIds = null;

            Log($"CombatFlowController: Player turn — '{player.ID}' waiting for input.", LogCategory.Combat);

            // Wait for UI input (with optional timeout)
            float elapsed = 0f;
            while (!_playerInputReceived)
            {
                if (_playerTurnTimeout > 0f && elapsed >= _playerTurnTimeout)
                {
                    // Auto-skip: basic attack first enemy
                    SkipPlayerTurn();
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Execute player action
            if (!string.IsNullOrEmpty(_pendingSkillId))
            {
                yield return ExecuteAction(player, _pendingSkillId, _pendingTargetIds, TimingGrade.Miss);
            }
        }

        // ─── Enemy Turn ───────────────────────────────────────────────────
        private IEnumerator EnemyTurnRoutine(Enemy enemy)
        {
            _state = CombatState.EnemyTurn;

            Log($"CombatFlowController: Enemy turn — '{enemy.ID}' deciding...", LogCategory.Combat);

            yield return new WaitForSeconds(0.3f); // Slight AI "thinking" delay

            // Build snapshots for AIController
            var snapshots = BuildSnapshots();

            // AI decision
            var decision = AIController.DecideAction(
                enemy.ToSnapshot(),
                snapshots,
                enemy.Behavior,
                SkillManager.Instance);

            // ── Fallback: use moveSet skill list when AIBehavior asset not assigned ─
            if (!decision.IsValid && enemy.SkillIds.Count > 0)
            {
                var alivePlayers = GetAliveEntityIds(_playerTeam.Cast<CombatEntity>().ToList(), max: int.MaxValue);
                foreach (var fallbackSkillId in enemy.SkillIds)
                {
                    if (string.IsNullOrEmpty(fallbackSkillId)) continue;
                    if (SkillManager.Instance != null &&
                        !SkillManager.Instance.CanUseSkill(enemy.ID, fallbackSkillId)) continue;

                    var fSkillData = DataManager.Instance?.LoadSkill(fallbackSkillId);
                    if (fSkillData == null) continue;

                    List<string> fTargets;
                    if (fSkillData.targetRule?.type == "self")
                        fTargets = new List<string> { enemy.ID };
                    else if (fSkillData.targetRule?.type == "all_enemies")
                        fTargets = alivePlayers;
                    else
                        fTargets = GetAliveEntityIds(_playerTeam.Cast<CombatEntity>().ToList(), max: 1);

                    if (fTargets.Count == 0) continue;

                    decision = new AIDecision(fallbackSkillId, fTargets, "moveSet fallback (no AIBehavior)");
                    Log($"CombatFlowController: '{enemy.ID}' moveSet fallback → '{fallbackSkillId}'",
                        LogCategory.Combat);
                    break;
                }
            }

            if (decision.IsValid)
            {
                Log($"CombatFlowController: AI '{enemy.ID}' → skill='{decision.skillId}' reason: {decision.reason}",
                    LogCategory.Combat);
                // Mở timing window cho player guard nếu skill là attack
                var skillData = DataManager.Instance?.LoadSkill(decision.skillId);
                bool isAttack = skillData?.type == "attack";

                if (isAttack && TimingSystem.Instance != null)
                {
                    _pendingTimingGrade = TimingGrade.Miss; // reset
                    bool gradeReceived = false;

                    void OnGrade(TimingGrade grade)
                    {
                        _pendingTimingGrade = grade;
                        gradeReceived = true;
                    }

                    TimingSystem.Instance.OnTimingResult += OnGrade;

                    var window = new TimingWindow(
                        openTime: Time.time,
                        duration: _guardWindowDuration,
                        perfectThreshold: _perfectThresholdMs,
                        goodThreshold: _goodThresholdMs);

                    TimingSystem.Instance.OpenWindow(window);

                    // Chờ grade (window tự đóng sau Duration)
                    yield return new WaitUntil(() => gradeReceived);

                    TimingSystem.Instance.OnTimingResult -= OnGrade;

                    Log($"CombatFlowController: Guard grade = {_pendingTimingGrade}", LogCategory.Combat);
                }
                else
                {
                    _pendingTimingGrade = TimingGrade.Miss;
                }

                yield return ExecuteAction(enemy, decision.skillId, decision.targetIds, _pendingTimingGrade);
            }
            else
            {
                Log($"CombatFlowController: AI '{enemy.ID}' has no valid action — skipping.", LogCategory.Combat);
                CombatLogger.LogAction(TurnManager.Instance.TurnCounter, enemy.ID, "SKIP", "No valid AI action");
            }
        }

        // ─── Execute Action ───────────────────────────────────────────────
        private IEnumerator ExecuteAction(CombatEntity actor, string skillId, List<string> targetIds, TimingGrade guard = TimingGrade.Miss)
        {
            _state = CombatState.ExecutingAction;

            // Load skill data from DataManager
            var skillData = DataManager.Instance?.LoadSkill(skillId);
            if (skillData == null)
            {
                Log($"ExecuteAction: skill '{skillId}' not found in DataManager.", LogCategory.Combat);
                _lastActionCost = 100;
                yield break;
            }

            // Resolve target entities from IDs
            var targets = ResolveTargets(targetIds);
            if (targets.Count == 0)
            {
                Log($"ExecuteAction: no valid targets for skill '{skillId}'.", LogCategory.Combat);
                _lastActionCost = skillData.actionCost?.timelineUnits > 0
                    ? skillData.actionCost.timelineUnits : 100;
                yield break;
            }

            // Create and validate action
            var action = new SkillAction(skillData);
            var validation = action.Validate(actor, targets, SkillManager.Instance);

            if (!validation.IsValid)
            {
                Log($"ExecuteAction: Validation failed — {validation.FailReason}", LogCategory.Combat);
                _lastActionCost = action.TimelineCost;
                yield break;
            }

            // Execute
            action.Execute(actor, targets, SkillManager.Instance, guard);
            _lastActionCost = action.TimelineCost;

            CombatLogger.LogAction(TurnNumber, actor.ID, skillId,
                $"targets: {string.Join(", ", targets.Select(t => t.ID))}");

            yield return new WaitForSeconds(_actionDelay);
        }

        // ─── Victory / Defeat ──────────────────────────────────────────────
        private bool CheckVictory() =>
            _enemyTeam.Count > 0 && _enemyTeam.All(e => e.IsDead);

        private bool CheckDefeat() =>
            _playerTeam.Count > 0 && _playerTeam.All(p => p.IsDead);

        private IEnumerator EndBattle(bool victory)
        {
            _state = CombatState.BattleEnd;

            string result = victory ? "VICTORY" : "DEFEAT";
            Log($"CombatFlowController: Battle ended — {result}", LogCategory.Combat);
            CombatLogger.LogCombatResult(victory, TurnNumber);

            EventBus.Instance.Publish(new CombatEndedEvent(victory));

            yield return new WaitForSeconds(0.5f);

            _state = CombatState.Idle;
        }

        // ─── Helpers ──────────────────────────────────────────────────────
        private CombatEntity FindEntity(string id) =>
            _allEntities.Find(e => e.ID == id);

        private List<CombatEntity> ResolveTargets(List<string> targetIds)
        {
            var targets = new List<CombatEntity>();
            if (targetIds == null) return targets;

            foreach (var id in targetIds)
            {
                var entity = FindEntity(id);
                if (entity != null && !entity.IsDead)
                    targets.Add(entity);
            }
            return targets;
        }

        private List<CombatEntitySnapshot> BuildSnapshots()
        {
            return _allEntities
                .Where(e => !e.IsDead)
                .Select(e => e.ToSnapshot())
                .ToList();
        }

        private List<string> GetAliveEntityIds(List<CombatEntity> pool, int max = 1)
        {
            return pool.Where(e => !e.IsDead)
                       .Take(max)
                       .Select(e => e.ID)
                       .ToList();
        }

        private void CleanupDeadEntities()
        {
            foreach (var entity in _allEntities.Where(e => e.IsDead).ToList())
            {
                TurnManager.Instance?.RemoveEntity(entity.ID);
                SkillManager.Instance?.UnregisterEntity(entity.ID);
            }
        }

        // ─── Query API ────────────────────────────────────────────────────
        public List<Character> GetPlayerTeam() => new List<Character>(_playerTeam);
        public List<Enemy> GetEnemyTeam() => new List<Enemy>(_enemyTeam);
        public List<CombatEntity> GetAllEntities() => new List<CombatEntity>(_allEntities);
        public CombatEntity GetCurrentActor() => _currentActor;
        public bool IsPlayerTurn() => _state == CombatState.PlayerTurn;
    }
}
