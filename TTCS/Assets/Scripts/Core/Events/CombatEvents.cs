namespace TTCS.Core.Events
{
    /// <summary>
    /// 🔵 Dev A - Combat Events
    /// Các event liên quan đến combat
    /// </summary>

    #region Turn & Timeline Events

    /// <summary>Event khi bắt đầu combat</summary>
    public class CombatStartedEvent : GameEvent
    {
        public int Seed { get; private set; }

        public CombatStartedEvent(int seed)
        {
            Seed = seed;
        }
    }

    /// <summary>Event khi kết thúc combat</summary>
    public class CombatEndedEvent : GameEvent
    {
        public bool Victory { get; private set; }

        public CombatEndedEvent(bool victory)
        {
            Victory = victory;
        }
    }

    /// <summary>Event khi bắt đầu turn mới</summary>
    public class TurnStartedEvent : GameEvent
    {
        public string EntityId { get; private set; }
        public int TurnNumber { get; private set; }

        public TurnStartedEvent(string entityId, int turnNumber)
        {
            EntityId = entityId;
            TurnNumber = turnNumber;
        }
    }

    /// <summary>Event khi kết thúc turn</summary>
    public class TurnEndedEvent : GameEvent
    {
        public string EntityId { get; private set; }

        public TurnEndedEvent(string entityId)
        {
            EntityId = entityId;
        }
    }

    /// <summary>Event khi timeline được update</summary>
    public class TimelineUpdatedEvent : GameEvent
    {
        public TimelineUpdatedEvent()
        {
        }
    }

    #endregion

    #region Action Events

    /// <summary>Event khi entity thực hiện action</summary>
    public class ActionExecutedEvent : GameEvent
    {
        public string ActorId { get; private set; }
        public string ActionType { get; private set; }
        public string[] TargetIds { get; private set; }

        public ActionExecutedEvent(string actorId, string actionType, params string[] targetIds)
        {
            ActorId = actorId;
            ActionType = actionType;
            TargetIds = targetIds;
        }
    }

    /// <summary>Event khi skill được cast</summary>
    public class SkillCastEvent : GameEvent
    {
        public string CasterId { get; private set; }
        public string SkillId { get; private set; }
        public string[] TargetIds { get; private set; }

        public SkillCastEvent(string casterId, string skillId, params string[] targetIds)
        {
            CasterId = casterId;
            SkillId = skillId;
            TargetIds = targetIds;
        }
    }

    #endregion

    #region Damage & Healing Events

    /// <summary>Event khi entity nhận damage</summary>
    public class DamageTakenEvent : GameEvent
    {
        public string TargetId { get; private set; }
        public string SourceId { get; private set; }
        public int DamageAmount { get; private set; }
        public bool IsCrit { get; private set; }
        public string DamageType { get; private set; }

        public DamageTakenEvent(string targetId, string sourceId, int damageAmount, bool isCrit, string damageType = "Normal")
        {
            TargetId = targetId;
            SourceId = sourceId;
            DamageAmount = damageAmount;
            IsCrit = isCrit;
            DamageType = damageType;
        }
    }

    /// <summary>Event khi entity được heal</summary>
    public class HealingReceivedEvent : GameEvent
    {
        public string TargetId { get; private set; }
        public string SourceId { get; private set; }
        public int HealAmount { get; private set; }

        public HealingReceivedEvent(string targetId, string sourceId, int healAmount)
        {
            TargetId = targetId;
            SourceId = sourceId;
            HealAmount = healAmount;
        }
    }

    /// <summary>Event khi mana của entity thay đổi (dùng skill, restore, drain)</summary>
    public class ManaChangedEvent : GameEvent
    {
        public string EntityId   { get; private set; }
        public int    CurrentMana { get; private set; }
        public int    MaxMana     { get; private set; }

        public ManaChangedEvent(string entityId, int currentMana, int maxMana)
        {
            EntityId    = entityId;
            CurrentMana = currentMana;
            MaxMana     = maxMana;
        }
    }

    /// <summary>Event khi shield của entity thay đổi.</summary>
    public class ShieldChangedEvent : GameEvent
    {
        public string EntityId { get; private set; }
        public int CurrentShield { get; private set; }

        public ShieldChangedEvent(string entityId, int currentShield)
        {
            EntityId = entityId;
            CurrentShield = currentShield;
        }
    }

    /// <summary>Event khi entity chết</summary>
    public class EntityDeathEvent : GameEvent
    {
        public string EntityId { get; private set; }
        public string KillerId { get; private set; }

        public EntityDeathEvent(string entityId, string killerId)
        {
            EntityId = entityId;
            KillerId = killerId;
        }
    }

    #endregion

    #region Status Effect Events

    /// <summary>Event khi status effect được apply</summary>
    public class StatusEffectAppliedEvent : GameEvent
    {
        public string TargetId { get; private set; }
        public string EffectId { get; private set; }
        public int Duration { get; private set; }

        public StatusEffectAppliedEvent(string targetId, string effectId, int duration)
        {
            TargetId = targetId;
            EffectId = effectId;
            Duration = duration;
        }
    }

    /// <summary>Event khi status effect hết hạn hoặc bị remove</summary>
    public class StatusEffectRemovedEvent : GameEvent
    {
        public string TargetId { get; private set; }
        public string EffectId { get; private set; }

        public StatusEffectRemovedEvent(string targetId, string effectId)
        {
            TargetId = targetId;
            EffectId = effectId;
        }
    }

    #endregion

    #region Timing Events

    /// <summary>Event khi player input timing</summary>
    public class TimingInputEvent : GameEvent
    {
        public enum TimingGrade
        {
            Miss,
            Normal,
            Good,
            Perfect
        }

        public string EntityId { get; private set; }
        public TimingGrade Grade { get; private set; }
        public float TimingOffset { get; private set; } // milliseconds

        public TimingInputEvent(string entityId, TimingGrade grade, float timingOffset)
        {
            EntityId = entityId;
            Grade = grade;
            TimingOffset = timingOffset;
        }
    }

    #endregion
}
