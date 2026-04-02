# Sprint 03 - Day 4 Unity Editor Setup Guide (Dev B)

Muc tieu Day 4:
- Hoan thien Main Menu UI wiring.
- Hoan thien Team Formation UI wiring theo cau truc moi.
- Team Formation khong dung StatusText/ValidateButton.
- Dung flow 3 slot -> picker panel (highlight cell) -> confirm trong picker -> quay lai team panel.

Code lien quan:
- Assets/Scripts/Flow/Common/FlowController.cs
- Assets/Scripts/Flow/MainMenu/MainMenuController.cs
- Assets/Scripts/Flow/TeamFormation/TeamFormationUIController.cs

## 1) Kiem tra ten scene trong Build Settings

Mo File -> Build Settings, dam bao co cac scene sau:
- Boot
- MainMenuScene
- TeamFormationScene
- GachaScene
- InventoryScene
- LevelSelectScene

Luu y:
- FlowController load scene bang ten chuoi, sai ten se khong load duoc.

## 2) Setup MainMenuScene

### 2.1 Tao MainMenuManager
1. Mo scene MainMenuScene.
2. Tao Empty GameObject: MainMenuManager.
3. Add component: MainMenuController.

### 2.2 Tao button trong Canvas
Tao cac button:
- PlayButton
- TeamButton
- GachaButton
- InventoryButton
- SettingsButton

### 2.3 Gan reference MainMenuController
Gan dung cac field:
- Play Button <- PlayButton
- Team Button <- TeamButton
- Gacha Button <- GachaButton
- Inventory Button <- InventoryButton
- Settings Button <- SettingsButton

## 3) Setup TeamFormationScene theo flow moi

### 3.1 Tao TeamFormationManager
1. Mo scene TeamFormationScene.
2. Tao Empty GameObject: TeamFormationManager.
3. Add component: TeamFormationUIController.

### 3.2 Tao Team Panel (panel chinh)
Trong Canvas tao TeamPanel gom:
- BackButton
- SlotButton_1, SlotButton_2, SlotButton_3
- SlotView_1, SlotView_2, SlotView_3 (moi slot view gom):
  - SlotHighlight (GameObject con, bat/tat khi slot dang active)
  - Portrait Image
  - Name Text
  - HP Text
  - HP Slider
  - Level Text

Luu y:
- Moi SlotButton mo picker cho dung slot tuong ung.
- Khong dung StatusText va khong dung ValidateButton.

### 3.3 Tao Picker Panel
Tao PickerPanel (ban dau inactive), gom:
- CharacterListRoot (VerticalLayoutGroup + ContentSizeFitter)
- CharacterItemPrefab (TeamFormationPickerCellView)
  - Portrait
  - Name text
  - Level text
  - Rarity text
  - Highlight GameObject
- Filter controls (co the la button hoac dropdown):
  - Sort Level Asc
  - Sort Level Desc
  - Sort Rarity Asc
  - Sort Rarity Desc
  - Role filter (All/Tank/Attacker/Support hoac role custom)
- ConfirmButton (xac nhan character dang highlight vao slot)
- ClosePickerButton (goi ham ClosePicker)

### 3.4 Tao QuickInfo panel (ben trai picker)
Trong PickerPanel tao QuickInfo gom:
- QuickInfoNameText
- QuickInfoRoleText
- QuickInfoElementText
- QuickInfoPortrait
- QuickInfoSkillRoot (list root)
- QuickInfoSkillItemPrefab (TeamFormationSkillQuickItemView)
  - Skill icon
  - Skill name

### 3.5 Gan reference TeamFormationUIController
Gan cac field trong Inspector:
- Back Button <- BackButton
- Slot Buttons <- SlotButton_1, SlotButton_2, SlotButton_3
- Slot Views <- SlotView_1, SlotView_2, SlotView_3
- Team Panel <- TeamPanel
- Picker Panel <- PickerPanel
- Picker List Root <- CharacterListRoot
- Picker Item Prefab <- CharacterItemPrefab
- Picker Confirm Button <- ConfirmButton
- Picker Close Button <- ClosePickerButton
- Quick Info Name Text <- QuickInfoNameText
- Quick Info Role Text <- QuickInfoRoleText
- Quick Info Element Text <- QuickInfoElementText
- Quick Info Portrait <- QuickInfoPortrait
- Quick Info Skill Root <- QuickInfoSkillRoot
- Quick Info Skill Item Prefab <- QuickInfoSkillItemPrefab
- Sort Dropdown / Role Dropdown (neu dung dropdown)

## 4) Wire OnClick trong Unity

### 4.1 Slot buttons
- SlotButton_1 -> TeamFormationUIController.OnSlotClicked(0)
- SlotButton_2 -> TeamFormationUIController.OnSlotClicked(1)
- SlotButton_3 -> TeamFormationUIController.OnSlotClicked(2)

### 4.2 Filter buttons (neu dung button)
- Level asc -> SetSortByLevelAsc()
- Level desc -> SetSortByLevelDesc()
- Rarity asc -> SetSortByRarityAsc()
- Rarity desc -> SetSortByRarityDesc()
- Role all -> SetRoleFilterAll()
- Role theo nhom -> SetRoleFilter("Tank") / SetRoleFilter("Attacker") / SetRoleFilter("Support")

### 4.3 Picker buttons
- ConfirmButton -> duoc controller gan listener trong Start()
- ClosePickerButton -> duoc controller gan listener trong Start()

Ghi chu:
- Character cua slot dang active duoc uu tien len dau danh sach picker.
- Character o slot khac khong hien trong picker.
- Character dang o slot active van hien trong picker.

## 5) Rule duoc ap dung trong picker

Danh sach picker chi hien thi nhan vat:
- Thuoc unlocked roster cua player.
- Chua duoc chon o slot khac.
- Chua bi danh dau da ra tran.
- Current HP > 0.

Filter duoc ho tro:
- Level asc/desc.
- Rarity asc/desc.
- Role.

## 6) Test checklist Day 4 (flow moi)

### 6.1 Navigation
1. Play tu Boot.
2. Vao MainMenuScene.
3. Bam Team -> vao TeamFormationScene.
4. Bam Back -> quay lai MainMenuScene.

Expected logs:
- [MainMenu] Team button clicked
- [Flow] Opening team formation scene

### 6.2 Team slot flow
1. Bam Slot 1.
2. PickerPanel mo ra voi danh sach character hop le.
3. Chon 1 character (cell duoc highlight).
4. QuickInfo ben trai cap nhat (ten, role, element, portrait, skills).
5. Bam Confirm trong picker.

Expected:
- Picker dong lai.
- TeamPanel hien lai.
- Slot 1 cap nhat portrait + HP slider + HP text + Level.

Toggle unequip:
- Neu bam vao cell dang highlight va do la character dang mang o slot active thi slot bi unequip ngay.

### 6.3 Filter flow
1. Mo picker.
2. Doi sort level/rarity.
3. Doi role filter.

Expected:
- List reorder dung sort.
- List chi con role da chon.

### 6.4 Exclusion rules
1. Tao case character HP = 0 trong save.
2. Tao case character da nam trong deployedCharacters.
3. Mo picker.

Expected:
- Character HP = 0 khong hien.
- Character da ra tran khong hien.

## 7) Loi thuong gap va cach xu ly

1. Bam slot nhung picker khong hien:
- Kiem tra TeamPanel/PickerPanel da drag vao controller.
- Kiem tra SlotButton goi dung OnSlotClicked(index).

2. Picker list trong:
- Kiem tra SaveManager co CurrentSave.
- Kiem tra unlockedCharacters co data.
- Kiem tra character khong bi loai bo boi dieu kien HP/deployed.

3. Confirm khong hien:
- Kiem tra ConfirmPanel, ConfirmText, Yes/No da drag dung.

4. Slot khong cap nhat portrait/stats:
- Kiem tra SlotView refs (portrait/name/hpSlider/hpText/level) da gan day du.
- Kiem tra character data co portraitPath (neu khong co thi portrait co the null).

5. QuickInfo khong hien skills:
- Kiem tra QuickInfoSkillRoot va QuickInfoSkillItemPrefab da gan.
- Kiem tra DataManager co load duoc skill data + skill icon map.

## 8) Definition of Done Day 4 (cap nhat)

- MainMenuController wire day du 5 button.
- TeamFormationUIController wire day du 3 slot + picker + confirm.
- Khong con su dung StatusText va ValidateButton.
- Slot click -> picker (highlight) -> confirm trong picker -> slot update hoat dong on dinh.
- Slot co nhan vat hien portrait + HP slider + HP text + level.
- Filter level/rarity/role hoat dong dung.
- Slot highlight bat dung khi dang active.
- QuickInfo cap nhat dung theo character dang highlight trong picker.
- Rule loai tru character het HP/da ra tran/da chon o slot khac hoat dong dung.
