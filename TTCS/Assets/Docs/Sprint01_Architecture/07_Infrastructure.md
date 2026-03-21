# 07 — Infrastructure

Các hệ thống nền tảng phục vụ toàn bộ codebase: EventBus, tất cả event classes, RNGService, Constants, DebugLogger.

---

## 1. EventBus

**File:** `Scripts/Core/EventBus.cs`
**Namespace:** `TTCS.Core`
**Pattern:** MonoBehaviour Singleton, `DontDestroyOnLoad`, thread-safe lock

### Internal structure
```csharp
private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
private readonly object _lock = new object();
```

### Public API

| Method | Signature | Mô tả |
|--------|-----------|-------|
| `Subscribe<T>` | `Subscribe<T>(Action<T> handler)` | Đăng ký lắng nghe event type T |
| `Unsubscribe<T>` | `Unsubscribe<T>(Action<T> handler)` | Huỷ đăng ký |
| `Publish<T>` | `Publish<T>(T eventData)` | Gửi event đến tất cả subscriber |

### Implementation notes
- Thread-safe khi Subscribe/Unsubscribe/Publish — dùng `lock(_lock)`
- Snapshot subscriber list trước khi invoke (tránh concurrent modification)
- `Publish<T>` invoke tất cả handler; exception của một handler không ngăn handler khác chạy

### Usage pattern (chuẩn)

```csharp
// SUBSCRIBE — trong Awake() hoặc OnEnable()
private void Awake()
{
    EventBus.Instance.Subscribe<DamageTakenEvent>(OnDamageTaken);
}

// UNSUBSCRIBE — trong OnDisable() hoặc OnDestroy()
private void OnDestroy()
{
    EventBus.Instance.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
}

// HANDLER
private void OnDamageTaken(DamageTakenEvent e)
{
    Debug.Log($"{e.EntityId} took {e.Amount} damage");
}

// PUBLISH
EventBus.Instance.Publish(new DamageTakenEvent(entityId, sourceId, 42, false));
```

---

## 2. Tất cả Event Classes

**File:** `Scripts/Core/Events/CombatEvents.cs`, `SystemEvents.cs`
**Namespace:** `TTCS.Core.Events`

### 2.1 Combat Events

#### CombatStartedEvent
```csharp
public class CombatStartedEvent : GameEvent
{
    public int Seed;
}
```
**Publisher:** `CombatFlowController` (InitBattle)
**Subscribers:** `RNGService`, `CombatLogger`, `CombatBridge` (Sprint 2)

---

#### CombatEndedEvent
```csharp
public class CombatEndedEvent : GameEvent
{
    public bool IsVictory;
}
```
**Publisher:** `CombatFlowController` (EndBattle)
**Subscribers:** `CombatBridge`, `SaveManager`, `ResultUI`

---

#### TurnStartedEvent
```csharp
public class TurnStartedEvent : GameEvent
{
    public string EntityId;
    public int TurnNumber;
}
```
**Publisher:** `TurnManager.StartTurn()`
**Subscribers:** `EffectComponent` (tick hiệu ứng), `SkillManager` (tick cooldown), `HUDController`

---

#### TurnEndedEvent
```csharp
public class TurnEndedEvent : GameEvent
{
    public string EntityId;
}
```
**Publisher:** `TurnManager.EndTurn()`
**Subscribers:** HUD, timeline UI

---

#### TimelineUpdatedEvent
```csharp
public class TimelineUpdatedEvent : GameEvent { }
```
**Publisher:** `TurnManager` (khi entity chết hoặc gauge thay đổi)
**Subscribers:** Timeline UI (Sprint 2)

---

#### ActionExecutedEvent
```csharp
public class ActionExecutedEvent : GameEvent
{
    public string ActorId;
    public string ActionType;
    public List<string> TargetIds;
}
```
**Publisher:** `SkillAction.Execute()`
**Subscribers:** `CombatLogger`, UI animators

---

#### SkillCastEvent
```csharp
public class SkillCastEvent : GameEvent
{
    public string CasterId;
    public string SkillId;
    public List<string> TargetIds;
}
```
**Publisher:** `SkillAction.Execute()`
**Subscribers:** Visual effects system (Sprint 2), Audio (Sprint 2)

---

#### DamageTakenEvent
```csharp
public class DamageTakenEvent : GameEvent
{
    public string EntityId;
    public string SourceId;
    public int Amount;
    public bool IsCrit;
}
```
**Publisher:** `HealthComponent.TakeDamage()`
**Subscribers:** HP bar UI, damage number VFX, `CombatLogger`

---

#### HealingReceivedEvent
```csharp
public class HealingReceivedEvent : GameEvent
{
    public string EntityId;
    public string SourceId;
    public int Amount;
}
```
**Publisher:** `HealthComponent.ReceiveHeal()`
**Subscribers:** HP bar UI, heal number VFX

---

#### ManaChangedEvent
```csharp
public class ManaChangedEvent : GameEvent
{
    public string EntityId;
    public int Current;
    public int Max;
}
```
**Publisher:** `SkillManager` (UseSkill, RestoreMana, DrainMana)
**Subscribers:** Mana bar UI

---

#### EntityDeathEvent
```csharp
public class EntityDeathEvent : GameEvent
{
    public string EntityId;
    public bool IsPlayer;
}
```
**Publisher:** `HealthComponent` (khi HP về 0)
**Subscribers:** `CombatFlowController` (cleanup), death animation trigger

---

#### StatusEffectAppliedEvent
```csharp
public class StatusEffectAppliedEvent : GameEvent
{
    public string EntityId;
    public string EffectId;
    public int Duration;
}
```
**Publisher:** `EffectComponent.ApplyEffect()` / `ActionResolver`
**Subscribers:** Status icon UI, `CombatLogger`

---

#### StatusEffectRemovedEvent
```csharp
public class StatusEffectRemovedEvent : GameEvent
{
    public string EntityId;
    public string EffectId;
}
```
**Publisher:** `EffectComponent` (effect hết hạn)
**Subscribers:** Status icon UI

---

#### TimingInputEvent
```csharp
public class TimingInputEvent : GameEvent
{
    public TimingGrade Grade;
}
```
**Publisher:** `TimingSystem` (Sprint 2), `InputBridge`
**Subscribers:** `CombatFlowController` (chờ input guard/attack timing)

---

### 2.2 System Events

#### DataLoadedEvent
```csharp
public class DataLoadedEvent : GameEvent
{
    public int CharacterCount;
    public int SkillCount;
    public int EnemyCount;
    public int StageCount;
}
```
**Publisher:** `DataManager` (sau khi load xong)
**Subscribers:** `CombatTestLoader`, loading screen UI

---

#### GameSavedEvent
```csharp
public class GameSavedEvent : GameEvent
{
    public int SlotIndex;
}
```
**Publisher:** `SaveManager.Save()`
**Subscribers:** Save UI confirmation

---

#### GameLoadedEvent
```csharp
public class GameLoadedEvent : GameEvent
{
    public int SlotIndex;
}
```
**Publisher:** `SaveManager.Load()`
**Subscribers:** Scene transition, data refresh

---

### 2.3 GameEvent base class

```csharp
public abstract class GameEvent
{
    public float Timestamp { get; }  // Time.time lúc tạo
    public GameEvent() { Timestamp = Time.time; }
}
```

---

### 2.4 Bảng Producer → Consumer

| Event | Producer | Consumers |
|-------|----------|-----------|
| `CombatStartedEvent` | CombatFlowController | RNGService, CombatLogger |
| `CombatEndedEvent` | CombatFlowController | CombatBridge, SaveManager |
| `TurnStartedEvent` | TurnManager | EffectComponent, SkillManager (cooldown tick) |
| `TurnEndedEvent` | TurnManager | HUD |
| `TimelineUpdatedEvent` | TurnManager | Timeline UI |
| `ActionExecutedEvent` | SkillAction | CombatLogger |
| `SkillCastEvent` | SkillAction | VFX, Audio |
| `DamageTakenEvent` | HealthComponent | HP bar, VFX, Logger |
| `HealingReceivedEvent` | HealthComponent | HP bar, VFX |
| `ManaChangedEvent` | SkillManager | Mana bar |
| `EntityDeathEvent` | HealthComponent | CombatFlowController |
| `StatusEffectAppliedEvent` | EffectComponent | Status UI |
| `StatusEffectRemovedEvent` | EffectComponent | Status UI |
| `TimingInputEvent` | TimingSystem | CombatFlowController |
| `DataLoadedEvent` | DataManager | CombatTestLoader |
| `GameSavedEvent` | SaveManager | Save UI |
| `GameLoadedEvent` | SaveManager | Scene manager |

---

## 3. RNGService

**File:** `Scripts/Core/RNGService.cs`
**Namespace:** `TTCS.Core`
**Pattern:** MonoBehaviour Singleton, lazy-create, `DontDestroyOnLoad`

### Lý do cần Seeded RNG
Dùng `System.Random` thay vì `UnityEngine.Random` để đảm bảo determinism — cùng seed cho kết quả giống nhau. Quan trọng cho replay, testing, và bug reproduction.

### State
```csharp
private System.Random _rng;
private int _currentSeed;
public bool IsInitialized { get; private set; }
```

### Public API

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `Initialize(seed)` | `int` | void | Reset RNG với seed cố định |
| `InitializeWithTimestamp()` | — | void | Dùng `DateTime.Now.Millisecond` làm seed |
| `RollCrit(critRate)` | `float` | `bool` | `Next() < critRate` |
| `Range(min, max)` | `int, int` | `int` | Random integer [min, max) |
| `Range(min, max)` | `float, float` | `float` | Random float [min, max) |
| `Next()` | — | `float` | Random float [0.0, 1.0) |

### Subscriber
Lắng nghe `CombatStartedEvent` để tự init với seed từ event:
```csharp
void OnCombatStarted(CombatStartedEvent e) => Initialize(e.Seed);
```

---

## 4. DataManager

> Chi tiết đầy đủ tại [03_Data_Layer.md](03_Data_Layer.md)

**File:** `Scripts/Data/DataManager.cs`
**Pattern:** MonoBehaviour Singleton, `DontDestroyOnLoad`

Tóm tắt nhanh entry points:
- `LoadAllData()` — load từ JSON, populate DataCache
- `GetCharacter(id)`, `GetSkill(id)`, `GetEnemy(id)`, `GetStage(id)`
- `IsDataLoaded` property

---

## 5. SaveManager

> Chi tiết đầy đủ tại [03_Data_Layer.md](03_Data_Layer.md)

**File:** `Scripts/Data/SaveManager.cs`
**Pattern:** MonoBehaviour Singleton

Tóm tắt nhanh:
- `Save(slotIndex)` — serializes game state to JSON at `Application.persistentDataPath`
- `Load(slotIndex)` → `SaveData`
- `DeleteSave(slotIndex)`
- `HasSave(slotIndex)`

---

## 6. Constants

**File:** `Scripts/Core/Constants.cs`
**Namespace:** `TTCS.Core`
**Pattern:** Static class với nested static classes

**Cấu trúc:**
```csharp
public static class Constants
{
    public static class Combat
    {
        public const float TURN_THRESHOLD    = 100f;
        public const int   DEFAULT_MAX_MANA  = 100;
        public const float STARTING_MANA     = 0.8f;
        public const float MAX_DEF_REDUCTION = 0.75f;
        public const float BASE_CRIT_MULT    = 1.5f;
        // timing thresholds (mirror inspector values for code use)
        public const float PERFECT_MS        = 500f;
        public const float GOOD_MS           = 800f;
    }

    public static class Data
    {
        public const string CHARACTERS_PATH = "Characters/";
        public const string SKILLS_PATH     = "Skills/";
        public const string ENEMIES_PATH    = "Enemies/";
        public const string STAGES_PATH     = "Stages/";
    }

    public static class Save
    {
        public const int   MAX_SLOTS        = 3;
        public const string FILE_EXTENSION  = ".json";
        public const string FILE_PREFIX     = "save_";
    }
}
```

---

## 7. DebugLogger & CombatLogger

### DebugLogger

**File:** `Scripts/Debug/DebugLogger.cs`
**Namespace:** `TTCS.Debugging`
**Pattern:** Static utility class

```csharp
public static class DebugLogger
{
    public static bool IsEnabled = true;
    public static LogCategory Filter = LogCategory.All;

    public static void Log(string msg, LogCategory cat = LogCategory.General)
    public static void LogWarning(string msg, LogCategory cat = LogCategory.General)
    public static void LogError(string msg, LogCategory cat = LogCategory.General)
}

public enum LogCategory
{
    General, Combat, Data, AI, Timing, UI, All
}
```

Wrap `UnityEngine.Debug.*` với prefix `[TTCS][Category]` và global on/off switch.

---

### CombatLogger

**File:** `Scripts/Debug/CombatLogger.cs`
**Namespace:** `TTCS.Debugging`
**Pattern:** MonoBehaviour (subscribe to events)

Subscribe các event combat và in ra structured log để debug trận đấu từng bước.

#### Events subscribed

| Event | Log message |
|-------|-------------|
| `CombatStartedEvent` | `=== COMBAT STARTED (seed={seed}) ===` |
| `TurnStartedEvent` | `[Turn {n}] {entityId} starts turn` |
| `SkillCastEvent` | `{caster} casts {skillId} on [{targets}]` |
| `DamageTakenEvent` | `{entity} takes {amt} dmg from {src} (crit={v})` |
| `HealingReceivedEvent` | `{entity} healed {amt} by {src}` |
| `StatusEffectAppliedEvent` | `{entity} affected by {effectId} ({dur} turns)` |
| `EntityDeathEvent` | `{entity} DIED` |
| `CombatEndedEvent` | `=== COMBAT ENDED (victory={v}) ===` |

---

## 8. Singleton MonoBehaviour template

Mọi Singleton trong project đều theo pattern:

```csharp
public class MyManager : MonoBehaviour
{
    public static MyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // ... init
    }
}
```

Danh sách tất cả Singleton:

| Class | Lazy-create? |
|-------|-------------|
| `EventBus` | No (phải có trong scene) |
| `DataManager` | No |
| `SaveManager` | No |
| `RNGService` | Yes (tạo nếu chưa có) |
| `CombatFlowController` | No |
| `TurnManager` | No |
| `SkillManager` | No |
