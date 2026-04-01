# Unity Editor Setup Guide - Sprint03 Dev A (Detailed, Scene-Based)

## 0. Phạm vi tài liệu này
- Tài liệu này bám trực tiếp scene `Assets/Scenes/TestCombatUI_devA.unity`.
- Mục tiêu là bạn chỉ cần làm đúng từng bước trong Unity Editor để hoàn thiện setup Dev A, đặc biệt phần `Victory/Defeat`.

## 1. Snapshot scene hiện tại (đã kiểm tra)
- Scene có các object chính: `CombatSceneManager`, `CombatUIController`, `CombatBridge`, `CombatManagers`, `TimingSystem`, `PlayerSlots`, `EnemySlots`, `CombatCanvas`, `BackgroundCanvas`, `EventSystem`.
- `CombatSceneManager` hiện đang set:
- `_defaultStageId = stage_01_tutorial`
- `_defaultSeed = 42`
- `_autoStartOnPlay = true`
- `_defaultPartyIds = [char_warrior, char_mage]`
- `_playerSlots = [P1, P2]`
- `_enemySlots = [E1, E2]`
- `CombatUIController` đang reference đúng các field core:
- `_battleHUD`, `_skillButtonPanel`, `_turnOrderDisplay`, `_actionResultDisplay`, `_timingFeedbackUI`
- `_resultPanel = ResultPanel`
- `_resultButton = ReturnButton`

## 2. Resource/prefab có sẵn để dùng ngay cho UI
- UI sprites:
- `Assets/Sprites/UI/Mobile - Fire Emblem_ Heroes - UI - Other/Other/Common_Window.png` (multi-sprite `Common_Window_0..12`)
- `Assets/Sprites/UI/Mobile - Fire Emblem_ Heroes - UI - Other/Other/Common_Button.png` (multi-sprite `Common_Button_0..23`)
- Result background:
- `Assets/Sprites/Miscellaneous/Mobile - Fire Emblem_ Heroes - Miscellaneous - Aether Raids (1)/Aether Raids/Result_BG.png` (`Result_BG_0`)
- `Result_BG02.png`, `Result_BG03.png`
- Prefab UI:
- `Assets/Prefabs/UI/SkillButton.prefab`
- `Assets/Prefabs/UI/TurnOrderSlot.prefab`
- `Assets/Prefabs/UI/FloatingText.prefab`
- Prefab combat:
- `Assets/Resources/Prefabs/Combat/Characters/char_warrior.prefab`
- `Assets/Resources/Prefabs/Combat/Characters/char_mage.prefab`
- `Assets/Resources/Prefabs/Combat/Enemies/enemy_bandit.prefab`

## 3. Làm lại ResultPanel theo kiểu “không còn default Unity”

### 3.1 Mở đúng object và backup nhanh
1. Mở scene `TestCombatUI_devA`.
2. Trong Hierarchy: `CombatCanvas > ResultPanel`.
3. Right click `ResultPanel` > `Duplicate` và rename bản copy thành `ResultPanel_BACKUP`.
4. Tắt `ResultPanel_BACKUP` để dự phòng rollback.

### 3.2 Setup nền mờ toàn màn hình
1. Chọn `ResultPanel`.
2. Ở `RectTransform`, bấm preset stretch full (Alt + Shift + chọn stretch cả 4 cạnh).
3. `Anchored Position = (0,0)`, `Left/Right/Top/Bottom = 0`.
4. Component `Image`:
- `Source Image`: giữ hoặc chọn sprite nền tối bất kỳ.
- `Color`: đặt khoảng `#000000B0` (đen alpha ~ 0.69) để dim nền combat.

### 3.3 Tạo card kết quả ở giữa
1. Right click `ResultPanel` > `UI > Image`, rename `ResultCard`.
2. `RectTransform`:
- Anchor: Center
- Width = `900`, Height = `520` (cho game view 16:9; mobile có thể giảm còn `760 x 460`)
- Pos = `(0, 0)`
3. `Image` của `ResultCard`:
- `Source Image` = `Common_Window_0`
- `Color` = `#FFFFFFFF`
- `Preserve Aspect` = On

### 3.4 Dùng 2 card riêng: WinCard/LoseCard
1. Trong `ResultCard`, tạo 2 object con:
- `WinCard` (`UI > Image`)
- `LoseCard` (`UI > Image`)
2. Với cả `WinCard` và `LoseCard`:
- Anchor center, size gợi ý `780 x 360`, pos `(0, 10)`.
3. Gán sprite:
- `WinCard` dùng `Result_BG02` hoặc `Result_BG03`.
- `LoseCard` dùng `Result_BG`.
4. Mặc định editor:
- `WinCard` active = false
- `LoseCard` active = false
5. Có thể xóa hẳn object `ResultText` cũ khỏi `ResultPanel`.

### 3.5 Làm lại ReturnButton
1. Chọn `ReturnButton`.
2. Đặt lại `RectTransform`:
- Anchor center-bottom của `ResultCard`
- Pos = `(0, -180)`
- Size = `(320, 92)`
3. Component `Image` của `ReturnButton`:
- `Source Image` = `Common_Button_20` (hoặc `Common_Button_21/22` nếu muốn kiểu khác)
- `Color` = trắng
4. Trong `ReturnButton/Text (TMP)`:
- Text mặc định = `Continue`
- Font Size = `34`
- Alignment = `Center`
- Color = `#2F3136FF`

## 4. Wire field Result theo mô hình WinCard/LoseCard

> Mình đã tách script riêng `ResultCardController` để chỉ điều khiển phần card result.

1. Chọn object `CombatUIController` trong scene.
2. Ở component `CombatUIController`, phần `Result Screen`:
- `_resultPanel` = kéo `ResultPanel`
- `_resultButton` = kéo `ReturnButton`
- `_resultButtonLabel` = kéo `ReturnButton/Text (TMP)`
- `_resultCardController` = kéo component `ResultCardController` (gắn trên `ResultCard`)
- `_victoryButtonLabel` = `Continue`
- `_defeatButtonLabel` = `Retry`
3. Không cần tự add `OnClick` cho `ReturnButton`, vì script tự bind ở runtime.

## 5. Setup ResultCardController (code riêng cho phần card)
1. Chọn `ResultCard`, Add Component: `ResultCardController`.
2. Gán field:
- `_winCard` = `WinCard`
- `_loseCard` = `LoseCard`
3. Tinh chỉnh animation:
- `_enterScaleFrom` ~ `0.78`
- `_enterDuration` ~ `0.28`
- `_enableLoopPulse` = On (nếu muốn card pulse loop)
- `_loopScale` ~ `1.06`
- `_loopHalfDuration` ~ `0.6`
4. Runtime:
- Win: bật `WinCard`, ẩn `LoseCard`.
- Lose: bật `LoseCard`, ẩn `WinCard`.
- Card active có animation xuất hiện rồi loop.

## 6. Kiểm tra chain Dev A sau khi chỉnh UI
1. Enter Play (scene này đang `_autoStartOnPlay = true`).
2. Nhấn `V` để force `Victory`.
3. Kiểm tra:
- `ResultPanel` hiện ra fade-in.
- `WinCard` hiện, `LoseCard` ẩn.
- `WinCard` animate xuất hiện rồi pulse loop.
- Nút hiển thị `Continue`.
4. Restart Play, để thua hoặc publish event defeat.
5. Kiểm tra:
- `LoseCard` hiện, `WinCard` ẩn.
- `LoseCard` animate xuất hiện rồi pulse loop.
- Nút hiển thị `Retry`.

## 7. Setup data/meta sprint03 (nhắc lại để complete Dev A)
1. Kiểm tra tồn tại:
- `Assets/Data/Chapters`
- `Assets/Data/Levels`
- `Assets/Data/Gacha`
- `Assets/Data/Items`
- `Assets/Data/Meta`
2. File tối thiểu:
- `chapter_01.json`, `chapter_02.json`
- `chapter_01_level_01.json`, `chapter_01_level_02.json`, `chapter_02_level_01.json`
- `pool_standard.json`
- `item_potion.json`, `item_energy.json`
- `skill_icon_map.json`

## 8. Lỗi thường gặp đúng với scene này
1. `ResultPanel` không hiện:
- Nguyên nhân: `_resultPanel` chưa gán hoặc object bị disable sai cấp cha.
- Cách sửa: reassign `_resultPanel` đúng object `ResultPanel` trên `CombatUIController`.
2. Bấm nút không reload:
- Nguyên nhân: `_resultButton` null.
- Cách sửa: gán lại `ReturnButton` vào `_resultButton`.
3. Thắng/thua không đổi card:
- Nguyên nhân: quên gán `_resultCardController` hoặc `_winCard/_loseCard`.
- Cách sửa: gán đủ references trong `ResultCardController` và `CombatUIController`.
4. Scene nhìn lệch trên mobile:
- Nguyên nhân: size card cứng quá lớn.
- Cách sửa: giảm `ResultCard` xuống `760 x 460`.
