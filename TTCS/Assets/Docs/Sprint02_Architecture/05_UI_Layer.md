# 05 — UI Layer — Panels, HUD, Information Display

## 1. Overview

UI Layer provides visual feedback and player control interface. It includes:
- Action selection (skill buttons)
- Character/enemy status display
- Turn order preview
- Battle log
- Effect indicators

---

## 2. CombatUIManager

**File**: `Scripts/Visual/UI/CombatUIManager.cs`

**Responsibility**: Central manager for all UI elements during combat.

### 2.1 Structure

```csharp
public class CombatUIManager : MonoBehaviour {
    // UI Panels
    [SerializeField] private ActionPanel actionPanel;
    [SerializeField] private StatusDisplay statusDisplay;
    [SerializeField] private TurnOrderDisplay turnOrderDisplay;
    [SerializeField] private BattleLogDisplay battleLogDisplay;
    
    // State
    private CombatUIState uiState = CombatUIState.Ready;
    private bool isPlayerTurn = false;
    
    // Events
    public event Action<int> OnSkillSelected;
    public event Action OnUIReady;
}

public enum CombatUIState {
    Ready,              // Ready for player input
    AnimatingAction,    // Action being executed visually
    Waiting,            // Waiting for automatic action (enemy turn)
    BattleEnded         // Battle finished
}
```

### 2.2 Key Methods

```csharp
public void Initialize() {
    actionPanel.Initialize();
    statusDisplay.Initialize();
    turnOrderDisplay.Initialize();
    battleLogDisplay.Initialize();
}

public void OnPlayerTurnStart(List<CombatEntity> availableActors) {
    isPlayerTurn = true;
    uiState = CombatUIState.Ready;
    actionPanel.EnablePanel();
    OnUIReady?.Invoke();
}

public void OnEnemyTurnStart(CombatEntity enemy) {
    isPlayerTurn = false;
    uiState = CombatUIState.Waiting;
    actionPanel.DisablePanel();
}

public void OnActionStarted() {
    uiState = CombatUIState.AnimatingAction;
    actionPanel.DisablePanel();
}

public void OnActionFinished() {
    if (isPlayerTurn) {
        uiState = CombatUIState.Ready;
        actionPanel.EnablePanel();
    } else {
        uiState = CombatUIState.Waiting;
    }
}

// Called by EventBridge
public void OnSkillCast(SkillCastEvent evt) {
    // Log action
    battleLogDisplay.AddLog($"{evt.CasterName} used {evt.SkillName}!");
}

public void OnDamageTaken(DamageTakenEvent evt) {
    // Update health bar
    statusDisplay.UpdateHealthBar(evt.TargetId, evt.NewHP, evt.MaxHP);
    
    // Log damage
    if (evt.IsCritical) {
        battleLogDisplay.AddLog($"{evt.TargetName} took {evt.Damage} CRITICAL damage!");
    } else {
        battleLogDisplay.AddLog($"{evt.TargetName} took {evt.Damage} damage.");
    }
}

public void OnHealing(HealingEvent evt) {
    statusDisplay.UpdateHealthBar(evt.TargetId, evt.NewHP, evt.MaxHP);
    battleLogDisplay.AddLog($"{evt.TargetName} healed for {evt.HealAmount}!");
}

public void OnEntityDied(EntityDiedEvent evt) {
    statusDisplay.RemoveCharacterStatus(evt.EntityId);
    battleLogDisplay.AddLog($"{evt.EntityName} has been defeated!");
}

public void EndBattle(BattleResult result) {
    uiState = CombatUIState.BattleEnded;
    actionPanel.DisablePanel();
    
    // Show result screen
    ShowBattleResultScreen(result);
}
```

---

## 3. ActionPanel

**File**: `Scripts/Visual/UI/ActionPanel.cs`

**Responsibility**: Display skill buttons and handle player action selection.

### 3.1 Skill Button States

```csharp
public enum SkillButtonState {
    Available,      // Can be used
    Cooldown,       // On cooldown
    ManInsufficient,// Not enough mana
    Locked          // Cannot use (e.g., stunned)
}
```

### 3.2 Class Structure

```csharp
public class ActionPanel : MonoBehaviour {
    [SerializeField] private SkillButton[] skillButtons = new SkillButton[4];
    [SerializeField] private CanvasGroup canvasGroup;
    
    private CombatEntity currentActor;
    private Dictionary<int, SkillData> slotSkillMap = new();
}
```

### 3.3 Skill Button Implementation

```csharp
public class SkillButton : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    
    private SkillData linkedSkill;
    private int cooldownRemaining = 0;
    private bool isAvailable = true;
    
    public event Action<SkillData> OnSkillSelected;
    
    public void SetupButton(SkillData skill) {
        linkedSkill = skill;
        skillIcon.sprite = skill.IconSprite;
        skillNameText.text = skill.Name;
        costText.text = $"MP: {skill.ManaCost}";
        
        button.onClick.AddListener(() => {
            if (isAvailable) {
                OnSkillSelected?.Invoke(linkedSkill);
            }
        });
    }
    
    public void SetButtonState(SkillButtonState state) {
        switch (state) {
            case SkillButtonState.Available:
                button.interactable = true;
                cooldownOverlay.fillAmount = 0;
                cooldownText.text = "";
                isAvailable = true;
                break;
                
            case SkillButtonState.Cooldown:
                button.interactable = false;
                // Show cooldown timer
                break;
                
            case SkillButtonState.ManInsufficient:
                button.interactable = false;
                costText.color = Color.red;
                break;
                
            case SkillButtonState.Locked:
                button.interactable = false;
                cooldownOverlay.color = Color.gray;
                break;
        }
    }
    
    public void UpdateCooldown(int remaining, int total) {
        cooldownRemaining = remaining;
        float fillAmount = (float)remaining / total;
        cooldownOverlay.fillAmount = fillAmount;
        cooldownText.text = remaining > 0 ? remaining.ToString() : "";
    }
}
```

### 3.4 Panel Methods

```csharp
public void SetupButtons(CombatEntity actor) {
    currentActor = actor;
    
    for (int i = 0; i < 4; i++) {
        if (i < actor.Skills.Count) {
            SkillData skill = actor.Skills[i];
            skillButtons[i].SetupButton(skill);
            skillButtons[i].OnSkillSelected += OnSkillButtonClicked;
        } else {
            skillButtons[i].gameObject.SetActive(false);
        }
    }
    
    RefreshButtonStates();
}

public void RefreshButtonStates() {
    for (int i = 0; i < skillButtons.Length; i++) {
        if (currentActor.Skills[i] == null) continue;
        
        SkillData skill = currentActor.Skills[i];
        
        // Check mana
        if (currentActor.CurrentMP < skill.ManaCost) {
            skillButtons[i].SetButtonState(SkillButtonState.ManInsufficient);
            continue;
        }
        
        // Check cooldown
        if (currentActor.GetSkillCooldown(skill.ID) > 0) {
            skillButtons[i].SetButtonState(SkillButtonState.Cooldown);
            int cd = currentActor.GetSkillCooldown(skill.ID);
            skillButtons[i].UpdateCooldown(cd, skill.Cooldown);
            continue;
        }
        
        // Check status effects (stun, etc)
        if (currentActor.IsStunned()) {
            skillButtons[i].SetButtonState(SkillButtonState.Locked);
            continue;
        }
        
        skillButtons[i].SetButtonState(SkillButtonState.Available);
    }
}

private void OnSkillButtonClicked(SkillData skill) {
    // Publish event for CombatEventBridge to forward to Combat
    OnSkillSelected?.Invoke(skill.ID);
}

public void EnablePanel() {
    canvasGroup.blocksRaycasts = true;
    canvasGroup.alpha = 1;
}

public void DisablePanel() {
    canvasGroup.blocksRaycasts = false;
}
```

---

## 4. StatusDisplay

**File**: `Scripts/Visual/UI/StatusDisplay.cs`

**Responsibility**: Display character and enemy health, mana, status effects.

### 4.1 Structure

```csharp
public class StatusDisplay : MonoBehaviour {
    [SerializeField] private CharacterStatusPanel characterStatusPrefab;
    [SerializeField] private EnemyStatusPanel enemyStatusPrefab;
    
    private Dictionary<string, CharacterStatusPanel> characterPanels = new();
    private Dictionary<string, EnemyStatusPanel> enemyPanels = new();
}
```

### 4.2 Character Status Panel

```csharp
public class CharacterStatusPanel : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image manaBar;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private GridLayoutGroup effectGrid;
    
    private string entityId;
    
    public void SetupPanel(CombatEntity character) {
        entityId = character.ID;
        characterName.text = character.Name;
        UpdateHealthBar(character.CurrentHP, character.MaxHP);
        UpdateManaBar(character.CurrentMP, character.MaxMP);
    }
    
    public void UpdateHealthBar(int currentHP, int maxHP) {
        float fillAmount = (float)currentHP / maxHP;
        healthBar.fillAmount = fillAmount;
        healthText.text = $"{currentHP}/{maxHP}";
        
        if (fillAmount < 0.2f) {
            healthBar.color = Color.red;
        } else if (fillAmount < 0.5f) {
            healthBar.color = new Color(1, 0.5f, 0);
        } else {
            healthBar.color = Color.green;
        }
    }
    
    public void UpdateManaBar(int currentMP, int maxMP) {
        float fillAmount = (float)currentMP / maxMP;
        manaBar.fillAmount = fillAmount;
        manaText.text = $"{currentMP}/{maxMP}";
    }
    
    public void AddStatusEffectIcon(string effectType, float duration) {
        var iconObj = Instantiate(effectIconPrefab, effectGrid.transform);
        // Setup icon visuals
    }
}
```

---

## 5. TurnOrderDisplay

**File**: `Scripts/Visual/UI/TurnOrderDisplay.cs`

**Responsibility**: Show turn order preview (next 3-5 actors).

```csharp
public class TurnOrderDisplay : MonoBehaviour {
    [SerializeField] private TurnIndicatorPrefab indicatorPrefab;
    [SerializeField] private HorizontalLayoutGroup turnLayout;
    
    private List<TurnIndicator> activeIndicators = new();
    
    public void UpdateTurnOrder(List<string> actorIds) {
        // Clear old
        foreach (var indicator in activeIndicators) {
            Destroy(indicator.gameObject);
        }
        activeIndicators.Clear();
        
        // Create new indicators (show first 5)
        for (int i = 0; i < Mathf.Min(5, actorIds.Count); i++) {
            var indicator = Instantiate(indicatorPrefab, turnLayout.transform);
            indicator.SetActorId(actorIds[i]);
            indicator.SetTurnPosition(i);
            
            if (i == 0) {
                indicator.Highlight(); // Current actor
            }
            
            activeIndicators.Add(indicator);
        }
    }
}

public class TurnIndicator : MonoBehaviour {
    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI positionText;
    
    private string actorId;
    
    public void SetActorId(string id) {
        actorId = id;
        characterIcon.sprite = GetCharacterIcon(id);
    }
    
    public void SetTurnPosition(int position) {
        positionText.text = (position + 1).ToString();
    }
    
    public void Highlight() {
        GetComponent<Image>().color = Color.yellow;
    }
}
```

---

## 6. BattleLogDisplay

**File**: `Scripts/Visual/UI/BattleLogDisplay.cs`

**Responsibility**: Show action history and combat log.

```csharp
public class BattleLogDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private int maxLines = 10;
    
    private Queue<string> logLines = new();
    
    public void AddLog(string message) {
        logLines.Enqueue(message);
        
        if (logLines.Count > maxLines) {
            logLines.Dequeue();
        }
        
        RefreshDisplay();
    }
    
    private void RefreshDisplay() {
        logText.text = string.Join("\n", logLines);
        
        // Scroll to bottom
        scrollView.verticalNormalizedPosition = 0;
    }
    
    public void Clear() {
        logLines.Clear();
        logText.text = "";
    }
}
```

---

## 7. UI Panel Hierarchy

```
Canvas (CombatUI)
├── ActionPanel
│   ├── Button_Skill_1
│   ├── Button_Skill_2
│   ├── Button_Skill_3
│   └── Button_Skill_4
├── StatusDisplay
│   ├── CharacterStatus_1
│   │   ├── HealthBar
│   │   ├── ManaBar
│   │   └── StatusEffectGrid
│   ├── EnemyStatus_1
│   │   ├── HealthBar
│   │   └── StatusEffectGrid
│   └── EnemyStatus_2
├── TurnOrderDisplay
│   ├── TurnIndicator_1
│   ├── TurnIndicator_2
│   ├── TurnIndicator_3
│   ├── TurnIndicator_4
│   └── TurnIndicator_5
└── BattleLogDisplay
    └── LogText
```

---

## 8. UI Performance Optimization

- **Canvas Batching**: Use single Canvas with CanvasGroups for layer control
- **Pooling**: Reuse StatusEffect icons
- **Dirty Flag**: Only update what changed
- **Late Update**: Sync UI updates after game logic

```csharp
private bool isDirty = true;

public void MarkDirty() {
    isDirty = true;
}

private void LateUpdate() {
    if (isDirty) {
        RefreshDisplay();
        isDirty = false;
    }
}
```

---

## 9. UI Integration Checklist

- [ ] All UI panels created in prefabs
- [ ] Skill buttons display correctly with icons and costs
- [ ] Health bar updates smooth animation
- [ ] Mana bar updates correctly
- [ ] Status effect icons display and disappear
- [ ] Turn order updates correctly
- [ ] Battle log scrolls naturally
- [ ] UI doesn't block scene camera during animation
- [ ] Touch/click detection works properly
- [ ] Cooldown timer shows correct remaining time
