# 🎮 Sprint 03 - Ngày 2-3 Unity Editor Setup Guide (Dev B)

> **Mục tiêu**: Setup scenes, UI, prefabs, và references để code dev B chạy được ngay.  
> **Người phụ trách code**: Dev B (Flow/UI Owner)  
> **Deadline**: Cuối Ngày 2 + Ngày 3

---

## 📋 Tổng quan

Code skeleton đã tạo cho:
- `FlowController.cs` (Ngày 2) — máy chủ điều hướng scene
- `TutorialFlowLogic.cs` (Ngày 2) — logic tutorial
- `MainMenuController.cs` (Ngày 2) — main menu UI
- `LevelSelectController.cs` (Ngày 2-3) — level select UI

Bạn cần setup trong Unity:
1. **Boot scene** — chứa FlowController singleton
2. **TutorialScene** — chứa TutorialFlowLogic + canvas
3. **MainMenuScene** — chứa MainMenuController + canvas
4. **LevelSelectScene** — chứa LevelSelectController + canvas

---

## 🔧 BƯỚC 1 — Setup Boot Scene (cho FlowController)

### 1.1 Tạo Boot Scene
1. Create → Scene → đặt tên `Boot`
2. Save tại: `Assets/Scenes/Flow/Boot.unity`

### 1.2 Tạo GameObject cho FlowController
1. Trong Boot scene Hierarchy, tạo empty GameObject
2. Đặt tên: `FlowManager`
3. Add Component → **FlowController** (từ `Assets/Scripts/Flow/Common/FlowController.cs`)

### 1.3 Cấu hình FlowController Inspector
| Field | Giá trị |
|-------|--------|
| Boot Scene Name | (để trống - Boot là nơi FlowController spawn) |
| Tutorial Scene Name | `TutorialScene` |
| Main Menu Scene Name | `MainMenuScene` |
| Level Select Scene Name | `LevelSelectScene` |
| Combat Scene Name | `CombatScene` |

### 1.4 Xác nhận DontDestroyOnLoad
- FlowController đã set `DontDestroyOnLoad` trong code ✓

> **Ghi chú**: Boot scene sẽ load lần đầu, FlowController sẽ kiểm tra tutorial state và routing đến Tutorial hoặc MainMenu.

---

## 🎬 BƯỚC 2 — Setup TutorialScene

### 2.1 Tạo TutorialScene
1. Create → Scene → đặt tên `TutorialScene`
2. Save tại: `Assets/Scenes/Flow/TutorialScene.unity`
3. Thêm Canvas (Right-click Hierarchy → UI → Canvas)

### 2.2 Tạo GameObject cho TutorialFlowLogic
1. Tạo empty GameObject tên `TutorialManager`
2. Add Component → **TutorialFlowLogic** (từ `Assets/Scripts/Flow/Tutorial/TutorialFlowLogic.cs`)

### 2.3 Cấu hình TutorialFlowLogic Inspector
| Field | Giá trị |
|-------|--------|
| Tutorial Duration | `15` (15 giây) |

### 2.4 Tạo UI cơ bản trong Canvas
1. **Text**: Tạo Text element "TutorialText"
   - Nội dung: "Hướng dẫn cơ bản cho người chơi mới..."
   - Size: 400x300, center screen
   
2. **Skip Button**: Tạo Button "SkipButton"
   - Text: "Bỏ qua"
   - Add listener → `TutorialFlowLogic.SkipTutorial()`

3. **Progress Bar** (tùy chọn): Image element để hiển thị progress

### 2.5 Wire Skip Button
1. Chọn SkipButton → Inspector
2. Tìm Button component → On Click event
3. Drag TutorialManager (GameObject với TutorialFlowLogic) vào object field
4. Dropdown → TutorialFlowLogic → SkipTutorial()

---

## 🏠 BƯỚC 3 — Setup MainMenuScene

### 3.1 Tạo MainMenuScene
1. Create → Scene → đặt tên `MainMenuScene`
2. Save tại: `Assets/Scenes/Flow/MainMenuScene.unity`
3. Thêm Canvas

### 3.2 Tạo GameObject cho MainMenuController
1. Tạo empty GameObject tên `MainMenuManager`
2. Add Component → **MainMenuController** (từ `Assets/Scripts/Flow/MainMenu/MainMenuController.cs`)

### 3.3 Thiết kế UI trong Canvas
Tạo các Button sau (Right-click Canvas → UI → Button):

| Button | Nội dung |
|--------|---------|
| PlayButton | "Chơi game" |
| TeamButton | "Đội hình" |
| GachaButton | "Gacha" |
| InventoryButton | "Hành trang" |
| SettingsButton | "Cài đặt" |

**Layout gợi ý:**
```
┌─────────────────┐
│  MAIN MENU      │
├─────────────────┤
│   [Play Game]   │
├─────────────────┤
│ [Team][Gacha]   │
│[Inventory][Set] │
└─────────────────┘
```

### 3.4 Wire Buttons vào MainMenuController
1. Chọn MainMenuManager (có MainMenuController)
2. Inspector → Các field Button (Play Button, Team Button, etc.)
3. Drag các button từ Hierarchy vào từng field

### 3.5 Xác nhận Wiring trong Code
- MainMenuController tự gọi `WireButtons()` trong `Start()` ✓
- Nếu button không wire trong Inspector, sẽ tự load lại

---

## 📊 BƯỚC 4 — Setup LevelSelectScene

### 4.1 Tạo LevelSelectScene
1. Create → Scene → đặt tên `LevelSelectScene`
2. Save tại: `Assets/Scenes/Flow/LevelSelectScene.unity`
3. Thêm Canvas

### 4.2 Tạo GameObject cho LevelSelectController
1. Tạo empty GameObject tên `LevelSelectManager`
2. Add Component → **LevelSelectController**
3. Add Component → **UIStateManager** (attached automatically)

### 4.3 Tạo Level Button Prefab
1. Tạo Button trong scene
2. **Cấu hình:**
   - Đặt tên: `LevelButtonTemplate`
   - Size: 80x80
   - Text: "1" (hoặc "Level 1")
   - Font size: 24
3. Drag vào folder: `Assets/Prefabs/UI/LevelButton.prefab`
4. Xóa instance từ scene (chỉ giữ prefab)

### 4.4 Cấu hình LevelSelectController Inspector
| Field | Gán vào |
|-------|---------|
| Level Button Container | Transform của panel chứa levels (tạo mới nếu cần) |
| Level Button Prefab | LevelButton.prefab (vừa tạo) |
| Back Button | Drag button "Back" từ scene |

### 4.5 Tạo UI Layout
1. **TopBar Panel**:
   - Text: "Chapter 1 - Levels"
   - BackButton: "← Back"

2. **LevelButtonContainer** (Grid/HorizontalLayoutGroup):
   - Prefab buttons sẽ instantiate vào container này

### 4.6 Test: Ngày 3 sẽ populate levels từ code

---

## 🔗 BƯỚC 5 — Liên kết Scenes trong Build Settings

### 5.1 Mở Build Settings
1. File → Build Settings (hoặc Ctrl+Shift+B)
2. **Scenes In Build** section:
   - Drag vào theo thứ tự:
     - 0: `Boot.unity`
     - 1: `TutorialScene.unity`
     - 2: `MainMenuScene.unity`
     - 3: `LevelSelectScene.unity`
     - 4: (CombatScene — Dev A sẽ thêm)

**Gợi ý**: Scene index phải match tên trong `FlowController.cs` hoặc dùng `SceneManager.LoadScene(sceneName)` theo tên (bạn đã dùng cách này ✓)

---

## ✅ BƯỚC 6 — Test Cơ bản (Ngày 2 Cuối)

### 6.1 Play Boot Scene
1. Mở `Boot.unity` scene
2. Press Play ▶
3. **Dự kiến:**
   - Console log: `[Flow] FlowController initialized with services`
   - Nếu là first-time:
     - Tự load **TutorialScene**
     - Hiển thị tutorial UI
   - Nếu tutorial đã done (check PlayerPrefs `TutorialCompleted=1`):
     - Tự load **MainMenuScene**

### 6.2 Test Tutorial Flow
1. Chạy tới TutorialScene
2. **Hoặc**: nhấn Skip button → phải transition đến MainMenu
3. **Hoặc**: chờ 15s → tự transition đến MainMenu
4. Check Console log tuần tự:
   ```
   [Tutorial] Tutorial scene started
   [Tutorial] Tutorial completed
   [Tutorial] Transitioning to main menu
   (MainMenuScene loaded)
   ```

### 6.3 Test MainMenu Flow
1. Ở MainMenuScene, nhấn "Play Game"
2. Dự kiến: Load LevelSelectScene
3. Check Console: `[MainMenu] Play button clicked - opening level select`

### 6.4 Test LevelSelect Flow
1. Ở LevelSelectScene, phải thấy 5 level buttons (hoặc mock)
2. Nhấn level → log `[LevelSelect] Level clicked:`
3. Nhấn Back → quay về MainMenu

---

## 🐛 Vấn đề thường gặp

| Triệu chứng | Nguyên nhân | Giải pháp |
|-------------|-------------|----------|
| "Scene 'Boot' not found" | Scene chưa add vào Build Settings | Thêm Boot vào Build Settings (Bước 5) |
| MainMenuController buttons không respond | Button không wire vào Inspector hoặc delegate null | Drag button vào Inspector field |
| TutorialFlowLogic.SkipTutorial() không gọi | OnClick event không wire | Vào Bước 2.5 wire lại skip button |
| Level buttons hiển thị nhưng locked/unlocked không đúng | UIStateManager logic có vấn đề | Check code logic `UpdateLevelButtonState` |
| "MockProgressionService" log | DevA chưa merge, bạn dùng Mock | Bình thường - sẽ replace khi DevA merge |

---

## 📝 Checklist Hoàn thành Ngày 2

- [ ] Boot scene tạo xong + FlowController setup
- [ ] TutorialScene tạo + TutorialFlowLogic + Skip button wire
- [ ] MainMenuScene tạo + MainMenuController + 5 buttons wire
- [ ] LevelSelectScene tạo + LevelSelectController + prefab button
- [ ] Tất cả 4 scenes add vào Build Settings theo thứ tự
- [ ] Test Boot → Tutorial → MainMenu → LevelSelect flow
- [ ] Console log toàn bộ không có lỗi

---

## 📌 Ghi chú Ngày 3

**Ngày 3 sẽ implement thêm:**
- Load level data từ JSON (hoặc API từ DataManager)
- Bind level state từ ProgressionService
- Team setup before combat (optional Day 3)
- Skill icon mapping (optional Day 3)

File hướng dẫn Ngày 3 sẽ được tạo riêng.

---

**Hỏi nếu có vấn đề!** ✅
