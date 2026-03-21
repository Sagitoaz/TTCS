# 01 — Tổng Quan Kiến Trúc Sprint 01

## 1. Mục tiêu thiết kế

Sprint 01 xây dựng **combat engine** hoàn chỉnh — chạy được mà không cần UI hay Visual. Mục tiêu kỹ thuật bao gồm:

- **Data-driven**: mọi nhân vật, skill, enemy, stage được định nghĩa bằng JSON, không hardcode.
- **Event-driven**: combat engine không gọi trực tiếp vào UI — thay vào đó publish event, UI/Audio/Visual subscribe.
- **Tách biệt trách nhiệm**: mỗi class chỉ làm một việc rõ ràng (Single Responsibility).
- **Deterministic RNG**: combat có thể replay với cùng seed.

---

## 2. Phân lớp kiến trúc

```
┌─────────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER  (Sprint 2 — chưa có trong Sprint 01)       │
│  UI • VFX • Audio • Animation                                    │
└──────────────────────────────┬──────────────────────────────────┘
                               │ subscribe EventBus
┌──────────────────────────────▼──────────────────────────────────┐
│  GAME LOGIC LAYER                                                │
│  CombatFlowController • TurnManager • SkillManager              │
│  SkillAction • ActionValidator • ActionResolver                  │
│  CombatEntity • EntityFactory • AIController                    │
└──────────────────────────────┬──────────────────────────────────┘
                               │ read/write
┌──────────────────────────────▼──────────────────────────────────┐
│  DATA LAYER                                                      │
│  DataManager • DataCache • DataValidator                        │
│  CharacterDataModel • SkillDataModel • EnemyDataModel           │
│  StageDataModel • SaveManager • SaveData                        │
└──────────────────────────────┬──────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│  INFRASTRUCTURE                                                  │
│  EventBus • RNGService • DebugLogger • CombatLogger             │
│  Constants • GameUtils • Singleton<T>                           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Cấu trúc thư mục

```
Assets/
├── Data/                            ← JSON data files (game content)
│   ├── Characters/
│   │   ├── char_warrior.json
│   │   └── char_mage.json
│   ├── Skills/
│   │   ├── skill_warrior_slash.json
│   │   ├── skill_mage_fireball.json
│   │   └── skill_heal.json
│   ├── Enemies/
│   │   ├── enemy_goblin.json
│   │   └── enemy_dark_knight.json
│   └── Stages/
│       └── stage_01_tutorial.json
│
└── Scripts/
    ├── Core/
    │   ├── Constants.cs                 ← Hằng số toàn cục
    │   ├── Events/
    │   │   ├── GameEvent.cs             ← Base class event
    │   │   ├── EventBus.cs              ← Pub/Sub singleton
    │   │   ├── CombatEvents.cs          ← 14 combat event classes
    │   │   └── SystemEvents.cs          ← 3 system event classes
    │   ├── Data/
    │   │   ├── DataManager.cs           ← Load/cache JSON
    │   │   ├── DataCache<T>.cs          ← Generic in-memory cache
    │   │   └── DataValidator.cs         ← Static validators
    │   ├── Save/
    │   │   ├── SaveManager.cs           ← Read/write JSON save files
    │   │   ├── SaveData.cs              ← Root save object
    │   │   └── SaveSlot.cs              ← Metadata per slot
    │   └── Utilities/
    │       ├── RNGService.cs            ← Seeded random số
    │       └── GameUtils.cs             ← Helper utilities
    │
    ├── Data/                            ← ScriptableObject wrappers + DataModel
    │   ├── CharacterDataModel.cs        ← JSON deserialization model
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
    │   │   ├── IAction.cs               ← Interface: Validate + Execute
    │   │   ├── ActionValidationResult.cs← Kết quả validate
    │   │   ├── ActionValidator.cs       ← Static: kiểm tra tính hợp lệ
    │   │   ├── ActionResolver.cs        ← Static: apply damage/heal/effect
    │   │   └── SkillAction.cs           ← Concrete IAction
    │   ├── AI/
    │   │   ├── AIBehavior.cs            ← ScriptableObject profile AI
    │   │   ├── AIController.cs          ← Static: DecideAction()
    │   │   ├── CombatEntitySnapshot.cs  ← Struct snapshot cho AI
    │   │   └── TargetSelector.cs        ← Static: chọn target theo rule
    │   ├── Components/
    │   │   ├── IEntityComponent.cs      ← Interface component
    │   │   ├── HealthComponent.cs       ← HP + Shield
    │   │   ├── StatsComponent.cs        ← Base stats + modifiers
    │   │   └── EffectComponent.cs       ← Status effect container
    │   ├── Effects/
    │   │   ├── StatusEffect.cs          ← Abstract base
    │   │   ├── BleedEffect.cs
    │   │   ├── BurnEffect.cs
    │   │   ├── HealEffect.cs
    │   │   ├── ShieldEffect.cs
    │   │   └── StunEffect.cs
    │   ├── Entities/
    │   │   ├── CombatEntity.cs          ← Abstract base entity
    │   │   ├── Character.cs             ← Player entity
    │   │   ├── Enemy.cs                 ← Enemy entity
    │   │   └── EntityFactory.cs         ← Static factory
    │   ├── Managers/
    │   │   ├── CombatFlowController.cs  ← State machine, vòng lặp trận
    │   │   ├── TurnManager.cs           ← CTB timeline
    │   │   ├── SkillManager.cs          ← Mana + cooldown
    │   │   └── CombatTestLoader.cs      ← Debug bootstrapper
    │   ├── Stats/
    │   │   ├── EntityStats.cs           ← Stats struct
    │   │   ├── StatModifier.cs          ← Buff/debuff modifier
    │   │   ├── StatCalculator.cs        ← Công thức damage/heal
    │   │   └── DamageType.cs            ← Enums: DamageType, Element, ActionSpeed
    │   └── Timing/                      ← (Sprint 2 — TimingSystem)
    │
    ├── Debug/
    │   ├── CombatLogger.cs
    │   ├── DebugLogger.cs
    │   └── Test/                        ← Test harness scripts
    │
    └── Utils/
        └── Singleton.cs                 ← Generic Singleton<T>
```

---

## 4. Singleton MonoBehaviours trong scene

Các class sau yêu cầu có GameObject trong scene và sử dụng `DontDestroyOnLoad`:

| Class | Namespace | Ghi chú |
|-------|-----------|---------|
| `DataManager` | `TTCS.Core.Data` | Load trước tất cả, Awake() gọi LoadAllData() |
| `SaveManager` | `TTCS.Core.Save` | Không tự load — cần gọi NewGame() hoặc Load() |
| `EventBus` | `TTCS.Core.Events` | Lazy-create nếu chưa có |
| `RNGService` | `TTCS.Core.Utilities` | Lazy-create nếu chưa có |
| `TurnManager` | `TTCS.Combat.Managers` | Phải có trong Combat scene |
| `SkillManager` | `TTCS.Combat.Managers` | Phải có trong Combat scene |
| `CombatFlowController` | `TTCS.Combat.Managers` | Phải có trong Combat scene |

Static classes (không cần scene): `EntityFactory`, `ActionValidator`, `ActionResolver`, `StatCalculator`, `AIController`, `TargetSelector`, `DataValidator`.

---

## 5. Nguyên tắc thiết kế đang áp dụng

### 5.1 Tách resource và outcome
- `SkillManager` quản lý **tài nguyên** (mana, cooldown, usage limit).
- `ActionResolver` xử lý **kết quả** (damage, heal, effect).
- Hai class **không bao giờ làm việc nhau**.

### 5.2 Tách điều phối và nghiệp vụ
- `CombatFlowController` điều phối vòng lặp — biết ai đến lượt, gọi ai.
- `TurnManager`, `SkillManager`, `ActionResolver` tự xử lý nghiệp vụ của mình.
- `CombatFlowController` không tính damage, không quản lý cooldown.

### 5.3 Event-driven output
- Combat logic không biết UI/Audio tồn tại.
- Khi xảy ra sự kiện (damage, death, turn start...) → publish event qua `EventBus`.
- Ai cần biết thì tự subscribe.

### 5.4 Component pattern cho Entity
- `CombatEntity` là container — gom 3 component: `HealthComponent`, `StatsComponent`, `EffectComponent`.
- Mỗi component độc lập, có thể test riêng.

### 5.5 Factory pattern
- `EntityFactory` tạo `Character`/`Enemy` — không tạo trực tiếp qua constructor.
- Tách logic tạo entity ra khỏi data model.
