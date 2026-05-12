# Character Collection UI — Unity Editor Setup

Tài liệu này hướng dẫn set up scene/UI cho `CharacterCollectionUIController` (List → Detail → Equip Accessory picker).

## 1) Yêu cầu scene (must-have)

Đảm bảo trong scene có các singleton/manager sau (có thể nằm ở bootstrap scene và `DontDestroyOnLoad`):

- `DataManager` (để load character/skill/item data + sprite)
- `SaveManager` (để lấy save hiện tại)
- `MetaServiceHub` (để lấy `InventoryService`)

Nếu thiếu 1 trong các manager trên, UI có thể không hiển thị data hoặc không equip được.

## 2) Tạo layout tổng

Tạo 3 phần UI chính:

1) **List Panel** (danh sách nhân vật)
2) **Detail Panel** (chi tiết nhân vật)
3) **Accessory Equip Picker Panel** (panel nhỏ chọn trang bị)

Flow:
- Vào scene: mở **List Panel**, ẩn **Detail Panel**
- Click 1 slot nhân vật: mở **Detail Panel**, ẩn **List Panel**
- Click ô trang bị: hiện 2 lựa chọn **Equip** / **Unequip**
  - Equip: mở **Accessory Equip Picker Panel**
  - Unequip: tháo trang bị đang đeo

## 3) Setup List Panel

### 3.1. Danh sách nhân vật (grid)

Tạo một container (ví dụ `ScrollView/Viewport/Content`) và dùng nó làm root spawn slot.

- Root spawn: `Transform` (ví dụ `Content`)
- Prefab cell: dùng lại **TeamFormation character cell**
  - Script: `TTCS.Flow.TeamFormation.TeamFormationPickerCellView`

Gợi ý:
- `Content` nên gắn `GridLayoutGroup` + `ContentSizeFitter` (nếu bạn đang dùng ScrollView).

### 3.2. Search + Filters

Tạo các UI input:

- Search: `TMP_InputField`
- Rarity: `TMP_Dropdown` (options sẽ được controller tự set, chỉ cần kéo reference)
- Role: `TMP_Dropdown`
- Element: `TMP_Dropdown`

Sort:
- Sort (Level): `TMP_Dropdown` (Level giảm dần / tăng dần)

### 3.3. Nút Back

Tạo nút Back để quay về Main Menu.

## 4) Setup Detail Panel (chia 2 nửa)

### 4.1. Left panel (portrait)

- `Image` portrait nhân vật

### 4.2. Right panel (info + stats + skills + equipment)

Tạo các text/slider sau (có thể tối giản, field nào không dùng có thể để trống):

- Name: `TMP_Text`
- Level: `TMP_Text`
- Level progress: `Slider` + `TMP_Text` (progress text)
- Rarity: `TMP_Text`
- Role icon: `Image`
- Element icon: `Image`

Stats (tùy UI của bạn):
- HP: `TMP_Text`
- Mana: `TMP_Text`
- ATK/DEF/SPD: `TMP_Text`
- CRIT/RESIST: `TMP_Text`
- Summary (optional): `TMP_Text`

### 4.3. Skills panel (reuse từ TeamFormation)

- Root spawn: `Transform` (container chứa các skill item)
- Prefab: `TTCS.Flow.TeamFormation.TeamFormationSkillQuickItemView`
- Skill detail popup: `TTCS.Flow.TeamFormation.SkillDetailPanelView`

Lưu ý:
- `SkillDetailPanelView` sẽ tự `Hide()` khi start; controller gọi `ShowSkillDetail()` khi click.

### 4.4. Equipment slot (Accessory)

Tạo UI cho 1 ô accessory đang equip + panel lựa chọn:

- Button (ô trang bị): `Button`
- Equipped icon: `Image`

Action panel (hiện khi bấm ô trang bị):
- Root: `GameObject`
- Equip option: `Button`
- Unequip option: `Button`

## 5) Setup Accessory Equip Picker Panel

Panel này dùng script: `TTCS.Flow.CharacterCollection.AccessoryEquipPickerPanel`.

Tạo 1 panel nhỏ (có thể overlay lên Detail Panel), gồm:

### 5.1. List accessories (grid)

- List root: `Transform` (nên là `GridLayoutGroup` để chạy trái→phải xuống dòng)
- Cell prefab: `TTCS.Flow.Inventory.InventoryItemCellView`

Yêu cầu cell prefab có:
- icon
- quantity
- selected effect (đã có trong `InventoryItemCellView.SetSelected()`)

### 5.2. Selection detail + stat lines

- Detail root: `GameObject` (bật/tắt khi có item)
- Name/Rarity/Description: `TMP_Text`
- Stat line root: `Transform`
- Stat line prefab: `TTCS.Flow.Inventory.InventoryAccessoryStatLineView`

### 5.3. Buttons

- Close: đóng picker
- Confirm: equip accessory đã chọn

## 6) Gắn reference trong Inspector

### 6.1. CharacterCollectionUIController

Chọn GameObject có script `TTCS.Flow.CharacterCollection.CharacterCollectionUIController` và kéo thả:

**Panels**
- List Panel → `_listPanel`
- Detail Panel → `_detailPanel`

**List**
- List Root (Content) → `_listRoot`
- Slot Prefab → `_characterSlotPrefab` (TeamFormationPickerCellView prefab)
- Feedback text (optional) → `_listFeedbackText`

**Search / Filters**
- `_searchInput`
- `_rarityDropdown`, `_roleDropdown`, `_elementDropdown`

**Sort**
- `_sortDropdown`

**List - Actions**
- Back to menu button → `_backToMenuButton`

**Detail - Actions**
- Back to list button → `_backToListButton`

**Detail - Left**
- Portrait image → `_portraitImage`

**Detail - Right**
- Name/Level → `_nameText`, `_levelText`
- Level progress → `_levelProgressSlider`, `_levelProgressText`
- Rarity → `_rarityText`

Role/Element icon
- `_roleIconImage`, `_elementIconImage`

**Detail - Stats**
- `_hpText`, `_manaText`
- `_atkText`, `_defText`, `_spdText`
- `_critText`, `_resistText`
- `_statsSummaryText` (optional)

**Detail - Skills**
- Skill root → `_skillRoot`
- Skill item prefab → `_skillItemPrefab` (TeamFormationSkillQuickItemView prefab)
- Skill detail panel → `_skillDetailPanel` (SkillDetailPanelView)

**Detail - Equipment**
- Equip button → `_equipAccessoryButton`
- Equipped icon → `_equippedAccessoryIcon`
- Action panel root → `_accessoryActionPanel`
- Action buttons → `_accessoryActionEquipButton`, `_accessoryActionUnequipButton`
- Picker panel reference → `_accessoryPicker` (AccessoryEquipPickerPanel)

### 6.2. AccessoryEquipPickerPanel

Chọn GameObject có script `TTCS.Flow.CharacterCollection.AccessoryEquipPickerPanel` và kéo thả:

- Panel root (optional) → `_panelRoot` (nếu để trống script sẽ dùng `gameObject.SetActive`)
- List root → `_listRoot`
- Cell prefab → `_cellPrefab` (InventoryItemCellView)
- Detail root → `_detailRoot`
- Name/Rarity/Description → `_nameText`, `_rarityText`, `_descriptionText`
- Stat line root/prefab → `_statLineRoot`, `_statLinePrefab` (InventoryAccessoryStatLineView)
- Close/Confirm buttons → `_closeButton`, `_confirmButton`
- Feedback text (optional) → `_feedbackText`

## 7) Dữ liệu item cần đúng để equip

Accessory sẽ xuất hiện trong picker nếu:

- Item tồn tại trong inventory (`SaveData.inventoryItems`) và quantity > 0
- `ItemDataModel.itemType == "accessory"`
- `ItemDataModel.equippable == true`

Stat accessory lấy từ:
- `ItemDataModel.statBonuses` (ưu tiên)
- hoặc parse từ `ItemDataModel.statDescription` theo format legacy (ví dụ `+10 ATK`)

## 8) Quick sanity checklist

- List panel hiển thị slot nhân vật
- Search/filter làm list đổi theo input
- Click slot mở detail và list bị ẩn
- Detail hiển thị portrait, name, level, stats, skills
- Click equip mở picker
- Click accessory: cell selected effect bật + detail/stat hiện
- Confirm: accessory equip vào nhân vật, picker đóng, detail stats cập nhật
- Unequip: bấm ô trang bị → chọn Unequip, detail stats cập nhật
