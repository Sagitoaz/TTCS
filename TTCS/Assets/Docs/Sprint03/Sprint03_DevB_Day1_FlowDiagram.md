# Sprint 3 Dev B - Flow Diagram & Navigation Route

> **Ngày tạo**: 2026-04-01 (Ngày 1)  
> **Mục đích**: Định nghĩa flow game cao cấp: Boot -> Tutorial? -> MainMenu -> Sub-scenes -> Combat -> Result

---

## 🎯 Flow Diagram (ASCII)

```
┌──────────────────────────────────────────────────────────────┐
│                      GAME BOOT FLOW                          │
└──────────────────────────────────────────────────────────────┘

                          [Boot Scene]
                               │
                    Check: tutorialCompleted?
                          /              \
                        /                  \
        ✅ NO (New Player)              ✅ YES (Returning)
           │                                 │
           ▼                                 ▼
    [Tutorial Scene]                 [Main Menu Scene]
           │                                 │
      Complete Tutorial                 [HOME UI]
           │                          /  │  │  │  \
      Save tutorialCompleted         /   │  │  │   \
           │                        /    │  │  │    \
           ▼                       ▼     ▼  ▼  ▼     ▼
    [Main Menu Scene]         [Team]  [Gacha]  [Inventory]  [LevelSelect]
           │                   │       │        │            │
           └─────────────┬─────┴───────┴────────┴────────────┘
                         │
                    (Navigation Return)
                         │
                   [Main Menu Scene]
                         │
                         └──→ [Level Select Scene]
                              │
                         (Select Level)
                              │
                              ▼
                         [Level Data Snapshot]
                         (Current Lineup)
                              │
                              ▼
                        [Combat Scene]
                              │
                         (Combat Logic - Dev A)
                              │
                              ▼
                        [Combat Result]
                              │
                         Reward Update
                         Inventory Update
                         Save Write
                              │
                          /        \
                     NO /            \ YES
                    Back /              \ Continue
                       /                  \
                      ▼                    ▼
            [Main Menu Scene]       [Level Select Scene]
```

---

## 🎬 Scene Navigation Map

### Boot Scene
- **Entry Point**: Game start
- **Logic**: 
  - Read save state (tutorialCompleted flag)
  - If new: load Tutorial scene
  - If returning: load Main Menu scene
- **Exit**: Load Tutorial OR Main Menu

### Tutorial Scene
- **Entry Point**: From Boot (new player only)
- **UI**:
  - Step text panel
  - Next/Skip buttons
  - Complete button
- **Logic**:
  - Step-by-step guidance
  - Skip button -> mark tutorial completed
  - Complete button -> mark tutorial completed + navigate to Main Menu
- **Exit**: Navigation to Main Menu

### Main Menu Scene
- **Entry Point**: From Boot (returning), From Tutorial, From sub-scenes
- **UI**:
  - Welcome text
  - 5 Buttons: Team, Gacha, Inventory, Level Select, Settings (optional)
- **Logic**:
  - Hub scene for navigation
  - Button click -> load corresponding sub-scene
- **Exit**: Load Team / Gacha / Inventory / Level Select scene

### Team Formation Scene
- **Entry Point**: From Main Menu (Team button)
- **UI**:
  - Team panel with 3 slot buttons
  - Picker panel (scrollable character list)
  - Confirm add panel (Yes/No)
  - Back button
- **Logic**:
  - Click slot button to open picker
  - Picker excludes dead characters (HP <= 0), deployed characters, and already-selected characters
  - Picker supports sorting (level/rarity asc-desc) and role filtering
  - Click character -> show confirm add dialog -> on Yes save lineup and return to team panel
- **Exit**: Back to Main Menu

### Gacha Scene
- **Entry Point**: From Main Menu (Gacha button)
- **UI**:
  - Pool selector (dropdown)
  - Roll buttons (1x, 10x)
  - Result panel (popup)
  - Back button
- **Logic**:
  - Select pool
  - Click roll -> call GachaService.Roll()
  - Show result (character + icon)
  - Confirm result -> update UI + go back to Main Menu
- **Exit**: Back to Main Menu

### Inventory Scene
- **Entry Point**: From Main Menu (Inventory button)
- **UI**:
  - Item list (scrollable)
  - Item detail panel
  - Use item button
  - Back button
- **Logic**:
  - Display items from IInventoryService
  - Click item -> show detail
  - Use button -> confirm dialog
  - Confirm -> call InventoryService.UseItem()
  - Update list + go back to Main Menu
- **Exit**: Back to Main Menu

### Level Select Scene
- **Entry Point**: From Main Menu (Level Select button)
- **UI**:
  - Chapter tabs (Chapter 1, 2, 3...)
  - Level grid per chapter
  - Level card (number, stars, lock icon)
  - Back button
- **Logic**:
  - Call IProgressionService.GetChapterState()
  - Display chapters + levels with unlock state
  - Click unlocked level -> prepare lineup snapshot -> load Combat scene
  - Click locked level -> show lock message
- **Exit**: Load Combat scene OR back to Main Menu

### Combat Scene
- **Entry Point**: From Level Select (level clicked)
- **Input Data**:
  - levelId
  - lineupSnapshot (from TeamService.GetCurrentLineup)
- **Logic**: Combat logic (Dev A responsibility)
- **Output**: CombatResult (to Result panel)
- **Exit**: Navigate to Result scene / panel

### Result Scene / Panel
- **Entry Point**: After Combat end (Dev A sends CombatResult)
- **UI**:
  - Result display (win/lose)
  - Reward display (XP, items, characters)
  - OK button
- **Logic**:
  - Display result
  - Apply reward (Dev A responsibility)
  - Update save with reward + level progress
  - Click OK -> navigate back to Main Menu OR Level Select
- **Exit**: Back to Main Menu OR Level Select

---

## 🔄 Expected State Transitions

| From | To | Trigger | Condition |
|---|---|---|---|
| Boot | Tutorial | Load | tutorialCompleted = false |
| Boot | Main Menu | Load | tutorialCompleted = true |
| Tutorial | Main Menu | Complete Btn | - |
| Main Menu | Team | Team Btn | - |
| Main Menu | Gacha | Gacha Btn | - |
| Main Menu | Inventory | Inventory Btn | - |
| Main Menu | Level Select | Level Select Btn | - |
| Team | Main Menu | Back Btn | - |
| Gacha | Main Menu | Back Btn | - |
| Inventory | Main Menu | Back Btn | - |
| Level Select | Main Menu | Back Btn | - |
| Level Select | Combat | Level Clicked | unlocked = true |
| Combat | Result | Combat End | - |
| Result | Main Menu | OK Btn | - |
| Result | Level Select | OK Btn (alt) | - |

---

## 📊 Scene Dependency

```
┌─ Boot (root)
│
├─ Tutorial (from Boot if new)
│  └─ Main Menu
│
├─ Main Menu (from Boot if returning, OR from Tutorial)
│  ├─ Team Formation
│  │  └─ Main Menu
│  ├─ Gacha
│  │  └─ Main Menu
│  ├─ Inventory
│  │  └─ Main Menu
│  └─ Level Select
│     ├─ Combat (from level click)
│     │  └─ Result
│     │     ├─ Main Menu
│     │     └─ Level Select
│     └─ Main Menu
```

---

## 🎮 Navigation Controller Responsibility

- Load/Unload scenes (with fade transition)
- Maintain navigation history (for back button)
- Pass data between scenes (levelId, lineupSnapshot, CombatResult)
- Handle scene-ready events (ensure UI initialized before interaction)

---

## ⚡ Event Publish Points

| Event | Trigger | Handler |
|---|---|---|
| TutorialCompletedEvent | Tutorial: Complete btn clicked | Save state update |
| LineupSavedEvent | Team Formation: Validate btn -> valid | Navigation back |
| GachaRollResolvedEvent | Gacha: Roll result received | UI update result panel |
| LevelSelectedEvent | Level Select: Level card clicked | Prepare combat data |
| CombatResultReceivedEvent | Combat end | Show result panel |
| InventoryItemUsedEvent | Inventory: Use item confirmed | Update list + save |

---

## 📝 Notes for Ngày 2

- [ ] Confirm with Dev A: does FlowController handle all these scene loads?
- [ ] Confirm: What does IFlowController.HandleCombatResult() expect as input/output?
- [ ] Confirm: Which events should FlowController subscribe to, which should UI?
- [ ] Confirm: Scene async load behavior (wait for ready, or load in background)?

---

**Created**: 2026-04-01  
**Dev**: Dev B  
**Status**: Draft - Awaiting Dev A confirmation on interface contract
