# 07 — Event Integration — Bridge Pattern & Energy Flow

## 1. Overview

Event Integration bridges the gap between Sprint 01 (Combat Engine) and Sprint 02 (Visual Layer). It ensures:
- No direct calls between layers
- One-way dependency (Combat → EventBus → Visual)
- Clean separation of concerns
- Easy to test and maintain

---

## 2. Event Architecture

### 2.1 Event Flow Diagram

```
Sprint 01 (Combat Logic)
    │
    ├─ SkillManager.CastSkill()
    ├─ ActionResolver.Execute()
    └─ TurnManager.NextTurn()
    │
    ▼
EventBus (Publish events)
    │
    ├─ SkillCastStartedEvent
    ├─ SkillCastFinishedEvent
    ├─ DamageTakenEvent
    ├─ HealingEvent
    ├─ StatusEffectAppliedEvent
    ├─ EntityDiedEvent
    ├─ TurnChangedEvent
    └─ BattleStateChangedEvent
    │
    ▼
Sprint 02 (Visual Layer - Subscribers)
    ├─ CombatEventBridge
    │   ├─ CharacterView
    │   ├─ CharacterAnimator
    │   ├─ ParticleEffectController
    │   ├─ CombatUIManager
    │   └─ AudioManager
    └─ Other listeners
```

---

## 3. CombatEventBridge (Main Integration Point)

**File**: `Scripts/Visual/Bridge/CombatEventBridge.cs`

**Responsibility**: Subscribe to combat events and coordinate all visual updates.

### 3.1 Class Structure

```csharp
public class CombatEventBridge : MonoBehaviour {
    [SerializeField] private EntityViewFactory viewFactory;
    [SerializeField] private ParticleEffectController effectController;
    [SerializeField] private CombatUIManager uiManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private MusicController musicController;
    
    private Dictionary<string, CharacterView> characterViewMap = new();
    private Dictionary<string, EnemyView> enemyViewMap = new();
    
    private CombatFlowController combatFlow;
}
```

### 3.2 Initialization & Subscription

```csharp
private void Start() {
    combatFlow = CombatFlowController.Instance;
    
    // Subscribe to all combat events
    EventBus.Subscribe<SkillCastStartedEvent>(OnSkillCastStarted);
    EventBus.Subscribe<SkillCastFinishedEvent>(OnSkillCastFinished);
    EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
    EventBus.Subscribe<HealingEvent>(OnHealing);
    EventBus.Subscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied);
    EventBus.Subscribe<StatusEffectRemovedEvent>(OnStatusEffectRemoved);
    EventBus.Subscribe<EntityDiedEvent>(OnEntityDied);
    EventBus.Subscribe<TurnChangedEvent>(OnTurnChanged);
    EventBus.Subscribe<BattleStartedEvent>(OnBattleStarted);
    EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
}

private void OnDestroy() {
    // Unsubscribe from all events
    EventBus.Unsubscribe<SkillCastStartedEvent>(OnSkillCastStarted);
    EventBus.Unsubscribe<SkillCastFinishedEvent>(OnSkillCastFinished);
    EventBus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
    EventBus.Unsubscribe<HealingEvent>(OnHealing);
    EventBus.Unsubscribe<StatusEffectAppliedEvent>(OnStatusEffectApplied);
    EventBus.Unsubscribe<StatusEffectRemovedEvent>(OnStatusEffectRemoved);
    EventBus.Unsubscribe<EntityDiedEvent>(OnEntityDied);
    EventBus.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
    EventBus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
    EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
}
```

---

## 4. Event Handlers in Detail

### 4.1 Skill Cast Handler

```csharp
private void OnSkillCastStarted(SkillCastStartedEvent evt) {
    // Get caster and target views
    CharacterView casterView = GetEntityView(evt.CasterId);
    List<CharacterView> targetViews = evt.TargetIds.Select(GetEntityView).ToList();
    
    if (casterView == null) {
        Debug.LogError($"Caster view not found: {evt.CasterId}");
        return;
    }
    
    // Coordinator: trigger multiple systems simultaneously
    
    // 1. Play caster animation
    ActionSnapshot actionSnapshot = GetActionConfig(evt.SkillId);
    casterView.PlayActionAnimation(evt.SkillId);
    
    // 2. Play effect (telegraph, wind-up)
    effectController.PlaySkillVFX(evt.SkillId, casterView.transform.position);
    
    // 3. Play audio
    audioManager.PlaySFX(GetSkillSoundId(evt.SkillId));
    
    // 4. Update UI log
    uiManager.OnSkillCast(evt);
    
    // Schedule damage application at exact moment
    StartCoroutine(WaitForDamageMoment(evt, actionSnapshot));
}

private IEnumerator WaitForDamageMoment(SkillCastStartedEvent evt, ActionSnapshot config) {
    yield return new WaitForSeconds(config.DamageFrameTime);
    
    // At this point, combat engine has already applied damage
    // (Combat is responsible for calling ActionResolver.ResolveDamage())
    // We just trigger visual response
    
    foreach (var targetId in evt.TargetIds) {
        var targetView = GetEntityView(targetId);
        if (targetView != null) {
            // Visual feedback for hit (but damage display comes from DamageTakenEvent)
            effectController.PlayImpactVFX(targetView.transform.position);
        }
    }
}

private void OnSkillCastFinished(SkillCastFinishedEvent evt) {
    CharacterView casterView = GetEntityView(evt.CasterId);
    
    if (casterView != null) {
        casterView.PlayAnimation("Idle");
    }
    
    // Signal to combat that visual animation is done
    // (optional - if combat waits for visual finish)
}
```

### 4.2 Damage Handler

```csharp
private void OnDamageTaken(DamageTakenEvent evt) {
    CharacterView targetView = GetEntityView(evt.TargetId);
    
    if (targetView == null) {
        Debug.LogWarning($"Target view not found: {evt.TargetId}");
        return;
    }
    
    // 1. Play damage animation
    targetView.PlayDamageVisual(evt.Damage, evt.SourcePosition);
    
    // 2. Update health bar
    targetView.UpdateHealthBar(evt.NewHP, evt.MaxHP);
    
    // 3. Show damage number
    effectController.ShowDamageNumber(
        damage: evt.Damage,
        position: targetView.transform.position,
        isCritical: evt.IsCritical
    );
    
    // 4. Play sound
    string hitSound = evt.IsCritical ? "sfx_crit_hit" : "sfx_hit";
    audioManager.PlaySFX(hitSound);
    
    // 5. Update UI
    uiManager.OnDamageTaken(evt);
    
    // 6. Check if target died
    if (evt.NewHP <= 0) {
        // EntityDiedEvent will be published separately
    }
}
```

### 4.3 Healing Handler

```csharp
private void OnHealing(HealingEvent evt) {
    CharacterView targetView = GetEntityView(evt.TargetId);
    
    if (targetView == null) return;
    
    // 1. Play healing animation
    targetView.PlayHealAnimation();
    
    // 2. Update health bar
    targetView.UpdateHealthBar(evt.NewHP, evt.MaxHP);
    
    // 3. Show healing number
    effectController.ShowHealingNumber(
        amount: evt.HealAmount,
        position: targetView.transform.position
    );
    
    // 4. Play sound
    audioManager.PlaySFX("sfx_heal");
    
    // 5. Update UI
    uiManager.OnHealing(evt);
}
```

### 4.4 Status Effect Handlers

```csharp
private void OnStatusEffectApplied(StatusEffectAppliedEvent evt) {
    CharacterView targetView = GetEntityView(evt.TargetId);
    
    if (targetView == null) return;
    
    // 1. Show status indicator
    targetView.AddStatusEffect(evt.EffectType, evt.Duration);
    
    // 2. Play effect VFX
    effectController.PlayStatusEffectVFX(evt.EffectType, targetView.transform.position);
    
    // 3. Play sound
    string sfxId = GetStatusEffectSound(evt.EffectType);
    audioManager.PlaySFX(sfxId);
    
    // 4. Update UI
    uiManager.AddStatusEffectIcon(evt.TargetId, evt.EffectType, evt.Duration);
}

private void OnStatusEffectRemoved(StatusEffectRemovedEvent evt) {
    CharacterView targetView = GetEntityView(evt.TargetId);
    
    if (targetView != null) {
        targetView.RemoveStatusEffect(evt.EffectType);
    }
    
    uiManager.RemoveStatusEffectIcon(evt.TargetId, evt.EffectType);
}
```

### 4.5 Entity Death Handler

```csharp
private void OnEntityDied(EntityDiedEvent evt) {
    CharacterView deadView = GetEntityView(evt.EntityId);
    
    if (deadView == null) return;
    
    // 1. Play death animation
    deadView.PlayDeath();
    
    // 2. Play death VFX
    effectController.PlayDeathVFX(deadView.transform.position);
    
    // 3. Play death sound
    audioManager.PlaySFX("sfx_death");
    
    // 4. Update UI
    uiManager.OnEntityDied(evt);
    
    // 5. Remove view from scene after animation
    StartCoroutine(RemoveViewAfterDelay(evt.EntityId, 1.5f));
}

private IEnumerator RemoveViewAfterDelay(string entityId, float delay) {
    yield return new WaitForSeconds(delay);
    viewFactory.RemoveView(entityId);
}
```

### 4.6 Turn Changed Handler

```csharp
private void OnTurnChanged(TurnChangedEvent evt) {
    // 1. Update turn order UI
    List<string> order = combatFlow.GetTurnOrder();
    uiManager.UpdateTurnOrderDisplay(order);
    
    // 2. Highlight current actor
    CharacterView currentView = GetEntityView(evt.CurrentActorId);
    if (currentView != null) {
        // Visual indicator
        currentView.Highlight();
    }
    
    // 3. If player's turn, enable action panel
    if (combatFlow.IsPlayerTurn()) {
        uiManager.OnPlayerTurnStart(combatFlow.GetPlayerCharacters());
    } else {
        uiManager.OnEnemyTurnStart(combatFlow.GetCurrentEnemy());
    }
}
```

### 4.7 Battle State Handlers

```csharp
private void OnBattleStarted(BattleStartedEvent evt) {
    // Initialize all views
    viewFactory.InitializeAllCharacters(evt.PlayerCharacters);
    viewFactory.InitializeAllEnemies(evt.Enemies);
    
    // Start music
    musicController.StartBattleMusic();
    
    // Show UI
    uiManager.Initialize();
}

private void OnBattleEnded(BattleEndedEvent evt) {
    // Stop animations/updates
    StopAllCoroutines();
    
    // Show result screen
    if (evt.IsVictory) {
        musicController.PlayVictoryMusic();
        uiManager.ShowVictoryScreen(evt.Result);
    } else {
        musicController.PlayDefeatMusic();
        uiManager.ShowDefeatScreen(evt.Result);
    }
}
```

---

## 5. Helper Methods

```csharp
// Get any entity view (character or enemy)
private CharacterView GetEntityView(string entityId) {
    return characterViewMap.TryGetValue(entityId, out var view) ? view
         : (CharacterView)(object)GetEnemyView(entityId);
}

private EnemyView GetEnemyView(string entityId) {
    return enemyViewMap.TryGetValue(entityId, out var view) ? view : null;
}

// Get action animation config
private ActionSnapshot GetActionConfig(string skillId) {
    return ActionAnimationConfig.AnimationDatabase.TryGetValue(skillId, out var config)
        ? config
        : ActionAnimationConfig.DefaultConfig;
}

// Map skill ID to sound ID
private string GetSkillSoundId(string skillId) {
    return skillId switch {
        "skill_slash" => "sfx_slash",
        "skill_fireball" => "sfx_magic_cast",
        "skill_heal" => "sfx_heal",
        _ => "sfx_default"
    };
}

private string GetStatusEffectSound(string effectType) {
    return effectType switch {
        "Poison" => "sfx_poison",
        "Burn" => "sfx_fire",
        "Freeze" => "sfx_ice",
        "Stun" => "sfx_stun",
        _ => "sfx_buff"
    };
}
```

---

## 6. VisualEventPublisher (Reverse Events)

**File**: `Scripts/Visual/Bridge/VisualEventPublisher.cs`

Visual layer can publish its own events for other systems:

```csharp
public class VisualEventPublisher : MonoBehaviour {
    // Custom Visual events
    public event Action<string> OnAnimationFinished;
    public event Action<string> OnVFXFinished;
    public event Action<int> OnUIActionSelected;
    
    public void PublishAnimationFinished(string animationId) {
        OnAnimationFinished?.Invoke(animationId);
    }
    
    public void PublishVFXFinished(string vfxId) {
        OnVFXFinished?.Invoke(vfxId);
    }
    
    public void PublishUIActionSelected(int skillId) {
        OnUIActionSelected?.Invoke(skillId);
    }
}
```

---

## 7. Critical Integration Points

### Point 1: Initialization Order

```
1. GameManager starts
   ↓
2. CombatFlowController initialized (Sprint 01)
   ↓
3. EventBus ready
   ↓
4. CombatEventBridge.Start() subscribes to events
   ↓
5. Combat begins → events publish → visual systems respond
```

### Point 2: Timing Synchronization

```
Combat frame 0:        SkillCastStartedEvent published
Visual frame 0.3:      Animation reaches damage frame
Combat frame 0.3:      ActionResolver applies damage (async)
Visual frame 0.3:      DamageTakenEvent published (combat publishes)
Visual frame 0.3:      ShowDamageNumber()
Audio frame 0.3:       PlaySFX(hit)
Visual frame 0.8:      Animation finishes
Combat frame 0.8:      SkillCastFinishedEvent published
Visual frame 0.8:      Return to Idle animation
```

---

## 8. Error Handling

```csharp
private void OnSkillCastStarted(SkillCastStartedEvent evt) {
    // Defensive checks
    if (evt == null || string.IsNullOrEmpty(evt.SkillId)) {
        Debug.LogError("Invalid SkillCastStartedEvent");
        return;
    }
    
    CharacterView casterView = GetEntityView(evt.CasterId);
    if (casterView == null) {
        Debug.LogWarning($"Caster view not found for {evt.CasterId}");
        return;  // Don't crash, just skip visual feedback
    }
    
    try {
        casterView.PlayActionAnimation(evt.SkillId);
    } catch (System.Exception ex) {
        Debug.LogException(ex);
    }
}
```

---

## 9. Event Bus Contract

**Critical**: Do NOT call combat methods directly from here.

```csharp
// ✓ CORRECT: Event-based
OnSkillSelected?.Invoke(skillId);
EventBus.Publish(new PlayerActionSelectedEvent(skillId));
// Combat.Instance.ExecuteAction(skillId);  // ✗ Do NOT do this

// ✓ Allow read-only queries
var enemyHP = combatFlow.GetEntityHP(enemyId);

// ✗ Do NOT modify combat state
// combatFlow.entity.TakeDamage(50);  // ✗ FORBIDDEN
```

---

## 10. Integration Checklist

- [ ] CombatEventBridge subscribed to all events
- [ ] All event handlers implemented
- [ ] View references properly initialized
- [ ] Animation timing matches combat timing
- [ ] Error handling for missing views
- [ ] Proper cleanup on battle end
- [ ] No direct calls to combat methods
- [ ] No GC allocation in event handlers
- [ ] Tested with various action types
