# Sprint 03 - Hướng Dẫn Setup Unity Ngày 6 (Dev B)

Sprint: Sprint 03  
Developer: Dev B (Flow/UI)  
Phạm vi Ngày 6: Inventory UI, Level Select wiring, Character Collection v1

## 1. Các script đã thêm cho Ngày 6

- Assets/Scripts/Flow/Inventory/InventoryUIController.cs
- Assets/Scripts/Flow/LevelSelect/LevelSelectUIController.cs
- Assets/Scripts/Flow/CharacterCollection/CharacterCollectionUIController.cs

Các cập nhật liên quan:
- Assets/Scripts/Core/Save/SaveData.cs (lưu current mana)
- Assets/Scripts/Core/Save/SaveManager.cs (khởi tạo giá trị current mana mặc định)
- Assets/Scripts/Flow/Common/FlowController.cs (OpenCharacterCollection)
- Assets/Scripts/Flow/Common/IFlowController.cs (contract OpenCharacterCollection)
- Assets/Scripts/Flow/MainMenu/MainMenuController.cs (nút Character Collection tùy chọn)

## 2. Setup scene Inventory

Tên scene đề xuất: InventoryScene

1. Tạo GameObject rỗng tên InventoryUIRoot.
2. Add component InventoryUIController vào InventoryUIRoot.
3. Khu vực danh sách item:
- Tạo ScrollView content root và gán vào Item List Root.
- Tạo item prefab dạng ô vuông, có component InventoryItemCellView, gán vào Item Cell Prefab.
- Bên trong prefab ô vuông, bind:
  - Button
  - Icon Image (avatar)
  - Quantity TMP text (định dạng x2)
  - Border Image (màu theo rarity)
4. Khu vực detail item:
- Thêm Image cho icon lớn và gán vào Detail Icon Image.
- Thêm TMP_Text và gán:
  - Item Name Text
  - Item Rarity Text
  - Item Description Text
  - Accessory Stat Title Text
  - Accessory Stat Text (text tóm tắt tùy chọn, có thể để trống nếu chỉ dùng stat line)
  - Use Item Button Label
  - Feedback Text
- Thêm 3 Image field và gán:
  - Detail Frame Image A
  - Detail Frame Image B
  - Detail Glow Image
- Thêm Image cho viền riêng của accessory và gán vào Accessory Border Image.
- Tạo 1 GameObject section cho accessory stats và gán vào Accessory Stat Root.
- Gán màu cho viền của Accessory Stat Root giống màu viền rarity của item.
- Gán màu cho Glow Image giống màu viền rarity của item.
- Tạo stat line container bên trong Accessory Stat Root và gán vào Accessory Stat Line Root.
- Tạo stat line prefab có component InventoryAccessoryStatLineView (Stat Name TMP_Text + Icon Image + Value TMP_Text), gán vào Accessory Stat Line Prefab.
5. Thêm nút Back và gán vào Back Button.

Sơ đồ Hierarchy ngắn cho phần Inventory Detail:

```text
InventoryUIRoot
├── ItemListRoot (ScrollView)
└── DetailRoot
  ├── DetailIcon
  ├── ItemNameText
  ├── ItemRarityText
  ├── AccessoryStatRoot
  │   └── AccessoryStatLineRoot
  │       └── AccessoryStatLinePrefab (lặp nhiều dòng)
  └── ItemDescriptionText
```

Ghi nhớ thứ tự hiển thị:
- Nếu item có stat, phần stat nằm trước description.
- Nếu item không có stat, chỉ giữ description ở phía trên, không hiển thị khối stat.

6. Play test:
- Kiểm tra mỗi ô item hiển thị: icon, số lượng xN, màu viền theo rarity.
- Click ô item để mở detail.
- Kiểm tra Frame A/B và Glow đổi màu theo rarity.
- Kiểm tra style text rarity:
  - SSR: gradient + text vàng
  - SR: #FF007F
  - R: #00F0FF
- Nếu là consumable: label nút là Use và action tiêu hao item.
- Nếu là accessory: chỉ hiển thị thông tin, không có thao tác Equip trong Inventory.
- Nếu là accessory: Accessory Stat Root phải hiển thị.
- Nếu là accessory: mỗi bonus stat hiển thị theo từng dòng, có icon load từ Resources + giá trị tăng (ví dụ +120, +8).
- Nếu có `Accessory Stat Text` thì đây chỉ là dòng summary; phần hiển thị chính vẫn là các stat line prefab.
- `Accessory Stat Title Text` chỉ dùng làm tiêu đề cho khối stat và sẽ đổi màu theo rarity của item.
- Thứ tự nội dung detail:
  - Nếu có stat: khối stat phải nằm trên, description nằm dưới.
  - Nếu không có stat: chỉ hiển thị description ở vị trí trên cùng, không cần khối stat.
- Click Back để quay về Main Menu.

## 3. Setup scene Level Select

Tên scene đề xuất: LevelSelectScene

1. Tạo GameObject LevelSelectUIRoot.
2. Add component LevelSelectUIController.
3. Chapter tabs:
- Tạo horizontal layout root và gán vào Chapter Tab Root.
- Tạo chapter tab prefab (Button + TMP_Text) và gán vào Chapter Tab Prefab.
4. Level grid:
- Tạo grid root và gán vào Level Grid Root.
- Tạo level card prefab (Button + TMP_Text) và gán vào Level Card Prefab.
5. Gán nút Back.
6. Play test:
- Kiểm tra chapter tabs được render.
- Kiểm tra mỗi chapter hiển thị level cards.
- Click level card để gọi FlowController.EnterCombat(levelId, lineup).

Ghi chú:
- Nếu thiếu Data/Chapters hoặc Data/Levels, controller sẽ tự fallback mock data 3 chapter x 3 level.

## 4. Setup scene Character Collection

Tên scene đề xuất: CharacterCollectionScene

1. Tạo GameObject CharacterCollectionUIRoot.
2. Add component CharacterCollectionUIController.
3. Khu vực card list:
- Tạo grid root và gán vào Card Grid Root.
- Tạo card prefab (Button + TMP_Text) và gán vào Character Card Prefab.
4. Search và sorting:
- Thêm TMP_InputField và gán vào Search Input.
- Wire UI toggle/dropdown đến:
  - OnSortRareChanged(bool ascending)
  - OnSortLevelChanged(bool ascending)
5. Detail panel:
- Gán các TMP_Text cho Name/Rarity/Level/Stats/HP/Mana.
- Gán Portrait Image và Skill Icon Image.
6. Action:
- Gán Feed Button và Back Button.
- Feedback Text là tùy chọn (nếu có thì gán).

## 5. Xác thực rule Ngày 6

Rule đã implement:
- Mỗi lần level-up từ feed action sẽ hồi đầy HP và Mana.

Cách verify:
1. Mở scene Character Collection.
2. Chọn 1 nhân vật.
3. Bấm Feed.
4. Xác nhận Level tăng 1 và HP/Mana đều về current = max ngay lập tức.

Thêm bước verify inventory:
1. Kiểm tra item_guardian_emblem xuất hiện trong inventory.
2. Mở detail accessory và kiểm tra section stat được hiển thị.
3. Xác nhận accessory trong Inventory chỉ để xem thông tin, không equip tại màn này.

## 6. Wiring tùy chọn ở Main Menu

MainMenuController hiện hỗ trợ nút Character Collection tùy chọn:
- Field: Character Collection Button
- Nếu gán field này, khi click sẽ gọi FlowController.OpenCharacterCollection().

Nếu không gán, sẽ không phát sinh runtime error.

## 7. Quick smoke checklist (Ngày 6)

- Inventory: list -> detail -> use (consumable) -> back hoạt động.
- Inventory: accessory hiển thị đúng thông tin/stat line, không có equip action.
- Level Select: chapter tab -> level card -> EnterCombat route hoạt động.
- Character Collection:
- render được unlocked characters
- search theo tên/id hoạt động
- sort theo rarity và level hoạt động
- click card hiển thị detail HP/Mana
- feed cập nhật level và hồi full HP/Mana
- nút Character Collection trên Main Menu (nếu có gán) mở đúng scene.
