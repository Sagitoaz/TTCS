# Sprint 3 Dev A - Unity Editor Setup (Core Services)

Quick checklist theo scene thật `Assets/Scenes/TestCombatUI_devA.unity`.
Nếu cần full chi tiết thao tác click-by-click, mở `06_Unity_Editor_Setup_Detailed.md`.

## Quick Execution Order
1. Mở scene `TestCombatUI_devA`.
2. Chọn `CombatSceneManager`, verify:
- `_defaultStageId = stage_01_tutorial`
- `_defaultPartyIds = [char_warrior, char_mage]`
- `_playerSlots = [P1, P2]`
- `_enemySlots = [E1, E2]`
3. Chọn `CombatUIController`, gán đủ:
- `_resultPanel`, `_resultButton`
- `_resultButtonLabel` = `ReturnButton/Text (TMP)`
- `_resultCardController` = component `ResultCardController` trên `ResultCard`
4. Chọn `ResultPanel`:
- thêm `ResultCard` (sprite `Common_Window_0`)
- thêm `WinCard` + `LoseCard` bên trong `ResultCard`
- gán sprite win/lose tương ứng cho từng card
- add `ResultCardController` và gán `_winCard`, `_loseCard`
5. Chọn `ReturnButton`:
- sprite `Common_Button_20`
- size `320 x 92`
6. Enter Play:
- nhấn `V` để test thắng
- test thua để verify đổi sang ảnh defeat
7. Kỳ vọng:
- result screen không còn style default Unity
- kết quả thắng/thua bật/tắt giữa `WinCard` và `LoseCard`
- nút hoạt động reload scene
- không có compile/runtime error đỏ trong Console
