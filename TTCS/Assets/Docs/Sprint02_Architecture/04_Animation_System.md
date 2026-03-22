# 04 — Animation System — State Management & Sequencing

## 1. Overview

Animation System quản lý toàn bộ character animation sequences và state transitions. Nó đảm bảo:
- Animations play đúng thứ tự
- Damage moment sync chính xác
- Timing windows khớp với combat logic
- Smooth transitions giữa states

---

## 2. CharacterAnimator

**File**: `Scripts/Visual/Animation/CharacterAnimator.cs`

**Responsibility**: Quản lý animator state transitions cho character.

### 2.1 Animation State Enum

```csharp
public enum AnimationState {
    Idle,
    Attack,
    AttackHeavy,
    Spell,
    Hit,
    Dodge,
    Heal,
    Die,
    BuffApplied,
    Debuff
}
```

### 2.2 Class Structure

```csharp
public class CharacterAnimator : MonoBehaviour {
    private Animator animator;
    private AnimationStateManager stateManager;
    private ActionAnimationController actionAnimController;
    
    // Current animation info
    private AnimationState currentState;
    private float currentAnimationLength;
    
    // Damage moment tracking
    public event Action<float> OnDamageMomentReached;
    
    // Knockback
    private Vector3 knockbackDirection;
    private float knockbackForce = 2f;
}
```

### 2.3 Main Methods

```csharp
public void PlayAnimation(string stateName) {
    AnimationState newState = ConvertStringToState(stateName);
    if (newState == currentState && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f) {
        return; // Already playing same animation
    }
    
    stateManager.TransitionTo(newState);
    animator.SetTrigger(stateName);
    currentState = newState;
    
    currentAnimationLength = GetAnimationDuration(stateName);
}

public void PlayAttackAnimation(ActionSnapshot action) {
    // Different animation based on action type
    string animState = action.ActionType switch {
        ActionType.PhysicalAttack => "Attack",
        ActionType.HeavyAttack => "AttackHeavy",
        ActionType.Spell => "Spell",
        ActionType.Heal => "Heal",
        _ => "Attack"
    };
    
    animator.SetTrigger(animState);
    StartCoroutine(AttackSequence(action));
}

private IEnumerator AttackSequence(ActionSnapshot action) {
    // Setup timing
    float damageFrameTime = action.DamageFrameTime;
    float totalDuration = action.AnimationDuration;
    
    // Wait for damage moment
    yield return new WaitForSeconds(damageFrameTime);
    OnDamageMomentReached?.Invoke(damageFrameTime);
    
    // Knockback if source position provided
    if (action.SourcePosition != Vector3.zero) {
        StartCoroutine(ApplyKnockback(action.SourcePosition));
    }
    
    // Wait for animation finish
    yield return new WaitForSeconds(totalDuration - damageFrameTime);
    
    // Return to idle
    PlayAnimation("Idle");
}

public void SetKnockbackDirection(Vector3 direction) {
    knockbackDirection = direction.normalized;
}

private IEnumerator ApplyKnockback(Vector3 sourcePosition) {
    Vector3 knockbackDir = (transform.position - sourcePosition).normalized;
    float elapsedTime = 0;
    float knockbackDuration = 0.15f;
    
    while (elapsedTime < knockbackDuration) {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / knockbackDuration;
        
        // Ease out: start fast, slow down
        float easeOut = 1 - (progress * progress);
        transform.position += knockbackDir * knockbackForce * easeOut * Time.deltaTime;
        
        yield return null;
    }
}

public void PlayHitAnimation() {
    animator.SetTrigger("Hit");
    currentState = AnimationState.Hit;
}

public void PlayHealAnimation() {
    animator.SetTrigger("Heal");
    currentState = AnimationState.Heal;
}

public void PlayDeath() {
    animator.SetTrigger("Die");
    currentState = AnimationState.Die;
    animator.SetBool("IsDead", true);
}

public float GetCurrentAnimationDuration() {
    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    return stateInfo.length;
}

public bool IsAnimationPlaying(string stateName) {
    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    return stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1f;
}
```

---

## 3. ActionAnimationController

**File**: `Scripts/Visual/Animation/ActionAnimationController.cs`

**Responsibility**: Quản lý timing và sequencing cho mỗi loại action.

### 3.1 ActionSnapshot Data

```csharp
public struct ActionSnapshot {
    public string ActionId;
    public ActionType ActionType;
    public float AnimationDuration;      // Total animation length
    public float DamageFrameTime;        // When damage should apply
    public float RecoveryFrameTime;      // When character returns to idle
    public Vector3 SourcePosition;       // For knockback direction
    public string[] TargetIds;           // Who this action targets
    public int BaseDamage;
    public float CriticalChance;
    public string EffectType;            // For status effects
}

public enum ActionType {
    PhysicalAttack,
    HeavyAttack,
    Spell,
    Heal,
    Buff,
    Debuff
}
```

### 3.2 Timing Configuration

```csharp
public class ActionAnimationConfig {
    public static readonly Dictionary<string, ActionSnapshot> AnimationDatabase = new() {
        {
            "skill_slash",
            new ActionSnapshot {
                ActionId = "skill_slash",
                ActionType = ActionType.PhysicalAttack,
                AnimationDuration = 0.8f,
                DamageFrameTime = 0.3f,      // Frame 24 at 60 FPS = 0.4s
                RecoveryFrameTime = 0.8f,
                BaseDamage = 25
            }
        },
        {
            "skill_fireball",
            new ActionSnapshot {
                ActionId = "skill_fireball",
                ActionType = ActionType.Spell,
                AnimationDuration = 1.2f,
                DamageFrameTime = 0.7f,      // Projectile travel time
                RecoveryFrameTime = 1.2f,
                BaseDamage = 40
            }
        },
        {
            "skill_heal",
            new ActionSnapshot {
                ActionId = "skill_heal",
                ActionType = ActionType.Heal,
                AnimationDuration = 1.0f,
                DamageFrameTime = 0.5f,      // Healing hits at 50% through animation
                RecoveryFrameTime = 1.0f,
                BaseDamage = 0
            }
        }
    };
}
```

---

## 4. AnimationStateManager

**File**: `Scripts/Visual/Animation/AnimationStateManager.cs`

**Responsibility**: Centralized state tracking, prevent conflicts, manage transitions.

### 4.1 Class Structure

```csharp
public class AnimationStateManager : MonoBehaviour {
    private AnimationState currentState = AnimationState.Idle;
    private AnimationState previousState;
    private Dictionary<AnimationState, AnimationStateData> stateTransitions;
    
    // State locking
    private bool isLocked = false;
    private float lockTimeRemaining = 0;
    
    public event Action<AnimationState> OnStateChanged;
}
```

### 4.2 State Transition Logic

```csharp
public bool CanTransitionTo(AnimationState newState) {
    // Prevent transition if locked
    if (isLocked) {
        return false;
    }
    
    // Prevent redundant same state
    if (newState == currentState) {
        return false;
    }
    
    // Allow certain transitions
    return stateTransitions[currentState].AllowedTransitions.Contains(newState);
}

public void TransitionTo(AnimationState newState) {
    if (!CanTransitionTo(newState)) {
        Debug.LogWarning($"Cannot transition from {currentState} to {newState}");
        return;
    }
    
    previousState = currentState;
    currentState = newState;
    OnStateChanged?.Invoke(newState);
    
    // Lock state during animation
    LockState(0.3f);
}

public void LockState(float duration) {
    isLocked = true;
    lockTimeRemaining = duration;
    StartCoroutine(UnlockStateAfterTime(duration));
}

private IEnumerator UnlockStateAfterTime(float duration) {
    yield return new WaitForSeconds(duration);
    isLocked = false;
}
```

---

## 5. Blend Tree Parameters

```csharp
public class AnimatorParameterManager {
    // Animation parameters
    private Animator animator;
    
    public void SetAttackDirection(float xDirection) {
        animator.SetFloat("AttackDirX", xDirection);
    }
    
    public void SetAttackType(int attackTypeId) {
        animator.SetInteger("AttackType", attackTypeId);
    }
    
    public void SetSpeed(float speed) {
        animator.SetFloat("Speed", speed);
    }
    
    public void SetDamaged(bool damaged) {
        animator.SetBool("IsDamaged", damaged);
    }
    
    public void TriggerSkill(string skillName) {
        animator.SetTrigger(skillName);
    }
}
```

---

## 6. Animation Setup Guide

### 6.1 Unity Animator Setup

```
Animator Controller: CharacterAnimator
├── Parameters:
│   ├── Float: AttackDirX (direction)
│   ├── Integer: AttackType (0=slash, 1=heavy, 2=spell)
│   ├── Float: Speed (movement speed)
│   ├── Bool: IsDamaged
│   ├── Bool: IsDead
│   ├── Trigger: Attack
│   ├── Trigger: AttackHeavy
│   ├── Trigger: Spell
│   ├── Trigger: Hit
│   ├── Trigger: Heal
│   └── Trigger: Die
│
├── States:
│   ├── Idle
│   │   └─ Transitions:
│   │      → Attack (Attack trigger)
│   │      → AttackHeavy (AttackHeavy trigger)
│   │      → Spell (Spell trigger)
│   │      → Hit (Hit trigger)
│   ├── Attack (0.8s animation)
│   │   └─ Transitions:
│   │      → Idle (when animation finishes)
│   │      → Hit (if damaged mid-animation)
│   ├── Hit (0.4s animation)
│   │   └─ Transitions:
│   │      → Idle
│   └── Die (1.2s animation, no transitions)
```

### 6.2 Frame Timeline

```
Attack Animation Timeline (60 FPS base, 0.8s total)
│
├─ Frame 0-18 (0.0-0.3s): Windup
│  └─ Sword coming up
│
├─ Frame 18-24 (0.3-0.4s): DAMAGE MOMENT ← Damage applies here
│  └─ Sword connects
│
├─ Frame 24-48 (0.4-0.8s): Recovery/Fallback
│  └─ Return to neutral
│
└─ Frame 48+ (>0.8s): Return to Idle
```

---

## 7. Integration with Combat Events

```csharp
// In CombatEventBridge
private void OnSkillCast(SkillCastEvent evt) {
    CharacterView casterView = viewFactory.GetCharacterView(evt.CasterId);
    
    // Get animation config
    ActionSnapshot actionConfig = ActionAnimationConfig.GetConfig(evt.SkillId);
    
    // Play animation with proper timing
    casterView.animator.PlayAttackAnimation(actionConfig);
    
    // Wait for damage moment, then apply damage
    StartCoroutine(WaitForDamageMoment(actionConfig, evt));
}

private IEnumerator WaitForDamageMoment(ActionSnapshot config, SkillCastEvent evt) {
    yield return new WaitForSeconds(config.DamageFrameTime);
    
    // At this point, Combat engine applies actual damage
    // (Happens in ActionResolver.ResolveDamage())
    
    yield return new WaitForSeconds(config.AnimationDuration - config.DamageFrameTime);
    
    // Animation finished, ready for next action
}
```

---

## 8. Animation Checklist

- [ ] All character animations created in Unity
- [ ] Animation duration measured and synchronized
- [ ] Damage frames identified for each action
- [ ] Animator controller setup with parameters and transitions
- [ ] CharacterAnimator properly triggering state changes
- [ ] AnimationStateManager preventing invalid transitions
- [ ] Knockback effect smooth and visually appealing
- [ ] Hit flash animation complete
- [ ] Death animation loops or holds final frame
- [ ] Blend trees handling smooth transitions for damage/direction
- [ ] Performance test: animation changes don't cause GC allocation

---

## 9. Timing Synchronization Table

| Skill | Duration | Damage Frame | Recovery | Animation Type |
|-------|----------|--------------|----------|-----------------|
| Slash | 0.8s | 0.3s | 0.8s | PhysicalAttack |
| Heavy Slash | 1.2s | 0.5s | 1.2s | HeavyAttack |
| Fireball | 1.2s | 0.7s | 1.2s | Spell |
| Lightning | 1.0s | 0.4s | 1.0s | Spell |
| Heal | 1.0s | 0.5s | 1.0s | Heal |

All timings must match ActionResolver.Execute() timing windows.
