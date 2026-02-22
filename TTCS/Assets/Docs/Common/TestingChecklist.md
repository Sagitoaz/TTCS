# ✅ Testing Checklist - TTCS Project

> Danh sách kiểm tra chất lượng cho từng feature và sprint
> **Test before merge, test before release**

---

## 📋 Table of Contents

1. [Unit Testing Guidelines](#unit-testing-guidelines)
2. [Integration Testing](#integration-testing)
3. [Feature Testing Checklists](#feature-testing-checklists)
4. [Sprint Demo Checklist](#sprint-demo-checklist)
5. [Performance Testing](#performance-testing)
6. [Test Case Templates](#test-case-templates)

---

## 🧪 Unit Testing Guidelines

### Test Structure: AAA Pattern

```csharp
[Test]
public void DamageCalculation_WithDefense_ReducesDamage()
{
    // ARRANGE - Setup
    var attacker = new CombatEntity { Attack = 100 };
    var defender = new CombatEntity { Defense = 50 };
    var calculator = new DamageCalculator();
    
    // ACT - Execute
    int damage = calculator.Calculate(attacker, defender);
    
    // ASSERT - Verify
    Assert.Less(damage, attacker.Attack, "Damage should be reduced by defense");
    Assert.Greater(damage, 0, "Damage should still be > 0");
}
```

### Naming Convention

**Format:** `MethodName_Scenario_ExpectedBehavior`

```csharp
// ✅ GOOD
[Test]
public void TakeDamage_WhenHealthAboveZero_ReducesHealth()
[Test]
public void TakeDamage_WhenDamageExceedsHealth_SetsHealthToZero()
[Test]
public void TakeDamage_WhenEntityDead_ThrowsException()

// ❌ BAD
[Test]
public void TestDamage()
[Test]
public void Test1()
[Test]
public void DamageTest()
```

### Coverage Goals

**Minimum Coverage:**
- Core systems: **80%+**
- Combat logic: **70%+**
- Utilities: **60%+**
- UI: **30%+** (focus on logic, not visuals)

**What to test:**
- ✅ Business logic
- ✅ Edge cases
- ✅ Error conditions
- ✅ State transitions
- ❌ Unity lifecycle (Awake, Start, etc.)
- ❌ Third-party libraries

---

## 🔗 Integration Testing

### Combat Flow Integration Test

```csharp
[UnityTest]
public IEnumerator CombatFlow_FullBattle_CompletesSuccessfully()
{
    // Arrange
    var stage = LoadTestStage("stage_test_01");
    var combatController = new CombatController();
    combatController.Initialize(stage);
    
    // Act
    combatController.StartBattle();
    
    // Wait for battle to complete (with timeout)
    float timeout = 30f;
    float elapsed = 0;
    while (!combatController.IsBattleOver && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    // Assert
    Assert.IsTrue(combatController.IsBattleOver, "Battle should complete");
    Assert.IsFalse(elapsed >= timeout, "Battle should not timeout");
    Assert.IsTrue(
        combatController.Result == BattleResult.Victory || 
        combatController.Result == BattleResult.Defeat,
        "Battle should have valid result"
    );
}
```

### AI Decision Test

```csharp
[Test]
public void AIController_LowHealth_UsesHealSkill()
{
    // Arrange
    var enemy = CreateTestEnemy();
    enemy.CurrentHP = enemy.MaxHP * 0.2f; // 20% HP
    var ai = new AIController(enemy);
    
    // Assume enemy has heal skill
    var healSkill = enemy.Skills.First(s => s.Type == SkillType.Heal);
    
    // Act
    var decision = ai.DecideAction();
    
    // Assert
    Assert.AreEqual(healSkill.ID, decision.SkillID, 
        "AI should prioritize heal when low HP");
}
```

---

## 📝 Feature Testing Checklists

### ⚔️ Combat System Checklist

#### Turn Manager
- [ ] **Turn order** calculated correctly based on SPD
- [ ] **Next actor** determined properly  
- [ ] **Action cost** advances initiative correctly
- [ ] **Turn counter** increments
- [ ] **Reset** initializes clean state
- [ ] **Events** fired: `OnTurnStart`, `OnTurnEnd`

**Test Cases:**
```
✓ Single entity takes turn
✓ Multiple entities ordered by SPD
✓ Fast action (cost 80) moves actor up in queue
✓ Slow action (cost 120) delays actor
✓ SPD tied → stable sort by initial order
```

---

#### Damage Calculation
- [ ] **Base damage** formula works: `ATK * multiplier`
- [ ] **Defense** reduces damage correctly
- [ ] **Critical hits** deal extra damage
- [ ] **Element bonus** applies (e.g., Fire vs Ice)
- [ ] **Minimum damage** is 1 (never 0)
- [ ] **True damage** ignores defense
- [ ] **Overflow** handled (damage > MaxInt)

**Test Cases:**
```
✓ Normal attack: 100 ATK → 100 dmg (no defense)
✓ With defense: 100 ATK vs 50 DEF → ~67 dmg
✓ Critical: 100 base → 150 dmg (1.5x multiplier)
✓ Element bonus: Fire vs Ice → 1.5x damage
✓ True damage: Ignores 999 defense
✓ Negative ATK → 1 damage minimum
```

---

#### Status Effects
- [ ] **Application** adds effect to entity
- [ ] **Duration** decrements each tick
- [ ] **Expiration** removes effect when duration = 0
- [ ] **Stacking** works (if stackable)
- [ ] **Max stacks** enforced
- [ ] **Immunity** prevents application
- [ ] **Cleanse** removes effect
- [ ] **Effects tick** in correct order

**Test Cases per Effect:**

**Bleed:**
```
✓ Applies for N turns
✓ Deals damage on tick
✓ Stacks up to max
✓ Damage scales with stacks
✓ Expires after duration
```

**Burn:**
```
✓ Fire DoT applied
✓ Damage = % of max HP
✓ Cannot stack (refreshes duration)
✓ Blocked by burn immunity
```

**Stun:**
```
✓ Skips entity's turn
✓ Duration decrements
✓ Removed after duration
✓ Cannot act while stunned
```

**Shield:**
```
✓ Absorbs damage
✓ Breaks when depleted
✓ Expires after duration
✓ Doesn't prevent status effects
```

---

#### Skill System
- [ ] **Cost check** prevents use if not enough mana
- [ ] **Cooldown** enforced (can't use skill on CD)
- [ ] **Cooldown tick** decrements each turn
- [ ] **Target validation** checks valid targets exist
- [ ] **Effect application** triggers correctly
- [ ] **Multi-target** hits all intended targets
- [ ] **Skill data** loads from JSON/SO correctly

**Test Cases:**
```
✓ Basic attack: no cooldown, always usable
✓ Special skill: 2 turn CD, uses mana
✓ Cannot use skill with CD remaining
✓ Cannot use skill without mana
✓ AoE skill hits all enemies
✓ Heal skill targets allies only
✓ Random target selects valid target
```

---

### 🤖 AI System Checklist

- [ ] **Decision making** runs without errors
- [ ] **Target selection** picks valid target
- [ ] **Skill priority** follows AI profile
- [ ] **Defensive behavior** when low HP
- [ ] **Aggressive behavior** when high HP
- [ ] **Random seed** makes decisions deterministic
- [ ] **No infinite loops** in decision tree

**Test Cases:**
```
✓ AI picks strongest attack when HP > 50%
✓ AI uses heal when HP < 30%
✓ AI uses defensive skill when HP < 20%
✓ AI targets lowest HP enemy
✓ AI doesn't use skill on cooldown
✓ AI behavior consistent with same seed
```

---

### 💾 Save System Checklist

- [ ] **Save** writes file successfully
- [ ] **Load** reads file successfully
- [ ] **Data integrity** - loaded data matches saved
- [ ] **Missing file** handled gracefully (new game)
- [ ] **Corrupted file** detected and handled
- [ ] **Multiple slots** work independently
- [ ] **Save prompt** triggers at appropriate times
- [ ] **Autosave** works (if implemented)

**Test Cases:**
```
✓ Save → Load → Data unchanged
✓ Currency saved and loaded correctly
✓ Roster saved and loaded correctly
✓ Progress flags saved and loaded
✓ Settings saved and loaded
✓ Delete save → New game starts
✓ Corrupted JSON → Error message, doesn't crash
```

---

### 📊 Data Loading Checklist

- [ ] **All characters** load without errors
- [ ] **All skills** load without errors
- [ ] **All enemies** load without errors
- [ ] **All stages** load without errors
- [ ] **Missing ID references** logged as warnings
- [ ] **Invalid data** caught by validation
- [ ] **Loading time** acceptable (<2s for all data)

**Test Cases:**
```
✓ Load 10 characters → No errors
✓ Character references existing skills
✓ Skill references existing effects
✓ Enemy references existing skills
✓ Stage references existing enemies
✓ Missing skill ID → Warning logged, graceful fallback
✓ Invalid stat value → Clamped to valid range
```

---

## 🎮 Sprint Demo Checklist

### Sprint 1 Demo (Week 2 End)

#### Pre-Demo Setup
- [ ] **Build** runs without errors
- [ ] **Scene** loads correctly
- [ ] **No console errors** on startup
- [ ] **Test data** loaded (2 characters, 2 enemies, 1 stage)

#### Demo Flow
- [ ] **1. Start game** → Main menu appears
- [ ] **2. Enter combat** → Scene transitions smoothly
- [ ] **3. Combat UI** → Health bars, skills visible
- [ ] **4. Player turn** → Can select skill
- [ ] **5. Execute action** → Animation plays, damage calculated
- [ ] **6. Enemy turn** → AI makes decision and acts
- [ ] **7. Combat loop** → Turns continue correctly
- [ ] **8. Status effects** → Burn/Bleed ticks visible
- [ ] **9. Victory/Defeat** → Battle ends with result
- [ ] **10. Rewards** → Gold/EXP awarded (if implemented)

#### Backup Scenarios (if main demo fails)
- [ ] Show unit tests passing
- [ ] Show combat log output
- [ ] Show data files loading
- [ ] Manual trigger demo scenario via debug panel

---

## ⚡ Performance Testing

### Frame Rate
**Target:** 60 FPS (16.67ms/frame)

**Test Scenarios:**
- [ ] **Idle scene** → 60 FPS stable
- [ ] **Combat with 3v3** → 60 FPS stable
- [ ] **Multiple VFX** → >55 FPS
- [ ] **Heavy animations** → >50 FPS

**Tools:**
- Unity Profiler
- Stats window (F8 in Game view)

### Memory
**Budget:** <500 MB for mobile, <1 GB for PC

**Test Scenarios:**
- [ ] **Initial load** → <300 MB
- [ ] **After 10 battles** → No leak (memory stable)
- [ ] **Asset loading** → Incremental, not all at once

**Check:**
- GC allocations per frame (<1 KB)
- No memory leaks (Profiler → Memory)

### Load Times
**Targets:**
- Scene load: <2s
- Data load: <1s
- Battle init: <0.5s

---

## 📄 Test Case Templates

### Manual Test Case Template

```markdown
**Test Case ID:** TC_COMBAT_001
**Feature:** Combat System - Damage Calculation
**Priority:** High
**Preconditions:** 
- Game running
- Test scene loaded
- 2 entities spawned

**Steps:**
1. Set Attacker ATK = 100
2. Set Defender DEF = 50
3. Execute basic attack
4. Observe damage dealt

**Expected Result:**
- Damage = ~67 (reduced by defense)
- Health bar updates
- Damage number displayed
- Combat log shows damage

**Actual Result:** _______

**Status:** [ ] Pass [ ] Fail

**Notes:** _______
```

### Bug Report Template

```markdown
**Bug ID:** BUG_001
**Title:** Damage calculation overflow with high ATK values

**Severity:** [ ] Critical [ ] High [x] Medium [ ] Low

**Steps to Reproduce:**
1. Set entity ATK to 999999
2. Attack enemy
3. Observe damage

**Expected Behavior:**
Damage capped at reasonable value

**Actual Behavior:**
Damage shows negative value, entity heals

**Environment:**
- Unity: 2021.3.16f1
- Platform: Windows 11
- Build: Development

**Screenshots/Logs:**
[Attach screenshot]

**Assignee:** _______
**Status:** [ ] Open [ ] In Progress [ ] Fixed [ ] Closed
```

---

## 🔄 Regression Testing

### After Each Change

**Quick Smoke Test (5 min):**
- [ ] Game starts
- [ ] No console errors
- [ ] Combat loads
- [ ] One battle completes

**Full Regression (30 min):**
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Manual test: Full combat loop
- [ ] Manual test: Save/Load
- [ ] Manual test: UI navigation

### Before Merge to Main

**Complete Test Suite:**
- [ ] All automated tests pass
- [ ] No new warnings
- [ ] Performance metrics met
- [ ] Code review approved
- [ ] Demo scenario works

---

## 📊 Test Metrics Tracking

### Sprint 1 Metrics

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| Unit Test Count | 20+ | ___ | ⬜ |
| Test Coverage | 70%+ | ___ | ⬜ |
| Passing Tests | 100% | ___ | ⬜ |
| Critical Bugs | 0 | ___ | ⬜ |
| Build Success | 100% | ___ | ⬜ |
| Demo Success | Pass | ___ | ⬜ |

**Legend:** ✅ Met | ⚠️ Close | ❌ Not Met | ⬜ Pending

---

## 🎯 Definition of Done (Testing Perspective)

Feature is "done" when:

- ✅ **Unit tests** written and passing
- ✅ **Integration test** passed (if applicable)
- ✅ **Manual testing** completed via checklist
- ✅ **No critical bugs** found
- ✅ **Performance** acceptable (no lag)
- ✅ **Code review** included test review
- ✅ **Demo-able** - Can show working feature

---

## 🛠️ Testing Tools Setup

### Unity Test Framework

**Installation:**
1. Window → Package Manager
2. Search "Test Framework"
3. Install

**Create Test Assembly:**
```
Assets/Tests/EditMode/
Assets/Tests/PlayMode/
```

**Example Test File:**
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CombatTests
{
    [Test]
    public void DamageCalculation_BasicTest()
    {
        // Test code
        Assert.AreEqual(100, damage);
    }
}
```

### Running Tests

**Editor:**
- Window → General → Test Runner
- Run All / Run Selected

**Command Line:**
```bash
Unity.exe -runTests -projectPath <path> -testResults <results.xml>
```

---

## 📚 Best Practices

### DO:
- ✅ Write tests for complex logic
- ✅ Test edge cases (0, negative, max values)
- ✅ Use descriptive test names
- ✅ Keep tests fast (<100ms each)
- ✅ Make tests independent (no shared state)
- ✅ Test one thing per test
- ✅ Use setup/teardown properly

### DON'T:
- ❌ Test Unity framework code
- ❌ Test third-party libraries
- ❌ Write flaky tests (random failures)
- ❌ Have tests depend on execution order
- ❌ Skip tests (fix or delete them)
- ❌ Test implementation details (test behavior)

---

## 🎓 Learning Resources

- [Unity Test Framework Docs](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [NUnit Documentation](https://docs.nunit.org/)
- [Test-Driven Development in Unity](https://unity.com/how-to/unit-testing-unity-projects)

---

**Remember:** 

> "Testing is not about finding bugs in code.  
> It's about building confidence that code works."

---

*Version: 1.0*  
*Last Updated: February 22, 2026*  
*Maintained by: TTCS QA Team*
