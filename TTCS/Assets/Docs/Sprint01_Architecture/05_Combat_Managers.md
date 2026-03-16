# 05 — Combat Managers

Ba class quản lý điều phối toàn bộ trận chiến: `CombatFlowController` (state machine tổng), `TurnManager` (CTB timeline), và `SkillManager` (tài nguyên skill).

---

## 1. Quan hệ giữa 3 manager

```
CombatFlowController (điều phối tổng)
    │
    ├──► TurnManager.GetNextActor()     → ai đến lượt?
    │    TurnManager.StartTurn(id)      → bắt đầu lượt
    │    TurnManager.EndTurn(id, cost)  → kết thúc lượt
    │
    ├──► SkillManager.RegisterEntity()  → setup state trước combat
    │    SkillManager.CanUseSkill()     → kiểm tra resource
    │    SkillManager.TickCooldowns()   → giảm cooldown sau lượt
    │
    └──► AIController.DecideAction()    → khi đến lượt enemy
         SkillAction.Validate/Execute() → thực hiện action
```

---

## 2. CombatFlowController

**File:** `Scripts/Combat/Managers/CombatFlowController.cs`
**Namespace:** `TTCS.Combat.Managers`
**Pattern:** MonoBehaviour Singleton, Coroutine-based state machine, `DontDestroyOnLoad`

### CombatState enum

```csharp
private enum CombatState
{
    Idle,             // Trước khi combat bắt đầu
    Initializing,     // Đang setup
    PlayerTurn,       // Chờ player input
    EnemyTurn,        // AI đang xử lý
    ExecutingAction,  // Đang resolve action
    PostAction,       // Sau action (delay, cleanup)
    CheckVictory,     // Kiểm tra thắng/thua
    BattleEnd         // Trận đã kết thúc
}
```

### Inspector settings

| Field | Type | Mặc định | Mô tả |
|-------|------|----------|-------|
| `_actionDelay` | `float` | 0.5s | Dừng sau mỗi hành động |
| `_betweenTurnDelay` | `float` | 0.2s | Dừng giữa các lượt |
| `_playerTurnTimeout` | `float` | 0 (vô hạn) | Timeout player input |
| `_guardWindowDuration` | `float` | 1.5s | Duration timing window parry |
| `_perfectThresholdMs` | `float` | 500ms | Ngưỡng Perfect |
| `_goodThresholdMs` | `float` | 800ms | Ngưỡng Good |
| `_forceFirstPlayerActsFirst` | `bool` | true | Player đầu tiên luôn mở trận |

### Public API

| Method | Tham số | Mô tả |
|--------|---------|-------|
| `StartBattle(players, enemies, seed)` | `List<Character>, List<Enemy>, int` | Entry point — bắt đầu combat loop |
| `SubmitPlayerAction(skillId, targetIds)` | `string, List<string>` | Nhận input từ UI khi đến lượt player |
| `SkipPlayerTurn()` | — | Bỏ lượt player (fallback) |
| `GetPlayerTeam()` | — | Trả về `List<Character>` hiện tại |
| `GetEnemyTeam()` | — | Trả về `List<Enemy>` hiện tại |
| `GetAllEntities()` | — | Tất cả entity còn sống |
| `GetCurrentActor()` | — | Entity đang ở lượt |
| `IsPlayerTurn()` | — | Đang ở PlayerTurn state không |

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `IsBattleActive` | `bool` | True khi không phải Idle/BattleEnd |
| `CurrentSeed` | `int` | RNG seed đang dùng |
| `TurnNumber` | `int` | Số lượt hiện tại |

### Combat Loop (Coroutine)

```
StartBattle(players, enemies, seed)
    └── StartCoroutine(CombatLoop())

CombatLoop():
  └── InitBattle()
        ├── RNGService.Initialize(seed)
        ├── Build _allEntities = players + enemies
        ├── TurnManager.ResetCombat + RegisterEntity(all)
        ├── SkillManager.ResetCombat + RegisterEntity(all)
        └── EventBus.Publish(CombatStartedEvent)

  Loop (while !IsOver):
    ├── actorId = TurnManager.GetNextActor()
    ├── actor = FindEntity(actorId)
    ├── TurnManager.StartTurn(actorId)
    ├── actor.OnTurnStart()
    │
    ├── [if actor.IsPlayer]
    │     └── PlayerTurnRoutine(actor)
    │           ├── Publish(PlayerTurnStartedEvent)
    │           ├── WaitUntil(_playerInputReceived) [or timeout]
    │           └── ExecuteAction(actor, _pendingSkillId, _pendingTargetIds, TimingGrade.Miss)
    │
    └── [if !actor.IsPlayer]
          └── EnemyTurnRoutine(actor as Enemy)
                ├── BuildSnapshots()
                ├── AIController.DecideAction(...)
                ├── [if skill.type == "attack"] TimingSystem.OpenWindow(...)
                │   └── WaitUntil(gradeReceived)
                └── ExecuteAction(actor, skillId, targetIds, guardGrade)

  Per turn end:
    ├── actor.OnTurnEnd()
    ├── SkillManager.TickCooldowns(actorId)
    ├── TurnManager.EndTurn(actorId, action.TimelineCost)
    ├── CleanupDeadEntities()
    └── CheckVictory() / CheckDefeat()

  EndBattle(victory):
    └── EventBus.Publish(CombatEndedEvent)
```

### ExecuteAction pipeline
```
ExecuteAction(actor, skillId, targetIds, grade):
    ├── DataManager.LoadSkill(skillId) → skillData
    ├── ResolveTargets(skillData, targetIds) → List<CombatEntity>
    ├── new SkillAction(skillData)
    ├── action.Validate(actor, targets, SkillManager)
    │     └── [if !IsValid] return early, log warning
    └── action.Execute(actor, targets, SkillManager, grade)
```

### CleanupDeadEntities
```
foreach entity in _allEntities where IsDead:
    ├── TurnManager.RemoveEntity(id)
    ├── SkillManager.UnregisterEntity(id)
    └── remove from _allEntities / _playerTeam / _enemyTeam
```

---

## 3. TurnManager

**File:** `Scripts/Combat/Managers/TurnManager.cs`
**Namespace:** `TTCS.Combat.Managers`
**Pattern:** MonoBehaviour Singleton, `DontDestroyOnLoad`

### Cơ chế CTB (Count Turn Battle)

```
TURN_THRESHOLD = 100f

Mỗi entity có gauge = 0 lúc bắt đầu.
GetNextActor():
  1. Tính minTicks = min(TURN_THRESHOLD - gauge[i]) / speed[i]) cho tất cả entity
  2. Advance tất cả: gauge[i] += minTicks * speed[i]
  3. Entity đầu tiên đạt >= 100 → trả về entityId đó

EndTurn(actorId, actionCost):
  gauge[actorId] -= actionCost  (mặc định 100)
  → Entity sẽ phải tích lũy lại gauge

Hiệu quả: Entity có SPD cao hơn → đến lượt thường xuyên hơn.
Penalty: ActionCost cao → actor phải chờ lâu hơn.
```

### State
```
_gauges         : Dictionary<string, float>   — gauge mỗi entity
_speeds         : Dictionary<string, int>     — SPD mỗi entity
_activeEntities : List<string>                — entities đang active
_turnCounter    : int                         — tổng số lượt đã đi
CurrentActor    : string                      — entity đang trong lượt
TurnCounter     : int                         — public access
```

### Public API

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `RegisterEntity(entityId, speed)` | `string, int` | void | Đăng ký entity vào timeline |
| `RemoveEntity(entityId)` | `string` | void | Loại khỏi timeline khi chết |
| `GetNextActor()` | — | `string` | Virtual tick và trả về actor tiếp theo |
| `StartTurn(entityId)` | `string` | void | Đánh dấu bắt đầu lượt, publish `TurnStartedEvent` |
| `EndTurn(entityId, actionCost)` | `string, float` | void | Kết thúc lượt, trừ gauge, publish `TurnEndedEvent` |
| `GetTimelinePreview(count)` | `int` | `List<string>` | Xem trước thứ tự N lượt kế tiếp |
| `GetGauge(entityId)` | `string` | `float` | Gauge hiện tại |
| `SetGauge(entityId, gauge)` | `string, float` | void | Chỉnh gauge thủ công |
| `ResetCombat()` | — | void | Xóa toàn bộ state |
| `InitializeCombat(entities)` | `IEnumerable<(id,spd)>` | void | Reset + register nhiều entity |

### Events published

| Event | Khi nào |
|-------|---------|
| `TurnStartedEvent(entityId, turnNumber)` | Mỗi khi `StartTurn()` |
| `TurnEndedEvent(entityId)` | Mỗi khi `EndTurn()` |
| `TimelineUpdatedEvent()` | Khi `RemoveEntity()` hoặc `SetGauge()` |

---

## 4. SkillManager

**File:** `Scripts/Combat/Managers/SkillManager.cs`
**Namespace:** `TTCS.Combat.Managers`
**Pattern:** MonoBehaviour Singleton, `DontDestroyOnLoad`

### State
```
_cooldowns   : Dictionary<string, Dictionary<string, int>>  — entityId → skillId → turns
_usageCount  : Dictionary<string, Dictionary<string, int>>  — entityId → skillId → count
_currentMana : Dictionary<string, int>                      — entityId → mana hiện tại
_maxMana     : Dictionary<string, int>                      — entityId → mana tối đa
```

### Default constants
```
DEFAULT_MAX_MANA     = 100
STARTING_MANA_RATIO  = 0.8f  (bắt đầu với 80% mana)
```

### Public API — Registration

| Method | Tham số | Mô tả |
|--------|---------|-------|
| `RegisterEntity(id, maxMana, startingMana)` | — | Setup state mana/cooldown. startingMana -1 = tự tính 80% |
| `UnregisterEntity(id)` | `string` | Xóa khỏi tất cả dictionaries |
| `ResetCombat()` | — | Xóa toàn bộ state |

### Public API — Skill check & use

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `CanUseSkill(entityId, skillId)` | `string, string` | `bool` | Check cooldown + mana + limit. Load skill từ DataManager |
| `CanUseSkill(entityId, skillData)` | `string, SkillDataModel` | `bool` | Overload không cần load |
| `UseSkill(entityId, skillId)` | — | void | Commit: trừ mana, reset cooldown, tăng usageCount |
| `UseSkill(entityId, skillData)` | — | void | Overload |
| `TickCooldowns(entityId)` | `string` | void | Giảm tất cả cooldown của entity 1 lượt |
| `SetCooldown(entityId, skillId, turns)` | — | void | Gán cooldown thủ công |
| `GetCooldown(entityId, skillId)` | — | `int` | Cooldown còn lại |
| `IsOnCooldown(entityId, skillId)` | — | `bool` | Skill đang hồi chưa |
| `ResetAllCooldowns(entityId)` | — | void | Xóa toàn bộ cooldown |
| `GetUsageCount(entityId, skillId)` | — | `int` | Số lần dùng trong trận |

### Public API — Mana

| Method | Mô tả |
|--------|-------|
| `GetMana(entityId)` | Mana hiện tại |
| `GetMaxMana(entityId)` | Mana tối đa |
| `RestoreMana(entityId, amount)` | Hồi mana, không vượt max |
| `DrainMana(entityId, amount)` | Trừ mana trực tiếp |
| `FullRestoreMana(entityId)` | Hồi đầy mana |

### Events published

| Event | Khi nào |
|-------|---------|
| `ManaChangedEvent(entityId, current, max)` | Sau `UseSkill()`, `RestoreMana()`, `DrainMana()` |

### CanUseSkill logic

```
CanUseSkill(entityId, skillData):
  1. ValidateEntity(entityId) — có trong dict không
  2. IsOnCooldown(entityId, skillId) → false nếu cooldown > 0
  3. currentMana >= skill.cost.mana → false nếu thiếu mana
  4. limitPerFight != -1 → false nếu usageCount >= limit
  5. Tất cả pass → true
```

---

## 5. CombatTestLoader

**File:** `Scripts/Combat/Managers/CombatTestLoader.cs`
**Pattern:** MonoBehaviour (debug tool, không phải Singleton)

### Trách nhiệm
Bootstrap nhanh một trận combat cho mục đích test/debug. Được thay thế bởi `CombatSceneManager` trong Sprint 2.

### Inspector settings
- `_stageId` — ID stage để load
- `_partyIds` — danh sách character id
- `_autoStart` — tự động bắt đầu khi scene load

### Methods

| Method | Mô tả |
|--------|-------|
| `StartTestCombat()` | Load stage + tạo entities + gọi CombatFlowController.StartBattle() |
| `DebugSubmitAction(skillId, targets)` | Ép submit action qua CombatFlowController |
| `DebugSkipTurn()` | Ép skip lượt |

Có OnGUI panel để debug trên screen với các nút: Start, Skip Turn, và danh sách skill buttons.
