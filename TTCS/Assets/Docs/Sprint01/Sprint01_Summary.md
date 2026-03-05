# 📦 Sprint 1 — Tổng kết & Hướng dẫn Sử dụng

> **Dự án**: TTCS — Those at The Crossroads of Story  
> **Sprint 1 hoàn thành**: 2026-03-05  
> **Scope**: Toàn bộ combat infrastructure — data pipeline, turn system, skill system, AI, entities, effects, actions, combat flow

---

## 1. Tổng quan những gì đã build

Sprint 1 xây dựng **combat engine hoàn chỉnh** chạy được không cần UI:

```
Data Layer         → DataManager đọc JSON → CharacterDataModel, EnemyDataModel, SkillDataModel
Turn System        → TurnManager (timeline-based, speed-sorted)
Resource System    → SkillManager (mana + cooldown)
Entity System      → CombatEntity (Health + Stats + Effects components)
Effect System      → StatusEffect (Bleed, Burn, Heal, Shield, Stun)
Action Pipeline    → SkillAction → ActionValidator → ActionResolver → StatCalculator
AI System          → AIController + AIBehavior ScriptableObject
Combat Flow        → CombatFlowController (coroutine state machine)
Save System        → SaveManager (PlayerPrefs)
Infrastructure     → EventBus, RNGService, CombatLogger, DebugLogger
```

---

## 2. Cấu trúc thư mục Scripts

```
Assets/Scripts/
├── Core/
│   ├── Constants.cs                 ← Global constants, DEBUG_LOGS_ENABLED
│   ├── Data/
│   │   ├── DataManager.cs           ← Singleton, tự load JSON trong Awake()
│   │   ├── DataCache.cs             ← Generic in-memory cache
│   │   └── DataValidator.cs         ← Validate data sau khi load
│   ├── Events/
│   │   ├── EventBus.cs              ← Publish/Subscribe, tự tạo GameObject nếu thiếu
│   │   ├── GameEvent.cs             ← Base class cho tất cả events
│   │   ├── CombatEvents.cs          ← DamageTaken, Heal, Death, SkillCast, ...
│   │   └── SystemEvents.cs          ← DataLoaded, SceneChanged, ...
│   ├── Save/
│   │   ├── SaveManager.cs           ← Singleton, PlayerPrefs-based
│   │   ├── SaveData.cs              ← Root save object
│   │   └── SaveSlot.cs              ← Per-slot data
│   └── Utilities/
│       ├── RNGService.cs            ← Seeded RNG singleton
│       └── GameUtils.cs             ← Helpers
│
├── Data/                            ← ScriptableObject wrappers + DataModel classes
│   ├── CharacterDataModel.cs        ← JSON model
│   ├── EnemyDataModel.cs
│   ├── SkillDataModel.cs
│   ├── StageDataModel.cs
│   ├── CharacterData.cs             ← ScriptableObject [CreateAssetMenu]
│   ├── EnemyData.cs
│   ├── SkillData.cs
│   └── StageData.cs
│
├── Combat/
│   ├── Actions/
│   │   ├── IAction.cs               ← Interface: Validate() + Execute()
│   │   ├── ActionValidator.cs       ← Static: kiểm tra mana/cooldown/targets
│   │   ├── ActionResolver.cs        ← Static: dispatch attack/heal/buff
│   │   └── SkillAction.cs           ← Concrete IAction sử dụng SkillDataModel
│   ├── AI/
│   │   ├── AIBehavior.cs            ← ScriptableObject cấu hình AI
│   │   ├── AIController.cs          ← Static: DecideAction() → AIDecision
│   │   └── TargetSelector.cs        ← Helper chọn target
│   ├── Components/
│   │   ├── IEntityComponent.cs      ← Interface: Initialize(id), Reset()
│   │   ├── HealthComponent.cs       ← HP + Shield, publish events
│   │   ├── StatsComponent.cs        ← Base stats + StatModifier list
│   │   └── EffectComponent.cs       ← StatusEffect list, Add/Tick/Remove
│   ├── Effects/
│   │   ├── StatusEffect.cs          ← Abstract base, TickTiming enum
│   │   ├── BleedEffect.cs           ← Stack count, EndTurn tick
│   │   ├── BurnEffect.cs            ← Stack value, EndTurn tick
│   │   ├── HealEffect.cs            ← HoT, StartTurn tick
│   │   ├── ShieldEffect.cs          ← Absorb damage, OnApply
│   │   └── StunEffect.cs            ← Skip turn, StartTurn tick
│   ├── Entities/
│   │   ├── CombatEntity.cs          ← Abstract base, 3 components
│   │   ├── Character.cs             ← IsPlayer=true
│   │   ├── Enemy.cs                 ← IsPlayer=false, AIBehavior
│   │   └── EntityFactory.cs         ← Static: CreateCharacter/Enemy/Party/Wave
│   ├── Managers/
│   │   ├── TurnManager.cs           ← Timeline turn order singleton
│   │   ├── SkillManager.cs          ← Mana + cooldown singleton
│   │   ├── CombatFlowController.cs  ← MonoBehaviour state machine
│   │   └── CombatTestLoader.cs      ← Debug bootstrapper với OnGUI panel
│   └── Stats/
│       ├── DamageType.cs            ← Enums: DamageType, Element, ActionSpeed
│       ├── EntityStats.cs           ← Serializable stats struct
│       ├── StatModifier.cs          ← Flat/Percentage modifier với duration
│       └── StatCalculator.cs        ← Static: CalculateDamage(), CalculateHeal()
│
├── Debug/
│   ├── CombatLogger.cs              ← Structured battle log (namespace TTCS.Combat)
│   ├── DebugLogger.cs               ← Category-based dev logger
│   └── Test/                        ← Test scripts (xóa trước release)
│       ├── ComponentTest.cs
│       ├── DamageCalcTest.cs
│       ├── ActionPipelineTest.cs
│       ├── EntityFactoryTest.cs
│       ├── TurnManagerTest.cs
│       ├── SkillManagerTest.cs
│       ├── SaveManagerTest.cs
│       ├── DataManagerTest.cs
│       └── AIControllerTest.cs
│
└── Utils/
    └── Singleton.cs                 ← Generic Singleton<T> base
```

---

## 3. Cấu trúc thư mục Data (JSON)

```
Assets/Data/
├── Characters/
│   ├── char_warrior.json
│   └── char_mage.json
├── Skills/
│   ├── skill_warrior_slash.json
│   ├── skill_mage_fireball.json
│   └── skill_heal.json
├── Enemies/
│   ├── enemy_goblin.json
│   └── enemy_dark_knight.json
└── Stages/
    └── stage_01_tutorial.json
```

### Cách thêm nhân vật mới (JSON)

Tạo file `Assets/Data/Characters/char_xxx.json`:

```json
{
  "id": "char_archer",
  "nameKey": "Archer",
  "baseStats": {
    "hp": 900,
    "atk": 160,
    "def": 60,
    "spd": 120,
    "crit": 0.15,
    "resist": 0.05
  },
  "skills": ["skill_arrow_shot", "skill_piercing_arrow"]
}
```

`DataManager` tự scan tất cả file trong `Assets/Data/Characters/` — không cần đăng ký thêm.

### Cách thêm skill mới (JSON)

Tạo file `Assets/Data/Skills/skill_xxx.json`:

```json
{
  "id": "skill_arrow_shot",
  "nameKey": "Arrow Shot",
  "type": "Attack",
  "targetRule": { "type": "SingleEnemy" },
  "cost": { "mana": 15, "cooldown": 0 },
  "damage": { "formula": "ATK * 1.1", "element": "Physical", "canCrit": true },
  "effects": [],
  "actionCost": { "timelineUnits": 90 }
}
```

**`type`** hợp lệ: `"Attack"`, `"Heal"`, `"Buff"`, `"Debuff"`  
**`targetRule.type`** hợp lệ: `"SingleEnemy"`, `"AllEnemies"`, `"SingleAlly"`, `"Self"`  
**`effects[].type`** hợp lệ: `"bleed"`, `"burn"`, `"heal_regen"`, `"shield"`, `"stun"`

### Cách thêm enemy mới (JSON)

```json
{
  "id": "enemy_orc",
  "nameKey": "Orc Warrior",
  "baseStats": { "hp": 700, "atk": 100, "def": 60, "spd": 70 },
  "moveSet": [
    { "skillId": "skill_orc_smash" }
  ],
  "rewards": { "goldBase": 25, "expBase": 40 }
}
```

---

## 4. Hướng dẫn sử dụng các System chính

### 4.1. DataManager

```csharp
// Load single item (trả về null nếu không tìm thấy)
CharacterDataModel charModel = DataManager.Instance.LoadCharacter("char_warrior");
SkillDataModel     skillModel = DataManager.Instance.LoadSkill("skill_warrior_slash");
EnemyDataModel     enemyModel = DataManager.Instance.LoadEnemy("enemy_goblin");

// Kiểm tra đã load xong chưa
if (DataManager.Instance.IsLoaded) { ... }
```

### 4.2. EntityFactory

```csharp
// Tạo nhân vật từ ID (DataManager load JSON)
Character warrior = EntityFactory.CreateCharacter("char_warrior");

// Tạo từ model có sẵn
Character warrior = EntityFactory.CreateCharacter(charModel);

// Tạo enemy
Enemy goblin = EntityFactory.CreateEnemy("enemy_goblin");

// Tạo cả party / wave
List<Character> party   = EntityFactory.CreateParty(new List<string> { "char_warrior", "char_mage" });
List<Enemy>     enemies = EntityFactory.CreateWave(new List<string> { "enemy_goblin", "enemy_goblin" });
```

### 4.3. CombatEntity — truy cập Components

```csharp
CombatEntity entity = EntityFactory.CreateCharacter("char_warrior");

// Health
int  hp      = entity.Health.CurrentHP;
int  maxHP   = entity.Health.MaxHP;
bool isDead  = entity.Health.IsDead;
entity.Health.TakeDamage(50);
entity.Health.Heal(30);
entity.Health.AddShield(100);

// Stats
float atk = entity.Stats.GetEffectiveStat(StatType.ATK);
float def = entity.Stats.GetEffectiveStat(StatType.DEF);
entity.Stats.AddModifier(new StatModifier(StatType.ATK, ModifierType.Flat, 20f, 3, "buff_src"));

// Effects
entity.ApplyEffect(new BurnEffect(intensity: 50f, duration: 3));
bool hasBurn = entity.Effects.HasEffect("burn");
entity.Effects.RemoveEffect(entity.Effects.GetEffect("burn"), entity);

// Turn lifecycle
entity.OnTurnStart();  // tick StartTurn effects + stat mod duration
entity.OnTurnEnd();    // tick EndTurn effects

// Snapshot (dùng cho AI)
CombatEntitySnapshot snap = entity.ToSnapshot();
```

### 4.4. TurnManager

```csharp
// Khởi tạo combat
TurnManager.Instance.InitializeCombat(new List<(string id, int speed)>
{
    ("char_warrior_0", 105),
    ("enemy_goblin_0", 85)
});

// Hoặc đăng ký từng entity
TurnManager.Instance.RegisterEntity("char_warrior_0", speed: 105);

// Lấy actor tiếp theo
string nextActorId = TurnManager.Instance.GetNextActor();

// Bắt đầu / kết thúc lượt
TurnManager.Instance.StartTurn("char_warrior_0");
TurnManager.Instance.EndTurn("char_warrior_0", timelineCost: 100);

// Xóa entity đã chết
TurnManager.Instance.RemoveEntity("enemy_goblin_0");

// Reset khi combat kết thúc
TurnManager.Instance.ResetCombat();

// Lượt hiện tại
int turn = TurnManager.Instance.TurnCounter;
```

### 4.5. SkillManager

```csharp
// Đăng ký entity
SkillManager.Instance.RegisterEntity("char_warrior_0", maxMana: 100, startingMana: 60);

// Kiểm tra có dùng được không
bool canUse = SkillManager.Instance.CanUseSkill("char_warrior_0", skillModel);

// Dùng skill (trừ mana + set cooldown)
SkillManager.Instance.UseSkill("char_warrior_0", skillModel);

// Tick cooldown sau mỗi turn
SkillManager.Instance.TickCooldowns("char_warrior_0");

// Hủy đăng ký
SkillManager.Instance.UnregisterEntity("char_warrior_0");
```

### 4.6. Action Pipeline

```csharp
// Cách chuẩn dùng action:
var action = new SkillAction(skillModel);
var result = action.Validate(actor, targets, SkillManager.Instance);

if (result.IsValid)
    action.Execute(actor, targets, SkillManager.Instance);
else
    Debug.Log(result.FailReason);
```

### 4.7. StatCalculator

```csharp
// Tính damage (trả về damage final và có crit không)
var (damage, isCrit) = StatCalculator.CalculateDamage(
    attacker: warrior,
    defender: goblin,
    skillMultiplier: 1.2f,
    element: Element.Physical,   // optional, mặc định 1.0x ở Sprint 1
    critMultiplier: 1.5f         // optional, mặc định 1.5x
);

// Tính heal
int healAmount = StatCalculator.CalculateHeal(caster: mage, skillMultiplier: 0.8f);
```

**Công thức damage:**
```
baseDmg   = ATK × multiplier
defFactor = DEF / (DEF + 100)  → tối đa 75%
finalDmg  = max(1, baseDmg × (1 - defFactor) × critBonus × elementMult)
```

### 4.8. EventBus

```csharp
// Subscribe (thường trong OnEnable / Start)
EventBus.Instance.Subscribe<DamageTakenEvent>(OnDamageTaken);

// Unsubscribe (thường trong OnDisable / OnDestroy)
EventBus.Instance.Unsubscribe<DamageTakenEvent>(OnDamageTaken);

// Publish
EventBus.Instance.Publish(new DamageTakenEvent
{
    EntityId = "char_warrior_0",
    Damage   = 80,
    SourceId = "enemy_goblin_0"
});

// Handler
private void OnDamageTaken(DamageTakenEvent e)
{
    Debug.Log($"{e.EntityId} took {e.Damage} damage");
}
```

**Danh sách events Sprint 1** (từ `CombatEvents.cs`):

| Event | Trigger |
|-------|---------|
| `DamageTakenEvent` | `HealthComponent.TakeDamage()` |
| `HealingReceivedEvent` | `HealthComponent.Heal()` |
| `EntityDeathEvent` | HP về 0 |
| `StatusEffectAppliedEvent` | `EffectComponent.AddEffect()` |
| `StatusEffectRemovedEvent` | `EffectComponent.RemoveEffect()` |
| `SkillCastEvent` | `ActionResolver.Resolve()` |
| `ActionExecutedEvent` | Sau khi `ActionResolver` hoàn thành |
| `CombatStartedEvent` | `CombatFlowController.InitBattle()` |

### 4.9. RNGService

```csharp
// Khởi tạo với seed (deterministic)
RNGService.Instance.Initialize(12345);

// Lấy giá trị
float rand  = RNGService.Instance.Value();          // 0.0 - 1.0
int   val   = RNGService.Instance.Range(1, 100);    // min inclusive, max exclusive
bool  crit  = RNGService.Instance.RollCrit(0.15f);  // true nếu roll thành công
bool  proc  = RNGService.Instance.RollChance(0.4f); // true 40% thời gian
```

### 4.10. SaveManager

```csharp
// Save
SaveManager.Instance.SaveGame(slotIndex: 0, saveData);

// Load
SaveData data = SaveManager.Instance.LoadGame(slotIndex: 0);

// Kiểm tra slot có data không
bool exists = SaveManager.Instance.SlotExists(0);

// Xóa slot
SaveManager.Instance.DeleteSlot(0);
```

### 4.11. CombatFlowController

```csharp
// Bắt đầu combat
CombatFlowController.Instance.StartBattle(party, enemies, seed: 12345);

// UI gọi khi player chọn skill + target
CombatFlowController.Instance.SubmitPlayerAction(
    skillId:   "skill_warrior_slash",
    targetIds: new List<string> { "enemy_goblin_0" }
);

// Skip lượt (debug)
CombatFlowController.Instance.SkipPlayerTurn();
```

---

## 5. Tạo ScriptableObject Assets trong Unity Editor

### AIBehavior (dùng cho Enemy AI)

1. **Assets > Create > TTCS > AI Behavior**
2. Điền Inspector:

| Field | Goblin | Dark Knight |
|-------|--------|-------------|
| Profile Name | `goblin_basic` | `dark_knight_elite` |
| HP Threshold Heal | `0.3` | `0.4` |
| HP Threshold Aggressive | `0.5` | `0.6` |
| Preferred Skill Ids | (trống) | (trống) |

3. Assign vào `EnemyData.asset` → field **AI Behavior**

### CharacterData / EnemyData / SkillData / StageData

1. **Assets > Create > TTCS > [loại]**
2. Điền Inspector với các thông số — đây là **editor-only reference**, runtime dùng JSON
3. Không bắt buộc cho combat hoạt động — `DataManager` đọc JSON trực tiếp

---

## 6. Setup TestCombat Scene (nhanh)

1. **File > New Scene (Empty)** → Save As `Assets/Scenes/TestCombat.unity`
2. Create Empty → tên `[CombatManagers]` → Add Components:
   - `DataManager`, `SaveManager`, `TurnManager`, `SkillManager`, `CombatFlowController`
   - `EventBus` và `RNGService` tự tạo GameObject nếu thiếu — không cần add tay
3. Create Empty → tên `[CombatLoader]` → Add Component: `CombatTestLoader`
4. Trong Inspector của `CombatTestLoader`:
   - Party: `char_warrior`, `char_mage`
   - Enemies: `enemy_goblin`, `enemy_goblin`
   - Auto Start On Play: ✅
5. **Play** → combat loop chạy tự động, dùng OnGUI buttons để điều khiển

---

## 7. Status Effects — cách sử dụng

```csharp
// Apply effect lên entity
entity.ApplyEffect(new BleedEffect(damagePerStack: 60f, duration: 3));
entity.ApplyEffect(new BurnEffect(intensity: 50f, duration: 3));
entity.ApplyEffect(new HealEffect(healPerTurn: 30f, duration: 4));
entity.ApplyEffect(new ShieldEffect(shieldAmount: 200f));
entity.ApplyEffect(new StunEffect(duration: 1));

// Stack: apply lần 2 cùng EffectId → tự gọi OnStack()
entity.ApplyEffect(new BleedEffect(60f, 2));  // StackCount +1

// Check
bool hasBurn = entity.Effects.HasEffect("burn");

// Tick thủ công (CombatFlowController gọi tự động)
entity.OnTurnStart();  // tick StartTurn effects (Heal, Stun)
entity.OnTurnEnd();    // tick EndTurn effects (Bleed, Burn)
```

**EffectId strings**: `"bleed"`, `"burn"`, `"heal_regen"`, `"shield"`, `"stun"`

---

## 8. StatModifier — buff/debuff tạm thời

```csharp
// Thêm buff ATK +30% trong 2 lượt
entity.Stats.AddModifier(new StatModifier(
    statType:   StatType.ATK,
    modType:    ModifierType.Percentage,
    value:      0.3f,         // +30%
    duration:   2,            // 2 turns, -1 = permanent
    sourceId:   "buff_warrior_rage"
));

// Giá trị hiệu quả sau khi áp buff
float effectiveAtk = entity.Stats.GetEffectiveStat(StatType.ATK);
// = (baseATK + sumFlat) × (1 + sumPercentage)

// StatType hợp lệ: HP, ATK, DEF, SPD, CritRate, Resist

// Xóa buff theo source
entity.Stats.RemoveModifiersBySource("buff_warrior_rage");

// Tick duration (CombatFlowController gọi tự động qua OnTurnStart)
entity.Stats.TickModifiers();
```

---

## 9. Giới hạn Sprint 1 (sẽ làm Sprint 2)

| Tính năng | Trạng thái | Ghi chú |
|-----------|-----------|---------|
| Combat UI (HP bar, skill buttons) | ❌ Chưa có | Sprint 2 |
| Nhân vật animation | ❌ Chưa connect | Sprint 2 — parts-based DOTween |
| Element multipliers | ⚠️ Stub (luôn 1.0x) | Sprint 2 |
| Advanced AI | ⚠️ Basic only | Sprint 2 |
| Audio | ❌ Chưa có | Sprint 2 |
| Stage progression | ❌ Chưa có | Sprint 2 |
| Gacha system | ❌ Chưa có | Sprint 3+ |

---

## 10. Lỗi thường gặp

| Lỗi | Nguyên nhân | Giải pháp |
|-----|------------|-----------|
| `MonoBehaviour using 'new'` | Singleton dùng `new` thay `AddComponent` | Đã fix trong EventBus — pattern: `go.AddComponent<T>()` |
| `No subscribers for DataLoadedEvent` | Warning bình thường khi chưa có UI | Bỏ qua hoặc tắt `DEBUG_LOGS_ENABLED` |
| Null ref khi `DataManager.Instance.LoadCharacter()` | JSON file không tồn tại hoặc sai format | Kiểm tra `Assets/Data/Characters/char_xxx.json` |
| Entity HP không giảm | `HealthComponent.Initialize()` chưa gọi | Dùng `EntityFactory` thay `new Character()` trực tiếp |
| `SkillActionCost` field not found | Dùng tên field sai | Đúng: `actionCost.timelineUnits` (KHÔNG phải `timelineCost`) |
| `rewards.gold` not found | Tên field sai trong JSON/model | Đúng: `rewards.goldBase`, `rewards.expBase` |

---

*Sprint 1 complete — TTCS Combat Engine*
