using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Entities;
using TTCS.Combat.Managers;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Data;
using TTCS.Debugging;
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

        /// <summary>Phe ally còn sống — dùng để chọn target mặc định cho skill đơn.</summary>
        private List<CombatEntity> _enemies = new();

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Instance.Subscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Instance.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Setup panel với entity và danh sách skill.
        /// Gọi từ CombatUIController.Initialize().
        /// </summary>
        public void Initialize(string entityId, List<string> skillIds, List<CombatEntity> enemies)
        {
            _currentEntityId = entityId;
            _currentSkillIds = skillIds ?? new List<string>();
            _enemies         = enemies  ?? new List<CombatEntity>();

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

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handlers

        private void OnTurnStarted(TurnStartedEvent e)
        {
            // Chỉ hiện panel khi đến lượt entity được quản lý bởi panel này
            if (e.EntityId != _currentEntityId) return;

            SetVisible(true);
            RefreshButtons();
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
            if (e.EntityId == _currentEntityId)
                SetVisible(false);
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
            var targetIds = ResolveTargets(skill);

            Log($"SkillButtonPanel: Player chọn '{skillId}' → {targetIds.Count} target(s).", LogCategory.UI);
            CombatFlowController.Instance?.SubmitPlayerAction(skillId, targetIds);
        }

        /// <summary>Tự động xác định targets theo targetRule của skill.</summary>
        private List<string> ResolveTargets(SkillDataModel skill)
        {
            var targets = new List<string>();
            string rule = skill.targetRule?.type ?? "single_enemy";

            switch (rule)
            {
                case "all_enemies":
                    foreach (var e in _enemies)
                        if (e != null && !e.IsDead) targets.Add(e.ID);
                    break;

                case "self":
                    targets.Add(_currentEntityId);
                    break;

                case "single_ally":
                    // Mặc định target bản thân — UI nâng cao có thể override sau
                    targets.Add(_currentEntityId);
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

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Refresh & Visibility

        private void RefreshButtons()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (i >= _currentSkillIds.Count || _skillModels[i] == null) continue;

                string skillId = _currentSkillIds[i];
                bool canUse    = SkillManager.Instance?.CanUseSkill(_currentEntityId, skillId) ?? true;
                int  cooldown  = SkillManager.Instance?.GetCooldown(_currentEntityId, skillId) ?? 0;

                _buttons[i].Refresh(canUse, cooldown);
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

        #endregion
    }
}
