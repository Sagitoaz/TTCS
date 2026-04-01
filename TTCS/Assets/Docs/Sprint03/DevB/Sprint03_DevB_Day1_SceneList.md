# Sprint 3 Dev B - Scene List & Specifications

> **Ngày tạo**: 2026-04-01 (Ngày 1)  
> **Mục đích**: Liệt kê 8 scene chính với entry/exit point, UI structure, và dependencies

---

## 📋 Scene List Overview

| # | Scene Name | Unity File | Dev A Dep | Dev B Task | Status |
|---|---|---|---|---|---|
| 1 | Boot | `Scenes/Boot.unity` | Save API | Load logic | ⬜ |
| 2 | Tutorial | `Scenes/Tutorial.unity` | Event def | Setup + flow | ⬜ |
| 3 | Main Menu | `Scenes/MainMenu.unity` | NavController | Nav hub | ⬜ |
| 4 | Team Formation | `Scenes/TeamFormation.unity` | ITeamService | Select + validate | ⬜ |
| 5 | Gacha | `Scenes/Gacha.unity` | IGachaService | Roll + result | ⬜ |
| 6 | Inventory | `Scenes/Inventory.unity` | IInventoryService | List + use item | ⬜ |
| 7 | Level Select | `Scenes/LevelSelect.unity` | IProgressionService | Chapter/level grid | ⬜ |
| 8 | Combat Result | `Scenes/CombatResult.unity` | Reward bridge | Result display | ⬜ |

---

## 🎮 Scene Details

### Scene 1: Boot

**File Path**: `Assets/Scenes/Boot.unity`

**Purpose**: 
- Initialize game state
- Check player progress (new vs returning)
- Route to appropriate starting scene

**Entry Point**: Game start / Application.LoadScene("Boot")

**Controller**: `BootController.cs`
```csharp
public class BootController : MonoBehaviour
{
    public IFlowController FlowController { get; set; }
    
    async void Start()
    {
        bool shouldShowTutorial = await FlowController.TryEnterTutorial();
        if (shouldShowTutorial)
            FlowController.OpenTutorial();
        else
            FlowController.OpenMainMenu();
    }
}
```

**Dependencies from Dev A**:
- [ ] SaveManager ready
- [ ] IFlowController.TryEnterTutorial() implementation
- [ ] tutorialCompleted field in save state

**UI Elements**: None (loading screen optional)

**Exit Condition**: Load Tutorial scene OR Main Menu scene

**Notes**:
- Keep boot logic simple, minimize time in Boot scene
- Fade to black while loading

---

### Scene 2: Tutorial

**File Path**: `Assets/Scenes/Tutorial.unity`

**Purpose**:
- Guide new players through core mechanics
- Set tutorial completed flag in save

**Entry Point**: From Boot (new player only)

**Controller**: `TutorialController.cs`
```csharp
public class TutorialController : MonoBehaviour
{
    public IFlowController FlowController { get; set; }
    
    public void ShowStep(int stepIndex)
    public void SkipTutorial()
    public void CompleteTutorial()
}
```

**UI Elements**:
- [ ] Step text panel (center screen)
- [ ] Character/visual (left or center)
- [ ] Next button
- [ ] Skip button (optional)
- [ ] Complete button (last step)

**Tutorial Steps** (example):
1. Welcome + UI orientation
2. Team selection basics
3. Level selection UI
4. Combat mechanics (intro)
5. Reward screen
6. Inventory check
7. Gacha intro
8. Complete confirmation

**Dependencies from Dev A**:
- [ ] TutorialCompletedEvent publish point
- [ ] Event subscription callback (save state update)

**Exit Condition**: Next -> advance step, OR Complete/Skip -> navigate to Main Menu

**Save Behavior**:
- On step 1: Create save backup
- On complete: Write tutorialCompleted = true
- On skip: Also write tutorialCompleted = true

**Notes**:
- Each step ~10 seconds (adjustable)
- Allow skip button to bypass entire tutorial
- Visual consistency with main UI theme

---

### Scene 3: Main Menu

**File Path**: `Assets/Scenes/MainMenu.unity`

**Purpose**:
- Central hub for all gameplay options
- Display player summary (level, team, recent progress)

**Entry Point**: From Boot, Tutorial, or sub-scenes (back button)

**Controller**: `MainMenuUIController.cs`
```csharp
public class MainMenuUIController : MonoBehaviour
{
    public IFlowController FlowController { get; set; }
    
    public void OnTeamButtonClicked() => FlowController.OpenTeamSelection();
    public void OnGachaButtonClicked() => FlowController.OpenGacha();
    public void OnInventoryButtonClicked() => FlowController.OpenInventory();
    public void OnLevelSelectButtonClicked() => FlowController.OpenLevelSelect();
}
```

**UI Elements**:
- [ ] Home panel
  - [ ] Welcome text (e.g., "Welcome back, Player!")
  - [ ] Player summary (optional: level, exp, coins)
  - [ ] 5 Buttons: Team, Gacha, Inventory, Level Select, Settings
- [ ] Settings panel (optional for Sprint 3)

**Layout Reference**:
```
┌─────────────────────────────────────┐
│    Welcome back, Player!            │
│                                     │
│  Lv 5  |  Exp: 250/500     Coins: 100
│                                     │
│  ┌─────────────────────────────┐   │
│  │ │ Team │ Gacha │ Inventory │   │
│  │ │      │       │           │   │
│  │ │ Level Select │ Settings  │   │
│  │ │              │           │   │
│  └─────────────────────────────┘   │
└─────────────────────────────────────┘
```

**Dependencies from Dev A**: None (UI only)

**Exit Condition**: Button click -> navigate to sub-scene

**Navigation Behavior**:
- Push scene onto stack (for back button)
- Maintain menu state (scroll position, selected tab)

**Notes**:
- This is the most important scene - test heavily
- Ensure all buttons are interactable (no miss-click dead zones)
- Smooth button animations (scale/glow on hover)

---

### Scene 4: Team Formation

**File Path**: `Assets/Scenes/TeamFormation.unity`

**Purpose**:
- Allow player to form a team (lineup) for upcoming level
- Validate lineup before confirming

**Entry Point**: From Main Menu (Team button)

**Controller**: `TeamFormationUIController.cs`
```csharp
public class TeamFormationUIController : MonoBehaviour
{
    public ITeamService TeamService { get; set; }
    
    public void DisplayCharacters()
    public void SelectCharacter(string charId)
    public void DeselectCharacter(int slotIndex)
    public void ValidateLineup()
    public void ConfirmAndBack()
}
```

**UI Elements**:
- [ ] Available characters list (scrollable)
  - [ ] Character card (icon, name, rarity, level)
- [ ] Lineup slots (3 slots)
  - [ ] Slot 1, 2, 3 (display selected character or "empty")
- [ ] Validate button
- [ ] Back button
- [ ] Error message panel (show validation error)

**Layout Reference**:
```
┌──────────────────────────────┐
│  Team Formation              │
│                              │
│  Available:      │ Lineup:   │
│  ┌──────────────┐│┌─────────┐│
│  │ Character 1  ││ Slot 1 ✓ ││
│  │ Character 2  ││ Slot 2   ││
│  │ Character 3  ││ Slot 3   ││
│  │ Character 4  ││          ││
│  └──────────────┘│└─────────┘│
│                              │
│  [Validate] [Back]           │
│                              │
│  Error: Invalid lineup       │
└──────────────────────────────┘
```

**Dependencies from Dev A**:
- [ ] ITeamService.ValidateLineup() implementation
- [ ] Character roster data structure
- [ ] LineupSavedEvent definition

**Exit Condition**: 
- Back button -> navigate to Main Menu (no save)
- Validate + valid -> call TeamService.SaveLineup() + navigate to Main Menu
- Validate + invalid -> show error message + allow retry

**Notes**:
- Support drag-and-drop or click to select
- Show character stats (HP, ATK, DEF) on selection
- Color-code validation error (red = invalid)

---

### Scene 5: Gacha

**File Path**: `Assets/Scenes/Gacha.unity`

**Purpose**:
- Allow player to roll for characters/items
- Display roll results and update roster

**Entry Point**: From Main Menu (Gacha button)

**Controller**: `GachaUIController.cs`
```csharp
public class GachaUIController : MonoBehaviour
{
    public IGachaService GachaService { get; set; }
    
    public void SelectPool(string poolId)
    public void OnRollButtonClicked(int count)
    public void ShowRollResult(GachaRollResult result)
    public void Back()
}
```

**UI Elements**:
- [ ] Pool selector (dropdown: "Standard", "Limited", etc.)
- [ ] Pool info panel (description, rates, pity counter)
- [ ] Roll buttons (1x, 10x)
- [ ] Result panel (show pulled character + icon + rarity)
- [ ] History button (optional)
- [ ] Back button

**Layout Reference**:
```
┌──────────────────────────────┐
│  Gacha Roll                  │
│                              │
│  Pool: [Standard ▼]          │
│  Rate: 5★ 3% | 4★ 20%       │
│  Pity: 4/90                  │
│                              │
│  [Roll 1x] [Roll 10x]        │
│                              │
│  ┌─ RESULT ─────────────────┐│
│  │ You got: ⭐⭐⭐⭐⭐      ││
│  │ Character: [Icon] Name    ││
│  │ [OK]                      ││
│  └──────────────────────────┘│
│                              │
│  [Back]                      │
└──────────────────────────────┘
```

**Dependencies from Dev A**:
- [ ] IGachaService.Roll() implementation
- [ ] GachaPoolInfo structure
- [ ] GachaRollResult structure
- [ ] GachaRollResolvedEvent definition

**Exit Condition**:
- Back button -> navigate to Main Menu
- Result OK -> close result panel + go back to roll screen
- Multiple rollers allowed before back

**Notes**:
- Show animation on roll (spinning wheel, etc.)
- Update inventory after roll
- Display pity counter
- Sound effects on roll

---

### Scene 6: Inventory

**File Path**: `Assets/Scenes/Inventory.unity`

**Purpose**:
- Display player items
- Allow using items (potions, consumables)
- Update save after item use

**Entry Point**: From Main Menu (Inventory button)

**Controller**: `InventoryUIController.cs`
```csharp
public class InventoryUIController : MonoBehaviour
{
    public IInventoryService InventoryService { get; set; }
    
    public void DisplayItems()
    public void OnItemClicked(string itemId)
    public void OnUseItemConfirmed(string itemId, int quantity)
    public void Back()
}
```

**UI Elements**:
- [ ] Item list (scrollable)
  - [ ] Item card (icon, name, quantity, rarity)
- [ ] Item detail panel
  - [ ] Description, usage rule
  - [ ] Use button (or multi-use via input field)
- [ ] Confirm dialog (before using item)
- [ ] Back button

**Layout Reference**:
```
┌────────────────────────────┐
│  Inventory                 │
│                            │
│  Items:        │ Detail:   │
│  ┌───────────┐│┌─────────┐│
│  │ Potion +5 ││ Health  ││
│  │ Elixir +3 ││ Potion  ││
│  │ Scroll +2 ││ Restore ││
│  │           ││ 100 HP  ││
│  └───────────┘│         │
│               │[Use]    │
│               │[Get More]││
│               └─────────┘│
│  [Back]                  │
└────────────────────────────┘
```

**Dependencies from Dev A**:
- [ ] IInventoryService.GetItems() implementation
- [ ] IInventoryService.CanUseItem() implementation
- [ ] IInventoryService.UseItem() implementation
- [ ] InventoryItemUsedEvent definition

**Exit Condition**:
- Back button -> navigate to Main Menu
- Use item -> confirm dialog -> execute + update list

**Notes**:
- Show item usage rule (e.g., "Can use outside combat")
- Confirm before consuming item
- Update display after item use

---

### Scene 7: Level Select

**File Path**: `Assets/Scenes/LevelSelect.unity`

**Purpose**:
- Display available chapters and levels
- Show unlock state (locked/unlocked/cleared)
- Prepare combat data and navigate to Combat scene

**Entry Point**: From Main Menu (Level Select button)

**Controller**: `LevelSelectUIController.cs`
```csharp
public class LevelSelectUIController : MonoBehaviour
{
    public IProgressionService ProgressionService { get; set; }
    public IFlowController FlowController { get; set; }
    
    public void DisplayChapters()
    public void OnChapterSelected(string chapterId)
    public void OnLevelClicked(string levelId)
    public void Back()
}
```

**UI Elements**:
- [ ] Chapter tabs (scrollable horizontal)
  - [ ] Tab: "Chapter 1", "Chapter 2", etc.
- [ ] Level grid (3x3 or similar, per chapter)
  - [ ] Level card (icon, name, stars earned, lock icon)
- [ ] Level detail panel (optional)
  - [ ] Description, recommended power, enemies
- [ ] Back button

**Layout Reference**:
```
┌─────────────────────────────────────┐
│  Level Select                       │
│                                     │
│  [Chapter 1] [Chapter 2] [Chapter 3]│
│                                     │
│  ┌─────────────────────────────────┐│
│  │ 1-1     │ 1-2     │ 1-3     │   │
│  │ ⭐⭐⭐ │ ⭐⭐    │ 🔒     │   │
│  │         │         │         │   │
│  ├─────────┼─────────┼─────────┤   │
│  │ 1-4     │ 1-5     │ 1-6     │   │
│  │ ⭐⭐    │ ⭐      │ 🔒     │   │
│  │         │         │         │   │
│  └─────────────────────────────────┘│
│                                     │
│  [Back]                             │
└─────────────────────────────────────┘
```

**Dependencies from Dev A**:
- [ ] IProgressionService.GetChapterState() implementation
- [ ] IProgressionService.GetLevelState() implementation
- [ ] IProgressionService.CanEnterLevel() implementation
- [ ] Chapter/Level JSON schema

**Exit Condition**:
- Back button -> navigate to Main Menu
- Unlocked level clicked -> load Combat scene with levelId + lineup

**Notes**:
- Show lock icon with "Not yet unlocked" message on locked level click
- Display stars earned (0-3) on cleared level
- Smooth transition to Combat scene

---

### Scene 8: Combat Result

**File Path**: `Assets/Scenes/CombatResult.unity` (or modal panel in Combat scene)

**Purpose**:
- Display combat result (win/lose)
- Show rewards (XP, items, characters)
- Apply reward to save and inventory
- Offer next navigation (back to menu or level select)

**Entry Point**: After Combat ends (Dev A sends CombatResult)

**Controller**: `CombatResultUIController.cs`
```csharp
public class CombatResultUIController : MonoBehaviour
{
    public IFlowController FlowController { get; set; }
    
    public void DisplayResult(CombatResult result)
    public void OnContinueClicked()
}
```

**UI Elements**:
- [ ] Result display (win/lose banner)
- [ ] Result summary
  - [ ] Stars earned (0-3)
  - [ ] Score achieved
- [ ] Reward display
  - [ ] XP gained
  - [ ] Items gained
  - [ ] Characters gained
- [ ] Continue button

**Layout Reference**:
```
┌──────────────────────────────┐
│  VICTORY!                    │
│  ⭐⭐⭐ Score: 1500         │
│                              │
│  Rewards:                    │
│  └─ XP: +100                │
│  └─ Gold: +50               │
│  └─ Potion: +2              │
│                              │
│  [Continue]                  │
└──────────────────────────────┘
```

**Dependencies from Dev A**:
- [ ] CombatResult data structure
- [ ] Reward bridge implementation
- [ ] Save write after reward apply

**Exit Condition**:
- Continue button -> navigate to Main Menu OR Level Select

**Notes**:
- Show detailed reward breakdown
- Play victory/defeat animation
- Prevent skip (show full reward info)

---

## 📊 Folder Structure (Assets/Scenes/)

```
Assets/Scenes/
├── Boot.unity
├── Tutorial.unity
├── MainMenu.unity
├── TeamFormation.unity
├── Gacha.unity
├── Inventory.unity
├── LevelSelect.unity
└── CombatResult.unity
```

---

## ✅ Checklist: Scene Infrastructure Setup (Ngày 2)

- [ ] Create all 8 .unity files
- [ ] Setup Canvas (UI Root) in each scene
- [ ] Setup EventSystem (if not global)
- [ ] Create controller prefab for each scene
- [ ] Configure scene in Build Settings (add all 8 scenes)
- [ ] Test scene load sequence (Boot -> Tutorial -> MainMenu -> etc.)

---

**Created**: 2026-04-01  
**Dev**: Dev B  
**Status**: Draft - Ready for Ngày 2 implementation
