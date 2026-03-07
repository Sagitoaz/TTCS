# 🧪 Tuần 2 — Setup & Test Guide (Dev A)

> **Sprint:** Sprint 02 — Visual Combat Layer  
> **Developer:** Developer A  
> **Tuần:** Tuần 2 (Ngày 6–10)  
> **Scene làm việc:** `TestCombat` (tiếp từ Tuần 1)  
> **Mục tiêu:** Xác minh CombatBridge, TimingSystem, TimingFeedbackUI, và AudioController hoạt động đúng

---

## Tổng quan Tuần 2

Tuần 1 xây dựng **UI tĩnh** (BattleHUD, SkillButtons, TurnOrder) và đã test thành công.  
Tuần 2 bổ sung **hệ thống động** — timing input của player và bridge giữa engine/visual:

| File | Hệ thống | Trạng thái |
|------|----------|------------|
| `CombatBridge.cs` | EventBus → Visual bridge + ICharacterAnimatorBridge interface | ✅ Built |
| `TimingWindow.cs` | Data class cho timing window | ✅ Built |
| `TimingSystem.cs` | Singleton — vòng đời window + input buffer | ✅ Built |
| `TimingInputHandler.cs` | Nhận Space/Guard input → TimingSystem | ✅ Built |
| `TimingFeedbackUI.cs` | Perfect/Good/Miss flash visual | ✅ Built |
| `AudioController.cs` | Event-driven SFX + BGM | ✅ Built (Tuần 1 Day 5) |

---

## Checklist Tuần 2

- [ ] Thêm `TimingSystem` + `TimingInputHandler` vào scene
- [ ] Thêm `CombatBridge` vào scene
- [ ] Tạo `TimingFeedbackUI` panel trong Canvas hierarchy
- [ ] Gán Inspector references cho `TimingFeedbackUI`
- [ ] Wire `TimingSystem.OnTimingResult` → `CombatUIController.ShowTimingResult`
- [ ] Test phím `T` → window mở → `Space` → nhận Perfect/Good/Miss
- [ ] Test phím `F` / `G` / `M` → flash visual đúng màu/text
- [ ] Verify Console log đúng grade từ TimingSystem
- [ ] Test phím `H` / `Y` vẫn hoạt động (không bị break từ Tuần 1)

---

## BƯỚC 1 — Scene từ Tuần 1

Mở scene `TestCombat` đã setup ở Tuần 1. Các thành phần sau **phải có sẵn**:

| GameObject | Component | Ghi chú |
|-----------|-----------|---------|
| `CombatSceneManager` | `CombatSceneManager`, `AudioController` | Từ Tuần 1 |
| `CombatCanvas` → `BattleHUD` | `BattleHUD` | Từ Tuần 1 |
| `CombatCanvas` → `SkillButtonPanel` | `SkillButtonPanel` | Từ Tuần 1 |
| `CombatCanvas` → `TurnOrderDisplay` | `TurnOrderDisplay` | Từ Tuần 1 |
| `CombatCanvas` → `ActionResultDisplay` | `ActionResultDisplay` | Từ Tuần 1 |

Nếu chưa setup Tuần 1, xem: `Docs/Sprint02/DevA_Day5_BattleHUD_Test.md`

---

## BƯỚC 2 — Thêm TimingSystem + TimingInputHandler

1. Trong Hierarchy, tìm hoặc tạo GameObject tên `TimingSystem`
2. **Add Component → `TimingSystem`**
3. **Add Component → `TimingInputHandler`**

Cấu hình `TimingInputHandler` Inspector:

| Field | Giá trị | Ghi chú |
|-------|---------|---------|
| `Action Name` | `Guard` | Dùng nếu có PlayerInput component |
| `Fallback Key` | `Space` | Dùng fallback (khuyến dùng khi testing) |

> **Không cần PlayerInput** trong testing — TimingInputHandler sẽ tự dùng Space fallback và log cảnh báo trong Console (là bình thường).

---

## BƯỚC 3 — Thêm CombatBridge

1. Tạo empty GameObject tên `CombatBridge`
2. **Add Component → `CombatBridge`**

> CombatBridge không có Inspector fields — nó tự subscribe EventBus trong `OnEnable()`.  
> Không cần gán gì thêm. Khi `CombatSceneManager` khởi tạo, các events sẽ tự flow qua Bridge.

---

## BƯỚC 4 — Tạo TimingFeedbackUI trong Canvas

### 4.1 Tạo Panel

Trong `CombatCanvas` Hierarchy:
1. Right-click `CombatCanvas` → **Create Empty** → đặt tên `TimingFeedbackUI`
2. **Add Component → `TimingFeedbackUI`**
3. Gán **RectTransform** để cover toàn màn hình:
   - Anchor: **Stretch Stretch** (góc trên-trái đến góc dưới-phải)
   - Left/Right/Top/Bottom = `0`

### 4.2 Tạo Flash Overlay

Trong `TimingFeedbackUI`:
1. Right-click → **UI → Image** → đặt tên `FlashOverlay`
2. `Image` component:
   - `Color` = `White` (alpha = 0 ban đầu — code sẽ set)
   - `Raycast Target` = **OFF** (quan trọng!)
3. RectTransform: Anchor stretch toàn màn hình (giống bước trên)
4. **Add Component → `Canvas Group`** lên `FlashOverlay`
   - `Alpha` = `0`
   - `Blocks Raycasts` = **OFF**

### 4.3 Tạo Grade Text

Trong `TimingFeedbackUI`:
1. Right-click → **UI → Text - TextMeshPro** → đặt tên `GradeText`
2. Cấu hình `TextMeshPro - Text (UI)`:
   - `Font Size` = `72`
   - `Alignment` = **Center / Middle**
   - `Color` = White (alpha = 0)
   - `Font Style` = **Bold**
3. RectTransform: Center, Width `400`, Height `100`, Pos Y = `80` (hơi cao giữa màn hình)

### 4.4 Gán references vào TimingFeedbackUI Inspector

| Field | Gán vào |
|-------|---------|
| `Flash Overlay` | Canvas Group trên `FlashOverlay` |
| `Grade Text` | TextMeshPro `GradeText` |
| `Perfect Color` | Mặc định: Gold `(1, 0.85, 0.1, 0.35)` |
| `Good Color` | Mặc định: Blue `(0.3, 0.7, 1, 0.25)` |
| `Miss Color` | Mặc định: Red `(0.8, 0.2, 0.2, 0.2)` |

### 4.5 Gán TimingFeedbackUI vào CombatUIController

1. Chọn GameObject có `CombatUIController`
2. Trong Inspector, tìm field `Timing Feedback UI`
3. Kéo `TimingFeedbackUI` GameObject vào field này

---

## BƯỚC 5 — Wire TimingSystem.OnTimingResult → UI

`CombatUIController.ShowTimingResult()` cần được gọi khi `TimingSystem` có kết quả.

**Cách wire trong code** (CombatUIController tự subscribe trong `Initialize()`):

> Kiểm tra file `CombatUIController.cs` — trong `Initialize()` phải có dòng:
> ```csharp
> TimingSystem.Instance.OnTimingResult += ShowTimingResult;
> ```

Nếu chưa có, `CombatUIController` sẽ không nhận kết quả từ `TimingSystem`. Kiểm tra:
1. Mở `CombatUIController.cs`
2. Tìm `Initialize()` method
3. Xác nhận `OnTimingResult` subscription

---

## BƯỚC 6 — Keyboard Test Reference

> **Lưu ý:** Tất cả hotkeys chỉ hoạt động khi `CombatSceneManager` đã `Initialize` xong và `_autoStartOnPlay = true`.

### Hotkeys Tuần 1 (vẫn hoạt động)

| Phím | Hành động | Kết quả mong đợi |
|------|-----------|-----------------|
| `H` | Damage 500 cho `char_warrior` | HP bar của Warrior giảm + số đỏ nổi |
| `Y` | Heal 300 cho `char_warrior` | HP bar của Warrior tăng + số xanh nổi |
| `V` | Kết thúc combat (victory) | Result panel hiện |

### Hotkeys Tuần 2 (mới thêm)

| Phím | Hành động | Kết quả mong đợi |
|------|-----------|-----------------|
| `T` | Mở timing window 2 giây | Console log: `"TimingSystem: Window opened..."` |
| `Space` | Register timing input | Console log: grade + `"TimingSystem: Input registered"` |
| `F` | Direct test: **Perfect** flash | Text `PERFECT!` màu vàng + gold overlay flash |
| `G` | Direct test: **Good** flash | Text `GOOD` màu xanh + blue overlay flash |
| `M` | Direct test: **Miss** flash | Text `MISS` màu đỏ + red overlay flash + screen shake |

### Workflow test timing đầy đủ:
1. Press `T` → **window mở** → Console: `"Window opened (duration=2.0s)"`
2. Nhấn `Space` ngay lập tức → **Perfect** hoặc **Good** tùy thời điểm
3. Đợi 2 giây mà không nhấn → **Miss** tự động
4. Kiểm tra `TimingFeedbackUI` hiển thị đúng

---

## BƯỚC 7 — Verify Console Logs

Khi Play mode hoạt động đúng, Console sẽ hiển thị:

```
[Combat] CombatSceneManager: Initializing stage 'stage_01_tutorial'...
[Combat] CombatSceneManager: 2 players, 2 enemies.
[Combat] CombatBridge: Subscribed to EventBus events.
[Combat] CombatSceneManager: Combat started.
[UI]    TimingInputHandler: Không có PlayerInput — dùng fallback key Space.   ← bình thường
```

Khi nhấn `T`:
```
[Combat] TimingSystem: Window opened (openTime=X.XXXs, duration=2.0s, ideal=X.XXXs).
```

Khi nhấn `Space` trong window:
```
[Combat] TimingSystem: Input registered at t=X.XXXs.
[Combat] TimingSystem: Grade=Perfect (offset=XXms).    ← hoặc Good/Miss
```

---

## Cấu trúc Hierarchy sau khi setup xong

```
TestCombat (Scene)
├── EventBus
├── DataManager
├── TurnManager
├── SkillManager
├── CombatFlowController
├── CombatSceneManager          ← CombatSceneManager + AudioController + 2x AudioSource
├── CombatBridge                ← CombatBridge (NEW Tuần 2)
├── TimingSystem                ← TimingSystem + TimingInputHandler (NEW Tuần 2)
└── CombatCanvas (Canvas)
    ├── BattleHUD               ← BattleHUD
    ├── SkillButtonPanel        ← SkillButtonPanel
    ├── TurnOrderDisplay        ← TurnOrderDisplay
    ├── ActionResultDisplay     ← ActionResultDisplay
    └── TimingFeedbackUI        ← TimingFeedbackUI (NEW Tuần 2)
        ├── FlashOverlay        ← Image + CanvasGroup
        └── GradeText           ← TextMeshPro
```

---

## Vấn đề thường gặp

| Triệu chứng | Nguyên nhân | Giải pháp |
|------------|-------------|-----------|
| `T` không mở window, không có log | `TimingSystem` chưa add vào scene | Thêm component `TimingSystem` |
| `Space` không register, log "No window active" | Nhấn Space khi không có window | Nhấn `T` trước, rồi `Space` |
| Flash visual không hiện | `TimingFeedbackUI` chưa gán reference | Kiểm tra `Flash Overlay` và `Grade Text` trong Inspector |
| `ShowTimingResult` không được gọi | `CombatUIController` chưa subscribe `OnTimingResult` | Xem Bước 5 |
| DOTween animation không chạy | DOTween chưa init | Thêm `DOTween.Init()` trong Awake hoặc kiểm tra DOTween package |
