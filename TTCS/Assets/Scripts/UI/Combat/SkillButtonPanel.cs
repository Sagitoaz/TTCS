using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Data;
using TTCS.Debugging;
using TTCS.Visual;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Skill Button Panel
    /// Panel chứa 4 SkillButton cho player turn.
    /// Hiện ra khi player turn bắt đầu, ẩn khi kết thúc.
    ///
    /// Subscribe: TurnStartedEvent, TurnEndedEvent
    /// Gọi: CombatFlowController.Instance.SubmitPlayerAction()
    /// </summary>
    public class SkillButtonPanel : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private SkillButton[] _buttons = new SkillButton[4];
        [SerializeField] private CanvasGroup   _canvasGroup;

        // ─── State ────────────────────────────────────────────────────────
        private string             _currentEntityId;
        private List<string>       _currentSkillIds = new();
        private List<SkillDataModel> _skillModels   = new();

        /// <summary>Phe enemy còn sống — dùng để chọn target cho skill lên enemy.</summary>
        private List<CombatEntity> _enemies = new();

        /// <summary>Phe ally còn sống — dùng cho buff/heal single_ally.</summary>
        private List<CombatEntity> _allies = new();

        // ─── Manual Target Selection State ─────────────────────────────
        private bool _isSelectingTarget;
        private bool _isSelectingEnemyTarget;
        private string _pendingSkillId;
        private List<CombatEntity> _targetCandidates = new();
        private int _currentTargetIndex;

        // ─── Input ──────────────────────────────────────────────────────
        private PlayerInput _playerInput;
        private InputAction _previousAction;
        private InputAction _nextAction;
        private InputAction _submitAction;
        private InputAction _cancelAction;

        // ─── Camera Focus ───────────────────────────────────────────────
        [Header("Target Selection Camera")]
        [SerializeField] private bool _enableSelectionCameraFocus = true;
        [SerializeField] private float _cameraFocusLerpSpeed = 10f;
        [SerializeField] private float _cameraSelectionSize = 3.5f;
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 0f, -10f);

        private Camera _mainCamera;
        private bool _hasCachedCameraState;
        private Vector3 _cameraOriginalPosition;
        private float _cameraOriginalSize;
        private Vector3 _cameraTargetPosition;
        private float _cameraTargetSize;

        // ─── Accessibility Hint ────────────────────────────────────────
        [Header("Target Selection UI")]
        [SerializeField] private bool _showSelectionHint = true;

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            SetVisible(false);
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<TurnEndedEvent>(OnTurnEnded);
            BindInputActions();
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            UnbindInputActions();
            ExitTargetSelectionMode(resetCamera: true, restoreButtons: false);
        }

        private void Update()
        {
            if (_isSelectingTarget)
            {
                HandleFallbackKeyboardInput();
                if (_isSelectingEnemyTarget)
                    NotifyEnemyTargetingState();
            }
        }

        private void LateUpdate()
        {
            UpdateCameraFocus();
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Setup panel với entity và danh sách skill.
        /// Gọi từ CombatUIController.Initialize().
        /// </summary>
        public void Initialize(string entityId, List<string> skillIds, List<CombatEntity> enemies, List<CombatEntity> allies = null)
        {
            _currentEntityId = entityId;
            _currentSkillIds = skillIds ?? new List<string>();
            _enemies         = enemies  ?? new List<CombatEntity>();
            _allies          = allies   ?? new List<CombatEntity>();

            ExitTargetSelectionMode(resetCamera: true, restoreButtons: true);

            _skillModels.Clear();
            foreach (var id in _currentSkillIds)
            {
                var model = DataManager.Instance?.LoadSkill(id);
                _skillModels.Add(model);
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                if (i < _skillModels.Count && _skillModels[i] != null)
                {
                    var skill = _skillModels[i];
                    _buttons[i].Setup(skill.id, skill.nameKey ?? skill.id, skill.cost?.mana ?? 0, this);
                }
                else
                {
                    _buttons[i].Hide();
                }
            }
        }

        /// <summary>
        /// Update enemy list khi wave progression xảy ra (gọi từ CombatFlowController).
        /// </summary>
        public void UpdateEnemies(List<CombatEntity> newEnemies)
        {
            _enemies = newEnemies ?? new List<CombatEntity>();
            ExitTargetSelectionMode(resetCamera: true, restoreButtons: true);
            Log($"SkillButtonPanel: Updated enemies list to {_enemies.Count} entities.", LogCategory.UI);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handlers

        private void OnTurnEnded(TurnEndedEvent e)
        {
            if (e.EntityId == _currentEntityId)
            {
                ExitTargetSelectionMode(resetCamera: true, restoreButtons: false);
                SetVisible(false);
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Skill Selection

        /// <summary>Gọi từ SkillButton.OnClick().</summary>
        public void OnSkillSelected(string skillId)
        {
            int idx = _currentSkillIds.IndexOf(skillId);
            if (idx < 0 || idx >= _skillModels.Count || _skillModels[idx] == null)
            {
                LogWarning($"SkillButtonPanel: Skill '{skillId}' không tìm thấy trong model list.", LogCategory.UI);
                return;
            }

            var skill = _skillModels[idx];
            if (RequiresManualTargetSelection(skill))
            {
                EnterTargetSelectionMode(skillId, skill);
                return;
            }

            SubmitResolvedSkill(skillId, ResolveTargets(skill));
        }

        /// <summary>Tự động xác định targets theo targetRule của skill.</summary>
        private List<string> ResolveTargets(SkillDataModel skill)
        {
            var targets = new List<string>();
            string rule = skill.targetRule?.type ?? "single_enemy";
            bool canTargetSelf = skill.targetRule?.canTargetSelf ?? false;

            switch (rule)
            {
                case "all_enemies":
                    foreach (var e in _enemies)
                        if (e != null && !e.IsDead) targets.Add(e.ID);
                    break;

                case "all_allies":
                    foreach (var ally in _allies)
                    {
                        if (ally == null || ally.IsDead) continue;
                        if (!canTargetSelf && ally.ID == _currentEntityId) continue;
                        targets.Add(ally.ID);
                    }
                    break;

                case "self":
                    targets.Add(_currentEntityId);
                    break;

                case "single_ally":
                    // Nếu skill cho phép target self thì ưu tiên bản thân, ngược lại chọn ally còn sống đầu tiên.
                    if (canTargetSelf)
                    {
                        targets.Add(_currentEntityId);
                    }
                    else
                    {
                        foreach (var ally in _allies)
                        {
                            if (ally != null && !ally.IsDead && ally.ID != _currentEntityId)
                            {
                                targets.Add(ally.ID);
                                break;
                            }
                        }
                    }
                    break;

                case "single_enemy":
                default:
                    // Tự động chọn enemy đầu tiên còn sống
                    foreach (var e in _enemies)
                    {
                        if (e != null && !e.IsDead)
                        {
                            targets.Add(e.ID);
                            break;
                        }
                    }
                    break;
            }

            return targets;
        }

        private bool RequiresManualTargetSelection(SkillDataModel skill)
        {
            if (skill == null) return false;
            string rule = skill.targetRule?.type ?? "single_enemy";
            return rule == "single_enemy" || rule == "single_ally";
        }

        private void EnterTargetSelectionMode(string skillId, SkillDataModel skill)
        {
            if (skill == null) return;

            string rule = skill.targetRule?.type ?? "single_enemy";
            _targetCandidates = BuildTargetCandidates(skill);

            if (_targetCandidates.Count == 0)
            {
                LogWarning($"SkillButtonPanel: Skill '{skillId}' không có mục tiêu hợp lệ.", LogCategory.UI);
                return;
            }

            // UX: nếu chỉ có 1 mục tiêu hợp lệ thì auto-confirm.
            if (_targetCandidates.Count == 1)
            {
                SubmitResolvedSkill(skillId, new List<string> { _targetCandidates[0].ID });
                return;
            }

            _pendingSkillId = skillId;
            _currentTargetIndex = 0;
            _isSelectingTarget = true;
            _isSelectingEnemyTarget = rule == "single_enemy";

            SetButtonsInteractable(false);
            ApplySelectionHighlight();
            FocusCameraOnCurrentTarget();
            NotifyEnemyTargetingState();

            Log($"SkillButtonPanel: Enter target selection for '{skillId}' ({rule})", LogCategory.UI);
        }

        private List<CombatEntity> BuildTargetCandidates(SkillDataModel skill)
        {
            var candidates = new List<CombatEntity>();
            var rule = skill?.targetRule?.type ?? "single_enemy";
            bool canTargetSelf = skill?.targetRule?.canTargetSelf ?? false;

            if (rule == "single_ally")
            {
                foreach (var ally in _allies)
                {
                    if (ally == null || ally.IsDead) continue;
                    if (!canTargetSelf && ally.ID == _currentEntityId) continue;
                    candidates.Add(ally);
                }
                return candidates;
            }

            if (rule == "all_allies")
            {
                foreach (var ally in _allies)
                {
                    if (ally == null || ally.IsDead) continue;
                    if (!canTargetSelf && ally.ID == _currentEntityId) continue;
                    candidates.Add(ally);
                }
                return candidates;
            }

            foreach (var enemy in _enemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                candidates.Add(enemy);
            }
            return candidates;
        }

        private void SubmitResolvedSkill(string skillId, List<string> targetIds)
        {
            ShowEnemyHudForTargets(targetIds);
            Log($"SkillButtonPanel: Player chọn '{skillId}' → {targetIds.Count} target(s).", LogCategory.UI);
            CombatFlowController.Instance?.SubmitPlayerAction(skillId, targetIds);
            // Giữ enemy HUD trong lúc action đang resolve; sẽ tắt khi turn kết thúc.
            ExitTargetSelectionMode(resetCamera: true, restoreButtons: true, keepEnemyHudVisible: true);
        }

        private void ConfirmCurrentTarget()
        {
            if (!_isSelectingTarget || _targetCandidates.Count == 0 || string.IsNullOrEmpty(_pendingSkillId)) return;

            var target = _targetCandidates[_currentTargetIndex];
            if (target == null || target.IsDead)
            {
                LogWarning("SkillButtonPanel: Target hiện tại không còn hợp lệ.", LogCategory.UI);
                return;
            }

            SubmitResolvedSkill(_pendingSkillId, new List<string> { target.ID });
        }

        private void CancelSelection()
        {
            if (!_isSelectingTarget) return;
            Log("SkillButtonPanel: Cancel target selection.", LogCategory.UI);
            ExitTargetSelectionMode(resetCamera: true, restoreButtons: true);
        }

        private void CycleTarget(int direction)
        {
            if (!_isSelectingTarget || _targetCandidates.Count <= 1) return;

            int count = _targetCandidates.Count;
            _currentTargetIndex = (_currentTargetIndex + direction + count) % count;
            ApplySelectionHighlight();
            FocusCameraOnCurrentTarget();
            NotifyEnemyTargetingState();
        }

        private void ExitTargetSelectionMode(bool resetCamera, bool restoreButtons, bool keepEnemyHudVisible = false)
        {
            if (_targetCandidates.Count > 0)
            {
                foreach (var entity in _targetCandidates)
                {
                    var view = GetView(entity?.ID);
                    if (view != null) view.SetHighlight(false);
                }
            }

            _isSelectingTarget = false;
            _isSelectingEnemyTarget = false;
            _pendingSkillId = null;
            _targetCandidates.Clear();
            _currentTargetIndex = 0;
            if (!keepEnemyHudVisible)
                CombatUIController.Instance?.EndEnemyTargetingHUD();

            if (restoreButtons)
            {
                SetButtonsInteractable(true);
            }

            if (resetCamera)
            {
                RestoreCamera();
            }
        }

        private void ShowEnemyHudForTargets(List<string> targetIds)
        {
            if (targetIds == null || targetIds.Count == 0)
            {
                CombatUIController.Instance?.EndEnemyTargetingHUD();
                return;
            }

            var enemyIds = new List<string>();
            foreach (var targetId in targetIds)
            {
                if (string.IsNullOrWhiteSpace(targetId)) continue;

                var enemy = _enemies.Find(e => e != null && !e.IsDead && e.ID == targetId);
                if (enemy != null)
                    enemyIds.Add(targetId);
            }

            if (enemyIds.Count == 0)
            {
                CombatUIController.Instance?.EndEnemyTargetingHUD();
                return;
            }

            CombatUIController.Instance?.BeginEnemyTargetingHUD(enemyIds, enemyIds[0]);
        }

        private void NotifyEnemyTargetingState()
        {
            if (!_isSelectingTarget || !_isSelectingEnemyTarget || _targetCandidates.Count == 0)
            {
                CombatUIController.Instance?.EndEnemyTargetingHUD();
                return;
            }

            string currentId = _targetCandidates[_currentTargetIndex]?.ID;
            if (string.IsNullOrWhiteSpace(currentId))
            {
                CombatUIController.Instance?.EndEnemyTargetingHUD();
                return;
            }

            CombatUIController.Instance?.BeginEnemyTargetingHUD(new List<string> { currentId }, currentId);
        }

        private void ApplySelectionHighlight()
        {
            for (int i = 0; i < _targetCandidates.Count; i++)
            {
                var entity = _targetCandidates[i];
                var view = GetView(entity?.ID);
                if (view == null) continue;
                view.SetHighlight(i == _currentTargetIndex);
            }
        }

        private CharacterView GetView(string entityId)
        {
            return ActionAnimationController.Instance?.GetViewForEntity(entityId);
        }

        private void BindInputActions()
        {
            _playerInput = FindFirstObjectByType<PlayerInput>();
            if (_playerInput?.actions == null) return;

            _previousAction = _playerInput.actions.FindAction("Previous", throwIfNotFound: false);
            _nextAction = _playerInput.actions.FindAction("Next", throwIfNotFound: false);
            _submitAction = _playerInput.actions.FindAction("Submit", throwIfNotFound: false);
            _cancelAction = _playerInput.actions.FindAction("Cancel", throwIfNotFound: false);

            if (_previousAction != null) _previousAction.performed += OnPreviousAction;
            if (_nextAction != null) _nextAction.performed += OnNextAction;
            if (_submitAction != null) _submitAction.performed += OnSubmitAction;
            if (_cancelAction != null) _cancelAction.performed += OnCancelAction;
        }

        private void UnbindInputActions()
        {
            if (_previousAction != null) _previousAction.performed -= OnPreviousAction;
            if (_nextAction != null) _nextAction.performed -= OnNextAction;
            if (_submitAction != null) _submitAction.performed -= OnSubmitAction;
            if (_cancelAction != null) _cancelAction.performed -= OnCancelAction;
            _previousAction = null;
            _nextAction = null;
            _submitAction = null;
            _cancelAction = null;
        }

        private void OnPreviousAction(InputAction.CallbackContext _)
        {
            if (_isSelectingTarget) CycleTarget(-1);
        }

        private void OnNextAction(InputAction.CallbackContext _)
        {
            if (_isSelectingTarget) CycleTarget(1);
        }

        private void OnSubmitAction(InputAction.CallbackContext _)
        {
            if (_isSelectingTarget) ConfirmCurrentTarget();
        }

        private void OnCancelAction(InputAction.CallbackContext _)
        {
            if (_isSelectingTarget) CancelSelection();
        }

        private void HandleFallbackKeyboardInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // A/D theo yêu cầu UX; luôn hoạt động trong selection mode.
            if (kb.aKey.wasPressedThisFrame) CycleTarget(-1);
            if (kb.dKey.wasPressedThisFrame) CycleTarget(1);

            if (kb.spaceKey.wasPressedThisFrame) ConfirmCurrentTarget();

            // X để hủy, Escape giữ lại như fallback chung.
            if (kb.xKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame) CancelSelection();
        }

        private void FocusCameraOnCurrentTarget()
        {
            if (!_enableSelectionCameraFocus || !_isSelectingTarget || _targetCandidates.Count == 0) return;

            _mainCamera ??= Camera.main;
            if (_mainCamera == null) return;

            if (!_hasCachedCameraState)
            {
                _cameraOriginalPosition = _mainCamera.transform.position;
                _cameraOriginalSize = _mainCamera.orthographic ? _mainCamera.orthographicSize : 0f;
                _hasCachedCameraState = true;
            }

            var target = _targetCandidates[_currentTargetIndex];
            var worldPos = GetTargetFocusWorldPosition(target?.ID);
            var desiredPosition = new Vector3(
                worldPos.x + _cameraOffset.x,
                worldPos.y + _cameraOffset.y,
                _mainCamera.transform.position.z
            );

            if (_mainCamera.orthographic)
            {
                _cameraTargetSize = _cameraSelectionSize;
                _cameraTargetPosition = ClampCameraToBattleBounds(desiredPosition, _cameraTargetSize);
            }
            else
            {
                _cameraTargetPosition = desiredPosition;
            }
        }

        private Vector3 ClampCameraToBattleBounds(Vector3 desiredPosition, float orthographicSize)
        {
            if (_mainCamera == null)
            {
                return desiredPosition;
            }

            var sceneManager = CombatSceneManager.Instance;
            var min = sceneManager != null
                ? Vector2.Min(sceneManager.GetBattlefieldMinBound(), sceneManager.GetBattlefieldMaxBound())
                : new Vector2(-8f, -4f);
            var max = sceneManager != null
                ? Vector2.Max(sceneManager.GetBattlefieldMinBound(), sceneManager.GetBattlefieldMaxBound())
                : new Vector2(8f, 4f);

            float halfHeight = Mathf.Max(0f, orthographicSize);
            float halfWidth = halfHeight * _mainCamera.aspect;

            float minX = min.x + halfWidth;
            float maxX = max.x - halfWidth;
            float minY = min.y + halfHeight;
            float maxY = max.y - halfHeight;

            float clampedX = minX > maxX ? (min.x + max.x) * 0.5f : Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = minY > maxY ? (min.y + max.y) * 0.5f : Mathf.Clamp(desiredPosition.y, minY, maxY);

            return new Vector3(clampedX, clampedY, desiredPosition.z);
        }

        private Vector3 GetTargetFocusWorldPosition(string entityId)
        {
            var view = GetView(entityId);
            if (view != null)
            {
                return view.WorldPosition;
            }

            var fallback = CombatUIController.Instance?.GetEntityWorldPos(entityId) ?? Vector3.zero;
            return fallback;
        }

        private void UpdateCameraFocus()
        {
            if (_mainCamera == null || !_hasCachedCameraState) return;

            var nextPosition = Vector3.Lerp(
                _mainCamera.transform.position,
                _cameraTargetPosition,
                Time.deltaTime * _cameraFocusLerpSpeed
            );

            if (_mainCamera.orthographic)
            {
                nextPosition = ClampCameraToBattleBounds(nextPosition, _mainCamera.orthographicSize);
            }

            _mainCamera.transform.position = nextPosition;

            if (_mainCamera.orthographic)
            {
                _mainCamera.orthographicSize = Mathf.Lerp(
                    _mainCamera.orthographicSize,
                    _cameraTargetSize,
                    Time.deltaTime * _cameraFocusLerpSpeed
                );

                var clampedPosition = ClampCameraToBattleBounds(_mainCamera.transform.position, _mainCamera.orthographicSize);
                _mainCamera.transform.position = new Vector3(
                    clampedPosition.x,
                    clampedPosition.y,
                    _mainCamera.transform.position.z);
            }
        }

        private void RestoreCamera()
        {
            if (!_hasCachedCameraState || _mainCamera == null) return;

            _cameraTargetPosition = _cameraOriginalPosition;
            _cameraTargetSize = _cameraOriginalSize;
            _hasCachedCameraState = false;

            _mainCamera.transform.position = _cameraOriginalPosition;
            if (_mainCamera.orthographic)
            {
                _mainCamera.orthographicSize = _cameraOriginalSize;
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Refresh & Visibility

        /// <summary>
        /// Gọi từ CombatUIController sau khi Initialize() — hiện panel và refresh trạng thái buttons.
        /// </summary>
        public void ShowForTurn()
        {
            SetVisible(true);
            RefreshButtons();
        }

        public void Hide()
        {
            ExitTargetSelectionMode(resetCamera: true, restoreButtons: false);
            SetVisible(false);
        }

        private void RefreshButtons()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (i >= _currentSkillIds.Count || _skillModels[i] == null) continue;

                string skillId   = _currentSkillIds[i];
                bool   canUse    = SkillManager.Instance?.CanUseSkill(_currentEntityId, skillId) ?? true;
                int    cooldown  = SkillManager.Instance?.GetCooldown(_currentEntityId, skillId) ?? 0;
                // BUG-4 FIX: truyền maxCooldown từ SkillDataModel để SkillButton tính fill ratio
                int    maxCd     = Mathf.Max(1, _skillModels[i].cost?.cooldown ?? 1);

                _buttons[i].Refresh(canUse, cooldown, maxCd);
            }
        }

        private void SetVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha          = visible ? 1f : 0f;
                _canvasGroup.interactable   = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }

        private void OnGUI()
        {
            if (!_showSelectionHint || !_isSelectingTarget || _targetCandidates.Count == 0) return;

            var target = _targetCandidates[_currentTargetIndex];
            string targetName = target?.DisplayName ?? target?.ID ?? "Unknown";
            string content =
                $"TARGET: {targetName}\n" +
                "A/D: Change Target\n" +
                "Space: Confirm\n" +
                "X: Cancel";

            var rect = new Rect(16f, 16f, 280f, 110f);
            GUI.Box(rect, content);
        }

        #endregion
    }
}
