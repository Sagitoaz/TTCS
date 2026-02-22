# 📅 Kế hoạch làm việc 2 tuần - TTCS Development

> **Chu kỳ phát triển:** Sprint 1 (Tuần 1-2)  
> **Team size:** 2 developers  
> **Focus:** Core Framework + Combat Foundation

---

## 👥 Phân công team

### 👨‍💻 Developer A (Core Systems & Backend)
**Expertise:** Architecture, Data Systems, Core Logic

### 👩‍💻 Developer B (Combat & Game Logic)
**Expertise:** Gameplay Programming, Combat Systems, AI

---

## � Cấu trúc thư mục & Phân chia công việc

### 🗂️ Chi tiết Folder Structure với Ownership

```
Assets/
│
├── 📂 Scripts/
│   │
│   ├── 📂 Core/                    🔵 Developer A OWNS
│   │   ├── Events/
│   │   │   ├── EventBus.cs        [A] Event system singleton
│   │   │   ├── GameEvent.cs       [A] Base event class
│   │   │   ├── CombatEvent.cs     [A] Combat-specific events
│   │   │   ├── UIEvent.cs         [A] UI events
│   │   │   └── SystemEvent.cs     [A] System events
│   │   │
│   │   ├── Data/
│   │   │   ├── DataManager.cs     [A] Data loading system
│   │   │   ├── DataCache.cs       [A] Caching mechanism
│   │   │   └── DataValidator.cs   [A] Validation utilities
│   │   │
│   │   ├── Save/
│   │   │   ├── SaveManager.cs     [A] Save/load system
│   │   │   ├── SaveData.cs        [A] Save data structure
│   │   │   └── SaveSlot.cs        [A] Multiple save slots
│   │   │
│   │   └── Utilities/
│   │       ├── RNGService.cs      [A] Random number generator
│   │       ├── Logger.cs          [A] Logging utility
│   │       └── Constants.cs       [SHARED] Game constants
│   │
│   ├── 📂 Combat/                  🟢 Developer B OWNS (majority)
│   │   │
│   │   ├── Entities/
│   │   │   ├── CombatEntity.cs    [B] Base entity class
│   │   │   ├── Character.cs       [B] Player character
│   │   │   ├── Enemy.cs           [B] Enemy entity
│   │   │   └── EntityFactory.cs   [B] Entity creation
│   │   │
│   │   ├── Components/
│   │   │   ├── IEntityComponent.cs    [B] Component interface
│   │   │   ├── HealthComponent.cs     [B] HP management
│   │   │   ├── StatsComponent.cs      [B] Stats & modifiers
│   │   │   └── EffectComponent.cs     [B] Status effects container
│   │   │
│   │   ├── Stats/
│   │   │   ├── EntityStats.cs         [B] Stats struct
│   │   │   ├── StatCalculator.cs      [B] Damage/stat formulas
│   │   │   └── StatModifier.cs        [B] Buff/debuff modifiers
│   │   │
│   │   ├── Actions/
│   │   │   ├── IAction.cs             [B] Action interface
│   │   │   ├── SkillAction.cs         [B] Skill execution
│   │   │   ├── ActionValidator.cs     [B] Validate actions
│   │   │   └── ActionResolver.cs      [B] Resolve outcomes
│   │   │
│   │   ├── Effects/
│   │   │   ├── StatusEffect.cs        [B] Base effect class
│   │   │   ├── BleedEffect.cs         [B] Bleed implementation
│   │   │   ├── BurnEffect.cs          [B] Burn implementation
│   │   │   ├── HealEffect.cs          [B] Heal effect
│   │   │   ├── ShieldEffect.cs        [B] Shield effect
│   │   │   └── StunEffect.cs          [B] Stun effect
│   │   │
│   │   ├── Managers/               🔵 Developer A OWNS
│   │   │   ├── TurnManager.cs         [A] Turn order system
│   │   │   ├── CombatFlowController.cs [B] Combat state machine
│   │   │   └── SkillManager.cs        [A] Skill cooldowns
│   │   │
│   │   └── AI/                     🔵 Developer A OWNS
│   │       ├── AIController.cs        [A] AI decision making
│   │       ├── AIBehavior.cs          [A] Behavior patterns
│   │       └── TargetSelector.cs      [A] Target selection logic
│   │
│   ├── 📂 Data/                    💜 SHARED (Use separate files!)
│   │   ├── CharacterData.cs       [B] Character ScriptableObject
│   │   ├── SkillData.cs           [B] Skill data structure
│   │   ├── EnemyData.cs           [B] Enemy data structure
│   │   ├── StageData.cs           [B] Stage configuration
│   │   └── GameConfig.cs          [A] Global config settings
│   │
│   ├── 📂 UI/                      ⚪ LOW PRIORITY (Sprint 2)
│   │   └── (Minimal work in Sprint 1)
│   │
│   └── 📂 Debug/                   🔵 Developer A OWNS
│       ├── DebugPanel.cs          [A] In-game debug UI
│       ├── CombatLogger.cs        [A] Combat event logging
│       └── DebugCommands.cs       [A] Cheat commands
│
├── 📂 Data/                        💜 JSON FILES - SHARED but coordinated
│   ├── Characters/
│   │   ├── char_warrior.json      [B] Week 2 - Dev B creates
│   │   └── char_mage.json         [B] Week 2 - Dev B creates
│   │
│   ├── Skills/
│   │   ├── skill_warrior_*.json   [B] Week 2 - Dev B creates
│   │   └── skill_mage_*.json      [B] Week 2 - Dev B creates
│   │
│   ├── Enemies/
│   │   ├── enemy_goblin.json      [B] Week 2 - Dev B creates
│   │   └── enemy_dark_knight.json [B] Week 2 - Dev B creates
│   │
│   └── Stages/
│       └── stage_01_tutorial.json [B] Week 2 - Dev B creates
│
├── 📂 Scenes/
│   ├── MainMenu.unity             [A] Week 2 - Dev A creates if time
│   ├── CombatScene.unity          [B] Week 2 - Dev B creates
│   └── TestScenes/
│       ├── TestCombat.unity       [B] For integration testing
│       └── TestFramework.unity    [A] For framework testing
│
├── 📂 Tests/
│   ├── EditMode/
│   │   ├── EventBusTests.cs       [A] Event system tests
│   │   ├── RNGServiceTests.cs     [A] RNG tests
│   │   ├── DataManagerTests.cs    [A] Data loading tests
│   │   ├── SaveManagerTests.cs    [A] Save/load tests
│   │   ├── TurnManagerTests.cs    [A] Turn order tests
│   │   ├── EntityTests.cs         [B] Entity tests
│   │   ├── ComponentTests.cs      [B] Component tests
│   │   ├── StatCalculatorTests.cs [B] Damage calc tests
│   │   └── EffectTests.cs         [B] Status effect tests
│   │
│   └── PlayMode/
│       ├── CombatFlowTests.cs     [B] Full battle integration
│       └── AITests.cs             [A] AI decision tests
│
└── 📂 Prefabs/                     (Created as needed)
    ├── Combat/                    [B] Combat-related prefabs
    └── UI/                        [A] UI prefabs (minimal)
```

---

### 🎯 File Ownership Matrix

| Zone | Owner | Files | Notes |
|------|-------|-------|-------|
| **Core/Events** | 🔵 Dev A | All event-related | Event bus infrastructure |
| **Core/Data** | 🔵 Dev A | DataManager, caching | Data pipeline |
| **Core/Save** | 🔵 Dev A | SaveManager, SaveData | Persistence |
| **Core/Utilities** | 🔵 Dev A | RNG, Logger | Shared utilities |
| **Combat/Entities** | 🟢 Dev B | Entity classes | Core combat entities |
| **Combat/Components** | 🟢 Dev B | All components | Entity component system |
| **Combat/Stats** | 🟢 Dev B | Stats & calculations | Stat formulas |
| **Combat/Actions** | 🟢 Dev B | Action pipeline | Skill execution |
| **Combat/Effects** | 🟢 Dev B | Status effects | All effect types |
| **Combat/Managers/Turn** | 🔵 Dev A | TurnManager | Turn order logic |
| **Combat/Managers/Flow** | 🟢 Dev B | CombatFlowController | State machine |
| **Combat/Managers/Skill** | 🔵 Dev A | SkillManager | Cooldown tracking |
| **Combat/AI** | 🔵 Dev A | AI system | Decision making |
| **Data/** (C# classes) | 🟢 Dev B | ScriptableObjects | Data structures |
| **Data/** (JSON files) | 💜 Both | Coordinated | Dev B leads content |
| **Debug/** | 🔵 Dev A | Debug tools | Developer tools |
| **Tests/** | 💜 Both | Own tests | Each tests their code |

**Legend:**
- 🔵 Dev A owns - Only Dev A modifies
- 🟢 Dev B owns - Only Dev B modifies  
- 💜 Shared - Needs coordination before editing

---

### ⚠️ Conflict Avoidance Guidelines

#### 1. **File-Level Ownership**
```
DO:
✅ Each developer works in their designated folders
✅ Create new files instead of editing shared files
✅ Communicate before touching 💜 SHARED files

DON'T:
❌ Edit files outside your ownership
❌ Modify Constants.cs without syncing
❌ Work on same scene simultaneously
```

#### 2. **Branch Strategy**

**During Sprint 1:**
```bash
# Main branches
main                    # Stable, tested code only

# Developer A branches
feature/event-bus       # Event system
feature/data-manager    # Data loading
feature/save-system     # Save/load
feature/turn-manager    # Turn order
feature/ai-controller   # AI system

# Developer B branches
feature/combat-entity   # Entity system
feature/components      # Component system
feature/stat-calculator # Stats & formulas
feature/action-pipeline # Actions
feature/status-effects  # Effects
feature/combat-flow     # Flow controller
```

#### 3. **Daily Sync Protocol**

**Morning (9:00 AM):**
```
1. Pull latest main
2. Merge main into your feature branch
3. Resolve conflicts (should be minimal!)
4. Daily standup - announce what files you'll touch
```

**Evening (5:00 PM):**
```
1. Commit your work
2. Push feature branch
3. If feature complete → Create PR
4. Notify teammate of merge to main
```

#### 4. **Shared Resource Rules**

**Constants.cs:**
```csharp
// RULE: Add constants, don't modify existing ones
// RULE: Comment with [DevA] or [DevB] tag

// [DevA] Event system constants
public const string EVENT_COMBAT_START = "CombatStart";

// [DevB] Combat constants  
public const int MAX_PARTY_SIZE = 3;
```

**Scenes:**
```
RULE: Do NOT work on same scene simultaneously
- Dev A: TestFramework.unity, MainMenu.unity
- Dev B: CombatScene.unity, TestCombat.unity

If need to share: Prefabs instead of scene edits!
```

**JSON Data:**
```
RULE: Each person creates their own files
- Dev B owns all content JSON creation (Week 2)
- Dev A reviews but doesn't edit
- Communicate before bulk changes
```

#### 5. **Integration Points** (Require coordination)

**Week 1 - Day 5:**
```
Integration: Turn Manager + Action Pipeline
- Dev A provides: TurnManager with public API
- Dev B provides: IAction interface
- Sync session: Test turn execution together
```

**Week 2 - Day 8:**
```
Integration: Combat Flow + AI
- Dev B provides: CombatFlowController states
- Dev A provides: AIController decisions
- Pair programming: Wire AI into flow
```

**Week 2 - Day 10:**
```
Full Integration Day
- Both merge all features to main
- Pair programming session
- Fix integration issues together
```

#### 6. **Communication Channels**

**Slack/Discord:**
```
Use for:
- "I'm about to edit Constants.cs"
- "Merging feature/event-bus to main"  
- "Need to touch Combat/Managers - OK?"
- "Breaking API change in IAction interface"
```

**Pull Requests:**
```
Tag teammate for review:
- Small PRs: Auto-merge if tests pass
- Large PRs: Wait for approval
- Breaking changes: Required review
```

**Code Comments:**
```csharp
// TODO(DevA): Need public method GetNextActor() from TurnManager
// FIXME(DevB): This depends on EventBus - waiting for merge
// NOTE: This interface will be used by DevA's AIController
```

---

### 📋 Pre-Work Checklist (Day 1 Morning)

**Developer A:**
- [ ] Create folder structure: `Scripts/Core/`
- [ ] Create all subfolders: `Events/`, `Data/`, `Save/`, `Utilities/`
- [ ] Push initial commit: "chore: setup Dev A folder structure"
- [ ] Announce in chat: "Core folders ready"

**Developer B:**
- [ ] Wait for Dev A's structure commit
- [ ] Pull latest
- [ ] Create folder structure: `Scripts/Combat/`
- [ ] Create subfolders: `Entities/`, `Components/`, `Stats/`, `Actions/`, `Effects/`
- [ ] Push initial commit: "chore: setup Dev B folder structure"
- [ ] Announce in chat: "Combat folders ready"

**Both:**
- [ ] Verify no conflicts in initial setup
- [ ] Ready to start coding in parallel

---

## �📊 Tổng quan mục tiêu

### 🎯 Sprint Goals

- ✅ **P0 - Core Framework**: Hoàn thành 100%
- ✅ **P1 - Combat Core**: Hoàn thành 60-70%
- ✅ **Integration**: Demo được 1 trận đấu cơ bản

### 📦 Deliverables

1. **Framework hoàn chỉnh**
   - Event Bus system
   - RNG Service 
   - Data Loading pipeline
   - Save/Load system (basic)

2. **Combat prototype**
   - Entity system (Character, Enemy)
   - Stat & Component system
   - Turn Manager
   - Action Pipeline (simplified)

3. **Content samples**
   - 2 playable characters (data)
   - 2 enemies (data)
   - 3 skills per character
   - 1 test stage

---

## 📅 TUẦN 1: Core Framework + Data Foundation

### 🗓️ Ngày 1-2 (24-25 Feb): Setup & Infrastructure

**🔒 Conflict Risk: MINIMAL** - Working in separate folders

#### Developer A: Project Structure & Event System

**Working Zone:** 🔵 `Scripts/Core/Events/`, `Scripts/Core/Utilities/`, `Tests/EditMode/`

**Tasks:**
- [ ] 📁 **Setup Core folder structure** (30 min)
  ```bash
  # Create folders (Dev A owns these)
  Scripts/Core/Events/
  Scripts/Core/Data/
  Scripts/Core/Save/
  Scripts/Core/Utilities/
  Tests/EditMode/
  ```
  
  **File:** Push empty `.gitkeep` in each folder
  **Commit:** `chore: setup Dev A folder structure`

- [ ] 🎯 **Implement Event Bus System** (4h)
  
  **Files to create:**
  - `Scripts/Core/Events/EventBus.cs` - Singleton pattern
  - `Scripts/Core/Events/GameEvent.cs` - Base event class
  - `Scripts/Core/Events/CombatEvent.cs` - Combat-specific events
  - `Scripts/Core/Events/UIEvent.cs` - UI events
  - `Scripts/Core/Events/SystemEvent.cs` - System events
  - `Tests/EditMode/EventBusTests.cs` - Unit tests
  
  **API to expose:**
  ```csharp
  public class EventBus {
      public static void Subscribe<T>(Action<T> handler) where T : GameEvent;
      public static void Unsubscribe<T>(Action<T> handler) where T : GameEvent;
      public static void Publish<T>(T eventData) where T : GameEvent;
  }
  ```
  
  **Deliverable:** Event system functional với test cases
  **Commit:** `feat: implement event bus system`

- [ ] 🎲 **Create RNG Service** (2h)
  
  **Files to create:**
  - `Scripts/Core/Utilities/RNGService.cs` - Seeded random
  - `Tests/EditMode/RNGServiceTests.cs` - Deterministic tests
  
  **API to expose:**
  ```csharp
  public class RNGService {
      public static void Initialize(int seed);
      public static int Range(int min, int max);
      public static float Range(float min, float max);
      public static bool Chance(float probability);
      public static T SelectWeighted<T>(T[] items, float[] weights);
  }
  ```
  
  **Deliverable:** RNG service với deterministic testing
  **Commit:** `feat: add RNG service with seeded random`

- [ ] 📝 **Create shared Constants** (30 min)
  
  **File:** `Scripts/Core/Utilities/Constants.cs`
  
  ```csharp
  // [DevA] Add your constants here with [DevA] tag
  public static class Constants {
      // [DevA] Event names
      public const string EVENT_COMBAT_START = "CombatStart";
      
      // [DevB] will add combat constants later
  }
  ```
  
  **Commit:** `chore: add Constants file with Dev A entries`

- [ ] 🧪 **Run Tests** (1h)
  - All Event Bus tests pass
  - All RNG tests pass
  - No compiler errors
  
**Time Budget:** 8 hours  
**Output:** Framework foundation ready
**Branch:** `feature/event-bus` + `feature/rng-service`

---

#### Developer B: Data Models & Combat Entities

**Working Zone:** 🟢 `Scripts/Combat/Entities/`, `Scripts/Data/`, `Tests/EditMode/`

**⚠️ Coordination:** Wait for Dev A to push folder structure first, then pull before creating your folders

**Tasks:**
- [ ] 📁 **Setup Combat folder structure** (30 min)
  ```bash
  # Create folders (Dev B owns these)
  Scripts/Combat/Entities/
  Scripts/Combat/Components/
  Scripts/Combat/Stats/
  Scripts/Combat/Actions/
  Scripts/Combat/Effects/
  Scripts/Data/
  Tests/EditMode/
  ```
  
  **File:** Push empty `.gitkeep` in each folder  
  **Commit:** `chore: setup Dev B folder structure`

- [ ] 📊 **Define Data Models** (3h)
  
  **Files to create:** (in `Scripts/Data/`)
  - `CharacterData.cs` - ScriptableObject for characters
  - `SkillData.cs` - ScriptableObject for skills
  - `EnemyData.cs` - ScriptableObject for enemies
  - `StageData.cs` - ScriptableObject for stages
  
  **Structure (example CharacterData.cs):**
  ```csharp
  [CreateAssetMenu(fileName = "CharacterData", menuName = "TTCS/Character")]
  public class CharacterData : ScriptableObject {
      public string id;
      public string nameKey;
      public Rarity rarity;
      public int baseHP;
      public int baseATK;
      public int baseDEF;
      public int baseSPD;
      // ... (see TechnicalDescription.md Section 3.1)
  }
  ```
  
  Properties theo spec trong TechnicalDescription.md
  
  **Commit:** `feat: add data model ScriptableObjects`

- [ ] ⚔️ **Create Combat Entity System** (4h)
  
  **Files to create:** (in `Scripts/Combat/Entities/`)
  - `CombatEntity.cs` - Base abstract class
  - `EntityStats.cs` - Stats struct (in `Scripts/Combat/Stats/`)
  - `StatusEffect.cs` - Base effect class (in `Scripts/Combat/Effects/`)
  
  **CombatEntity.cs API:**
  ```csharp
  public abstract class CombatEntity {
      public string ID { get; protected set; }
      public EntityStats BaseStats { get; protected set; }
      public int CurrentHP { get; protected set; }
      public List<StatusEffect> ActiveEffects { get; }
      
      public virtual void TakeDamage(int amount);
      public virtual void Heal(int amount);
      public void ApplyEffect(StatusEffect effect);
      public bool IsDead();
  }
  ```
  
  **EntityStats.cs:**
  ```csharp
  [System.Serializable]
  public struct EntityStats {
      public int hp;
      public int atk;
      public int def;
      public int spd;
      public float crit;
      public float resist;
  }
  ```
  
  **StatusEffect.cs:**
  ```csharp
  public abstract class StatusEffect {
      public int Duration { get; set; }
      public abstract void OnApply(CombatEntity target);
      public abstract void OnTick(CombatEntity target);
      public abstract void OnRemove(CombatEntity target);
  }
  ```
  
  **Deliverable:** Entity system with basic stats
  **Commit:** `feat: implement combat entity base system`

- [ ] 🧪 **Write Unit Tests** (1h)
  
  **Files to create:** (in `Tests/EditMode/`)
  - `EntityTests.cs` - Test entity creation, HP, death
  - `EntityStatsTests.cs` - Test stat initialization
  
  **Test cases:**
  - Entity initializes with correct stats
  - TakeDamage reduces HP correctly
  - HP doesn't go below 0
  - IsDead() returns true when HP = 0
  - Heal restores HP but doesn't exceed max
  
  **Commit:** `test: add combat entity unit tests`

- [ ] 📝 **Update Constants (if needed)** (15 min)
  
  **File:** `Scripts/Core/Utilities/Constants.cs` (💜 SHARED - coordinate with Dev A)
  
  ```csharp
  // [DevB] Combat constants
  public const int MAX_PARTY_SIZE = 3;
  public const int MAX_ENEMY_SIZE = 5;
  public const int MIN_DAMAGE = 1;
  ```
  
  **⚠️ Before editing:** Announce in chat "Adding combat constants to Constants.cs"
  **Commit:** `chore: add combat constants`

**Time Budget:** 8 hours  
**Output:** Game entities defined and tested
**Branch:** `feature/combat-entity`

---

**🔄 End of Day 2 Sync:**
```
Both developers:
1. Push all commits to feature branches
2. Create PRs if features are complete
3. Quick review of each other's code structure
4. Merge to main if tests pass
5. Both pull latest main before Day 3
```

---

### 🗓️ Ngày 3-4 (26-27 Feb): Data Loading & Save System

**🔒 Conflict Risk: LOW** - Working in separate folders, minimal shared files

#### Developer A: Data Management

**Working Zone:** 🔵 `Scripts/Core/Data/`, `Scripts/Core/Save/`, `Tests/EditMode/`

**Tasks:**
- [ ] 📂 **Data Loading Service** (4h)
  
  **Files to create:** (in `Scripts/Core/Data/`)
  - `DataManager.cs` - Singleton data loader
  - `DataCache.cs` - In-memory caching
  - `DataValidator.cs` - Validation utilities
  - `Tests/EditMode/DataManagerTests.cs` - Unit tests
  
  **DataManager.cs API:**
  ```csharp
  public class DataManager : MonoBehaviour {
      public static DataManager Instance { get; }
      
      // Load methods (cache results)
      public CharacterData LoadCharacter(string id);
      public SkillData LoadSkill(string id);
      public EnemyData LoadEnemy(string id);
      public StageData LoadStage(string id);
      
      // Bulk load
      public void LoadAllData();
      public List<CharacterData> GetAllCharacters();
      
      // Validation
      public bool ValidateAllData();
  }
  ```
  
  **Support:**
  - Load from JSON (using `JsonUtility`)
  - Load from ScriptableObject (using `Resources.Load`)
  - Error handling for missing files
  - Cache loaded data to avoid re-loading
  
  **Deliverable:** Data can be loaded from JSON/SO
  **Commit:** `feat: implement data loading system`

- [ ] 💾 **Save System** (4h)
  
  **Files to create:** (in `Scripts/Core/Save/`)
  - `SaveManager.cs` - Save/load singleton
  - `SaveData.cs` - Serializable save data class
  - `SaveSlot.cs` - Helper for multiple save slots
  - `Tests/EditMode/SaveManagerTests.cs` - Unit tests
  
  **SaveData.cs structure:**
  ```csharp
  [System.Serializable]
  public class SaveData {
      public int playerLevel;
      public int gold;
      public List<string> unlockedCharacters;
      public List<string> unlockedStages;
      public Dictionary<string, int> characterLevels;
      // ... more save fields
  }
  ```
  
  **SaveManager.cs API:**
  ```csharp
  public class SaveManager : MonoBehaviour {
      public static SaveManager Instance { get; }
      
      public void Save(int slotIndex = 0);
      public SaveData Load(int slotIndex = 0);
      public void DeleteSave(int slotIndex);
      public bool HasSaveData(int slotIndex);
      
      // Current save data in memory
      public SaveData CurrentSave { get; }
  }
  ```
  
  **Implementation:**
  - Use `Application.persistentDataPath`
  - JSON serialization with `JsonUtility`
  - Support up to 3 save slots
  - Auto-save option (optional)
  
  **Deliverable:** Save/Load working with test data
  **Commit:** `feat: implement save/load system`

- [ ] 🧪 **Integration Testing** (1h)
  
  **Test scenarios:**
  - Save data → Close game → Load data → Verify same
  - Save to slot 1 → Save to slot 2 → Load each → Verify independent
  - Corrupted file → Graceful error handling
  - Missing file → Return new SaveData
  
  **Files:** `Tests/EditMode/SaveManagerTests.cs`, `DataManagerTests.cs`
  
  **Commit:** `test: add save/load integration tests`

**Time Budget:** 8 hours (flexible: 4h data + 4h save)  
**Output:** Complete data pipeline
**Branch:** `feature/data-manager` + `feature/save-system`

---

#### Developer B: Combat Components & Stats System

**Working Zone:** 🟢 `Scripts/Combat/Components/`, `Scripts/Combat/Stats/`, `Tests/EditMode/`

**Tasks:**
- [ ] 🔧 **Component System** (3h)
  
  **Files to create:** (in `Scripts/Combat/Components/`)
  - `IEntityComponent.cs` - Component interface
  - `HealthComponent.cs` - HP management
  - `StatsComponent.cs` - Stats with modifiers
  - `EffectComponent.cs` - Status effects container
  - `Tests/EditMode/ComponentTests.cs` - Unit tests
  
  **IEntityComponent.cs:**
  ```csharp
  public interface IEntityComponent {
      void Initialize(CombatEntity owner);
      void OnTurnStart();
      void OnTurnEnd();
  }
  ```
  
  **HealthComponent.cs:**
  ```csharp
  public class HealthComponent : IEntityComponent {
      public int CurrentHP { get; private set; }
      public int MaxHP { get; private set; }
      
      public void TakeDamage(int amount);
      public void Heal(int amount);
      public void SetMaxHP(int max);
      public bool IsDead();
      public float GetHPPercent();
  }
  ```
  
  **StatsComponent.cs:**
  ```csharp
  public class StatsComponent : IEntityComponent {
      public EntityStats BaseStats { get; set; }
      private List<StatModifier> modifiers;
      
      public int GetEffectiveStat(StatType type);
      public void AddModifier(StatModifier mod);
      public void RemoveModifier(StatModifier mod);
      public void ClearModifiers();
  }
  ```
  
  **EffectComponent.cs:**
  ```csharp
  public class EffectComponent : IEntityComponent {
      private List<StatusEffect> activeEffects;
      
      public void AddEffect(StatusEffect effect);
      public void RemoveEffect(StatusEffect effect);
      public void TickEffects();
      public bool HasEffect(Type effectType);
      public List<StatusEffect> GetActiveEffects();
  }
  ```
  
  **Deliverable:** Component system integrated with CombatEntity
  **Commit:** `feat: implement entity component system`

- [ ] 📈 **Stat Calculation System** (3h)
  
  **Files to create:** (in `Scripts/Combat/Stats/`)
  - `StatCalculator.cs` - All combat formulas
  - `StatModifier.cs` - Buff/debuff modifiers
  - `DamageType.cs` - Enum for damage types
  - `Element.cs` - Enum for elements
  - `Tests/EditMode/StatCalculatorTests.cs` - Formula tests
  
  **StatCalculator.cs API:**
  ```csharp
  public static class StatCalculator {
      // Damage formulas
      public static int CalculateDamage(
          CombatEntity attacker, 
          CombatEntity defender, 
          SkillData skill
      );
      
      public static int ApplyDefense(int baseDamage, int defense);
      public static int CalculateCritical(int damage, float critRate, float critDmg);
      public static float GetElementMultiplier(Element attack, Element defense);
      
      // Stat calculations
      public static int GetEffectiveHP(CombatEntity entity);
      public static int GetEffectiveATK(CombatEntity entity);
      // ...
  }
  ```
  
  **Damage Formula (reference from TechnicalDescription.md):**
  ```csharp
  // Basic formula: (ATK * skillMultiplier) * (1 - defenseReduction)
  int baseDmg = attacker.ATK * skill.multiplier;
  float defReduction = defender.DEF / (defender.DEF + 100f);
  defReduction = Mathf.Min(0.75f, defReduction); // Cap at 75%
  int finalDmg = (int)(baseDmg * (1 - defReduction));
  finalDmg = Mathf.Max(1, finalDmg); // Minimum 1 damage
  ```
  
  **StatModifier.cs:**
  ```csharp
  [System.Serializable]
  public class StatModifier {
      public StatType targetStat;
      public ModifierType type; // Flat, Percentage
      public float value;
      public int duration; // -1 = permanent
      public string sourceId;
  }
  ```
  
  **Deliverable:** All damage calculations working
  **Commit:** `feat: implement stat calculation system`

- [ ] 🧪 **Integration Tests** (2h)
  
  **Files:** `Tests/EditMode/StatCalculatorTests.cs`, `Tests/EditMode/ComponentTests.cs`
  
  **Test cases:**
  ```csharp
  // Damage calculation
  [Test] public void DamageCalculation_WithNoDefense_ReturnsBaseDamage()
  [Test] public void DamageCalculation_WithHighDefense_ReducesDamage()
  [Test] public void DamageCalculation_WithCrit_IncreaseDamage()
  [Test] public void DamageCalculation_MinimumDamageIs1()
  
  // Components
  [Test] public void HealthComponent_TakeDamage_ReducesHP()
  [Test] public void HealthComponent_Heal_RestoresHP_NotExceedMax()
  [Test] public void StatsComponent_AddModifier_AffectsEffectiveStat()
  [Test] public void EffectComponent_AddEffect_TracksActiveEffects()
  ```
  
  **Commit:** `test: add stat calculator and component tests`

- [ ] 🔗 **Update CombatEntity Integration** (1h)
  
  **File:** `Scripts/Combat/Entities/CombatEntity.cs` (existing file - Dev B owns)
  
  **Integrate components:**
  ```csharp
  public abstract class CombatEntity {
      // Add component system
      public HealthComponent Health { get; private set; }
      public StatsComponent Stats { get; private set; }
      public EffectComponent Effects { get; private set; }
      
      protected virtual void InitializeComponents() {
          Health = new HealthComponent();
          Stats = new StatsComponent();
          Effects = new EffectComponent();
          
          Health.Initialize(this);
          Stats.Initialize(this);
          Effects.Initialize(this);
      }
  }
  ```
  
  **Commit:** `refactor: integrate components into CombatEntity`

**Time Budget:** 8 hours (3h components + 3h stats + 2h tests)  
**Output:** Stats & components functional
**Branch:** `feature/components` + `feature/stat-calculator`

---

**🔄 End of Day 4 Sync:**
```
Quick standup:
- Dev A: Data & Save systems ready?
- Dev B: Components & Stats ready?
- Both: Any blockers for next integration?

Prepare for Day 5:
- Dev A will need: CombatEntity API (from Dev B)
- Dev B will need: EventBus (from Dev A)
- Plan integration point for Turn Manager + Action execution
```
  - Test stat modifiers
  - Test component interactions

**Time Budget:** 8 hours  
**Output:** Stats & components functional

---

### 🗓️ Ngày 5 (28 Feb): Turn Manager & Action Pipeline (Part 1)

#### Developer A: Turn Order System

**Tasks:**
- [ ] ⏱️ **Turn Manager Implementation** (6h)
  - `TurnManager.cs`
  - Timeline-based turn order (SPD-based)
  
  **Core Logic:**
  ```csharp
  public class TurnManager {
      List<CombatEntity> timeline;
      
      // Determine next actor by lowest initiative
      CombatEntity GetNextActor()
      
      // Advance actor's initiative by action cost
      void ExecuteAction(action, cost)
      
      // Reset timeline for new battle
      void InitializeTurn(entities)
  }
  ```
  
  - Action cost system (normal=100, fast=80, slow=120)
  - Turn counter tracking
  - Event emission: `OnTurnStart`, `OnTurnEnd`
  
  **Deliverable:** Turn order working correctly

- [ ] 📝 **Logging System** (2h)
  - `CombatLogger.cs`
  - Log combat events to console/file
  - Format: `[Turn X] Actor: Y | Action: Z | Result: W`
  
**Time Budget:** 8 hours  
**Output:** Turn system functional

---

#### Developer B: Action System Foundation

**Tasks:**
- [ ] 🎬 **Action Pipeline - Phase 1** (6h)
  - `IAction` interface
  - `SkillAction.cs` - Concrete implementation
  
  **Pipeline Stages (Simplified):**
  ```
  1. Validate → 2. Commit → 3. Resolve → 4. Emit Events
  ```
  
  - `ActionValidator.cs`
    - Check cost, cooldown, target validity
  
  - `ActionResolver.cs`  
    - Calculate damage
    - Apply effects
    - Trigger on-hit events
  
  - Basic skill execution (no timing yet)

- [ ] 🎯 **Target Selection System** (2h)
  - `TargetingRule` enum
  - `TargetSelector.cs`
    - Single enemy
    - All enemies
    - Single ally
    - Random target
    - Lowest/Highest HP

**Time Budget:** 8 hours  
**Output:** Actions can be executed

---

## 📅 TUẦN 2: Combat Core + Integration

### 🗓️ Ngày 6-7 (3-4 Mar): Skills & Effects

#### Developer A: Skill System

**Tasks:**
- [ ] ⚡ **Skill Manager** (4h)
  - `SkillManager.cs`
  - Cooldown tracking per entity
  - Mana/resource cost system
  - Skill availability checks
  
  **Methods:**
  ```csharp
  bool CanUseSkill(entity, skillId)
  void UseSkill(entity, skillId, target)
  void TickCooldowns(entity)
  ```

- [ ] 📊 **Create Sample Skills** (4h)
  - Basic Attack (no cooldown, low dmg)
  - Fireball (2 turn CD, medium dmg, burn chance)
  - Heal (3 turn CD, restore HP)
  - Shield (4 turn CD, damage absorption)
  
  - Create ScriptableObjects hoặc JSON
  - Link với SkillData definitions

**Time Budget:** 8 hours  
**Output:** Skills usable in combat

---

#### Developer B: Status Effects Implementation

**Tasks:**
- [ ] 🩸 **Core Effects** (5h)
  Implement các effect cơ bản:
  
  - `BleedEffect.cs` - Tick damage
  - `BurnEffect.cs` - Fire DoT
  - `HealEffect.cs` - Restore HP
  - `ShieldEffect.cs` - Absorb damage
  - `StunEffect.cs` - Skip turn
  
  Each với:
  - Duration tracking
  - Stack logic (nếu có)
  - Visual indicator flag

- [ ] 🔄 **Effect Application System** (2h)
  - Effect stacking rules
  - Effect immunity/resistance
  - Effect dispel logic
  - Event triggers: `OnEffectApplied`, `OnEffectExpired`

- [ ] 🧪 **Effect Tests** (1h)
  - Test effect duration
  - Test stacking
  - Test tick damage

**Time Budget:** 8 hours  
**Output:** Status effects working

---

### 🗓️ Ngày 8 (5 Mar): AI System (Basic)

#### Developer A: AI Decision Making

**Tasks:**
- [ ] 🤖 **AI Controller** (6h)
  - `AIController.cs`
  - Simple decision tree
  
  **Decision Logic:**
  ```
  IF HP < 30% AND has heal → Use Heal
  ELIF target HP < 50% → Use strongest attack
  ELIF cooldown ready → Use special skill
  ELSE → Basic attack
  ```
  
  - Weight-based skill selection
  - Target priority (lowest HP, highest threat)
  - Defensive behavior when low HP

- [ ] 📝 **AI Config Data** (2h)
  - `AIBehavior` ScriptableObject
  - Properties: aggression, defensiveness, skill preferences
  - Per-enemy AI profiles

**Time Budget:** 8 hours  
**Output:** Enemies can act autonomously

---

#### Developer B: Combat Flow Integration

**Tasks:**
- [ ] 🔄 **Combat State Machine** (5h)
  - `CombatFlowController.cs`
  
  **States:**
  ```
  Init → PlayerTurn → ExecuteAction → EnemyTurn → 
  ExecuteAction → CheckVictory → (loop) → End
  ```
  
  - State transitions
  - Win/Loss detection
  - Event coordination

- [ ] 🏆 **Victory/Defeat Conditions** (2h)
  - Check if all enemies dead → Victory
  - Check if all players dead → Defeat
  - Event: `OnBattleEnd(result)`
  - Rewards calculation (basic)

- [ ] ⚙️ **Battle Initialization** (1h)
  - Load stage data
  - Spawn entities
  - Setup turn order
  - Initialize RNG seed

**Time Budget:** 8 hours  
**Output:** Full combat loop working

---

### 🗓️ Ngày 9 (6 Mar): Content Creation

#### Developer A: Character Data

**Tasks:**
- [ ] 👤 **Create 2 Player Characters** (4h)
  - Character 1: Warrior (Tank/Attacker)
    - High HP, medium ATK
    - Skills: Slash, Power Strike, Guard, Taunt
  
  - Character 2: Mage (Support/Damage)
    - Low HP, high ATK
    - Skills: Fireball, Heal, Shield, Lightning
  
  - Full stat definitions
  - Skill assignments
  - Visual references (placeholder)

- [ ] 🎨 **Basic UI Data Binding** (4h)
  - Health bars
  - Skill buttons
  - Turn indicator
  - Combat log display

**Time Budget:** 8 hours  
**Output:** Playable characters defined

---

#### Developer B: Enemy Data & Stages

**Tasks:**
- [ ] 👹 **Create 2 Enemy Types** (3h)
  - Enemy 1: Goblin (Common, low HP)
    - Weak attack, no special skills
  
  - Enemy 2: Dark Knight (Elite, high HP)
    - Strong attack, defensive stance, counter
  
  - AI behavior profiles
  - Loot tables (basic)

- [ ] 🗺️ **Create Test Stage** (2h)
  - Stage 1: Tutorial Battle
    - Wave 1: 2 Goblins
    - Wave 2: 1 Dark Knight
  
  - Stage config file
  - Rewards: Gold, XP

- [ ] 🧪 **Integration Test - Full Battle** (3h)
  - Run complete battle loop
  - Test với Player vs AI
  - Verify turn order
  - Check damage calculations
  - Validate victory conditions

**Time Budget:** 8 hours  
**Output:** Testable battle scenario

---

### 🗓️ Ngày 10 (7 Mar): Testing, Debug & Polish

#### Cả 2 Developers - Pair Programming & Integration

**Morning Session (4h):**
- [ ] 🐛 **Bug Fixing Sprint**
  - Fix crashes
  - Fix calculation errors
  - Fix state machine issues
  - Fix data loading problems

- [ ] 🧪 **Automated Testing**
  - Run all unit tests
  - Run integration tests
  - Fix failing tests
  - Add missing test coverage

**Afternoon Session (4h):**
- [ ] 🔧 **Debug Tools** (Dev A focus)
  - Debug panel in Unity
  - Spawn enemy button
  - Set HP/stats controls
  - Skip turn button
  - Seed override input

- [ ] 📊 **Combat Logger Enhancement** (Dev B focus)
  - Color-coded logs
  - Filter options
  - Export to file
  - Replay data capture

**Time Budget:** 8 hours/person = 16 hours total  
**Output:** Stable, debuggable build

---

## 📈 Sprint Review & Demo (End of Week 2)

### ✅ Demo Checklist

- [ ] Start game → Load character data
- [ ] Enter test battle
- [ ] Player turn - select skill
- [ ] Execute player action
- [ ] Enemy turn - AI decides
- [ ] Combat continues
- [ ] Victory/Defeat detection
- [ ] Battle ends cleanly

### 📊 Metrics to Track

| Metric | Target | Actual |
|--------|--------|--------|
| Core Framework Complete | 100% | ___ |
| Combat Core Complete | 70% | ___ |
| Test Coverage | >60% | ___ |
| Critical Bugs | 0 | ___ |
| Build Success Rate | 100% | ___ |

---

## 📋 Definition of Done (DoD)

Mỗi feature được coi là "done" khi:

- ✅ Code implemented và committed
- ✅ Unit tests passed (nếu có)
- ✅ Integration tested
- ✅ Code reviewed (pair programming)
- ✅ Documented (inline comments + README cập nhật)
- ✅ No critical bugs
- ✅ Performance acceptable (no lag trong test)

---

## 🚧 Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| 🔴 **Timeline quá tight** | High | Medium | Chặt chẽ theo DoD, cắt scope nếu cần |
| 🟡 **Integration issues** | Medium | High | Daily sync, pair programming ngày 10 |
| 🟢 **Scope creep** | Medium | Medium | Chỉ làm P0+P1, không thêm features |
| 🟡 **Thiếu kinh nghiệm Unity** | Medium | Low | Pair programming, code review chặt |

---

## 📞 Communication & Sync

### Daily Standup (15 phút/ngày)
**Khi nào:** Mỗi sáng 9:00 AM

**Format:**
1. Hôm qua làm gì?
2. Hôm nay làm gì?
3. Có blockers không?
4. Cần help từ người kia không?

### Mid-Sprint Check (Ngày 5)
- Review tiến độ
- Adjust plan nếu cần
- Resolve blockers lớn

### Sprint Retrospective (Ngày 10 - cuối ngày)
- Gì hoạt động tốt?
- Gì cần cải thiện?
- Action items cho sprint sau

---

## 🎯 Success Criteria

Sprint 1 thành công nếu:

1. ✅ **Functional demo**: 1 trận đấu hoàn chỉnh từ đầu đến cuối
2. ✅ **Code quality**: Clean, maintainable, tested
3. ✅ **Foundation solid**: Event bus, data pipeline, entity system stable
4. ✅ **Team sync**: Cả 2 hiểu architecture và có thể work độc lập
5. ✅ **Documentation**: Code có comment, README.md có hướng dẫn setup

---

## 📚 Resources & References

### Documents cần đọc trước khi bắt đầu:
- `TechnicalDescription.md` - Architecture overview
- `projectdescription.md` - Game design vision
- `CodeConventions.md` - Coding standards *(sẽ tạo)*
- `DataTemplates/` - Data structure examples *(sẽ tạo)*

### Unity Packages cần import:
- TextMeshPro (UI text)
- DOTween (optional - for animations)

### Tools:
- Git + GitLFS
- Visual Studio / Rider
- Unity 2021.3+ LTS

---

## 📝 Notes

### Điều chỉnh plan khi cần:
- Nếu chậm: **Cắt scope**, focus vào core loop trước
- Nếu nhanh: **Thêm polish**, hoặc bắt đầu timing system
- Nếu block: **Switch tasks**, giúp nhau unblock

### Productivity Tips:
- 🍅 **Pomodoro**: 25 phút focus + 5 phút break
- 🤝 **Pair khi stuck**: Đừng ngồi 1 mình debug >1 giờ
- 💬 **Over-communicate**: Tốt hơn là under-communicate
- 🎮 **Test often**: Đừng code 1 ngày mới test

---

## 🔐 Conflict Management Strategy

### 📊 Daily Workflow để tránh conflicts

**Mỗi sáng (9:00 AM):**
```bash
# 1. Pull latest changes
git checkout main
git pull origin main

# 2. Update your feature branch
git checkout feature/your-feature
git merge main

# 3. Resolve any conflicts NOW (nếu có)
# 4. Announce in standup: "Working on [file list]"
```

**Mỗi tối (5:00 PM):**
```bash
# 1. Commit your day's work
git add .
git commit -m "feat: describe what you did"

# 2. Push to your feature branch
git push origin feature/your-feature

# 3. If feature complete, create PR
# 4. Notify teammate if you merged to main
```

---

### 🚨 Conflict Resolution Protocol

**Nếu gặp merge conflict:**

**STEP 1: Identify the conflict**
```bash
git status  # Shows conflicted files
```

**STEP 2: Communicate**
```
Slack/Discord:
"Hey [teammate], I have a conflict in [filename]. 
Can we sync for 5 min to resolve?"
```

**STEP 3: Resolve together**
```
Options:
A) Screen share + resolve together
B) One person takes lead, other reviews
C) Use "Accept Both Changes" if compatible
```

**STEP 4: Test after resolution**
```bash
# After resolving conflict
git add [resolved-files]
git commit -m "fix: resolve merge conflict in [files]"

# Run tests!
# Make sure code still compiles
```

**STEP 5: Verify with teammate**
```
"Conflict resolved in [file]. 
Changes: [brief description]
Can you pull and verify?"
```

---

### 🎯 File-by-File Conflict Prevention

**Constants.cs (💜 SHARED):**
```csharp
// STRATEGY: Add only, don't modify existing
// Each person adds in their own section

public static class Constants {
    // ============================================
    // [DevA] EVENT & SYSTEM CONSTANTS
    // ============================================
    public const string EVENT_COMBAT_START = "CombatStart";
    public const string EVENT_TURN_START = "TurnStart";
    
    // ============================================
    // [DevB] COMBAT CONSTANTS
    // ============================================
    public const int MAX_PARTY_SIZE = 3;
    public const int MIN_DAMAGE = 1;
}
```

**Protocol:**
1. Announce before editing: "Adding constants"
2. Add in your section only
3. Don't reorder or remove existing
4. Commit immediately after adding
5. Push right away to avoid conflict window

---

**Scenes (⚠️ HIGH CONFLICT RISK):**
```
BEST PRACTICE: Avoid working on same scene!

Strategy:
- Dev A: TestFramework.unity, MainMenu.unity
- Dev B: CombatScene.unity, TestCombat.unity
- Use PREFABS for shared objects
- Scene changes should be minimal in Sprint 1

If must edit same scene:
1. Coordinate specific time slots
2. One person commits + pushes
3. Other person pulls before editing
4. Alternative: Use scene prefab variants
```

---

**JSON Data Files:**
```
STRATEGY: Each person creates separate files

Good:
✅ Dev B creates: char_warrior.json
✅ Dev B creates: char_mage.json
✅ No conflicts!

Bad:
❌ Both edit: char_warrior.json
❌ Merge conflict in JSON = painful!

Protocol:
- Dev B creates ALL content JSON (Week 2)
- If need to fix typo in teammate's JSON:
  → Ask first!
  → Or create GitHub issue
```

---

### 📅 Critical Sync Points (Must coordinate)

**Day 2 End:**
```
Topic: Folder structure complete
Action: Both merge initial setup to main
Risk: Low
```

**Day 5 Mid-day:**
```
Topic: Turn Manager + Action Pipeline integration
Action: Dev A exposes TurnManager API
       Dev B exposes IAction interface
       Quick pair session to verify integration
Risk: Medium
```

**Day 8 Afternoon:**
```
Topic: AI + Combat Flow integration  
Action: Pair programming session
       Wire AI decisions into combat flow
Risk: High - requires coordination
```

**Day 10 Full Day:**
```
Topic: Full integration
Action: PAIR PROGRAMMING ENTIRE DAY
       Merge all feature branches
       Fix integration issues together
Risk: High - final integration
```

---

### 🛡️ Conflict Prevention Checklist

**Before starting work:**
- [ ] Pulled latest main this morning?
- [ ] Merged main into my feature branch?
- [ ] Announced what files I'll work on?
- [ ] Checked if teammate needs any of my files?

**Before committing:**
- [ ] Only modified files in my ownership zone?
- [ ] If touched shared file, coordinated first?
- [ ] Tested that code compiles?
- [ ] No accidental commits (temp files, etc.)?

**Before merging to main:**
- [ ] All tests passing?
- [ ] Pulled latest main again?
- [ ] Resolved any conflicts?
- [ ] Notified teammate about merge?

---

### 🆘 Emergency Conflict Scenarios

**Scenario 1: "Both edited same file at same time"**
```
Solution:
1. Stop work immediately
2. Person B pulls Person A's changes
3. Person B manually merges their changes
4. Both review the merged result
5. Person B commits and pushes
6. Person A pulls to verify
```

**Scenario 2: "Merge broke the build"**
```
Solution:
1. Identify which merge caused it
2. Revert the merge: git revert [commit-hash]
3. Fix issues in feature branch
4. Re-merge when fixed
```

**Scenario 3: "Accidentally committed to main"**
```
Solution:
1. git reset --soft HEAD~1  (undo commit, keep changes)
2. git checkout -b feature/should-be-in-branch
3. git commit -m "feat: description"
4. git checkout main
5. git reset --hard origin/main  (reset main)
```

**Scenario 4: "Lost work due to bad merge"**
```
Solution:
1. git reflog  (shows all commits)
2. Find your lost commit hash
3. git cherry-pick [hash]  (restore it)
```

---

### 💡 Pro Tips for Smooth Collaboration

**Use clear commit messages:**
```bash
✅ feat(combat): add damage calculation formula
✅ fix(events): resolve event unsubscribe bug
✅ test(entity): add HP boundary tests
✅ docs(api): document TurnManager public methods

❌ update
❌ fix bug
❌ WIP
```

**Branch naming:**
```bash
✅ feature/event-bus
✅ feature/combat-entity
✅ fix/damage-overflow
✅ refactor/stat-calculator

❌ mybranch
❌ test
❌ dev-a-work
```

**PR descriptions:**
```markdown
✅ Good PR:
## What
Implemented event bus system with pub/sub pattern

## Files Changed
- Core/Events/EventBus.cs (new)
- Core/Events/GameEvent.cs (new)
- Tests/EditMode/EventBusTests.cs (new)

## Testing
All unit tests pass (12/12)

## Integration Points
Provides EventBus.Subscribe/Publish for combat events

❌ Bad PR:
"Added stuff"
```

---

### 📞 Quick Communication Templates

**Before editing shared file:**
```
"@teammate I need to add constants to Constants.cs - OK to edit now?"
```

**After merging to main:**
```
"Merged feature/event-bus to main. Please pull before your next commit!"
```

**When stuck:**
```
"Having issues with [X]. Can we pair for 15 min to unblock?"
```

**Daily standup format:**
```
Yesterday: Implemented EventBus system
Today: Working on DataManager (Core/Data/DataManager.cs)
Blockers: Need CombatEntity API from DevB for integration
```

---

**Good luck, team! 🚀**

Hãy nhớ: **Perfect is the enemy of done**. Focus vào working software trước, polish sau!

---

*Document created: February 22, 2026*  
*Sprint Period: Feb 24 - Mar 7, 2026*
