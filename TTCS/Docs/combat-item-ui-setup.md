# Combat Item UI Setup

Tài liệu này mô tả phần đã được thêm vào cho combat item và những gì cần kiểm tra trong Unity Editor.

## 1. Scripts đã thay đổi

- `Assets/Scripts/Meta/Inventory/InventoryService.cs`
- `Assets/Scripts/Combat/Managers/CombatFlowController.cs`
- `Assets/Scripts/UI/Combat/CombatUIController.cs`

## 2. Hành vi đã được thêm

- Consumable có thể dùng trong context `combat`.
- Trong lượt của player, combat UI có thêm button `Item`.
- Bấm `Item` sẽ mở panel item.
- List item dùng lại `InventoryItemCellView` từ inventory prefab.
- Chọn item sẽ hiện:
  - tên
  - mô tả
  - thông tin sau khi dùng
  - `Use`
  - `Cancel`
- Khi `Use`:
  - item bị consume khỏi inventory
  - effect được áp ngay lên character đang tới lượt
  - lượt kết thúc ngay

## 3. Item effect hiện đang hỗ trợ

Trong combat hiện mới support:

- `heal_hp`
- `restore_energy`
- `restore_mana`

Với data hiện tại:

- `item_potion` -> heal HP
- `item_energy` -> hồi mana

## 4. Setup trong Unity Editor cho scene combat hiện tại

### CombatScene

Mở scene:

- `Assets/Scenes/CombatScene.unity`

Chọn object:

- `CombatUIController`

Kiểm tra field sau trong Inspector:

- `Combat Item Cell Prefab`

Field này phải trỏ tới:

- `Assets/Prefabs/UI/ItemPrefab.prefab`

Scene `CombatScene.unity` và `CombatScene 1.unity` đã được cập nhật reference này trong file scene. Dù vậy vẫn nên mở Inspector kiểm tra lại sau khi Unity reimport.

## 5. Setup cho scene combat mới

Nếu bạn tạo một combat scene mới, cần làm các bước sau:

1. Thêm `CombatUIController` vào canvas combat như scene hiện tại.
2. Gán toàn bộ các field cũ của `CombatUIController` như trước.
3. Gán thêm field mới:
   - `Combat Item Cell Prefab` -> `Assets/Prefabs/UI/ItemPrefab.prefab`
4. Đảm bảo trong scene có:
   - `CombatFlowController`
   - `SkillManager`
   - `MetaServiceHub`
   - `SaveManager`
   - `DataManager`

Nếu thiếu các manager trên, item button có thể hiện nhưng không consume/apply effect được.

## 6. Lưu ý về UI

Panel item combat hiện được tạo bằng code runtime trong `CombatUIController`.

Điều đó có nghĩa là trong Hierarchy bạn không cần tạo tay:

- item button
- item panel
- scroll view
- detail panel

Code sẽ tự dựng các object này khi `CombatUIController.Initialize()` được gọi.

## 7. Cách test nhanh trong Editor

1. Cho save hiện tại có sẵn:
   - `item_potion`
   - `item_energy`
2. Vào combat scene.
3. Tới lượt player.
4. Kiểm tra button `Item` xuất hiện.
5. Bấm `Item`.
6. Chọn `Health Gem` khi HP chưa đầy.
7. Bấm `Use`.
8. Xác nhận:
   - HP tăng ngay
   - item quantity giảm
   - turn kết thúc ngay
9. Lặp lại với `Energy Gem` khi mana chưa đầy.

## 8. Những case nên kiểm tra thêm

- HP đầy -> potion không cho `Use`
- Mana đầy -> energy gem không cho `Use`
- Không có consumable trong inventory -> button item bị disable
- Lượt enemy -> panel item bị ẩn

## 9. Giới hạn hiện tại

- Item combat hiện chỉ dùng lên `character đang tới lượt`
- Chưa có target ally/enemy cho item
- Chưa có animation riêng cho dùng item

## 10. Effect icon path

HUD effect icon hiện được load từ `Resources`.

Đặt sprite tại:

- `Assets/Resources/UI/Effects/`

Tên file nên map trực tiếp theo `EffectId` của status effect. Ví dụ:

- `Assets/Resources/UI/Effects/burn.png`
- `Assets/Resources/UI/Effects/bleed.png`
- `Assets/Resources/UI/Effects/shield.png`
- `Assets/Resources/UI/Effects/stun.png`
- `Assets/Resources/UI/Effects/heal_regen.png`
- `Assets/Resources/UI/Effects/crit_up.png`

Code cũng có fallback thứ hai:

- `Assets/Resources/UI/Effects/effect_<effectId>.png`

Ví dụ:

- `Assets/Resources/UI/Effects/effect_burn.png`

Ưu tiên dùng tên trùng `EffectId` để đỡ rối.

Nếu cần bước tiếp theo hợp lý nhất là:

- cho item chọn target ally
- tách logic item effect ra service riêng
- thêm feedback VFX/SFX khi dùng item
