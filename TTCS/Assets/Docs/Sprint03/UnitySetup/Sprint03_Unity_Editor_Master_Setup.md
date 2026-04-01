# Sprint03 Unity Editor Master Setup

## Mục tiêu
Chạy setup một lần theo checklist này để:
- Nạp đầy đủ manager/service cần cho Sprint03
- Wire đúng flow combat reward bridge + save/progression
- Có thể test full loop và bàn giao cho team

## A. Chuẩn bị project
1. Mở project `TTCS` bằng Unity Hub.
2. Chờ compile/import xong hoàn toàn.
3. Mở Console và clear log cũ.

## B. Setup GameObjects bắt buộc trong scene
Đảm bảo tồn tại các object:
- `EventBus`
- `DataManager`
- `SaveManager`
- `MetaServiceHub`
- `CombatSceneManager`
- `CombatFlowController`
- `CombatBridge`
- `CombatRewardBridge`
- `AudioController`

Nếu thiếu:
1. Create Empty GameObject.
2. Đặt đúng tên.
3. Add đúng script component.

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

## D. Khởi tạo save + service
1. Enter Play mode.
2. Gọi `SaveManager.NewGame()`.
3. Xác nhận:
- `schemaVersion = 1`
- `tutorialCompleted = false`
- `unlockedChapters` có `chapter_01`
- `lineup` có nhân vật mặc định
4. Trên `MetaServiceHub`, xác nhận service đã init:
- Inventory
- Gacha
- Progression
- Team

## E. Smoke test nhanh
1. Gắn `MetaServicesSmokeTest` vào object test.
2. Chạy context menu `Run Meta Services Smoke Test`.
3. Kỳ vọng PASS log trong Console.

## F. Combat -> Reward -> Save flow
1. Dùng `CombatSceneManager` khởi chạy combat.
2. Kết thúc combat với victory.
3. Kỳ vọng:
- `CombatRewardBridge` log chạy.
- Item reward được cộng.
- Progress level được cập nhật.
- Gacha progression feed được áp dụng.
4. Gọi `Save(0)` rồi `Load(0)` để verify persistence.

## G. Build và kiểm thử
1. Làm theo `01_Build_Instructions.md`.
2. Chạy lần lượt:
- `02_Unit_Test_Instructions.md`
- `03_Integration_Test_Instructions.md`
- `05_E2E_Test_Instructions.md`
3. Benchmark theo `04_Performance_Test_Instructions.md`.

## H. Tiêu chí hoàn tất
- Không có compile error đỏ trong Unity Console.
- Full loop chạy ổn: menu -> level -> combat -> reward -> save/load.
- Không crash/null-reference ở reward bridge/meta services.
- Checklist test đã pass theo summary.
