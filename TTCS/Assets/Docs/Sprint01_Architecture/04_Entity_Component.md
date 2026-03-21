# 04 — Entity & Component System

Nhóm này định nghĩa **thực thể chiến đấu** (Combat Entity) và các **component** gắn vào nó. Đây là nền tảng dữ liệu runtime mà tất cả combat logic vận hành trên đó.

---

## 1. Kiến trúc tổng quan

```
CombatEntity (abstract)
    ├── Identity: ID, DisplayName, IsPlayer
    ├── HealthComponent  ── quản lý HP + Shield, publish events
    ├── StatsComponent   ── base stats + runtime modifier
    └── EffectComponent  ── danh sách StatusEffect đang active

        ▲                          ▲
        │                          │
   Character                    Enemy
  (IsPlayer=true)          (IsPlayer=false)
   List<skillIds>           AIBehavior ref
                            List<skillIds>

EntityFactory (static) ── tạo Character/Enemy từ DataModel hoặc ID
```

---

## 2. CombatEntity

**File:** `Scripts/Combat/Entities/CombatEntity.cs`
**Namespace:** `TTCS.Combat.Entities`
**Pattern:** Abstract base class (không phải MonoBehaviour — plain C# object)

### Trách nhiệm
- Container trung tâm cho một đơn vị chiến đấu.
- Gom 3 component (Health, Stats, Effects) dưới một API thống nhất.
- Cung cấp `OnTurnStart()` / `OnTurnEnd()` để combat flow trigger lifecycle.

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `ID` | `string` | Combat ID duy nhất (vd: "char_warrior", "enemy_goblin_1") |
| `DisplayName` | `string` | Tên hiển thị |
| `IsPlayer` | `bool` | true = phe player, false = enemy |
| `Health` | `HealthComponent` | |
| `Stats` | `StatsComponent` | |
| `Effects` | `EffectComponent` | |
| `IsDead` | `bool` | `Health.IsDead` |
| `HPPercent` | `float` | `Health.HPPercent` (0.0–1.0) |
| `ATK` | `int` | `Stats.GetEffectiveStat(ATK)` — sau modifier |
| `DEF` | `int` | `Stats.GetEffectiveStat(DEF)` — sau modifier |
| `SPD` | `int` | `Stats.GetEffectiveStat(SPD)` — dùng bởi TurnManager |
| `CritRate` | `float` | `Stats.GetEffectiveStat(CritRate)` |

### Methods

| Method | Tham số | Mô tả |
|--------|---------|-------|
| `Initialize(id, name, stats, isPlayer)` | — | `protected` — gọi từ subclass constructor |
| `TakeDamage(amount, sourceId)` | `int, string` | Delegate xuống `Health.TakeDamage` |
| `Heal(amount, sourceId)` | `int, string` | Delegate xuống `Health.Heal` |
| `ApplyEffect(effect)` | `StatusEffect` | Delegate xuống `Effects.AddEffect` |
| `RemoveEffect(effect)` | `StatusEffect` | Delegate xuống `Effects.RemoveEffect` |
| `OnTurnStart()` | — | Tick `StartTurn` effects + stat modifiers |
| `OnTurnEnd()` | — | Tick `EndTurn` effects |
| `ResetForBattle()` | — | Reset toàn bộ state (HP, modifiers, effects) |
| `ToSnapshot()` | — | Tạo `CombatEntitySnapshot` cho AI |

---

## 3. Character

**File:** `Scripts/Combat/Entities/Character.cs`
**Kế thừa:** `CombatEntity`

### Trách nhiệm
Đại diện nhân vật do người chơi điều khiển (`IsPlayer = true`).

### Constructor
```csharp
public Character(CharacterDataModel model, int instanceIndex = 0)
```
- Parse `model.baseStats` → `EntityStats`
- Tạo `combatId = instanceIndex > 0 ? $"{model.id}_{instanceIndex}" : model.id`
- Gọi `Initialize(combatId, nameKey, stats, isPlayer: true)`
- Copy `model.skills` → `SkillIds`

### Properties thêm

| Property | Type | Mô tả |
|----------|------|-------|
| `CharacterId` | `string` | ID của `CharacterDataModel` gốc |
| `SkillIds` | `List<string>` | Danh sách skill ID nhân vật này sở hữu |

### Methods thêm

| Method | Mô tả |
|--------|-------|
| `HasSkill(skillId)` | Kiểm tra nhân vật có sở hữu skill không |

---

## 4. Enemy

**File:** `Scripts/Combat/Entities/Enemy.cs`
**Kế thừa:** `CombatEntity`

### Constructor
```csharp
public Enemy(EnemyDataModel model, int spawnIndex = 0)
```
- Parse `model.baseStats` → `EntityStats`
- Trích danh sách `skillId` từ `model.moveSet`
- Gọi `Initialize(combatId, nameKey, stats, isPlayer: false)`

### Properties thêm

| Property | Type | Mô tả |
|----------|------|-------|
| `EnemyTemplateId` | `string` | ID của `EnemyDataModel` gốc |
| `SkillIds` | `List<string>` | Skills enemy có thể dùng |
| `Behavior` | `AIBehavior` | Profile AI (ScriptableObject, có thể null) |
| `RewardGold` | `int` | Gold khi bị tiêu diệt |
| `RewardXP` | `int` | XP khi bị tiêu diệt |

---

## 5. EntityFactory

**File:** `Scripts/Combat/Entities/EntityFactory.cs`
**Pattern:** Static class (no instantiation)

### Trách nhiệm
Tập trung logic tạo entity — không tạo `new Character(...)` ở bất kỳ đâu ngoài factory.

### API

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `CreateCharacter(model, index)` | `CharacterDataModel, int` | `Character` | Từ model trực tiếp |
| `CreateCharacter(id, index)` | `string, int` | `Character` | Qua DataManager |
| `CreateParty(characterIds)` | `IEnumerable<string>` | `List<Character>` | Tạo cả party |
| `CreateEnemy(model, index)` | `EnemyDataModel, int` | `Enemy` | Từ model trực tiếp |
| `CreateEnemy(id, index)` | `string, int` | `Enemy` | Qua DataManager |
| `CreateWave(enemyIds)` | `IEnumerable<string>` | `List<Enemy>` | Tạo cả wave |

> **Lưu ý:** Khi dùng overload nhận `id`, `DataManager.Instance` phải đã khởi tạo trước.

---

## 6. HealthComponent

**File:** `Scripts/Combat/Components/HealthComponent.cs`
**Implements:** `IEntityComponent`

### State
```
_entityId : string
_maxHP    : int
_currentHP: int
_shield   : int
```

### Thứ tự xử lý damage
```
rawDamage → absorb shield trước → reduce HP → publish events
```

### Methods

| Method | Mô tả |
|--------|-------|
| `Initialize(entityId)` | Gắn với entity (IEntityComponent) |
| `Reset()` | Restore HP = MaxHP, shield = 0 |
| `SetMaxHP(max)` | Set MaxHP và reset currentHP về max |
| `TakeDamage(rawDamage, sourceId)` | Xử lý damage (shield hấp thụ trước), return actual HP lost |
| `Heal(amount, sourceId)` | Hồi HP, return actual HP healed |
| `AddShield(amount)` | Tăng shield |
| `RemoveShield(amount)` | Giảm shield |
| `ClearShield()` | Xóa toàn bộ shield |

### Events published

| Tình huống | Event |
|------------|-------|
| Nhận damage | `DamageTakenEvent(entityId, sourceId, actualDamage, isCrit)` |
| HP về 0 | `EntityDeathEvent(entityId, sourceId)` |
| Hồi HP | `HealingReceivedEvent(entityId, sourceId, actualHeal)` |

---

## 7. StatsComponent

**File:** `Scripts/Combat/Components/StatsComponent.cs`
**Implements:** `IEntityComponent`

### Trách nhiệm
Giữ base stats và danh sách `StatModifier` đang active. Tính effective stat sau tất cả modifier.

### Công thức effective stat
```
effectiveStat = (base + Σ flatBonus) × (1 + Σ percentBonus)
min = 0 (không âm)
```

### Methods

| Method | Mô tả |
|--------|-------|
| `Initialize(entityId)` | |
| `Reset()` | Xóa toàn bộ modifier |
| `SetBaseStats(stats)` | Gán EntityStats ban đầu |
| `GetEffectiveStat(StatType)` | Tính stat sau modifier (float) |
| `AddModifier(mod)` | Thêm StatModifier |
| `RemoveModifier(mod)` | Gỡ cụ thể |
| `RemoveModifiersBySource(sourceId)` | Gỡ theo nguồn gốc |
| `ClearModifiers()` | Gỡ tất cả |
| `TickModifiers()` | Giảm duration từng modifier, xóa expired |
| `GetActiveModifiers()` | Lấy list modifier còn active |

### StatType & ModifierType enums

```csharp
enum StatType    { HP, ATK, DEF, SPD, CritRate, Resist }
enum ModifierType { Flat, Percentage }
```

### StatModifier

| Field | Type | Mô tả |
|-------|------|-------|
| `targetStat` | `StatType` | Stat bị ảnh hưởng |
| `type` | `ModifierType` | Flat (cộng thẳng) hay Percentage (nhân %) |
| `value` | `float` | Giá trị (ví dụ: 50f hoặc 0.2f cho +20%) |
| `duration` | `int` | -1 = vĩnh viễn, 0 = expired, >0 = số lượt |
| `sourceId` | `string` | "skill_xxx" hoặc "effect_xxx" để trace |

---

## 8. EffectComponent

**File:** `Scripts/Combat/Components/EffectComponent.cs`
**Implements:** `IEntityComponent`

### Trách nhiệm
Container của `StatusEffect` list. Xử lý add/tick/remove với stack logic.

### Methods

| Method | Mô tả |
|--------|-------|
| `Initialize(entityId)` | |
| `Reset()` | Xóa tất cả effect (không gọi OnRemove) |
| `AddEffect(effect, owner)` | Nếu cùng EffectId đã có → `OnStack`. Nếu mới → `OnApply` + publish event |
| `RemoveEffect(effect, owner)` | Gỡ + `OnRemove` + publish event |
| `TickEffects(owner, timing)` | Tick tất cả effect phù hợp timing, auto-remove expired |
| `HasEffect(effectId)` | Kiểm tra theo ID |
| `HasEffect<T>()` | Kiểm tra theo type |
| `GetEffect<T>()` | Lấy instance theo type |
| `GetActiveEffects()` | Toàn bộ effects đang active |
| `ClearEffects(owner)` | Gỡ tất cả và gọi OnRemove cho từng cái |

### Events published

| Tình huống | Event |
|------------|-------|
| Add effect mới | `StatusEffectAppliedEvent(entityId, effectId, duration)` |
| Remove effect | `StatusEffectRemovedEvent(entityId, effectId)` |

---

## 9. StatusEffect (Abstract Base)

**File:** `Scripts/Combat/Effects/StatusEffect.cs`

### Properties

| Property | Type | Mô tả |
|----------|------|-------|
| `EffectId` | `string` | "bleed", "burn", "heal_regen", "shield", "stun" |
| `Duration` | `int` | Số lượt còn lại. 0 = expired |
| `Intensity` | `float` | Cường độ: damage/tick, heal amount, shield amount |
| `StackCount` | `int` | Số stack hiện tại |
| `TickTiming` | `TickTiming` | StartTurn / EndTurn / OnHit / OnBeingHit / Immediate |
| `IsExpired` | `bool` | `Duration <= 0` |

### Lifecycle hooks (abstract)

| Method | Khi nào gọi |
|--------|------------|
| `OnApply(target)` | Lần đầu apply lên entity |
| `OnTick(target)` | Mỗi khi tick theo TickTiming |
| `OnRemove(target)` | Khi bị gỡ (hết hạn hoặc dispel) |
| `OnStack(incoming)` | Khi cùng EffectId được apply lại (default: refresh duration) |

### TickTiming enum

| Value | Khi tick |
|-------|---------|
| `StartTurn` | Đầu lượt của entity bị ảnh hưởng |
| `EndTurn` | Cuối lượt của entity bị ảnh hưởng |
| `OnHit` | Khi đánh trúng (trigger từ ngoài) |
| `OnBeingHit` | Khi bị đánh (trigger từ ngoài) |
| `Immediate` | Đã xử lý ngay lúc OnApply |

---

## 10. Các StatusEffect cụ thể

| Class | EffectId | TickTiming | OnTick behavior |
|-------|----------|------------|-----------------|
| `BleedEffect` | "bleed" | `EndTurn` | `TakeDamage(Intensity * StackCount)` |
| `BurnEffect` | "burn" | `EndTurn` | `TakeDamage(Intensity)`, tăng theo stack |
| `HealEffect` | "heal_regen" | `StartTurn` | `Heal(Intensity)` |
| `ShieldEffect` | "shield" | `Immediate` | `AddShield(Intensity)` trong OnApply |
| `StunEffect` | "stun" | `StartTurn` | Flag `isStunned = true`, skip turn |

### Stack behavior

| Effect | OnStack |
|--------|---------|
| `BleedEffect` | Tăng `StackCount`, refresh duration |
| `BurnEffect` | Tăng `Intensity` |
| `HealEffect` | Refresh duration |
| `ShieldEffect` | Add thêm shield (cumulative) |
| `StunEffect` | Refresh duration |

---

## 11. EntityStats

**File:** `Scripts/Combat/Stats/EntityStats.cs`
**Pattern:** Plain C# class (serializable)

### Constructor
```csharp
public EntityStats(int hp, int atk, int def, int spd, float crit = 0.05f, float res = 0f)
```

### Properties
`MaxHP`, `CurrentHP`, `Attack`, `Defense`, `Speed`, `CritRate`, `Resist`, `IsDead`, `HPPercent`

### Methods
`SetCurrentHP(value)`, `ModifyHP(delta)`, `Heal(amount)`, `TakeDamage(damage)`, `FullHeal()`, `Reset()`, `Clone()`
