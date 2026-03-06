using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TTCS.Combat.Entities;
using TTCS.Core.Data;
using TTCS.Core.Events;
using TTCS.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Battle HUD
    /// Hiển thị HP/MP bars cho tối đa 3 ally và 3 enemy.
    /// Lắng nghe DamageTakenEvent và HealingReceivedEvent để animate thanh HP.
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        // ─── Inner Type ───────────────────────────────────────────────────
        [System.Serializable]
        private class HUDSlot
        {
            public string EntityId;
            public Slider         HPSlider;
            public Slider         MPSlider;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI HPText;
            public CanvasGroup    SlotGroup;

            private float _maxHP = 1f;
            private float _maxMP = 1f;

            public void Initialize(CombatEntity entity, int maxMP)
            {
                EntityId  = entity.ID;
                _maxHP    = entity.Health.MaxHP;
                _maxMP    = Mathf.Max(1, maxMP);

                NameText.text = entity.DisplayName;
                HPSlider.value = entity.HPPercent;
                MPSlider.value = TTCS.Combat.Managers.SkillManager.Instance != null
                    ? (float)TTCS.Combat.Managers.SkillManager.Instance.GetMana(entity.ID) / _maxMP
                    : 1f;

                SetHPText();
                SlotGroup.alpha = 1f;
                gameObject.SetActive(true);
            }

            public void AnimateHP(float newPercent)
            {
                HPSlider.DOValue(newPercent, 0.4f).SetEase(Ease.OutCubic);
                SetHPText();
            }

            public void SetDead()
            {
                SlotGroup.DOFade(0.4f, 0.5f);
            }

            private void SetHPText()
            {
                if (HPText != null)
                    HPText.text = $"{Mathf.RoundToInt(HPSlider.value * _maxHP)}/{Mathf.RoundToInt(_maxHP)}";
            }

            private GameObject gameObject => HPSlider.gameObject.transform.parent.gameObject;
        }

        // ─── Inspector ────────────────────────────────────────────────────
        [Header("Ally Slots (max 3)")]
        [SerializeField] private HUDSlot[] _allySlots  = new HUDSlot[3];

        [Header("Enemy Slots (max 3)")]
        [SerializeField] private HUDSlot[] _enemySlots = new HUDSlot[3];

        // ─── Slot Lookup ──────────────────────────────────────────────────
        private readonly Dictionary<string, HUDSlot> _slotMap = new();

        // ──────────────────────────────────────────────────────────────────
        #region Initialization

        /// <summary>
        /// Gán entity vào các slot và bắt đầu lắng nghe events.
        /// </summary>
        public void InitializeSlots(List<CombatEntity> allies, List<CombatEntity> enemies)
        {
            _slotMap.Clear();

            InitGroup(allies,  _allySlots);
            InitGroup(enemies, _enemySlots);

            SubscribeEvents();
        }

        private void InitGroup(List<CombatEntity> entities, HUDSlot[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < entities.Count && entities[i] != null)
                {
                    int maxMP = TTCS.Combat.Managers.SkillManager.Instance != null
                        ? TTCS.Combat.Managers.SkillManager.Instance.GetMaxMana(entities[i].ID)
                        : 100;
                    slots[i].Initialize(entities[i], maxMP);
                    _slotMap[entities[i].ID] = slots[i];
                }
                else
                {
                    // Ẩn slot thừa
                    if (slots[i].HPSlider != null)
                        slots[i].HPSlider.gameObject.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handling

        private void SubscribeEvents()
        {
            var bus = EventBus.Instance;
            bus.Subscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Subscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Subscribe<EntityDeathEvent>(OnEntityDeath);
        }

        private void UnsubscribeEvents()
        {
            var bus = EventBus.Instance;
            bus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            bus.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
            bus.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
        }

        private void OnDamageTaken(DamageTakenEvent e)
        {
            if (!_slotMap.TryGetValue(e.TargetId, out var slot)) return;

            // Tính percent mới dựa trên entity thực — cần lấy từ CombatUIController
            float newPercent = CombatUIController.Instance != null
                ? CombatUIController.Instance.GetEntityHPPercent(e.TargetId)
                : Mathf.Max(0f, slot.HPSlider.value - 0.05f);

            slot.AnimateHP(newPercent);
        }

        private void OnHealingReceived(HealingReceivedEvent e)
        {
            if (!_slotMap.TryGetValue(e.TargetId, out var slot)) return;

            float newPercent = CombatUIController.Instance != null
                ? CombatUIController.Instance.GetEntityHPPercent(e.TargetId)
                : Mathf.Min(1f, slot.HPSlider.value + 0.1f);

            slot.HPSlider.DOValue(newPercent, 0.4f).SetEase(Ease.OutCubic);
        }

        private void OnEntityDeath(EntityDeathEvent e)
        {
            if (_slotMap.TryGetValue(e.EntityId, out var slot))
                slot.SetDead();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion
    }
}
