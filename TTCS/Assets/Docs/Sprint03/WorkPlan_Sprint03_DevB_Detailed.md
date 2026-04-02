# Sprint 3 - Kế hoạch chi tiết Dev B (Flow/UI Owner) - 14 ngày

> **Người thực hiện**: Dev B  
> **Mục tiêu sprint**: Hoàn thiện UI flow (Tutorial, Main Menu, Team/Gacha/Inventory UI), skill icon pipeline, scene wiring.  
> **Thời gian**: 14 ngày (Giai đoạn 1: 2 ngày, Giai đoạn 2: 5 ngày, Giai đoạn 3: 5 ngày, Giai đoạn 4: 2 ngày)  
> **Phụ thuộc từ Dev A**: Contract service (Progression/Team/Gacha/Inventory), Event definitions, Save schema migration API.

---

## 🎯 Tóm tắt Trách nhiệm Dev B

### Trách nhiệm chính
- ✅ Tutorial scene flow cho người chơi mới
- ✅ Chapter/Level select scene + điều hướng vào combat
- ✅ Skill icon pipeline trong UI (bind + fallback)
- ✅ Main menu UI flow và navigation state
- ✅ Team setup UI (chọn đội hình trước trận)
- ✅ Gacha UI (roll screen + result panel)
- ✅ Inventory UI (list item, filter cơ bản, action dùng item)
- ✅ Wiring scene/prefab/UI để chạy full flow
- ✅ Checklist test scene và regression UI flow

### Không phụ trách chính
- ❌ Thiết kế schema save/progression lõi (Dev A)
- ❌ Rule reward domain (Dev A)

---

## 📋 Giai đoạn 1: Thiết kế và khóa interface (Ngày 1-2)

### Ngày 1 - Kickoff & Draft Design

**Sáng (Morning)**
- [ ] Đọc kỹ `Sprint03_Phase1_Kickoff_Package.md` + `Sprint03_Standardization_Agreement.md`
- [ ] Xác nhận interface `IFlowController` từ Dev A (contract đồng bộ)
- [ ] Tạo branch: `feature/devB-tutorial-scene`

**Chiều (Afternoon)**
- [ ] Draft flow diagram: Boot -> Tutorial? -> MainMenu -> Team/Gacha/Inventory/LevelSelect -> Combat -> Result
- [ ] Tạo mô tả scene list:
  - [ ] BootScene (skip tutorial nếu đã làm)
  - [ ] TutorialScene (hướng dẫn người chơi mới)
  - [ ] MainMenuScene (hub điều hướng)
  - [ ] TeamFormationScene (chọn đội hình)
  - [ ] GachaScene (roll + result panel)
  - [ ] InventoryScene (list item, use item)
  - [ ] LevelSelectScene (chapter/level/unlock state)
  - [ ] CombatScene (vào từ LevelSelect với data)
  - [ ] ResultScene (reward + save + back to menu/level select)

**Deliverables ngày 1**
- [ ] Flow diagram draft (có thể sketch/ASCII)
- [ ] Scene list với entry point và exit condition
- [ ] Checklist UI element cần để render (Team panel, Gacha result, Inventory list, Level grid, etc.)

**Sync cuối ngày với Dev A**
- [ ] Xác nhận: TryEnterTutorial() API từ FlowController hoạt động như thế nào (return bool)
- [ ] Xác nhận: Save state có trường `tutorialCompleted` boolean
- [ ] Xác nhận: Event `TutorialCompletedEvent` publish point

---

### Ngày 2 - Scene Setup & Nav State Machine

**Sáng (Morning)**
- [ ] Setup khung 8 scene cơ bản trong Assets/Scenes/:
  - [ ] `Boot.unity` - Kiểm tra tutorial state, load MainMenu hoặc Tutorial
  - [ ] `Tutorial.unity` - Hướng dẫn step-by-step
  - [ ] `MainMenu.unity` - Home UI + nav buttons
  - [ ] `TeamFormation.unity` - Chọn đội hình
  - [ ] `Gacha.unity` - Roll UI + result panel
  - [ ] `Inventory.unity` - List item + action
  - [ ] `LevelSelect.unity` - Chapter/level grid
  - [ ] `CombatResult.unity` - Result panel + back nav

**Chiều (Afternoon)**
- [ ] Tạo `FlowStateManager` lớp (đơn giản: current scene, nav history)
  ```csharp
  public class FlowStateManager
  {
      public string CurrentScene { get; private set; }
      public Stack<string> NavHistory { get; private set; }
      
      public void NavigateTo(string sceneName)
      public void Back()
      public bool TryEnterTutorial()
  }
  ```
- [ ] Tạo `NavigationController` để handle scene load/unload:
  ```csharp
  public class NavigationController : MonoBehaviour
  {
      public async Task LoadScene(string sceneName)
      public async Task UnloadScene(string sceneName)
  }
  ```

**Deliverables ngày 2**
- [ ] 8 scene skeleton tạo xong, push branch
- [ ] FlowStateManager class cơ bản
- [ ] NavigationController class cơ bản
- [ ] Test Boot scene chuyển logic: new player sau 5s -> Tutorial, returning -> MainMenu

**Sync cuối ngày với Dev A**
- [ ] Input/output contract từ `IFlowController` interface (đặc biệt: `HandleCombatResult(result)` trả về gì)
- [ ] Xác nhận event publish points: `TutorialCompletedEvent`, `LevelEnteredEvent`, `CombatResultReceivedEvent`

---

## 🟢 Giai đoạn 2: Xây core song song (Ngày 3-7)

### Ngày 3 - Tutorial Scene & Save State Hook

**Sáng (Morning)**
- [ ] Layout tutorial UI
  - [ ] Text panel (dẫn dắt bước)
  - [ ] Skip button (optional)
  - [ ] Next / Complete button
- [ ] Tạo `TutorialController` class:
  ```csharp
  public class TutorialController : MonoBehaviour
  {
      public IFlowController FlowController { get; set; }
      
      public void ShowStep(int stepIndex)
      public void SkipTutorial()
      public void CompleteTutorial()
  }
  ```

**Chiều (Afternoon)**
- [ ] Hook save state:
  - [ ] Call `IFlowController.TryEnterTutorial()` ở Boot (check `tutorialCompleted` via Dev A's API)
  - [ ] Subscribe `TutorialCompletedEvent` -> toggle save state
  - [ ] Test flow: new player -> tutorial -> complete -> save write -> restart -> skip tutorial

**Deliverables ngày 3**
- [ ] Tutorial UI layout + controller
- [ ] Boot scene khóa tutorial entry logic
- [ ] Event subscription working
- [ ] Checklist: New player run tutorial -> save written; Restart -> skip tutorial

**Sync bắt buộc với Dev A (Ngày 3)**
- [ ] Verify `IProgressionService.GetChapterState()` có trả về unlock state
- [ ] Verify `SaveManager` đã implement migration từ old save schema
- [ ] Checklist: "Tutorial state lưu/tải via Save contract"

---

### Ngày 4 - Main Menu & Team Setup UI

**Sáng (Morning)**
- [ ] Main Menu UI layout:
  - [ ] Home panel (welcome text + 4 buttons)
  - [ ] Team button (-> TeamFormation scene)
  - [ ] Gacha button (-> Gacha scene)
  - [ ] Inventory button (-> Inventory scene)
  - [ ] Level Select button (-> LevelSelect scene)

- [ ] Tạo `MainMenuUIController`:
  ```csharp
  public class MainMenuUIController : MonoBehaviour
  {
      public IFlowController FlowController { get; set; }
      
      public void OnTeamButtonClicked() => FlowController.OpenTeamSelection();
      public void OnGachaButtonClicked() => FlowController.OpenGacha();
      // ...
  }
  ```

**Chiều (Afternoon)**
- [ ] Team Formation scene layout:
  - [ ] Team panel voi 3 slot nhan vat (moi slot co 1 button de mo picker)
  - [ ] Character picker panel (scroll list)
  - [ ] Confirm add panel (Yes/No)
  - [ ] Filter trong picker: level asc/desc, rarity asc/desc, role
  - [ ] Rule loai tru trong picker: nhan vat het HP, nhan vat da ra tran, nhan vat da duoc chon o slot khac

- [ ] Tạo `TeamFormationUIController`:
  ```csharp
  public class TeamFormationUIController : MonoBehaviour
  {
      public ITeamService TeamService { get; set; }

      public void OnSlotClicked(int slotIndex)
      public void SetSortByLevelAsc()
      public void SetSortByLevelDesc()
      public void SetSortByRarityAsc()
      public void SetSortByRarityDesc()
      public void SetRoleFilter(string roleTag)
      public void Back()
  }
  ```

**Deliverables ngày 4**
- [ ] Main Menu scene fully wired
- [ ] Team Formation scene layout + basic controller
- [ ] Navigation Main Menu -> Team -> Back to Main Menu working
- [ ] Test: Slot button -> picker -> chon nhan vat -> confirm Yes -> quay lai team panel
- [ ] Test: Slot co nhan vat phai hien portrait + HP slider + HP text + level
- [ ] Test: Filter level/rarity/role trong picker hoat dong dung
- [ ] Test: Picker loai tru dung nhan vat het HP/da ra tran/da co o slot khac

**Sync bắt buộc**
- [ ] Input structure cho Team (expect `IReadOnlyList<string>` lineup)
- [ ] Validation error types từ `ITeamService.ValidateLineup()`

---

### Ngày 5 - Skill Icon Pipeline & Gacha UI v1

**Sáng (Morning)**
- [ ] Skill icon data structure:
  - [ ] `Assets/Data/Meta/skill_icon_map.json` (từ standardization agreement)
  - [ ] Map: `skillId` -> `assetPath` (or null for fallback)
  - [ ] Tạo `SkillIconLoader` class:
    ```csharp
    public class SkillIconLoader
    {
        public Sprite LoadSkillIcon(string skillId)
        // Return actual icon or fallback texture
    }
    ```

- [ ] Tạo fallback texture (placeholder icon)

**Chiều (Afternoon)**
- [ ] Gacha scene layout:
  - [ ] Pool selector (dropdown: "Standard", "Limited", etc.)
  - [ ] Roll button (1x, 10x)
  - [ ] Result panel (show pulled character + icon)
  - [ ] Gacha result history (optional v1)

- [ ] Tạo `GachaUIController`:
  ```csharp
  public class GachaUIController : MonoBehaviour
  {
      public IGachaService GachaService { get; set; }
      
      public void OnRollButtonClicked(int count)
      public void ShowRollResult(GachaRollResult result)
      public void Back()
  }
  ```

**Deliverables ngày 5**
- [ ] skill_icon_map.json sample data tạo xong
- [ ] SkillIconLoader class working (load from atlas hoặc fallback)
- [ ] Gacha scene layout + controller
- [ ] Test: Gacha roll trigger -> show result with icon (mock GachaService)

---

### Ngày 6 - Inventory UI & Level Select Wiring

**Sáng (Morning)**
- [ ] Inventory scene layout:
  - [ ] Item list (scrollable: potions, consumables, etc.)
  - [ ] Item detail panel (name, description, use button)
  - [ ] Use item button + confirm dialog
  - [ ] Back button

- [ ] Tạo `InventoryUIController`:
  ```csharp
  public class InventoryUIController : MonoBehaviour
  {
      public IInventoryService InventoryService { get; set; }
      
      public void DisplayItems()
      public void OnItemClicked(string itemId)
      public void OnUseItemConfirmed(string itemId, int quantity)
  }
  ```

**Chiều (Afternoon)**
- [ ] Level Select scene layout:
  - [ ] Chapter tabs (Chapter 1, 2, 3, etc. - scrollable)
  - [ ] Level grid per chapter (level_01, level_02, etc.)
  - [ ] Level card (level number, stars earned, unlock icon nếu locked)
  - [ ] Back to MainMenu button

- [ ] Tạo `LevelSelectUIController`:
  ```csharp
  public class LevelSelectUIController : MonoBehaviour
  {
      public IProgressionService ProgressionService { get; set; }
      
      public void DisplayChapters()
      public void OnChapterSelected(string chapterId)
      public void OnLevelClicked(string levelId)
      public void Back()
  }
  ```

**Deliverables ngày 6**
- [ ] Inventory scene layout + controller
- [ ] Level Select scene layout + controller (mock data for 3 chapters, 3 levels each)
- [ ] Test: Click level -> call `EnterCombat()` from FlowController
- [ ] Skill icon appear in character select (if visible in team formation)

---

### Ngày 7 - Integration lần 1 & Regression

**Sáng (Morning)**
- [ ] Test full flow (Dev B + Dev A collaboration):
  - [ ] [ ] Boot -> Tutorial (new player) -> Complete -> Save
  - [ ] [ ] Restart -> Skip tutorial -> Main Menu
  - [ ] [ ] Main Menu -> Team -> Select lineup -> Validate -> Back
  - [ ] [ ] Main Menu -> Gacha -> Roll -> Show result -> Back
  - [ ] [ ] Main Menu -> Inventory -> Use item -> Back
  - [ ] [ ] Main Menu -> Level Select -> Click level -> Enter combat

**Chiều (Afternoon)**
- [ ] Lập danh sách lỗi từ integration test
  - [ ] Classify: Blocker (C0), Critical (C1), Major (C2), Minor (C3)
  - [ ] Phân công fix (Dev B cho UI flow/scene, escalate to Dev A nếu service)

- [ ] Checklist regression:
  - [ ] Không crash UI
  - [ ] Navigation back working
  - [ ] Save state consistency
  - [ ] Event fires đúng sequence
  - [ ] No regex lỗi compile

**Deliverables ngày 7**
- [ ] Full flow end-to-end integration test (pass hoặc danh sách lỗi)
- [ ] PR #1 ready: `feature/devB-tutorial-scene` (Ngày 1-3)
- [ ] PR #2 ready: `feature/devB-menu-team-gacha-inventory` (Ngày 4-7)
- [ ] Checklist kiểm thử UI flow (bắt đầu fix lỗi từ Giai đoạn 3)

---

## 🟠 Giai đoạn 3: Ổn định và hoàn thiện (Ngày 8-12)

### Ngày 8-9 - UI Polish & Bug Fix

**Sáng Ngày 8**
- [ ] Từ danh sách lỗi Ngày 7, pick top 5 blocker/critical UI issues
  - [ ] Scene transition smooth animation (fade/slide)
  - [ ] Button disable state khi loading
  - [ ] Error message display + auto dismiss
  - [ ] Navigation history correct (back button behavior)
  - [ ] UI state reset khi navigate lại (e.g., refresh item list)

**Chiều Ngày 8 + Sáng Ngày 9**
- [ ] Implement fix cho mỗi issue
- [ ] Add unit test nếu có (validator, state manager)

**Chiều Ngày 9**
- [ ] Regression test mỗi fix: navigate full flow again
- [ ] Checklist:
  - [ ] Animations smooth
  - [ ] No UI glitch
  - [ ] Event sequence correct
  - [ ] No null reference exceptions

**Deliverables Ngày 8-9**
- [ ] Top 5 issues resolved
- [ ] PR #3 ready: Bug fix batch 1
- [ ] UI feel improved

---

### Ngày 10 - Sync chính giữa Dev A + Dev B (MANDATORY)

**Yêu cầu bắt buộc**
- [ ] Full flow test từ account mới + cũ
- [ ] Regression test với inventory/gacha/combat hiện có
- [ ] Chốt danh sách bug cuối sprint
- [ ] Checklist kiểm thử kỹ thuật hoàn tất

**Sáng**
- [ ] Dev B + Dev A run end-to-end test cùng nhau
  - [ ] [ ] Tạo account mới, run tutorial
  - [ ] [ ] Restart, skip tutorial, go to level select
  - [ ] [ ] Select team, enter level, exit (mock combat result)
  - [ ] [ ] Check reward applied, inventory updated, save written
  - [ ] [ ] Restart, verify save restore

**Chiều**
- [ ] Regression checklist:
  - [ ] Không break combat flow cũ
  - [ ] Gacha pooling vẫn hoạt động
  - [ ] Inventory item use rule không thay đổi
  - [ ] No new crash

**Deliverables Ngày 10**
- [ ] Sign-off: Full flow pass (hoặc danh sách lỗi còn lại và ETA fix)
- [ ] Regression pass confirm
- [ ] Test report document (optional, or just checklist ✓)

---

### Ngày 11-12 - Scene Polish & Content Ready

**Ngày 11**
- [ ] Hoàn thiện UX tutorial scene:
  - [ ] [ ] Clear instruction text cho mỗi step
  - [ ] [ ] Visual feedback (highlight, button glow)
  - [ ] [ ] Timing reasonable (not too fast, not too slow)
  - [ ] [ ] Skip button behavior confirm (safe to skip)

- [ ] Hoàn thiện icon/visuals fallback:
  - [ ] [ ] Skill icon fallback chạy stable
  - [ ] [ ] Character portrait fallback chạy stable
  - [ ] [ ] Item icon fallback chạy stable

**Ngày 12**
- [ ] Finalize content data:
  - [ ] [ ] Mẫu JSON cho Chapters (3 chapters, mỗi 3 level)
  - [ ] [ ] Mẫu JSON cho Levels (unlock condition clear)
  - [ ] [ ] Gacha pool config ready (standard + limited)
  - [ ] [ ] Item list config ready (potions, etc.)

- [ ] Documentation:
  - [ ] [ ] README.md cho UI folder (scene list, script list)
  - [ ] [ ] List UI states/transitions (diagram hoặc table)

**Deliverables Ngày 11-12**
- [ ] PR #4: Scene finalization + content data samples
- [ ] Tutorial UX polished
- [ ] Content JSON ready for game design phase

---

## 🔴 Giai đoạn 4: Core Freeze (Ngày 13-14)

### Ngày 13 - Final Regression & Feature Freeze

**MANDATORY: Feature Freeze từ hôm nay**
- Không thêm tính năng mới
- Chỉ fix blocker/critical lỗi

**Sáng**
- [ ] Regression pass cuối (checklist đầy đủ)
  - [ ] Tutorial flow: new player ok, skip ok
  - [ ] Main menu: nav working
  - [ ] Team selection: validate ok
  - [ ] Gacha: roll display ok
  - [ ] Inventory: use item ok
  - [ ] Level select: unlock state correct
  - [ ] Save/load: consistency ok
  - [ ] No crash, no regression

**Chiều**
- [ ] Bug review: chỉ fix C0 (blocker) hoặc C1 (critical)
  - [ ] Identify C0 từ regression
  - [ ] Fix + retest
  - [ ] Log lại vào bug tracker

**Deliverables Ngày 13**
- [ ] Regression checklist 100% pass hoặc documented C0/C1 list
- [ ] No non-blocker PRs merged từ giờ

---

### Ngày 14 - Build Confirm & Handover

**Sáng**
- [ ] Final build (production ready):
  - [ ] Build development + test in mock environment
  - [ ] Verify: No crash, no console error
  - [ ] Verify: Scenes load, UI render, no asset missing

**Chiều**
- [ ] Handover documentation:
  - [ ] [ ] Scene list + entry point
  - [ ] [ ] Navigation map (scene -> scene)
  - [ ] [ ] UI state machine doc (if complex)
  - [ ] [ ] Known limitations (if any)
  - [ ] [ ] Next phase TODOs (e.g., more polish, more content, etc.)

- [ ] Merge vào tích hợp branch + tag mốc
  ```bash
  git tag sprint3-devB-flow-ui-complete
  ```

**Deliverables Ngày 14**
- [ ] Build confirmed stable
- [ ] All PRs merged
- [ ] Handover doc ready
- [ ] Tag created + announce

---

## 📊 Checklist Theo Dõi Tiến Độ Hàng Ngày

### Ngày 1
- [ ] Đọc kickoff package
- [ ] Draft flow diagram
- [ ] Scene list + UI elements checklist
- [ ] Branch created
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 2
- [ ] 8 scenes skeleton
- [ ] FlowStateManager class
- [ ] NavigationController class
- [ ] Boot logic test
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 3
- [ ] Tutorial UI layout
- [ ] TutorialController
- [ ] Save state hook
- [ ] Event subscription test
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 4
- [ ] Main Menu UI
- [ ] Team Formation UI
- [ ] MainMenuUIController
- [ ] TeamFormationUIController
- [ ] Navigation test
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 5
- [ ] Skill icon map data
- [ ] SkillIconLoader class
- [ ] Fallback texture
- [ ] Gacha scene layout
- [ ] GachaUIController
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 6
- [ ] Inventory scene layout
- [ ] InventoryUIController
- [ ] Level Select scene layout
- [ ] LevelSelectUIController
- [ ] Skill icon integration
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 7
- [ ] Full flow integration test
- [ ] Lập danh sách lỗi
- [ ] PR #1 + #2 ready
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 8-9
- [ ] Bug fix batch 1 (top 5 issues)
- [ ] Regression test
- [ ] PR #3 ready
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 10
- [ ] Full flow sign-off with Dev A
- [ ] Regression pass
- [ ] Test report
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 11-12
- [ ] Tutorial UX polish
- [ ] Icon fallback finalize
- [ ] Content JSON ready
- [ ] Documentation
- [ ] PR #4 ready
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 13
- [ ] Final regression pass
- [ ] C0/C1 bug fix
- [ ] Feature freeze
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

### Ngày 14
- [ ] Final build confirm
- [ ] Handover doc
- [ ] Tag created
- [ ] **Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

---

## 📌 Test Checklist Chi Tiết

### Functional Test Checklist (Ngày 7 + Ngày 10 + Ngày 13)

#### Tutorial Flow
- [ ] New player enter -> tutorial trigger
- [ ] Tutorial step display correct
- [ ] Next button -> advance step
- [ ] Complete button -> save + event + go to main menu
- [ ] Skip button -> skip + save + go to main menu (if allowed)
- [ ] Returning player -> bypass tutorial

#### Main Menu Navigation
- [ ] Team button -> team formation scene
- [ ] Gacha button -> gacha scene
- [ ] Inventory button -> inventory scene
- [ ] Level Select button -> level select scene
- [ ] Back button (trong sub-scene) -> back to main menu

#### Team Formation
- [ ] Display 3 slot buttons (slot 1/2/3)
- [ ] Slot click -> open picker panel
- [ ] Picker excludes dead/deployed/already-selected characters
- [ ] Picker filter works: level asc/desc, rarity asc/desc, role
- [ ] Character click -> show confirm Yes/No
- [ ] Confirm Yes -> save lineup + return to team panel
- [ ] Slot displays portrait + current HP + mana + level

#### Gacha UI
- [ ] Display pool list
- [ ] Select pool
- [ ] Roll button (1x) -> call GachaService.Roll()
- [ ] Roll button (10x) -> call GachaService.Roll()
- [ ] Result panel -> show pulled character + icon
- [ ] Back button -> return to main menu

#### Inventory UI
- [ ] Display item list
- [ ] Click item -> show detail panel
- [ ] Use button -> confirm dialog
- [ ] Confirm -> call InventoryService.UseItem()
- [ ] Success feedback
- [ ] Back button -> return to main menu

#### Level Select
- [ ] Display chapter tabs
- [ ] Click chapter tab -> show level grid
- [ ] Level card show: level name, stars, unlock icon
- [ ] Click unlocked level -> call FlowController.EnterCombat()
- [ ] Click locked level -> show lock message
- [ ] Back button -> return to main menu

#### Save/Load Consistency
- [ ] Startup: read save state
- [ ] After tutorial: write tutorial completed flag
- [ ] After team select: checkpoint save
- [ ] After inventory use item: update inventory in save
- [ ] After gacha: update roster in save
- [ ] Restart: verify all state restored

#### Integration Flow (Full Loop)
- [ ] Start app -> boot scene
- [ ] If new: tutorial scene
- [ ] Complete tutorial -> main menu
- [ ] Navigate: menu -> team -> gacha -> inventory -> level select
- [ ] Select level -> combat
- [ ] Combat mock return -> result panel
- [ ] Confirm result -> back to main menu or level select
- [ ] Restart app -> skip tutorial -> main menu

---

### Technical Test Checklist (Ngày 13 + 14)

#### Build Quality
- [ ] No compile error
- [ ] No missing assets reference
- [ ] No console error when running
- [ ] No null reference exception
- [ ] Memory usage reasonable (no leak)

#### Performance
- [ ] Scene load time < 2s
- [ ] Button click -> instant response
- [ ] Animation smooth (60 fps)
- [ ] No frame drop during navigation

#### Code Quality
- [ ] No hardcode secret/password
- [ ] Logging points available (debug)
- [ ] Code comment where complex
- [ ] Consistent naming convention (PascalCase/camelCase)

#### UI State Machine
- [ ] UI enable/disable correct per scene
- [ ] Button interactable state correct (e.g., disable khi loading)
- [ ] Input not blocked when UI loading
- [ ] No duplicate init (avoid OnEnable multiple action)

---

## 🔗 Dependencies from Dev A

| Hạng mục | Dev A API | Expected Ngày |
|---|---|---|
| IFlowController interface | Contract definition | Ngày 1-2 |
| Save schema migration | SaveManager API | Ngày 3-4 |
| IProgressionService | Contract + mock | Ngày 4-5 |
| ITeamService | Contract + mock | Ngày 4-5 |
| IGachaService | Contract + mock | Ngày 5-6 |
| IInventoryService | Contract + mock | Ngày 6-7 |
| Event definitions | Event class list | Ngày 3-7 |
| Combat result data | CombatResult structure | Ngày 6-7 |

**Action**: Sync cuối mỗi ngày để xác nhận Dev A có delivery đúng hạn.

---

## 📋 PR Checklist (Bắt buộc cho mỗi PR)

```
## PR: [Feature Name]

### Scope ✅
- [ ] Scope đúng ownership (Dev B: UI/Flow)
- [ ] 1 PR = 1 mục tiêu rõ
- [ ] Không trộn multiple features

### Quality ✅
- [ ] [ ] Compile pass
- [ ] [ ] No breaking change
- [ ] [ ] Code review self-check
- [ ] [ ] Test checklist attached

### Testing ✅
- [ ] [ ] Unit test (if applicable)
- [ ] [ ] Scene test manual (step-by-step)
- [ ] [ ] Integration point test (if feature integrates with Dev A)
- [ ] [ ] Regression: không break flow cũ

### Data ✅
- [ ] [ ] JSON sample included (if schema change)
- [ ] [ ] Migration doc (if save structure change)

### Review ✅
- [ ] [ ] Dev A approve (if touch shared interface)
- [ ] [ ] Code review pass
- [ ] [ ] Merge and close

---

## 🎯 Success Criteria - Sprint 3 Là DONE Khi

- [x] Tutorial scene hoạt động: new player -> tutorial -> save; returning player -> skip
- [x] Main menu điều hướng: đến Team/Gacha/Inventory/LevelSelect ok
- [x] Team formation UI: chọn đội hình, validate, save ok
- [x] Gacha UI: roll, show result, update ok
- [x] Inventory UI: list item, use item, save ok
- [x] Level Select: show chapters, levels, unlock state, enter combat ok
- [x] Skill icon: load + fallback stable
- [x] Full flow: boot -> tutorial(?) -> menu -> sub-scenes -> combat -> result -> menu ok
- [x] Save/Load: state consistent across restart
- [x] No crash, no compile error, no console error
- [x] PR review by Dev A pass
- [x] Regression test pass
- [x] Final build stable tag released

---

## 📞 Communication Points

### Daily (Hàng ngày)
- Slack / Discord: report blocker, ask clarification

### Ngày 3 (Sync 1)
- 30min sync: khóa interface từ code
- Confirm: Save API, Event definitions, FlowController behavior

### Ngày 7 (Sync 2 - Integration)
- 1h sync: full flow test + danh sách lỗi
- Phân công fix: Dev A (service), Dev B (UI)

### Ngày 10 (Sync 3 - MANDATORY)
- 1h sync: full regression + sign-off
- Confirm: Ready for Giai đoạn 3

### Ngày 13 (Sync 4)
- 30min: final check + bug triage
- Only blocker/critical fix allowed

---

## 🎁 Deliverables Summary

| Ngày | PR | Deliverable |
|---|---|---|
| 1-2 | setup | Scene skeleton, Flow state manager |
| 3 | #1 | Tutorial scene + Boot logic |
| 4-7 | #2 | Main Menu, Team, Gacha, Inventory, Level Select |
| 8-9 | #3 | Bug fix batch 1 |
| 11-12 | #4 | Scene polish + Content JSON |
| 14 | - | Final build + Tag |

---

**Xác nhận**: Kế hoạch này tuân thủ `Sprint03_Standardization_Agreement.md` và schedule từ `WorkPlan_Sprint03.md`.  
**Ngày tạo**: 2026-04-01  
**Dev B**: Confirm nhận kế hoạch?
