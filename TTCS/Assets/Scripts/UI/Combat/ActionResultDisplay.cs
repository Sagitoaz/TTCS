using System.Collections.Generic;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.UI.Combat
{
    /// <summary>
    /// 🔵 Dev A - Action Result Display
    /// Quản lý pool 10 FloatingText, spawn số damage/heal tại vị trí world của entity.
    ///
    /// Subscribe: DamageTakenEvent, HealingReceivedEvent
    /// </summary>
    public class ActionResultDisplay : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [SerializeField] private FloatingText _floatingTextPrefab;
        [SerializeField] private int          _poolSize = 10;
        [SerializeField] private Vector3      _spawnOffset = new Vector3(0f, 1.2f, 0f);

        // ─── Colors ───────────────────────────────────────────────────────
        private static readonly Color _damageColor   = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color _criticalColor = new Color(1f, 0.85f, 0.1f, 1f);
        private static readonly Color _healColor     = new Color(0.3f, 1f, 0.4f, 1f);

        // ─── Pool ─────────────────────────────────────────────────────────
        private readonly Queue<FloatingText> _pool = new();

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            BuildPool();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Instance.Subscribe<HealingReceivedEvent>(OnHealingReceived);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Instance.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Pool

        private void BuildPool()
        {
            if (_floatingTextPrefab == null)
            {
                DebugLogger.LogWarning("ActionResultDisplay: FloatingText prefab chưa được gán!", LogCategory.UI);
                return;
            }

            for (int i = 0; i < _poolSize; i++)
            {
                var instance = Instantiate(_floatingTextPrefab, transform);
                instance.gameObject.SetActive(false);
                instance.OnComplete = ReturnToPool;
                _pool.Enqueue(instance);
            }
        }

        private FloatingText GetFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            // Pool cạn — tạo thêm (soft fallback)
            
            DebugLogger.LogWarning("ActionResultDisplay: Pool cạn — spawning thêm.", LogCategory.UI);
            var extra = Instantiate(_floatingTextPrefab, transform);
            extra.OnComplete = ReturnToPool;
            return extra;
        }

        private void ReturnToPool(FloatingText item)
        {
            _pool.Enqueue(item);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Event Handlers

        private void OnDamageTaken(DamageTakenEvent e)
        {
            Color color = e.IsCrit ? _criticalColor : _damageColor;
            SpawnText(e.TargetId, e.DamageAmount.ToString(), color, e.IsCrit);
        }

        private void OnHealingReceived(HealingReceivedEvent e)
        {
            SpawnText(e.TargetId, $"+{e.HealAmount}", _healColor, isCritical: false);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Spawn

        private void SpawnText(string entityId, string text, Color color, bool isCritical)
        {
            Vector3 worldPos = GetEntityWorldPos(entityId) + _spawnOffset;
            var ft = GetFromPool();
            ft.Show(text, color, worldPos, isCritical);
        }

        private Vector3 GetEntityWorldPos(string entityId)
        {
            if (CombatUIController.Instance != null)
                return CombatUIController.Instance.GetEntityWorldPos(entityId);
            return Vector3.zero;
        }

        #endregion
    }
}
