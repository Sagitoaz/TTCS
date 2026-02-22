# 📂 FOLDER OWNERSHIP - Quick Reference

> **Xem file này khi không chắc file/folder nào bạn nên làm việc**  
> **Updated: February 22, 2026**

---

## 🎯 TÓM TẮT NHANH

### 🔵 Developer A Owns (Core Systems)

```
Scripts/Core/Events/          ← Event Bus system
Scripts/Core/Data/            ← Data loading
Scripts/Core/Save/            ← Save/Load system
Scripts/Core/Utilities/       ← RNG, Logger (partial)
Scripts/Combat/Managers/      ← Turn Manager, Skill Manager
Scripts/Combat/AI/            ← AI Controller
Scripts/Debug/                ← Debug tools
Scenes/TestFramework.unity    ← Test scene
Scenes/MainMenu.unity         ← Main menu (Week 2)
```

### 🟢 Developer B Owns (Combat & Gameplay)

```
Scripts/Combat/Entities/      ← CombatEntity, Character, Enemy
Scripts/Combat/Components/    ← Health, Stats, Effects components
Scripts/Combat/Stats/         ← Stat calculations, formulas
Scripts/Combat/Actions/       ← Action pipeline, skill execution
Scripts/Combat/Effects/       ← Status effects (Bleed, Burn, etc.)
Scripts/Data/                 ← ScriptableObject data definitions
Data/ (JSON files)            ← All game content JSON
Scenes/CombatScene.unity      ← Combat scene
Scenes/TestCombat.unity       ← Combat test scene
```

### 💜 Shared (Coordinate Before Editing!)

```
Scripts/Core/Utilities/Constants.cs   ← Game constants (add only!)
Data/ (JSON files)                    ← Dev B creates, Dev A reviews
```

---

## 📊 Full Ownership Matrix

| Folder/File | Owner | Can Others Edit? | Notes |
|-------------|-------|------------------|-------|
| **Scripts/Core/Events/** | 🔵 Dev A | ❌ No | Event system only |
| **Scripts/Core/Data/** | 🔵 Dev A | ❌ No | Data loading only |
| **Scripts/Core/Save/** | 🔵 Dev A | ❌ No | Save system only |
| **Scripts/Core/Utilities/RNGService.cs** | 🔵 Dev A | ❌ No | RNG only |
| **Scripts/Core/Utilities/Logger.cs** | 🔵 Dev A | ❌ No | Logging only |
| **Scripts/Core/Utilities/Constants.cs** | 💜 Both | ⚠️ Coordinate | Add constants in your section |
| **Scripts/Combat/Entities/** | 🟢 Dev B | ❌ No | Entity classes only |
| **Scripts/Combat/Components/** | 🟢 Dev B | ❌ No | Components only |
| **Scripts/Combat/Stats/** | 🟢 Dev B | ❌ No | Stat system only |
| **Scripts/Combat/Actions/** | 🟢 Dev B | ❌ No | Action pipeline only |
| **Scripts/Combat/Effects/** | 🟢 Dev B | ❌ No | Status effects only |
| **Scripts/Combat/Managers/TurnManager.cs** | 🔵 Dev A | ❌ No | Turn order |
| **Scripts/Combat/Managers/CombatFlowController.cs** | 🟢 Dev B | ❌ No | Combat flow |
| **Scripts/Combat/Managers/SkillManager.cs** | 🔵 Dev A | ❌ No | Skill cooldowns |
| **Scripts/Combat/AI/** | 🔵 Dev A | ❌ No | AI system only |
| **Scripts/Data/** | 🟢 Dev B | ❌ No | Data structures only |
| **Scripts/Debug/** | 🔵 Dev A | ❌ No | Debug tools only |
| **Data/ (JSON)** | 🟢 Dev B | ⚠️ Ask first | Content creation - Dev B leads |
| **Scenes/CombatScene.unity** | 🟢 Dev B | ❌ No | Combat scene only |
| **Scenes/TestCombat.unity** | 🟢 Dev B | ❌ No | Dev B test scene |
| **Scenes/TestFramework.unity** | 🔵 Dev A | ❌ No | Dev A test scene |
| **Scenes/MainMenu.unity** | 🔵 Dev A | ❌ No | Week 2 - if time |
| **Tests/EditMode/** | 💜 Both | ✅ Yes | Each tests their own code |
| **Tests/PlayMode/** | 💜 Both | ✅ Yes | Each tests their own systems |

---

## ⚠️ CONFLICT ZONES - Be Extra Careful!

### 🔴 HIGH RISK (Require coordination)

```
Constants.cs              ← Announce before editing
*.unity scenes            ← Never edit simultaneously
Shared prefabs            ← Use separate prefabs if possible
```

### 🟡 MEDIUM RISK (Coordinate during integration)

```
Week 1 Day 5:  Turn Manager ↔ Action Pipeline
Week 2 Day 8:  AI Controller ↔ Combat Flow
Week 2 Day 10: Full integration (pair programming)
```

### 🟢 LOW RISK (Safe to work independently)

```
Your owned folders        ← Work freely
Your test files           ← Safe
Your feature branches     ← Independent
```

---

## 🚦 Before Editing ANY File - Quick Checklist

```
❓ Is this file in MY ownership zone?
   ✅ YES → Go ahead!
   ❌ NO  → Check next question

❓ Is this a SHARED file?
   ✅ YES → Coordinate with teammate first
   ❌ NO  → DON'T EDIT (it's teammate's file)

❓ Do I REALLY need to edit teammate's file?
   💡 Consider:
      - Can I create a new file instead?
      - Can I use an interface/API?
      - Can I ask teammate to add the feature?
   
   If still yes → Pair program the change together
```

---

## 📅 Daily Workflow

### Morning (9:00 AM)

```bash
# 1. Pull latest
git checkout main
git pull origin main

# 2. Update your branch
git checkout feature/your-feature
git merge main

# 3. Review ownership
# Check this file to remind what you own!

# 4. Standup - announce files you'll touch
"Today I'm working on:
- Scripts/Core/Events/EventBus.cs
- Scripts/Core/Events/CombatEvent.cs
- Tests/EventBusTests.cs"
```

### During Work

```
✅ Stick to your owned folders
✅ Create new files freely in your zone
✅ Commit frequently (every 1-2 hours)

⚠️ If need shared file:
   1. Check with teammate first
   2. Make minimal changes
   3. Commit immediately
   4. Push right away
```

### Evening (5:00 PM)

```bash
# 1. Commit your work
git add .
git commit -m "feat: detailed description"

# 2. Push to remote
git push origin feature/your-feature

# 3. Sync with teammate
"Pushed feature/event-bus
Changes: [list files]
No shared files touched ✅"
```

---

## 🆘 Emergency: "I need to edit teammate's file!"

### Option 1: DON'T - Use their API instead
```csharp
// Instead of editing CombatEntity.cs directly
// Use the public API they provide

// ❌ BAD: Edit teammate's file
// Add method to CombatEntity.cs

// ✅ GOOD: Use existing API
var entity = new CombatEntity();
entity.TakeDamage(50);  // Use their public method
```

### Option 2: Ask them to add the feature
```
"Hey, can you add a GetCurrentHP() method to CombatEntity?
I need it for the AI to check enemy health.
Public method is fine, just returns CurrentHP value."
```

### Option 3: Pair program the change
```
1. Screen share
2. Discuss what needs to change
3. One person types, other reviews
4. Commit together
5. Both verify it works
```

---

## 💡 Pro Tips

### Create interface boundaries
```csharp
// Dev A creates interface
public interface ITurnManager {
    CombatEntity GetNextActor();
    void ExecuteAction(IAction action);
}

// Dev B uses interface (doesn't need TurnManager.cs file)
public class CombatFlowController {
    private ITurnManager turnManager;
    
    void NextTurn() {
        var actor = turnManager.GetNextActor();
    }
}
```

### Use events instead of direct coupling
```csharp
// Dev A publishes event
EventBus.Publish(new CombatStartEvent());

// Dev B subscribes (no direct reference needed)
EventBus.Subscribe<CombatStartEvent>(OnCombatStart);
```

### Prefabs over scene edits
```
Instead of:
❌ Both editing CombatScene.unity

Use:
✅ Dev A creates: Prefabs/UI/HealthBar.prefab
✅ Dev B creates: Prefabs/Combat/Enemy.prefab
✅ Reference prefabs in scene (minimal conflict)
```

---

## 📞 Communication Templates

**Need to edit shared file:**
```
"@teammate Need to add [X] to Constants.cs - OK now? 
Will commit + push immediately after."
```

**Completed work notification:**
```
"Feature complete: EventBus system
Files added:
- Core/Events/EventBus.cs
- Core/Events/GameEvent.cs
PR created: #12
Safe to pull from main ✅"
```

**Integration point coming up:**
```
"Day 5 - we need to integrate TurnManager + ActionPipeline
Can we sync at 2pm to wire them up?
I'll expose ITurnManager interface by then."
```

---

## 🎯 Summary - Remember These Rules!

1. **STAY IN YOUR LANE** - Only edit files you own
2. **COORDINATE SHARED FILES** - Ask before touching Constants.cs
3. **AVOID SCENES** - Use prefabs instead of scene edits
4. **COMMUNICATE DAILY** - Announce what you're working on
5. **PULL OFTEN** - Morning + before each work session
6. **COMMIT SMALL** - Frequent small commits > big commit
7. **PAIR FOR INTEGRATION** - Don't integrate alone
8. **ASK WHEN UNSURE** - 5 min question > 2 hour conflict fix

---

**When in doubt, CHECK THIS FILE! 📂**

---

*Quick Reference v1.0*  
*Last Updated: February 22, 2026*
