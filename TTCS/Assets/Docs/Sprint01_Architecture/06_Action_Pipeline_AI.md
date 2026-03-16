# 06 — Action Pipeline & AI System

Mô tả hành trình từ lúc một action được quyết định đến khi hiệu ứng được áp dụng lên entity.

---

## 1. Tổng quan pipeline

```
[Quyết định action]
  Player: SubmitPlayerAction(skillId, targetIds)
  AI:     AIController.DecideAction() → AIDecision

         ↓
[CombatFlowController.ExecuteAction()]
  DataManager.LoadSkill(skillId)
  ResolveTargets(skillData, targetIds)
  new SkillAction(skillData)

         ↓
[SkillAction.Validate(actor, targets, skillManager)]
  ActionValidator.Validate()
  → ValidationResult (IsValid, ErrorMessage)

         ↓ (if IsValid)
[SkillAction.Execute(actor, targets, skillManager, grade)]
  SkillManager.UseSkill()          — commit resource
  ActionResolver.Resolve()         — tính toán & apply effects

         ↓
[ActionResolver]
  → Dispatch theo skill.type
  ├── "attack"  → ResolveAttack()
  ├── "heal"    → ResolveHeal()
  └── "effect"  → ResolveEffectsOnly()

         ↓
[Effects applied to CombatEntity]
  HealthComponent.TakeDamage() / ReceiveHeal()
  EffectComponent.ApplyEffect()
```

---

## 2. IAction interface

**File:** `Scripts/Combat/Actions/IAction.cs`
**Namespace:** `TTCS.Combat.Actions`

```csharp
public interface IAction
{
    string ActionType { get; }
    float TimelineCost { get; }
    ValidationResult Validate(CombatEntity actor, List<CombatEntity> targets, SkillManager skillManager);
    ActionResult Execute(CombatEntity actor, List<CombatEntity> targets, SkillManager skillManager, TimingGrade grade);
}
```

| Property/Method | Mô tả |
|-----------------|-------|
| `ActionType` | "skill", "item",... |
| `TimelineCost` | CTB cost sau action |
| `Validate(...)` | Kiểm tra, không thay đổi state |
| `Execute(...)` | Thực thi, có side effect |

---

## 3. SkillAction

**File:** `Scripts/Combat/Actions/SkillAction.cs`
**Namespace:** `TTCS.Combat.Actions`
**Implements:** `IAction`

### Trách nhiệm
Wrapper đặc thù cho mọi skill combat. Kết nối `SkillDataModel` với validation/execution pipeline.

### Constructor
```csharp
public SkillAction(SkillDataModel skillData)
```

### Properties

| Property | Type | Nguồn |
|----------|------|-------|
| `ActionType` | `string` | `"skill"` |
| `TimelineCost` | `float` | `_skillData.cost.timeline` (default 100) |
| `SkillData` | `SkillDataModel` | readonly |

### Validate(actor, targets, skillManager)
```
Delegate toàn bộ sang ActionValidator.Validate(actor, targets, _skillData, skillManager)
```

### Execute(actor, targets, skillManager, grade)
```
1. skillManager.UseSkill(actor.EntityId, _skillData)   — trừ mana, set cooldown
2. ActionResolver.Resolve(actor, targets, _skillData, grade) — apply effects
3. EventBus.Publish(SkillCastEvent(actorId, skillId, targetIds[]))
4. EventBus.Publish(ActionExecutedEvent(...))
5. return ActionResult(success=true)
```

---

## 4. ActionValidator

**File:** `Scripts/Combat/Validators/ActionValidator.cs`
**Namespace:** `TTCS.Combat.Validators`
**Pattern:** Static utility class

### public static Validate(actor, targets, skillData, skillManager)

Trả về `ValidationResult { bool IsValid, string ErrorMessage }`

Chuỗi kiểm tra (theo thứ tự):

| Bước | Điều kiện | Lỗi nếu fail |
|------|-----------|-------------|
| 1 | `actor != null && !actor.IsDead` | "Actor is null or dead" |
| 2 | `targets != null && targets.Count > 0` | "No valid targets" |
| 3 | Mọi target không null (lọc) | Remove dead nếu skill loại attack |
| 4 | `skillData != null` | "Skill data is null" |
| 5 | `skillManager.CanUseSkill(actorId, skillData)` | "Cannot use skill: {reason}" |
| 6 | Target count phù hợp với `skillData.targeting.maxTargets` | Warning (không fail) |

---

## 5. ActionResolver

**File:** `Scripts/Combat/Resolvers/ActionResolver.cs`
**Namespace:** `TTCS.Combat.Resolvers`
**Pattern:** Static utility class

### Timing grade multipliers

| Situation | Grade | Multiplier |
|-----------|-------|-----------|
| Phòng thủ (guard) | Perfect | ×0.2 damage nhận |
| Phòng thủ (guard) | Good | ×0.6 damage nhận |
| Phòng thủ (guard) | Miss | ×1.0 damage nhận |
| Tấn công | Perfect | ×1.5 damage gây ra |
| Tấn công | Good | ×1.2 damage gây ra |
| Tấn công | Miss | ×1.0 damage gây ra |

### Entry point
```csharp
public static void Resolve(
    CombatEntity actor,
    List<CombatEntity> targets,
    SkillDataModel skill,
    TimingGrade defenderGrade = TimingGrade.Miss)
```

### Dispatch logic
```
switch (skill.type):
  "attack" → ResolveAttack(actor, target, skill, defenderGrade) foreach target
  "heal"   → ResolveHeal(actor, target, skill) foreach target
  "effect" → ResolveEffectsOnly(actor, target, skill) foreach target
  default  → log warning
```

### ResolveAttack flow
```
1. attackMult = skill.attackMultiplier (or 1.0f default)
2. timingMult = GetAttackTimingMultiplier(attackerGrade)
3. finalMult  = attackMult * timingMult
4. damage = StatCalculator.CalculateDamage(actor, target, finalMult, skill.element)
5. guardMult  = GetGuardMultiplier(defenderGrade)
6. finalDmg  = (int)(damage * guardMult)
7. target.HealthComponent.TakeDamage(finalDmg, actor.EntityId)
8. EventBus.Publish(DamageTakenEvent(target.EntityId, actor.EntityId, finalDmg, isCrit))
9. ApplyEffects(actor, target, skill.onHitEffects) nếu có
```

### ResolveHeal flow
```
1. healAmount = StatCalculator.CalculateHeal(actor, skill.healMultiplier)
2. target.HealthComponent.ReceiveHeal(healAmount)
3. EventBus.Publish(HealingReceivedEvent(target.EntityId, actor.EntityId, healAmount))
4. ApplyEffects(actor, target, skill.onHitEffects) nếu có
```

### ResolveEffectsOnly flow
```
foreach effect in skill.effects:
    target.EffectComponent.ApplyEffect(effect, actor.EntityId)
    EventBus.Publish(StatusEffectAppliedEvent(...))
```

---

## 6. StatCalculator

**File:** `Scripts/Combat/Resolvers/StatCalculator.cs`
**Namespace:** `TTCS.Combat.Resolvers`
**Pattern:** Static utility class

### Hằng số
```
MAX_DEF_REDUCTION = 0.75f   (DEF không thể cắt giảm quá 75% damage)
```

### Công thức tính sát thương

```
CalculateDamage(attacker, defender, multiplier, element, critMultiplier=1.5f):

1. baseDmg  = attacker.Stats.ATK * multiplier
2. defFactor = CalculateDefenseReduction(defender.Stats.DEF)
              = DEF / (DEF + 100)          // soft cap formula
              = clamp(result, 0, MAX_DEF_REDUCTION)
3. netDmg   = baseDmg * (1f - defFactor)
4. isCrit   = RNGService.RollCrit(attacker.Stats.CritRate)
5. if isCrit: netDmg *= critMultiplier
6. elemMult = GetElementMultiplier(element, defender.element)  // stub = 1.0f
7. finalDmg = (int)(netDmg * elemMult)
8. return (max(1, finalDmg), isCrit)
```

**Ví dụ:** ATK=100, DEF=50, mult=1.2
- baseDmg = 120
- defFactor = 50/150 = 0.333
- netDmg = 120 × 0.667 = 80
- (nếu crit ×1.5) = 120

### Công thức hồi máu

```
CalculateHeal(caster, multiplier):
  healAmount = caster.Stats.ATK * multiplier
  return (int)max(1, healAmount)
```

### Public API

| Method | Tham số | Trả về | Mô tả |
|--------|---------|--------|-------|
| `CalculateDamage(attacker, defender, mult, element, critMult)` | — | `(int dmg, bool isCrit)` | Sát thương cuối |
| `CalculateHeal(caster, mult)` | — | `int` | Lượng hồi |
| `CheckCrit(critRate)` | `float` | `bool` | Dùng RNGService |
| `GetElementMultiplier(atk, def)` | `string, string` | `float` | Luôn 1.0f (stub) |
| `CalculateDefenseReduction(def)` | `int` | `float` | Trả về 0..0.75 |

---

## 7. AI System

### AIController

**File:** `Scripts/Combat/AI/AIController.cs`
**Namespace:** `TTCS.Combat.AI`
**Pattern:** Static utility class (không có state, không phải MonoBehaviour)

#### Entry point
```csharp
public static AIDecision DecideAction(
    CombatEntity self,
    List<CombatEntity> allEntities,
    AIBehavior behavior,
    SkillManager skillManager)
```

#### Decision tree flow

```
DecideAction():
  ├── selfHpRatio = self.HealthComponent.CurrentHP / MaxHP
  │
  ├── [Heal check]
  │     if selfHpRatio <= behavior.hpThresholdHeal
  │    && behavior.healSkillId != null
  │    && skillManager.CanUseSkill(self.id, behavior.healSkillId)
  │      → return AIDecision { skill=healSkill, targets=[self] }
  │
  ├── [Aggressive check]
  │     lowestHpEnemy = GetLowestHpPlayer()
  │     enemyHpRatio  = lowestHpEnemy.HP / MaxHP
  │     if enemyHpRatio <= behavior.hpThresholdAggressive
  │    && behavior.strongAttackSkillId != null
  │    && skillManager.CanUseSkill(...)
  │      → return AIDecision { skill=strongAttack, targets=[lowestEnemy] }
  │
  ├── [Skill preferences]
  │     foreach entry in behavior.skillPreferences (by priority):
  │       if skillManager.CanUseSkill(self.id, entry.skillId):
  │         targets = TargetSelector.SelectTargets(entry.targetRule, allEntities)
  │         → return AIDecision { skill=entry.skillId, targets=targets }
  │
  └── [Fallback]
      targets = TargetSelector.SelectTargets("single_enemy", allEntities)
      → return AIDecision { skill=behavior.basicAttackSkillId, targets }
```

#### AIDecision struct

```csharp
public struct AIDecision
{
    public string SkillId;
    public List<CombatEntity> Targets;
    public bool IsValid;
}
```

---

### AIBehavior

**File:** `Scripts/Combat/AI/AIBehavior.cs`
**Namespace:** `TTCS.Combat.AI`
**Pattern:** ScriptableObject

Mỗi loại enemy có một `AIBehavior` asset riêng trong `Data/AI/`.

#### Inspector fields

| Field | Type | Default | Mô tả |
|-------|------|---------|-------|
| `hpThresholdHeal` | `float` | 0.3f | Dưới % HP → dùng heal |
| `hpThresholdAggressive` | `float` | 0.5f | Địch dưới % HP → tấn công mạnh |
| `aggression` | `float` | 0.5f | (reserved) |
| `defensiveness` | `float` | 0.5f | (reserved) |
| `basicAttackSkillId` | `string` | — | Fallback khi không có gì khả dụng |
| `strongAttackSkillId` | `string` | null | Skill tấn công mạnh |
| `healSkillId` | `string` | null | Skill tự heal |
| `skillPreferences` | `SkillPreference[]` | [] | Ưu tiên skill theo thứ tự |
| `preferredTargetRule` | `string` | "single_enemy" | Rules dùng cho basic attack |

#### SkillPreference struct
```csharp
[Serializable]
public struct SkillPreference
{
    public string skillId;
    public string targetRule;   // overrides default targeting
    public int priority;        // sort ascending before use
}
```

---

### TargetSelector

**File:** `Scripts/Combat/AI/TargetSelector.cs`
**Namespace:** `TTCS.Combat.AI`
**Pattern:** Static utility class

```csharp
public static List<CombatEntity> SelectTargets(
    string targetRule,
    List<CombatEntity> allEntities,
    CombatEntity self = null)
```

#### Các target rule

| Rule | Hành vi |
|------|---------|
| `"single_enemy"` | 1 enemy còn sống, HP thấp nhất |
| `"all_enemies"` | Tất cả enemy còn sống |
| `"single_ally"` | 1 ally còn sống, HP thấp nhất (trừ self) |
| `"all_allies"` | Tất cả ally còn sống |
| `"lowest_hp"` | Entity bất kỳ HP thấp nhất |
| `"highest_hp"` | Entity bất kỳ HP cao nhất |
| `"random_enemy"` | 1 enemy random (dùng RNGService) |
| `"self"` | Trả về `[self]` |

> **Notes:** "enemy"/"ally" được xác định dựa trên `self.IsPlayer` xOR `target.IsPlayer`. Nếu self=null trong môi trường player dùng rule, trả về danh sách rỗng.

---

## 8. Timing system (enum và kết nối)

**File:** `Scripts/Combat/Enums/TimingGrade.cs`

```csharp
public enum TimingGrade
{
    Perfect,  // Trong _perfectThresholdMs
    Good,     // Trong _goodThresholdMs
    Miss      // Ngoài window hoặc không input
}
```

`CombatFlowController` gán `grade` khi nhận `TimingInputEvent`:
- Khi enemy tấn công → `timingWindow.OpenWindow(duration)` rồi chờ player bấm guard
- Khi player tấn công → `timingWindow.OpenWindow(duration)` rồi chờ player bấm attack timing

TimingGrade rồi được truyền vào `ExecuteAction(actor, skillId, targets, grade)` → `SkillAction.Execute()` → `ActionResolver.Resolve(..., defenderGrade)`.
