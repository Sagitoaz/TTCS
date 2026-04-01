# Sprint 3 Dev B - Ngày 1 Sync Checklist

> **Ngày**: 2026-04-01  
> **Mục đích**: Xác nhân interface contract từ Dev A để không block code Ngày 2-3

---

## 📝 Documentation Shared

Dev B đã tạo:
- ✅ Flow diagram (ASCII) - `Sprint03_DevB_Day1_FlowDiagram.md`
- ✅ Scene list & specifications - `Sprint03_DevB_Day1_SceneList.md`
- ✅ UI elements checklist - `Sprint03_DevB_Day1_UIElements.md`
- ✅ This sync checklist

---

## 🤝 Questions for Dev A (End-of-Day Sync - Ngày 1)

### 1) FlowController Interface & Tutorial Entry

**Question**: 
```csharp
public interface IFlowController
{
    bool TryEnterTutorial();  // ???
    void OpenMainMenu();
    void OpenTutorial();
    // ...
}
```

- If `TryEnterTutorial()` returns `true`, should Dev B call `OpenTutorial()` separately?
- Or does `TryEnterTutorial()` internally call `OpenTutorial()` if needed?
- **Expected**: Clarify the contract - does it return bool, or return void (with side effect)?

**Dev A's Answer**: _______________________________________________________

---

### 2) Save State Field: tutorialCompleted

**Question**: 
- What's the exact field name and type in save state?
  - `SaveData.tutorialCompleted` (bool)?
  - Or `SaveData.playerProgress.tutorialCompleted`?
- When should it be read? (At Boot scene startup)
- When should it be written? (On Tutorial complete OR skip)

**Dev A's Answer**: _______________________________________________________

---

### 3) Event: TutorialCompletedEvent

**Question**:
- What's the event class name and namespace?
  - `TTCS.Core.TutorialCompletedEvent`?
  - What properties does it have? (timestamp, userId, etc.)
- Who publishes it? (TutorialController? Or SaveManager after write?)
- Who subscribes? (SaveManager to write save, or BootController to navigate?)

**Dev A's Answer**: _______________________________________________________

---

### 4) Save Manager API

**Question**:
- Is SaveManager a singleton or injected service?
- What's the API to read/write save state?
  ```csharp
  SaveManager.Instance.GetSaveData(slotId)
  SaveManager.Instance.WriteSaveData(slotId, data)
  ```
- Is it async (Task) or sync?

**Dev A's Answer**: _______________________________________________________

---

### 5) Scene Loading & Navigation

**Question**:
- Should Dev B implement NavigationController, or does Dev A have a base one?
- Scene loading: use `SceneManager.LoadScene()` or custom loader?
- Should scene transitions have fade animation managed by nav controller?
- How to pass data between scenes (e.g., levelId from LevelSelect -> Combat)?

**Dev A's Answer**: _______________________________________________________

---

### 6) IProgressionService, ITeamService, IGachaService, IInventoryService

**Question**:
- When can Dev A deliver interface (skeleton) for Dev B to UI-bind against?
- Expected delivery date (Ngày 2 or 3)?
- Can Dev A provide mock implementations for Dev B to test UI binding?

**Dev A's Answer**: _______________________________________________________

---

### 7) Data Folder Structure

**Question**:
- Where should Dev B create JSON data files (e.g., skill_icon_map.json)?
  - `Assets/Data/Meta/skill_icon_map.json`?
  - Or `Assets/Resources/Data/Meta/`?
- Who owns updating these files? (Dev A or Dev B or both?)

**Dev A's Answer**: _______________________________________________________

---

### 8) Asset Dependency

**Question**:
- What assets need to be ready for Dev B to proceed?
  - Character icons (for Team UI)?
  - Item icons (for Inventory UI)?
  - Skill icons (for skill icon pipeline)?
- Can placeholders be used until final assets arrive?

**Dev A's Answer**: _______________________________________________________

---

## ✅ Confirmations Needed from Dev A

### Must-Have for Ngày 2
- [ ] IFlowController interface contract finalized (method signatures + return types)
- [ ] Save state structure confirmed (tutorialCompleted field location)
- [ ] Event definitions (TutorialCompletedEvent, etc.)
- [ ] SaveManager API confirmed
- [ ] Scene loading approach confirmed

### Should-Have for Ngày 3-4
- [ ] Service interface skeletons (IProgressionService, ITeamService, etc.)
- [ ] Mock service implementations (for Dev B to test UI binding)
- [ ] Asset placeholder confirmation (Dev B can use placeholders or final?)

### Nice-to-Have
- [ ] Data folder structure confirmed
- [ ] Event subscription pattern confirmed

---

## 📋 Dev A's Ngày 1 Deliverables (For Comparison)

From `WorkPlan_Sprint03.md` Ngày 1 (Dev A tasks):
- [ ] Định nghĩa contract progression và save state mới
- [ ] Draft schema JSON cho chapter/level/unlock
- [ ] Định nghĩa contract player meta-state (lineup, roster, inventory snapshot)
- [ ] Sync cuối ngày: Chốt interface giữa UI flow và progression service

**Dev A's Draft Outputs** (to be shared):
- Expected doc: Contract definitions
- Expected doc: Save state schema draft
- Expected doc: Service interface definitions

---

## 🎯 Sync Meeting Agenda (Cuối Ngày 1)

**Duration**: 30-45 minutes

**Topics** (in order of priority):

1. **FlowController Interface** (5 min)
   - Confirm method contract
   - Clarify navigation responsibility (FlowController vs NavigationController)

2. **Save State Structure** (5 min)
   - tutorialCompleted field location
   - Read/write API

3. **Event Definitions** (5 min)
   - TutorialCompletedEvent
   - Other events (LevelSelected, CombatResult, etc.)

4. **Service Interfaces** (10 min)
   - IProgressionService methods
   - ITeamService methods
   - IGachaService methods
   - IInventoryService methods
   - Timeline for skeleton implementation

5. **Asset & Data** (5 min)
   - Asset availability (placeholder ok?)
   - JSON data folder confirmation

6. **Action Items** (5 min)
   - Document decisions in code/wiki
   - Confirm Ngày 2 start ready

---

## 📌 Sign-Off

**Dev B Meeting Status**:
- [ ] All documents created (flow diagram, scene list, UI checklist, sync checklist)
- [ ] Sync meeting scheduled with Dev A
- [ ] Awaiting Dev A's answers to proceed with Ngày 2

**Meeting Date/Time**: TBD

**Attendees**: Dev A, Dev B

**Notes**:
```
(To be filled during sync)
```

---

**Created**: 2026-04-01  
**Dev**: Dev B  
**Status**: Ready for end-of-day sync
