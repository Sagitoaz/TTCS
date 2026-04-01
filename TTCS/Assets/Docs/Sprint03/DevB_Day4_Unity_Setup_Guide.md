# Sprint 03 - Day 4 Unity Editor Setup Guide (Dev B)

Muc tieu Day 4:
- Hoan thien Main Menu UI wiring.
- Hoan thien Team Formation UI wiring.
- Test flow Main Menu -> Team -> Back -> Main Menu.
- Test validate lineup (toi da 3 nhan vat).

Code lien quan da co:
- Assets/Scripts/Flow/Common/FlowController.cs
- Assets/Scripts/Flow/MainMenu/MainMenuController.cs
- Assets/Scripts/Flow/TeamFormation/TeamFormationUIController.cs

## 1) Kiem tra ten scene trong Build Settings

Mo File -> Build Settings, dam bao co cac scene sau (ten phai dung y nhu ben duoi):
- Boot
- MainMenuScene
- TeamFormationScene
- GachaScene
- InventoryScene
- LevelSelectScene

Luu y:
- FlowController dang load scene bang ten chuoi, sai ten se bi khong load duoc.

## 2) Setup MainMenuScene

### 2.1 Tao MainMenuManager
1. Mo scene MainMenuScene.
2. Tao Empty GameObject: MainMenuManager.
3. Add component: MainMenuController.

### 2.2 Tao cac button trong Canvas
Tao cac Button sau:
- PlayButton
- TeamButton
- GachaButton
- InventoryButton
- SettingsButton

### 2.3 Gan reference trong Inspector
Chon MainMenuManager, trong MainMenuController drag dung cac button vao field:
- Play Button <- PlayButton
- Team Button <- TeamButton
- Gacha Button <- GachaButton
- Inventory Button <- InventoryButton
- Settings Button <- SettingsButton

### 2.4 Kiem tra FlowController scene name
Mo Boot scene, chon object co FlowController, kiem tra:
- Main Menu Scene Name = MainMenuScene
- Team Formation Scene Name = TeamFormationScene
- Gacha Scene Name = GachaScene
- Inventory Scene Name = InventoryScene
- Level Select Scene Name = LevelSelectScene

## 3) Setup TeamFormationScene

### 3.1 Tao TeamFormationManager
1. Mo scene TeamFormationScene.
2. Tao Empty GameObject: TeamFormationManager.
3. Add component: TeamFormationUIController.

### 3.2 Tao UI co ban
Trong Canvas tao:
- 1 Text: StatusText (hien thong bao validate).
- 1 Button: ValidateButton.
- 1 Button: BackButton.
- Danh sach button nhan vat (vi du Character_A, Character_B, Character_C, Character_D).

### 3.3 Gan reference TeamFormationUIController
Chon TeamFormationManager, gan:
- Validate Button <- ValidateButton
- Back Button <- BackButton
- Status Text <- StatusText

### 3.4 Wire character buttons
Voi moi button nhan vat:
1. OnClick -> add listener.
2. Drag TeamFormationManager vao object.
3. Chon function: TeamFormationUIController.OnCharacterClicked(string).
4. Nhap string characterId tuong ung, vi du:
   - "char_warrior"
   - "char_mage"
   - "char_archer"

Khuyen nghi:
- Dat ten characterId on dinh theo convention cua du an de sau nay map voi data that.

## 4) Test checklist Day 4

### 4.1 Test navigation Main Menu -> Team -> Back
1. Play tu Boot.
2. Vao MainMenuScene.
3. Bam Team -> phai vao TeamFormationScene.
4. Bam Back -> quay lai MainMenuScene.

Expected console logs:
- [MainMenu] Team button clicked
- [Flow] Opening team formation scene

### 4.2 Test lineup selection
1. Vao TeamFormationScene.
2. Bam 3 character buttons khac nhau.
3. Bam them character thu 4.
4. Bam Validate.

Expected:
- Khi vuot qua 3 slots: StatusText hien "Lineup full (max 3)".
- Validate thanh cong: StatusText hien "Lineup validated and saved".

### 4.3 Test toggle chon/bo chon
1. Bam vao 1 character da chon lan nua.
2. Character do bi remove khoi lineup.

Expected:
- StatusText hien "Removed: <id> (.../3)".

## 5) Loi thuong gap va cach xu ly

1. Bam Team khong chuyen scene:
- Kiem tra TeamFormationScene da add vao Build Settings.
- Kiem tra dung ten TeamFormationScene trong FlowController inspector.

2. Validate/Back khong hoat dong:
- Kiem tra ValidateButton va BackButton da drag vao TeamFormationUIController.

3. Bam character khong thay doi status:
- Kiem tra OnClick cua button character da wire den OnCharacterClicked(string).
- Kiem tra parameter string khong rong.

4. Console bao TeamService not found:
- Day la fallback expected khi chua gan implementation that tu DevA.
- Van test duoc flow bang local validation.

## 6) Definition of Done Day 4 (phan setup)

- MainMenuController duoc wire day du 5 button.
- TeamFormationUIController duoc wire day du Validate/Back/StatusText.
- Character buttons goi duoc OnCharacterClicked(string).
- Flow Main Menu -> Team -> Back chay on dinh.
- Validate lineup toi da 3 nhan vat hoat dong dung nhu expected.
