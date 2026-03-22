# 08 — Runtime Flows — Detailed Call Chains

## 1. Overview

Runtime Flows trace complete call chains for major combat scenarios. Each scenario shows:
- When events publish
- What visual systems respond
- Timing of animations and effects
- State transitions

---

## 2. Scenario 1: Player Uses Physical Attack

### Timeline & Call Chain

```
T=0.0s: Player clicks "Attack" button
├─ ActionPanel.OnSkillButtonClicked()
│  └─ EventBus.Publish(PlayerActionSelectedEvent)
│
├─ CombatFlowController receives event (Sprint 01)
│  └─ ActionValidator.ValidateAction()
│
T=0.05s: Combat validates action (quick)
├─ CombatFlowController.ExecuteAction()
│  └─ EventBus.Publish(SkillCastStartedEvent)
│
T=0.05s: CombatEventBridge.OnSkillCastStarted()
├─ CharacterView.PlayActionAnimation("Attack")
│  ├─ animator.SetTrigger("Attack")
│  └─ StartCoroutine(AttackSequence) with 0.3s damage frame
├─ ParticleEffectController.PlaySkillVFX("slash")
├─ AudioManager.PlaySFX("sfx_slash")
└─ BattleLogDisplay.AddLog("Player used Attack!")
│
T=0.3s: Damage moment reached
├─ CharacterAnimator.OnDamageMomentReached()
├─ ActionResolver.ResolveDamage()
│  └─ Calculate damage: baseDmg + critChance + stats
│
T=0.3s: Combat publishes DamageTakenEvent
├─ CombatEventBridge.OnDamageTaken()
├─ EnemyView.PlayDamageVisual()
│  └─ Play "Hit" animation
├─ EnemyView.UpdateHealthBar(newHP)
├─ FloatingTextController.ShowDamageNumber(damage)
│  └─ Create pooled float text with "-35"
├─ ParticleEffectController.PlayImpactVFX()
├─ AudioManager.PlaySFX("sfx_hit")
└─ BattleLogDisplay.AddLog("Enemy took 35 damage!")
│
T=0.5s: Damage number fades out
└─ FloatingTextController returns object to pool
│
T=0.8s: Animation finishes
├─ CharacterAnimator.PlayAnimation("Idle")
├─ ActionResolver.Finish()
└─ EventBus.Publish(ActionFinishedEvent)
│
T=0.8s: CombatFlowController.NextTurn()
├─ EventBus.Publish(TurnChangedEvent)
│
└─ CombatEventBridge.OnTurnChanged()
   └─ TurnOrderDisplay.UpdateTurnOrder()
```

---

## 3. Scenario 2: Enemy Uses Fireball (AOE Spell)

### Timeline & Call Chain

```
T=0.0s: AIController.DecideAction() (no user input)
├─ TargetSelector.SelectTargets()
│  └─ Returns all player characters
├─ ActionValidator.ValidateAction()
└─ EventBus.Publish(SkillCastStartedEvent)
│
T=0.0s: CombatEventBridge.OnSkillCastStarted()
├─ EnemyView.ShowTelegraph()
│  ├─ Draw AOE radius indicator
│  └─ Show target preview
├─ EnemyView.PlayActionAnimation("Spell")
├─ ParticleEffectController.PlaySkillVFX("fireball_windup")
├─ AudioManager.PlaySFX("sfx_magic_cast")
└─ BattleLogDisplay.AddLog("Enemy is casting Fireball!")
│
T=0.2s: Telegraph disappears
└─ EnemyView.HideTelegraph()
│
T=0.7s: Damage moment reached (projectile travels)
├─ ActionResolver.ResolveDamage()
│  ├─ For each target:
│  │  └─ Calculate damage
│  └─ EventBus.Publish(DamageTakenEvent) × 2
│
T=0.7s: CombatEventBridge.OnDamageTaken() × 2
├─ [Target 1] CharacterView.PlayDamageVisual()
├─ [Target 1] FloatingTextController.ShowDamageNumber("42")
├─ [Target 1] ParticleEffectController.PlayImpactVFX()
├─ [Target 2] CharacterView.PlayDamageVisual()
├─ [Target 2] FloatingTextController.ShowDamageNumber("38")
├─ [Target 2] ParticleEffectController.PlayImpactVFX()
├─ ParticleEffectController.PlaySkillVFX("fireball_impact")
├─ AudioManager.PlaySFX("sfx_explosion")
└─ BattleLogDisplay.AddLog("Player 1 took 42 damage!")
└─ BattleLogDisplay.AddLog("Player 2 took 38 damage!")
│
T=1.2s: Animation finishes
├─ EnemyView.PlayAnimation("Idle")
└─ EventBus.Publish(ActionFinishedEvent)
│
T=1.2s: TurnManager.NextTurn()
└─ [Turn continues]
```

---

## 4. Scenario 3: Character Takes Critical Damage

### Timeline & Call Chain

```
T=0.3s: ActionResolver.ResolveDamage()
├─ Calculate damage = 45
├─ Check critical: 20% chance → HIT
├─ Final damage = 45 × 2.0 = 90
├─ Entity.TakeDamage(90)
├─ Health: 100 → 10 HP
└─ EventBus.Publish(DamageTakenEvent { IsCritical=true })
│
T=0.3s: CombatEventBridge.OnDamageTaken()
├─ CharacterView.PlayDamageVisual()
│  ├─ Play "Hit" animation
│  ├─ Greater knockback (more intense)
│  └─ Apply red flash (critical indicator)
├─ CharacterView.UpdateHealthBar(10, 100)
│  └─ Bar fills only 10%, color = red
├─ FloatingTextController.ShowDamageNumber()
│  ├─ Text: "-90"
│  ├─ Color: orange/red (CRITICAL)
│  ├─ Size: larger than normal
│  └─ Add screen shake effect
├─ ParticleEffectController.PlayImpactVFX("crit_hit")
├─ AudioManager.PlaySFX("sfx_crit_hit")  // Special sound
│
├─ MusicController.UpdateIntensity(10%)
│  └─ Transition BGM: normal → danger
│
└─ BattleLogDisplay.AddLog("Player took 90 CRITICAL damage!")
```

---

## 5. Scenario 4: Healing Effect

### Timeline & Call Chain

```
T=0.0s: SkillAction.Execute() (Heal spell)
├─ ActionValidator.ValidateAction()
├─ EventBus.Publish(SkillCastStartedEvent)
│
T=0.0s: CombatEventBridge.OnSkillCastStarted()
├─ CharacterView.PlayActionAnimation("Spell")
├─ ParticleEffectController.PlaySkillVFX("heal_cast")
├─ AudioManager.PlaySFX("sfx_magic_cast")
└─ BattleLogDisplay.AddLog("Player is casting Heal!")
│
T=0.5s: Damage moment = healing moment
├─ ActionResolver.ResolveHealing()
│  ├─ Calculate healing = 35
│  └─ Target.Health.Heal(35)
└─ EventBus.Publish(HealingEvent)
│
T=0.5s: CombatEventBridge.OnHealing()
├─ CharacterView.PlayHealAnimation()
│  └─ Play healing glow animation
├─ CharacterView.UpdateHealthBar(135, 100)  // Overflow capped
├─ FloatingTextController.ShowHealingNumber(35)
│  ├─ Text: "+35"
│  ├─ Color: green
│  └─ Position: above character
├─ ParticleEffectController.PlaySkillVFX("heal_impact")
├─ AudioManager.PlaySFX("sfx_heal")
└─ BattleLogDisplay.AddLog("Player healed 35 HP!")
│
T=1.0s: Animation finishes
└─ CharacterView.PlayAnimation("Idle")
```

---

## 6. Scenario 5: Status Effect Application

### Timeline & Call Chain

```
T=0.3s: ActionResolver.ResolveEffect()
├─ Check effect hit chance: 70% → HIT
├─ Apply StatusEffect: Poison (3 turns)
├─ Entity.AddStatusEffect(poison)
└─ EventBus.Publish(StatusEffectAppliedEvent)
│
T=0.3s: CombatEventBridge.OnStatusEffectApplied()
├─ CharacterView.AddStatusEffect("Poison", duration=3)
│  └─ Add icon to effect grid
├─ EffectIndicatorUI.UpdateDuration(remaining=3)
├─ ParticleEffectController.PlayStatusEffectVFX("poison")
│  └─ Show purple particle cloud
├─ AudioManager.PlaySFX("sfx_poison")
└─ BattleLogDisplay.AddLog("Enemy was poisoned!")
│
T=0.5s: VFX fades
│
Every turn (while active):
├─ EffectTickEvent published
├─ ParticleEffectController.PlayStatusTickVFX("poison")
└─ BattleLogDisplay.AddLog("Enemy takes 5 poison damage!")
│
After 3 turns: Effect expires
├─ EventBus.Publish(StatusEffectRemovedEvent)
├─ CharacterView.RemoveStatusEffect("Poison")
└─ EffectIndicatorUI.RemoveIcon("Poison")
```

---

## 7. Scenario 6: Entity Dies

### Timeline & Call Chain

```
T=0.3s: DamageTakenEvent published
├─ Health = 5 HP remaining
├─ New health: 5 - 45 = -40 (death)
└─ Combat detects death
│
T=0.3s: Entity.Die()
├─ Health set to 0
├─ Status: Alive = false
├─ EventBus.Publish(EntityDiedEvent)
│
T=0.3s: CombatEventBridge.OnEntityDied()
├─ EnemyView.PlayDeath()
│  ├─ animator.SetTrigger("Die")
│  └─ animator.SetBool("IsDead", true)
├─ ParticleEffectController.PlayDeathVFX()
│  └─ Body dissipates effect
├─ AudioManager.PlaySFX("sfx_death")
├─ BattleLogDisplay.AddLog("Enemy has been defeated!")
├─ StatusDisplay.RemoveCharacterStatus(enemyId)
│
T=1.5s: Death animation finishes
├─ EntityViewFactory.RemoveView(enemyId)
│  └─ Destroy EnemyView GameObject
│
T=1.5s: CombatFlowController checks victory
├─ All enemies dead?
├─ EventBus.Publish(BattleEndedEvent { IsVictory=true })
│
T=1.5s: CombatEventBridge.OnBattleEnded()
├─ MusicController.PlayVictoryMusic()
├─ CombatUIManager.ShowVictoryScreen()
│  └─ Display Level Up, Rewards
└─ Disable action panel
```

---

## 8. Scenario 7: Turn Order Update

### Timeline & Call Chain

```
T=0.8s: Previous action finishes
├─ TurnManager.CalculateNextTurn()
│  └─ CTB: Calculate turn based on speed stats
├─ TurnManager.SetCurrentActor(nextActorId)
└─ EventBus.Publish(TurnChangedEvent)
│
T=0.8s: CombatEventBridge.OnTurnChanged()
├─ TurnOrderDisplay.UpdateTurnOrder()
│  ├─ Get turn list: [Enemy_1, Player_2, Player_1, Enemy_2, ...]
│  ├─ Highlight Enemy_1 (current)
│  └─ Show next 4 in queue
│
├─ Determine actor type
├─ If Player turn:
│  ├─ CombatUIManager.OnPlayerTurnStart()
│  │  └─ ActionPanel.EnablePanel()
│  └─ Wait for player input
│
└─ If Enemy turn:
   ├─ CombatUIManager.OnEnemyTurnStart()
   │  └─ ActionPanel.DisablePanel()
   └─ AIController.DecideAction(async)
      └─ 0.5-1.5s thinking time
```

---

## 9. Critical Timing Windows

| Event | Window | Purpose |
|-------|--------|---------|
| SkillCastStartedEvent | T=0.0s | Animation starts |
| Animation damage frame | T=0.3-0.7s | Action resolves |
| DamageTakenEvent | T=0.3-0.7s | Damage number shown |
| ActionFinishedEvent | T=0.8-1.2s | Return to idle |
| TurnChangedEvent | T=0.8-1.2s | Update turn UI |

---

## 10. Concurrent Events Handling

When multiple targets hit simultaneously:

```
T=0.7s: Fireball hits 2 players
├─ DamageTakenEvent (Target1) published
│  └─ CombatEventBridge.OnDamageTaken() executed immediately
├─ DamageTakenEvent (Target2) published
│  └─ CombatEventBridge.OnDamageTaken() executed immediately
│
Visual layer must handle:
├─ 2 animations playing simultaneously ✓
├─ 2 damage numbers floating ✓
├─ 2 impact VFX playing ✓
└─ Audio mixing (no clipping) ✓
```

---

## 11. Performance Profiling Checkpoints

Key performance measurements:

```
Frame 1:
├─ SkillCastStartedEvent publish: < 1ms
├─ All event handlers complete: < 3ms
└─ Memory allocation: 0 bytes

Frame time (T=0.3s):
├─ Animation playback: 0.5ms
├─ VFX update: 1.0ms
├─ UI update: 0.5ms
├─ Audio playback: minimal
└─ Total frame time: < 16.67ms (60 FPS)
```

---

## 12. Race Condition Prevention

**Potential issues to avoid:**

```csharp
// ❌ BAD: Animation finishes before damage applies
T=0.3: PlayAnimation(0.3s duration)
T=0.3: Attack finishes
T=0.4: Damage applies ← LATE!

// ✓ GOOD: Wait for exact damage moment
T=0.3: PlayAnimation(0.8s duration)
  Wait 0.3s
T=0.3: Damage applies ← TIMED
  Wait 0.5s more
T=0.8: Animation finishes ← SYNC
```

Use `WaitForSeconds(exact_time)` not animation events.

---

## 13. Debugging & Logs

Recommended logging for tracing:

```csharp
[SerializeField] private bool debugLogging = true;

private void LogEvent(string eventName, object data) {
    if (!debugLogging) return;
    Debug.Log($"[{Time.time:F2}s] {eventName}: {data}");
}

// Usage in CombatEventBridge
LogEvent("SkillCastStarted", evt.SkillId);
LogEvent("DamageTaken", $"{evt.TargetId} took {evt.Damage}");
LogEvent("AnimationFinished", animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
```

---

## 14. Runtime Flow Checklist

- [ ] All events publish in correct order
- [ ] Timing windows match combat logic
- [ ] Animations complete before next action
- [ ] No race conditions between systems
- [ ] Concurrent animations handled properly
- [ ] Memory allocation minimal (pool reuse)
- [ ] Frame rate maintained (60 FPS)
- [ ] GC spikes avoided
- [ ] Audio/visual sync perfect
