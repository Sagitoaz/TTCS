using System;
using System.Collections.Generic;
using TTCS.Combat.Effects;
using TTCS.Combat.Entities;
using TTCS.Core.Events;

namespace TTCS.Combat.Components
{
    /// <summary>
    /// 🟢 Dev B - Effect Component
    /// Quản lý danh sách StatusEffect đang active trên một CombatEntity.
    ///
    /// Chức năng chính:
    ///   - AddEffect: thêm mới hoặc stack vào effect đã có
    ///   - RemoveEffect: gỡ effect + gọi OnRemove
    ///   - TickEffects(timing): tick tất cả effect phù hợp với timing, xóa expired
    ///   - HasEffect / GetActiveEffects: query
    /// </summary>
    public class EffectComponent : IEntityComponent
    {
        // ─── State ────────────────────────────────────────────────────────
        private string              _entityId;
        private List<StatusEffect>  _activeEffects = new List<StatusEffect>();

        // ─── IEntityComponent ─────────────────────────────────────────────
        public string EntityId => _entityId;

        public void Initialize(string entityId)
        {
            _entityId = entityId;
        }

        public void Reset()
        {
            _activeEffects.Clear();
        }

        // ─── Effect Management ───────────────────────────────────────────
        /// <summary>
        /// Thêm effect vào entity.
        /// Nếu đã có effect cùng EffectId → gọi OnStack thay vì duplicate.
        /// </summary>
        public void AddEffect(StatusEffect effect, CombatEntity owner)
        {
            if (effect == null || owner == null) return;

            var existing = _activeEffects.Find(e => e.EffectId == effect.EffectId);
            if (existing != null)
            {
                existing.OnStack(effect);
            }
            else
            {
                _activeEffects.Add(effect);
                effect.OnApply(owner);
                EventBus.Instance.Publish(
                    new StatusEffectAppliedEvent(_entityId, effect.EffectId, effect.Duration));
            }
        }

        /// <summary>Gỡ effect khỏi entity, gọi OnRemove và publish event</summary>
        public void RemoveEffect(StatusEffect effect, CombatEntity owner)
        {
            if (effect == null || !_activeEffects.Remove(effect)) return;

            effect.OnRemove(owner);
            EventBus.Instance.Publish(
                new StatusEffectRemovedEvent(_entityId, effect.EffectId));
        }

        /// <summary>
        /// Tick tất cả effect có TickTiming khớp với timing đã cho.
        /// Effect nào IsExpired sau tick → tự động remove.
        /// </summary>
        public void TickEffects(CombatEntity owner, TickTiming timing)
        {
            // Snapshot để tránh modify-while-iterate
            var snapshot  = new List<StatusEffect>(_activeEffects);
            var toRemove  = new List<StatusEffect>();

            foreach (var effect in snapshot)
            {
                if (effect.TickTiming != timing) continue;

                effect.OnTick(owner);

                if (effect.IsExpired)
                    toRemove.Add(effect);
            }

            foreach (var e in toRemove)
                RemoveEffect(e, owner);
        }

        // ─── Query ───────────────────────────────────────────────────────
        public bool HasEffect(string effectId) =>
            _activeEffects.Exists(e => e.EffectId == effectId);

        public bool HasEffect<T>() where T : StatusEffect =>
            _activeEffects.Exists(e => e is T);

        public T GetEffect<T>() where T : StatusEffect =>
            _activeEffects.Find(e => e is T) as T;

        public StatusEffect GetEffect(string effectId) =>
            _activeEffects.Find(e => e.EffectId == effectId);

        public List<StatusEffect> GetActiveEffects() =>
            new List<StatusEffect>(_activeEffects);

        public int Count => _activeEffects.Count;

        /// <summary>Xóa tất cả effect (dùng khi end battle hoặc cleanse)</summary>
        public void ClearEffects(CombatEntity owner)
        {
            foreach (var e in new List<StatusEffect>(_activeEffects))
                RemoveEffect(e, owner);
        }
    }
}
