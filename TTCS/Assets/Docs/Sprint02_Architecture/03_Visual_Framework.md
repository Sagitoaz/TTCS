# 03 — Visual Framework — Views, Rendering, Entity Representation

## 1. Core Concept

Visual Framework encapsulates how **physically** character/enemy entities appear on screen—their position, rotation, sprite/model, health bar, and overlay indicators. It acts as a **visual twin** to the logical entity in Sprint 01.

---

## 2. CharacterView (Player character visual)

**File**: `Scripts/Visual/Views/CharacterView.cs`

**Responsibility**: Manage visual representation of player character during combat.

### 2.1 Class Structure

```csharp
public class CharacterView : MonoBehaviour {
    [SerializeField] private string entityId;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private CharacterAnimator characterAnimator;
    
    // Pos/Rot
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    // Status indicators
    [SerializeField] private EffectIndicatorUI effectIndicator;
    
    // Events
    private CombatEntity linkedEntity;  // Reference to logical entity
}
```

### 2.2 Key Methods

```csharp
// Initialization
public void Initialize(CombatEntity entity)
{
    linkedEntity = entity;
    entityId = entity.ID;
    healthBar.SetupForCharacter(entity);
    gameObject.name = $"View_{entity.Name}";
}

// Visual feedback methods (called by CombatEventBridge)
public void PlayDamageVisual(int damage, Vector3 sourcePos)
{
    // Hit flash
    StartCoroutine(HitFlash());
    
    // Knockback
    characterAnimator.PlayHitAnimation();
    characterAnimator.SetKnockbackDirection((transform.position - sourcePos).normalized);
    
    // Shake effect
    StartCoroutine(ScreenShake());
}

public void PlayHealingVisual(int healAmount)
{
    characterAnimator.PlayHealAnimation();
    // Healing glow particle effect
}

public void PlayActionAnimation(string actionId)
{
    characterAnimator.PlayAnimation(actionId);
}

public void SetPosition(Vector3 newPos)
{
    targetPosition = newPos;
    StartCoroutine(SmoothMove(duration: 0.3f));
}

public void Shake(float intensity = 0.1f, float duration = 0.15f)
{
    StartCoroutine(ShakeRoutine(intensity, duration));
}

// Health bar update (called by CombatEventBridge)
public void UpdateHealthBar(int currentHP, int maxHP)
{
    healthBar.SetHP(currentHP, maxHP);
    
    if (currentHP <= 0) {
        PlayDeathVisual();
    }
}

public void AddStatusEffect(StatusEffect effect)
{
    effectIndicator.AddIcon(effect.Type, effect.Duration);
}

public void RemoveStatusEffect(string effectId)
{
    effectIndicator.RemoveIcon(effectId);
}
```

### 2.3 Health Bar Component

```csharp
public class HealthBar : MonoBehaviour {
    [SerializeField] private Image foreground;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI hpText;
    
    private int currentHP;
    private int maxHP;
    private Animator animator;
    
    public void SetHP(int hp, int maxHp) {
        currentHP = hp;
        maxHP = maxHp;
        
        float fill = (float)currentHP / maxHP;
        StartCoroutine(AnimateBar(fill));
        
        hpText.text = $"{currentHP}/{maxHP}";
        
        // Color change: red if critical
        if (fill < 0.25f) {
            foreground.color = Color.red;
        } else if (fill < 0.5f) {
            foreground.color = new Color(1, 0.5f, 0);
        } else {
            foreground.color = Color.green;
        }
    }
    
    private IEnumerator AnimateBar(float targetFill) {
        // Smooth bar fill animation
        float duration = 0.3f;
        float elapsed = 0;
        float startFill = foreground.fillAmount;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            foreground.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            yield return null;
        }
        foreground.fillAmount = targetFill;
    }
}
```

---

## 3. EnemyView (Enemy visual)

**File**: `Scripts/Visual/Views/EnemyView.cs`

**Responsibility**: Manage visual representation of enemy during combat.

### 3.1 Structure

```csharp
public class EnemyView : MonoBehaviour {
    [SerializeField] private string enemyId;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private EnemyAnimator enemyAnimator;
    [SerializeField] private TelegraphVisual telegraph;
    
    private CombatEntity linkedEntity;
}
```

### 3.2 Differences from CharacterView

- **Telegraph Visual**: Shows target area before enemy skill
- **Attack Direction**: Enemy attacks player at specific direction
- **Death Handling**: Enemy disappears or falls down

```csharp
public void ShowTelegraph(SkillData skillData, CombatEntity[] targets)
{
    telegraph.ShowAOE(skillData.TargetType, targets);
    telegraph.ShowDamagePreview(skillData.BaseDamage);
}

public void PlayAttackAnimation()
{
    enemyAnimator.PlayAttack();
    // Face player direction
    Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
    transform.localScale = new Vector3(
        dirToPlayer.x > 0 ? 1 : -1,
        1, 1
    );
}
```

---

## 4. EntityViewFactory

**File**: `Scripts/Visual/Views/EntityViewFactory.cs`

**Responsibility**: Create and manage views for characters and enemies.

```csharp
public class EntityViewFactory : MonoBehaviour {
    [SerializeField] private CharacterView characterViewPrefab;
    [SerializeField] private EnemyView enemyViewPrefab;
    
    private Dictionary<string, CharacterView> characterViews = new();
    private Dictionary<string, EnemyView> enemyViews = new();
    
    // Initialize all character views
    public void InitializeAllCharacters(List<CombatEntity> characters) {
        foreach (var character in characters) {
            var viewInstance = Instantiate(
                characterViewPrefab,
                GetCharacterSpawnPos(character),
                Quaternion.identity,
                transform
            );
            viewInstance.Initialize(character);
            characterViews[character.ID] = viewInstance;
        }
    }
    
    // Initialize all enemy views
    public void InitializeAllEnemies(List<CombatEntity> enemies) {
        foreach (var enemy in enemies) {
            var viewInstance = Instantiate(
                enemyViewPrefab,
                GetEnemySpawnPos(enemy),
                Quaternion.identity,
                transform
            );
            viewInstance.Initialize(enemy);
            enemyViews[enemy.ID] = viewInstance;
        }
    }
    
    // Get view by entity ID
    public CharacterView GetCharacterView(string entityId) {
        return characterViews.TryGetValue(entityId, out var view) ? view : null;
    }
    
    public EnemyView GetEnemyView(string enemyId) {
        return enemyViews.TryGetValue(enemyId, out var view) ? view : null;
    }
    
    public void RemoveView(string entityId) {
        if (characterViews.Remove(entityId, out var charView)) {
            Destroy(charView.gameObject);
        }
        if (enemyViews.Remove(entityId, out var enemyView)) {
            Destroy(enemyView.gameObject);
        }
    }
}
```

---

## 5. Visual Layout Hierarchy

```
Canvas (CombatScreen)
├── Background
│   └── BattleArena (Sprite background)
├── CharacterLayer
│   ├── PlayerCharacterView_0
│   │   ├── SpriteRenderer (character model)
│   │   ├── HealthBar (Canvas)
│   │   │   ├── HPForeground (Image)
│   │   │   └── HPText (TextMeshPro)
│   │   └── EffectIndicator (Canvas)
│   │       └── StatusIcons (Grid)
│   └── PlayerCharacterView_1 (if party)
├── EnemyLayer
│   ├── EnemyView_0
│   │   ├── SpriteRenderer (enemy model)
│   │   ├── HealthBar
│   │   ├── Telegraph (shows AOE)
│   │   └── EffectIndicator
│   └── EnemyView_1
└── EffectLayer
    ├── FloatingText (damage numbers)
    ├── ParticleEffects (VFX)
    └── ScreenShake (post-processing)
```

---

## 6. Positioning Strategy

### 6.1 Character Positioning

```csharp
// Left side of screen (player team)
private Vector3 GetCharacterSpawnPos(CombatEntity character) {
    float xPos = -4f;
    float yPos = (character.stats.Weight == WeightClass.Heavy) ? -1f : 0.5f;
    return new Vector3(xPos, yPos, 0);
}

// Stagger multiple characters vertically
private Vector3 GetCharacterSpawnPos(CombatEntity char, int partyIndex) {
    float xPos = -4f;
    float yOffset = partyIndex * 1.5f;
    return new Vector3(xPos, yOffset, 0);
}
```

### 6.2 Enemy Positioning

```csharp
// Right side of screen (enemy team)
private Vector3 GetEnemySpawnPos(CombatEntity enemy) {
    float xPos = 4f;
    float yPos = (enemy.stats.Weight == WeightClass.Heavy) ? -1f : 0.5f;
    return new Vector3(xPos, yPos, 0);
}
```

---

## 7. Effect Indicators

**File**: `Scripts/Visual/Effects/EffectIndicatorUI.cs`

```csharp
public class EffectIndicatorUI : MonoBehaviour {
    [SerializeField] private GridLayoutGroup iconGrid;
    [SerializeField] private StatusEffectIconPrefab iconPrefab;
    
    private Dictionary<string, StatusEffectIcon> activeIcons = new();
    
    public void AddIcon(string effectType, float duration) {
        var icon = Instantiate(iconPrefab, iconGrid.transform);
        icon.SetupIcon(effectType, duration);
        activeIcons[effectType] = icon;
    }
    
    public void RemoveIcon(string effectType) {
        if (activeIcons.Remove(effectType, out var icon)) {
            Destroy(icon.gameObject);
        }
    }
    
    public void UpdateDuration(string effectType, float remainingTime) {
        if (activeIcons.TryGetValue(effectType, out var icon)) {
            icon.UpdateTimer(remainingTime);
        }
    }
}

public class StatusEffectIcon : MonoBehaviour {
    private Image iconImage;
    private TextMeshProUGUI durationText;
    private float duration;
    private float timeRemaining;
    
    public void SetupIcon(string effectType, float dur) {
        duration = dur;
        timeRemaining = dur;
        iconImage.sprite = EffectIconDatabase.GetIcon(effectType);
        StartCoroutine(CountdownTimer());
    }
    
    private IEnumerator CountdownTimer() {
        while (timeRemaining > 0) {
            timeRemaining -= Time.deltaTime;
            durationText.text = Mathf.Max(0, timeRemaining).ToString("F1");
            yield return null;
        }
        Destroy(gameObject);
    }
}
```

---

## 8. Screen Shake Effect

```csharp
private IEnumerator ShakeRoutine(float intensity, float duration) {
    Vector3 originalPos = transform.position;
    float elapsed = 0;
    
    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        
        float randomX = Random.Range(-intensity, intensity);
        float randomY = Random.Range(-intensity, intensity);
        
        Vector3 shakeOffset = new Vector3(randomX, randomY, 0);
        
        // Apply shake to camera or main canvas
        Camera.main.transform.position = originalPos + shakeOffset;
        
        yield return null;
    }
    
    Camera.main.transform.position = originalPos;
}
```

---

## 9. Integration with CombatEventBridge

```csharp
// In CombatEventBridge.cs
public class CombatEventBridge : MonoBehaviour {
    [SerializeField] private EntityViewFactory viewFactory;
    
    private void Start() {
        // Subscribe to combat events
        EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
        EventBus.Subscribe<HealingEvent>(OnHealing);
        EventBus.Subscribe<EntityDiedEvent>(OnEntityDied);
    }
    
    private void OnDamageTaken(DamageTakenEvent evt) {
        CharacterView targetView = viewFactory.GetCharacterView(evt.TargetId)
            ?? (CharacterView)(object)viewFactory.GetEnemyView(evt.TargetId);
        
        if (targetView != null) {
            targetView.PlayDamageVisual(evt.Damage, evt.SourcePosition);
        }
    }
    
    private void OnHealing(HealingEvent evt) {
        CharacterView targetView = viewFactory.GetCharacterView(evt.TargetId);
        if (targetView != null) {
            targetView.PlayHealingVisual(evt.HealAmount);
        }
    }
    
    private void OnEntityDied(EntityDiedEvent evt) {
        viewFactory.RemoveView(evt.EntityId);
    }
}
```

---

## 10. Performance Optimizations

### 10.1 Object Pooling for Views

```csharp
public class CharacterViewPool {
    private Stack<CharacterView> pool = new();
    private CharacterView prefab;
    
    public void Initialize(CharacterView prefab, int poolSize) {
        for (int i = 0; i < poolSize; i++) {
            var instance = Instantiate(prefab);
            instance.gameObject.SetActive(false);
            pool.Push(instance);
        }
    }
    
    public CharacterView GetView() {
        if (pool.Count == 0) {
            // Expand pool if needed
            return Instantiate(prefab);
        }
        var view = pool.Pop();
        view.gameObject.SetActive(true);
        return view;
    }
    
    public void ReturnView(CharacterView view) {
        view.gameObject.SetActive(false);
        pool.Push(view);
    }
}
```

### 10.2 Canvas Batching

- Use single Canvas for combat screen
- Separate CanvasGroups for character/enemy layers to enable efficient culling
- Use `GraphicRaycaster` optimization: only visible UI elements

### 10.3 Sprite vs Mesh

- **Sprites**: Use for 2D-style combat
- **Meshes**: Use for 3D skeletal animation (if applicable)
- Consider using **LOD** if many enemies active

---

## 11. Visual Framework Checklist

- [ ] CharacterView prefab created and tested
- [ ] EnemyView prefab created and tested
- [ ] HealthBar displays correct HP values
- [ ] Effect indicators show/hide properly
- [ ] Screen shake working on damage
- [ ] Knockback animation smooth
- [ ] Position transitions smooth (0.3s)
- [ ] View destruction on entity death
- [ ] EntityViewFactory integrated with combat startup
- [ ] CombatEventBridge connected to all view methods
