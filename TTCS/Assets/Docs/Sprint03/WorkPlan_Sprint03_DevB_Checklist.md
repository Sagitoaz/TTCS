# Sprint 3 - Daily Progress Checklist Dev B

> **Ngày cập nhật lần cuối**: 2026-04-01  
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
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Setup 8 scene skeleton trong Assets/Scenes/
- [ ] Tạo FlowStateManager class
- [ ] Tạo NavigationController class
- [ ] Test Boot scene: new player -> Tutorial, returning -> MainMenu
- [ ] Push branch
- [ ] Sync cuối ngày với Dev A (IFlowController contract, event publish points)

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

## 🟢 Giai đoạn 2: Xây Core Song Song (Ngày 3-7)

### Ngày 3 - Tutorial Scene & Save State Hook
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Tạo Tutorial scene layout + controller
- [ ] Hook save state: TryEnterTutorial() từ Boot
- [ ] Subscribe TutorialCompletedEvent
- [ ] Test: new player -> tutorial -> complete -> save
- [ ] Test: restart -> skip tutorial
- [ ] **SYNC BẮT BUỘC**: Verify IProgressionService, SaveManager API, event subscription

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 4 - Main Menu & Team Setup UI
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Tạo Main Menu scene layout + controller
- [ ] Tạo Team Formation scene layout + controller
- [ ] Test: Main Menu -> Team -> Back to Main Menu
- [ ] Team panel co 3 slot, moi slot co button mo picker
- [ ] Picker panel hien list nhan vat so huu, co confirm Yes/No truoc khi add vao slot
- [ ] Picker filter: level asc/desc, rarity asc/desc, role
- [ ] Rule loai tru picker: character het HP, character da ra tran, character da duoc chon o slot khac
- [ ] Slot da chon phai hien portrait + HP slider + HP text + level
- [ ] Sync: Input structure for Team, validation error types

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 5 - Skill Icon Pipeline & Gacha UI v1
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Tạo skill_icon_map.json sample data
- [ ] Tạo SkillIconLoader class (load + fallback)
- [ ] Tạo fallback texture
- [ ] Tạo Gacha scene layout + controller
- [ ] Test: Gacha roll -> show result with icon (mock GachaService)

**Notes**: _____________________________________________________

**Blocker** (nếu có): _____________________________________________________

**ETA Complete**: _____________________

---

### Ngày 6 - Inventory UI & Level Select Wiring
**Status**: ⬜ Not started / 🟡 In progress / 🟢 Done

**Task List**:
- [ ] Tạo Inventory scene layout + controller
- [ ] Tạo Level Select scene layout + controller (mock data: 3 chapters x 3 levels)
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
  - [ ] Main Menu -> Team -> Gacha -> Inventory -> Level Select -> Combat
- [ ] Lập danh sách lỗi (classify: C0, C1, C2, C3)
- [ ] Verify: No crash, no compile error, event sequence correct
- [ ] PR #1 ready: feature/devB-tutorial-scene
- [ ] PR #2 ready: feature/devB-menu-team-gacha-inventory
- [ ] Prepare bug list for Giai đoạn 3

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
| 1 | 1-2 | ⬜ | Scene skeleton |
| 2 | 3-7 | ⬜ | Tutorial, Menu, UI flows |
| 3 | 8-12 | ⬜ | Polish, finalize |
| 4 | 13-14 | ⬜ | Freeze, handover |

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
