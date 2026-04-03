# Sprint 3 - Daily Progress Checklist Dev B

> **Ngày cập nhật lần cuối**: 2026-04-01 (Audit + update Day 1-3)  
> **Dev**: Dev B  
> **Mục tiêu**: Theo dõi task hàng ngày, đánh dấu hoàn thành, lập danh sách blocker

---

## 📅 Giai đoạn 1: Thiết kế & Khóa Interface (Ngày 1-2)

### Ngày 1 - Kickoff & Draft Design
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done ✅

**Task List**:
- [x] Đọc kickoff package + standardization agreement
- [x] Xác nhận interface IFlowController từ Dev A
- [x] Tạo branch: feature/devB-tutorial-scene (Ready)
- [x] Draft flow diagram: Boot -> Tutorial? -> MainMenu -> scenes (Completed: Sprint03_DevB_Day1_FlowDiagram.md)
- [x] Tạo mô tả scene list (8 scenes) (Completed: Sprint03_DevB_Day1_SceneList.md)
- [x] Checklist UI elements cần render (Completed: Sprint03_DevB_Day1_UIElements.md)
- [ ] Sync cuối ngày với Dev A (TryEnterTutorial API, tutorialCompleted field, event publish point) - PENDING

**Deliverables Completed**:
1. Sprint03_DevB_Day1_FlowDiagram.md - Flow diagram + scene dependency + event publish points
2. Sprint03_DevB_Day1_SceneList.md - 8 scene specifications + role/responsibility
3. Sprint03_DevB_Day1_UIElements.md - UI element inventory + shared components + animation checklist
4. Sprint03_DevB_Day1_SyncChecklist.md - 8 questions for Dev A + sync meeting agenda

**Notes**: All design documents drafted and ready for review. Awaiting Dev A sync to confirm interface contracts.

**Blocker** (nếu có): None - design work complete, sync pending

**ETA Complete**: 2026-04-01 (End of day after Dev A sync)

---

### Ngày 2 - Scene Setup & Nav State Machine
**Status**: ⬜ Not started / 🟡 In progress ✅ / 🟢 Done

**Task List**:
- [x] Setup scene skeleton core flow (Boot, TutorialScene, MainMenuScene, LevelSelectScene)
- [ ] Setup đủ 8 scene skeleton trong Assets/Scenes/ (còn thiếu: TeamFormation, Gacha, Inventory, CombatResult)
- [x] Tạo FlowStateManager class
- [x] Tạo NavigationController class
- [x] Test Boot scene: new player -> Tutorial, returning -> MainMenu
- [x] Push branch (feature/devB-tutorial-scene)
- [ ] Sync cuối ngày với Dev A (IFlowController contract, event publish points)

**Notes**: Đã hoàn thành flow core scene cho Day 2 và class hạ tầng điều hướng. Các scene meta-loop còn thiếu sẽ setup trong Unity Editor trước Day 4.

**Blocker** (nếu có): Chưa merge Dev A nên chưa xác nhận event publish points chính thức.

**ETA Complete**: 2026-04-02 (partial complete); phần còn lại chuyển sang setup Unity trước Day 4.

---

## 🟢 Giai đoạn 2: Xây Core Song Song (Ngày 3-7)

### Ngày 3 - Tutorial Scene & Save State Hook
**Status**: ⬜ Not started / 🟡 In progress ✅ / 🟢 Done

**Task List**:
- [x] Tạo Tutorial scene layout + controller (TutorialFlowLogic + TutorialController API)
- [x] Hook save state: TryEnterTutorial() từ Boot
- [x] Subscribe TutorialCompletedEvent
- [x] Test: new player -> tutorial -> complete -> save
- [x] Test: restart -> skip tutorial
- [ ] **SYNC BẮT BUỘC**: Verify IProgressionService, SaveManager API, event subscription

**Notes**: Đã thêm Flow events (`TutorialCompletedEvent`, `LevelEnteredEvent`, `CombatResultReceivedEvent`) và publish/subscribe points trong FlowController + TutorialFlowLogic.

**Blocker** (nếu có): Save contract chính thức từ Dev A chưa merge, hiện dùng fallback PlayerPrefs.

**ETA Complete**: 2026-04-03 (functional complete với mock/fallback, chờ sync contract Dev A).

---

### Ngày 4 - Main Menu & Team Setup UI + Skill Rendering 補充
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done ✅

**Task List**:
- [x] Tạo Main Menu scene layout + controller (controller wired: Team/Gacha/Inventory/LevelSelect)
- [x] Tạo Team Formation scene layout + controller (TeamFormationUIController added)
- [x] Test: Main Menu -> Team -> Back to Main Menu (IN-EDITOR PASS ✅)
- [x] Test: Select 3 characters, validate (ITeamService.ValidateLineup working ✅)
- [x] **補充 (Day 4.5)**: Skill image rendering in QuickInfo panel
  - [x] SkillIconQuickItemView.cs tạo xong
  - [x] TeamFormationUIController.DisplaySkillsInQuickInfo() implemented
  - [x] QuickInfo Skill Root + Prefab setup trong scene
- [x] Sync setup checklist complete

**Notes**: Ngày 4 HOÀN THÀNH + bổ sung skill rendering từ Day 4.5. Setup guide: `DevB_Day4and5_Unified_Setup_Guide.md`

**Blocker** (nếu có): None - feature complete ✅

**ETA Complete**: 2026-04-03 (DONE) ✅

---

### Ngày 5 - Skill Icon Pipeline & Gacha UI v1
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Tạo SkillIconLoader class (load + fallback)
- [ ] Tạo skill_icon_map.json sample data
- [ ] Tạo fallback texture (grey icon)
- [ ] Setup Gacha scene layout + controller (GachaUIController)
- [ ] Wire roll buttons (1x, 10x) and result panel
- [ ] Test: Roll -> Show result with character portrait + rarity
- [ ] Test: skill icons appear đúng trong picker + gacha
- [ ] Verify fallback handling (missing icon path)
- [ ] Integration test: MainMenu -> Gacha -> Roll -> Result -> Back

**Notes**: Setup guide chi tiết: `DevB_Day4and5_Unified_Setup_Guide.md` (Part 2: Day 5 + Part 3: Gacha Scene + Part 4: Test Checklist)

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: 2026-04-03 (bắt đầu hôm nay)

---

### Ngày 6 - Inventory UI & Level Select Wiring
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Tạo Inventory scene layout + controller
- [ ] Tạo Level Select scene layout + controller (mock data: 3 chapters x 3 levels)
- [ ] Tạo Character Collection scene layout + controller
- [ ] Tạo list ô vuông character card (avatar + rare badge góc phải trên)
- [ ] Tạo search theo tên character
- [ ] Tạo sort/filter rare asc/desc
- [ ] Tạo sort/filter level asc/desc
- [ ] Tạo detail panel khi click card (name, rare, level, portrait, stats, HP hiện tại, Mana hiện tại)
- [ ] Tạo feed button tăng level character
- [ ] Rule: mỗi lần level up thì hồi full HP + Mana
- [ ] Test: Click level -> call EnterCombat()
- [ ] Verify skill icon appear trong UI

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 7 - Integration lần 1 & Regression
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Full flow integration test (with Dev A):
  - [ ] Boot -> Tutorial -> Complete -> Save -> Restart -> Skip -> Main Menu
  - [ ] Main Menu -> Team -> Gacha -> Inventory -> Character Collection -> Level Select -> Combat
- [ ] Lập danh sách lỗi (classify: C0, C1, C2, C3)
- [ ] Verify: No crash, no compile error, event sequence correct
- [ ] PR #1 ready: feature/devB-tutorial-scene
- [ ] PR #2 ready: feature/devB-menu-team-gacha-inventory-character
- [ ] Prepare bug list cho Giai đoạn 3
- [ ] DevB tự triển khai feed level + cập nhật HP/Mana hiện tại sau level up (không phụ thuộc sync contract DevA)

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

## 🟠 Giai đoạn 3: Ổn Định & Hoàn Thiện (Ngày 8-12)

### Ngày 8-9 - UI Polish & Bug Fix Batch 1
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Pick top 5 blocker/critical issues từ Ngày 7
- [ ] Implement fix cho scene transition animation
- [ ] Implement fix cho button disable state
- [ ] Implement fix cho error message display
- [ ] Implement fix cho navigation history
- [ ] Implement fix cho UI state reset on navigate
- [ ] Regression test: full flow again after each fix
- [ ] PR #3: Bug fix batch 1

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 10 - MANDATORY Sync: Dev A + Dev B
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] **SYNC BẮT BUỘC**: 1h sync với Dev A
- [ ] Full flow integration test (new account + old account)
- [ ] Regression: không break combat/gacha/inventory cũ
- [ ] Lập danh sách bug còn lại (nếu có)
- [ ] Sign-off: Full flow pass hoặc documented issue list
- [ ] Checklist kiểm thử hoàn tất ✓ hoặc 🔴 failed

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 11-12 - Scene Polish & Content Ready
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Polish Tutorial scene UX (clear instruction, visual feedback, timing)
- [ ] Finalize icon/visuals fallback (stable)
- [ ] Tạo JSON sample: Chapters (3 chapters x 3 levels)
- [ ] Tạo JSON sample: Levels (với unlock condition)
- [ ] Tạo JSON sample: Gacha pool config
- [ ] Tạo JSON sample: Item list config
- [ ] Polish Character Collection UX (search/filter/sort mượt, giữ state)
- [ ] Polish feedback feed level-up (toast/effect + refresh stat card)
- [ ] Tạo README.md cho UI folder
- [ ] Tạo diagram/table: UI states & transitions
- [ ] PR #4: Scene finalization + content data

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

## 🔴 Giai đoạn 4: Core Freeze (Ngày 13-14)

### Ngày 13 - Final Regression & Feature Freeze
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] **FEATURE FREEZE từ hôm nay**: Không thêm feature mới
- [ ] Regression checklist đầy đủ (tutorial, menu, team, gacha, inventory, level, save/load)
- [ ] Bug triage: Identify C0 blocker only
- [ ] Fix C0 bugs + retest
- [ ] Log bugs vào tracker

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 14 - Final Build & Handover
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Final build (production ready)
- [ ] Verify: No crash, no console error, no asset missing
- [ ] Handover doc: Scene list, navigation map, UI state machine, known limitations, next TODOs
- [ ] Merge all PRs vào integration branch
- [ ] Create tag: sprint3-devB-flow-ui-complete
- [ ] Announce completion

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

## 📊 Summary Progress Tracker

| Giai đoạn | Ngày | Status | Notes |
|---|---|---|---|
| 1 | 1-2 | ✅ DONE | Scene skeleton complete |
| 2 | 3 | ✅ DONE | Tutorial + Save state |
| 2 | 4 | ✅ DONE (補充 4.5) | MainMenu + Team + Skill rendering |
| 2 | 5 | 🟡 IN PROGRESS | Skill Icon Pipeline + Gacha UI v1 |
| 2 | 6 | ⬜ PENDING | Inventory + Level Select |
| 2 | 7 | ⬜ PENDING | Integration test |
| 3 | 8-9 | ⬜ PENDING | Polish + Bug fix |
| 3 | 10 | ⬜ PENDING | Mandatory sync DevA |
| 3 | 11-12 | ⬜ PENDING | Scene finalize + content |
| 4 | 13-14 | ⬜ PENDING | Feature freeze + handover |

---

## 🚦 Blocker Tracking

| Date | Blocker | Severity | Status | Resolution |
|---|---|---|---|---|
| TBD | | C0/C1 | Open/In progress/Resolved | |
| TBD | | C0/C1 | Open/In progress/Resolved | |

---

## 👥 Sync Meeting Log

### Ngày 3 Sync (Interface Kickoff)
- **Participants**: Dev A, Dev B
- **Duration**: 30 mins
- **Confirmation**:
  - [ ] Save API (tutorialCompleted field)
  - [ ] IFlowController behavior
  - [ ] Event publish points
- **Notes**: _____________________________________________________

---

### Ngày 7 Sync (Integration v1)
- **Participants**: Dev A, Dev B
- **Duration**: 1 hour
- **Issues Found**:
  - (list here)
- **Action Items**: (Dev A, Dev B assignments)
- **Notes**: _____________________________________________________

---

### Ngày 10 Sync (MANDATORY)
- **Participants**: Dev A, Dev B
- **Duration**: 1 hour
- **Regression Results**:
  - [ ] Tutorial flow: PASS / FAIL
  - [ ] Menu navigation: PASS / FAIL
  - [ ] Save/Load consistency: PASS / FAIL
  - [ ] No crash during full loop: PASS / FAIL
- **Sign-off**: Ready for Giai đoạn 3 / Blockers remain
- **Notes**: _____________________________________________________

---

### Ngày 13 Sync (Final Check)
- **Participants**: Dev A, Dev B
- **Duration**: 30 mins
- **Status**: Ready for handover / Issues remain
- **Notes**: _____________________________________________________

---

## 📋 Quick Reference Checklist

### Must-Haves Before Ngày 7
- [ ] 8 scenes skeleton created
- [ ] Tutorial flow working
- [ ] Main menu navigation working
- [ ] Save/load tested
- [ ] No crash on full loop test
- [ ] PR #1 + #2 ready for review

### Must-Haves Before Ngày 10
- [ ] Top 5 bugs fixed
- [ ] Regression pass
- [ ] Event sequence correct
- [ ] No new crash introduced
- [ ] PR #3 merged

### Must-Haves Before Ngày 14
- [ ] No C0 blocker remains
- [ ] Content JSON ready
- [ ] Final build stable
- [ ] Tag created
- [ ] Handover doc written

---

## 🎯 Success Checklist (Final Sign-Off)

- [ ] Tutorial scene: new player -> tutorial -> save; returning -> skip
- [ ] Main menu: all navigation buttons working
- [ ] Team formation: select, validate, save
- [ ] Gacha UI: roll, show result, update
- [ ] Inventory UI: list, use item, save
- [ ] Character Collection UI: search/filter/sort/detail/feed level hoạt động đúng
- [ ] Level select: show chapters, levels, unlock state, enter combat
- [ ] Skill icon: load + fallback stable
- [ ] Full loop: boot -> tutorial? -> menu -> sub-scenes -> combat -> result -> menu
- [ ] Save/Load: state consistent across restart
- [ ] No crash, no compile error, no missing asset
- [ ] PR review by Dev A: PASS
- [ ] Regression test: PASS
- [ ] Final build: STABLE
- [ ] Tag created: ✓

**Xác nhận hoàn thành**: _____________ (signature)  
**Ngày**: _____________

---

**Note**: Update checklist này hàng ngày hoặc sau mỗi milestone. Dùng ⬜ (not started), 🟡 (in progress), 🟢 (done), 🔴 (blocked).
