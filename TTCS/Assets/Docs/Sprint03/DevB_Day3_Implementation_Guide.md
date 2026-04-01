# 🎮 Sprint 03 - Ngày 3 Implementation Guide (Dev B)

> **Mục tiêu Ngày 3**: Implement tutorial trigger, scene routing, UI state management  
> **Trạng thái**: Ngày 2 UI setup hoàn tất, Ngày 3 implement logic  
> **Sync point**: Cuối Ngày 3 với Dev A

---

## 📋 Tóm tắt Ngày 3

### Tasks Chính
1. **Tutorial Trigger Logic** — Detect first-time player và auto-route
2. **Scene Routing** — Implement Boot → Tutorial/MainMenu → LevelSelect → Combat
3. **UI State Management** — Locked/Unlocked/Cleared states cho level buttons

---

## 1️⃣ TASK 1 — Tutorial Trigger Logic

### Mục tiêu
- Boot scene khởi động FlowController
- FlowController kiểm tra `PlayerPrefs["TutorialCompleted"]`
- Nếu chưa xong: route đến Tutorial
- Nếu xong: route đến MainMenu

### Code (đã có trong FlowController.cs)
```csharp
public bool TryEnterTutorial()
{
    bool tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

    if (!tutorialCompleted)
    {
        Debug.Log("[Flow] Entering tutorial scene (first-time player)");
        UnityEngine.SceneManagement.SceneManager.LoadScene(_tutorialSceneName);
        return true;
    }
    else
    {
        Debug.Log("[Flow] Tutorial already completed, skipping");
        return false;
    }
}
```

### Setup trong Boot Scene
1. Mở `Boot.unity` scene
2. **Thêm code gọi TryEnterTutorial vào một script**:
   - Tạo script `BootSceneInitializer.cs` hoặc thêm vào FlowController

**Option A: Trong FlowController Awake**
```csharp
private void Awake()
{
    // ... existing code ...
    
    // After initialization, check if should enter tutorial
    Invoke(nameof(CheckFirstTime), 0.5f);
}

private void CheckFirstTime()
{
    if (!TryEnterTutorial())
    {
        // Tutorial skipped, go to main menu
        OpenMainMenu();
    }
}
```

**Option B: Tạo BootSceneInitializer.cs riêng**
```csharp
public class BootSceneInitializer : MonoBehaviour
{
    private void Start()
    {
        if (!FlowController.Instance.TryEnterTutorial())
        {
            FlowController.Instance.OpenMainMenu();
        }
    }
}
```

### Test Ngày 3 Task 1
1. Delete `PlayerPrefs` trước test:
   ```csharp
   // Tạm thời thêm vào Start của BootSceneInitializer để test
   PlayerPrefs.DeleteKey("TutorialCompleted");
   ```
2. Play Boot scene
3. Dự kiến: Auto-load TutorialScene
4. Nhấn Skip → auto-load MainMenuScene
5. Xóa debug code trước commit

---

## 2️⃣ TASK 2 — Scene Routing Implementation

### Mục tiêu
- TutorialFlowLogic → MainMenuScene
- MainMenuController → LevelSelectScene
- LevelSelectController → CombatScene (stub from DevA)
- CombatResult → Back to LevelSelect

### Code đã có
- ✅ `TutorialFlowLogic.CompleteTutorial()` → `GoToMainMenu()`
- ✅ `MainMenuController.OnPlayClicked()` → `FlowController.OpenLevelSelect()`
- ✅ `LevelSelectController.OnLevelClicked()` → `FlowController.EnterCombat()`

### FIX: MainMenuController WireButtons

**Vấn đề**: Có typo "wir eButtons" trong MainMenuController

**Cách sửa**: 
1. Mở `Assets/Scripts/Flow/MainMenu/MainMenuController.cs`
2. Tìm dòng: `wir eButtons();`
3. Sửa thành: `WireButtons();`

### Test Ngày 3 Task 2
1. Play Boot scene
2. Tutorial → MainMenu → LevelSelect sequence
3. Kiểm tra Console log:
   ```
   [Flow] Entering tutorial scene...
   [Tutorial] Tutorial scene started
   [Tutorial] Tutorial completed
   [Tutorial] Transitioning to main menu
   [MainMenu] Main menu loaded
   [MainMenu] Play button clicked - opening level select
   [LevelSelect] Level select scene loaded for chapter: chapter_01
   ```

---

## 3️⃣ TASK 3 — UI State Management

### Mục tiêu
- Level buttons hiển thị locked/unlocked/cleared state
- Onclick callback wire vào level clicked logic
- Back button quay về MainMenu

### Code (đã có trong LevelSelectController.cs)

`UIStateManager` class xử lý:
```csharp
public void UpdateLevelButtonState(GameObject buttonGo, LevelState levelState)
{
    var image = buttonGo.GetComponent<Image>();
    if (!levelState.Unlocked)
    {
        image.color = new Color(0.5f, 0.5f, 0.5f); // Gray = locked
        buttonGo.GetComponentInChildren<Text>().text += " [LOCKED]";
    }
    else if (levelState.Cleared)
    {
        image.color = new Color(0.7f, 1f, 0.7f); // Green = cleared
        buttonGo.GetComponentInChildren<Text>().text += $" [{levelState.BestStars}★]";
    }
    else
    {
        image.color = Color.white; // Normal = available
    }
}
```

### Setup trong LevelSelect Scene (Ngày 2 đã xong)
- ✅ LevelSelectController tự create buttons
- ✅ UIStateManager tự update UI state
- ✅ OnClick listener tự wire từ code

### Verify Component Chain
1. Mở `LevelSelectScene.unity`
2. Chọn LevelSelectManager (GameObject với LevelSelectController)
3. Inspector:
   - ✅ Level Button Container: drag Panel hoặc GridLayout vào
   - ✅ Level Button Prefab: drag LevelButton.prefab vào
   - ✅ Back Button: drag button vào
4. **Không cần wire On Click từ Inspector** — code tự gọi khi Button click

### Test Ngày 3 Task 3
1. Từ MainMenu, click Play
2. Vào LevelSelect scene
3. Kiểm tra level buttons:
   - 5 buttons hiển thị (Level 1-5)
   - Tất cả là green (unlocked + cleared) hoặc white (available)
   - Click level 1 → log `[LevelSelect] Level clicked: chapter_01_level_01`
4. Click non-existent level → không gì xảy ra (guard clause)
5. Click Back → log `[LevelSelect] Back button clicked` → MainMenu

---

## 📊 Full Flow Integration Test (Cuối Ngày 3)

### Scenario 1: First-Time Player
1. **Clear PlayerPrefs**: Delete `TutorialCompleted`
2. **Play Boot**:
   ```
   Boot → TutorialScene (auto) → [wait 15s or skip] → MainMenuScene
   ```
3. **Console log**:
   ```
   [Flow] Entering tutorial scene (first-time player)
   [Tutorial] Tutorial scene started
   [Tutorial] Tutorial completed / skipped
   [Tutorial] Transitioning to main menu
   [MainMenu] Main menu loaded
   ```

### Scenario 2: Returning Player
1. **Set PlayerPrefs**: `PlayerPrefs.SetInt("TutorialCompleted", 1)`
2. **Play Boot**:
   ```
   Boot → MainMenuScene (auto, skip Tutorial)
   ```
3. **Console log**:
   ```
   [Flow] Entering tutorial scene (first-time player)
   [Flow] Tutorial already completed, skipping
   (MainMenu loaded)
   ```

### Scenario 3: Level Select & Back
1. **From MainMenu**:
   - Click Play → LevelSelect
   - Click Level 1 → (stub, no combat yet)
   - Click Back → MainMenu
2. **Console log**:
   ```
   [MainMenu] Play button clicked - opening level select
   [LevelSelect] Level select scene loaded for chapter: chapter_01
   [LevelSelect] Level clicked: chapter_01_level_01
   [LevelSelect] Back button clicked - returning to main menu
   [MainMenu] Main menu loaded
   ```

---

## 🔧 Checklist Hoàn thành Ngày 3

- [ ] Tutorial Trigger Logic test ✓
  - [ ] First-time: Boot → Tutorial
  - [ ] Returning: Boot → MainMenu
- [ ] Scene Routing test ✓
  - [ ] Tutorial → MainMenu
  - [ ] MainMenu → LevelSelect
  - [ ] LevelSelect → Back to MainMenu
- [ ] UI State Management test ✓
  - [ ] Level buttons populate
  - [ ] Locked/Unlocked/Cleared states show
  - [ ] Click behavior correct
- [ ] Fix MainMenuController WireButtons typo ✓
- [ ] Full flow integration test (both scenarios) ✓
- [ ] Console logs clean (no errors) ✓
- [ ] Ready for sync with Dev A ✓

---

## 📝 Chuẩn bị Merge với DevA

### Trước khi merge (Ngày 3 cuối):
1. **Xóa debug code**:
   - `PlayerPrefs.DeleteKey("TutorialCompleted")` — nếu có
   - Bất kỳ hardcode temporary fix nào

2. **Xác nhận Mock Services**:
   - FlowController dùng MockProgressionService, MockTeamService, etc.
   - Khi DevA merge, thay thế = inject real services
   - **Không cần sửa code bây giờ**

3. **Checklist PR**:
   - [ ] No compile errors
   - [ ] Test checklist đi kèm (xem ở dưới)
   - [ ] Không phá flow cũ (Combat tương cũ vẫn work)
   - [ ] Console logs rõ ràng và useful

### Test Checklist cho PR
```
## Test Checklist - Dev B Ngày 2-3

### Tutorial Flow
- [x] First-time player: Boot → Tutorial (auto)
- [x] Skip button: Tutorial → MainMenu
- [x] Auto-complete after 15s: Tutorial → MainMenu

### MainMenu Navigation
- [x] Play button: MainMenu → LevelSelect
- [x] All 5 buttons (Team, Gacha, Inventory, Settings) có click handler
- [x] Scene không lag, UI response nhanh

### LevelSelect
- [x] 5 level buttons hiển thị (mocked data)
- [x] Level 1-5 có locked/unlocked/cleared state
- [x] Click available level: log "Level clicked"
- [x] Click locked level: không gì xảy ra (guarded)
- [x] Back button: LevelSelect → MainMenu

### Full Integration
- [x] Boot → Tutorial → MainMenu → LevelSelect → MainMenu (full loop)
- [x] No scene stuck bug
- [x] No null reference error
- [x] Console log sequential và dễ hiểu

### Technical
- [x] No compile error
- [x] FlowController singleton work (one instance across scenes)
- [x] PlayerPrefs tutorial state persist correctly
- [x] Mock services inject properly
```

---

## 🎯 Sync Point Cuối Ngày 3

**Hỏi Dev A**:
1. Real ProgressionService, TeamService, GachaService ready?
2. DataManager hoặc API để load chapter/level data?
3. Combat scene đã setup để receive levelId + lineup?

**Bạn sẵn sàng**:
- ✅ Flow skeleton ready
- ✅ UI stub ready
- ✅ Mock implementations ready
- ✅ Integration test pass

---

**Bước tiếp theo: Ngày 4 onwards — Load dữ liệu thật, Team Setup UI, Gacha/Inventory UI**

---

Git branch recommendation:
```bash
git checkout -b feature/devB-tutorial-scene
git pull origin main
# ... make changes ...
git add Assets/Scripts/Flow/ Assets/Scenes/Flow/ Assets/Docs/Sprint03/
git commit -m "feat: tutorial flow logic, scene routing, level select UI (Day 2-3)"
git push origin feature/devB-tutorial-scene
```

---

**Ready to go!** ✅
