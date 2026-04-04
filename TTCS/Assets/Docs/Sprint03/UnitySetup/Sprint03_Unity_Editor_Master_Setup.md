# Sprint03 Unity Editor Master Setup

## Mục tiêu
Chạy setup một lần theo checklist này để:
- Hoàn tất đúng phần Dev A Sprint03 theo scene `TestCombatUI_devA`
- Wire đủ meta services + reward flow
- Làm đẹp màn `Victory/Defeat` bằng resource có sẵn trong project

## A. Mở đúng scene và baseline
1. Mở project `TTCS` bằng Unity Hub.
2. Mở scene `Assets/Scenes/TestCombatUI_devA.unity`.
3. Mở Console và clear log cũ.
4. Chờ compile/import xong hoàn toàn.

## B. Xác nhận object/hierarchy bắt buộc (theo scene thực tế)
Đảm bảo có các object:
- `CombatSceneManager`
- `CombatUIController`
- `CombatBridge`
- `CombatManagers`
- `TimingSystem`
- `PlayerSlots` (có `P1`, `P2`)
- `EnemySlots` (có `E1`, `E2`)
- `CombatCanvas` (có `ResultPanel`, `ReturnButton`)

## C. Kiểm tra dữ liệu Sprint03
Trong Project panel, xác nhận có:
- `Assets/Data/Chapters`
- `Assets/Data/Levels`
- `Assets/Data/Gacha`
- `Assets/Data/Items`
- `Assets/Data/Meta`

Và các file mẫu:
- `chapter_01.json`, `chapter_02.json`
- `chapter_01_level_01.json`, `chapter_01_level_02.json`, `chapter_02_level_01.json`
- `pool_standard.json`
- `item_potion.json`, `item_energy.json`
- `skill_icon_map.json`

## D. Setup CombatSceneManager (khớp scene này)
1. Chọn `CombatSceneManager`.
2. Verify:
- `_defaultStageId = stage_01_tutorial`
- `_defaultSeed = 42`
- `_autoStartOnPlay = true`
- `_defaultPartyIds = [char_warrior, char_mage]`
- `_playerSlots = [P1, P2]`
- `_enemySlots = [E1, E2]`

## E. Setup Result UI (Victory/Defeat) - bắt buộc làm
1. Mở `CombatCanvas > ResultPanel`.
2. Tạo `ResultCard` (Image), set sprite `Common_Window_0`.
3. Tạo 2 object con trong `ResultCard`: `WinCard` và `LoseCard`.
4. Set sprite:
- `WinCard` dùng `Result_BG02`/`Result_BG03`
- `LoseCard` dùng `Result_BG`
5. Chọn `ReturnButton`:
- Image sprite = `Common_Button_20` (hoặc 21/22)
- Size gợi ý = `320 x 92`
6. Chọn `ReturnButton/Text (TMP)`:
- Text mặc định `Continue`
- Font size `34`

## F. Wire field Result mới trên CombatUIController
1. Chọn object `CombatUIController`.
2. Gán đầy đủ:
- `_resultPanel` = `ResultPanel`
- `_resultButton` = `ReturnButton`
- `_resultButtonLabel` = `ReturnButton/Text (TMP)`
- `_resultCardController` = component `ResultCardController` trên `ResultCard`

## G. Play test nhanh (ngay trong scene này)
1. Enter Play.
2. Nhấn `V` để force victory.
3. Verify:
- Panel hiện fade in
- `WinCard` hiện, `LoseCard` ẩn
- `WinCard` animate xuất hiện + loop
- Nút `Continue`
4. Test defeat, verify:
- `LoseCard` hiện, `WinCard` ẩn
- `LoseCard` animate xuất hiện + loop
- Nút `Retry`

## H. Tiêu chí hoàn tất
- Không có compile error đỏ trong Unity Console.
- Result screen không còn UI mặc định Unity.
- Full loop Dev A chạy ổn: combat -> reward -> save/load.
- Không null-reference ở `ProgressionService`, `CombatRewardBridge`, `CombatUIController`.

## Tài liệu chi tiết thao tác
- File chính: `06_Unity_Editor_Setup_Detailed.md`
- Bản nhanh 1 trang: `08_Unity_Editor_Quick_Setup.md`
