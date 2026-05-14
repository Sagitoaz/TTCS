# Start Menu Setup Guide

## Muc tieu

Bo code da duoc sua de:

- Gioi han he thong save ve duy nhat `slot 0`
- Boot scene se di thang vao `MainMenuScene`
- `MainMenuScene` co the hien `Start Menu` truoc, gom:
  - `Continue`
  - `New Game`
  - `Tutorial`
  - `Settings`
- Tutorial la chuoi panel anh voi `Back / Continue / Exit`
- Settings co the dieu khien volume qua `AudioController`

Ban se tu setup toan bo trong Unity Editor theo cac buoc duoi day.

## File code moi / da doi

- `Assets/Scripts/Core/Save/SaveManager.cs`
- `Assets/Scripts/Flow/Common/FlowController.cs`
- `Assets/Scripts/Flow/MainMenu/MainMenuController.cs`
- `Assets/Scripts/Flow/MainMenu/TutorialPanelSequenceController.cs`
- `Assets/Scripts/Flow/MainMenu/SettingsPanelController.cs`

## 1. MainMenuScene: tach Start Menu va Main Menu

Trong `MainMenuScene`, tao 2 root object UI rieng:

1. `StartMenuRoot`
2. `MainMenuRoot`

`MainMenuRoot` chua UI main menu hien tai cua ban:

- Play
- Team
- Gacha
- Inventory
- Character Collection
- Settings
- Gold text

`StartMenuRoot` chua 4 nut:

1. `ContinueButton`
2. `NewGameButton`
3. `TutorialButton`
4. `SettingsButton`

Gan tat ca reference nay vao component `MainMenuController`.

## 2. MainMenuController references

Mo object dang gan `MainMenuController` va map:

- `Start Menu Root` -> `StartMenuRoot`
- `Main Menu Root` -> `MainMenuRoot`
- `Continue Button` -> nut Continue
- `New Game Button` -> nut New Game
- `Tutorial Button` -> nut Tutorial
- `Start Menu Settings Button` -> nut Settings trong start menu
- Cac field cu cua main menu -> giu nguyen nhu truoc
- `Gold Text` -> text hien gold trong `MainMenuRoot`

Neu ban muon backward-compatible trong luc setup tung phan, co the de trong `StartMenuRoot` tam thoi null. Khi do scene se vao thang main menu nhu cu.

## 3. Tutorial panel sequence

Tao mot panel UI, vi du `TutorialPanelRoot`, dat inactive mac dinh.

Beneath panel nay tao:

1. `TutorialImage`
2. `BackButton`
3. `ContinueButton`
4. `ExitButton`
5. Tuy chon: `PageIndicatorText`
6. Tuy chon: label text con cua `ContinueButton`

Gan component `TutorialPanelSequenceController` len `TutorialPanelRoot` hoac mot object manager bat ky, roi map:

- `Panel Root` -> `TutorialPanelRoot`
- `Tutorial Image` -> `TutorialImage`
- `Panel Sprites` -> danh sach cac sprite tutorial
- `Back Button` -> `BackButton`
- `Continue Button` -> `ContinueButton`
- `Exit Button` -> `ExitButton`
- `Page Indicator Text` -> neu co
- `Continue Button Text` -> neu co

Luu y:

- Anh tutorial nen la `Sprite`
- Ban tu them danh sach sprite vao `Panel Sprites`
- `Back` quay lai panel truoc
- `Continue` sang panel sau
- `Exit` dong panel
- O panel cuoi, `Continue` se bi disable

Sau do quay lai `MainMenuController` va gan field:

- `Tutorial Panel Controller` -> component `TutorialPanelSequenceController`

## 4. Settings panel

Tao mot panel UI, vi du `SettingsPanelRoot`, dat inactive mac dinh.

Ben trong tao:

1. `MasterSlider`
2. `SfxSlider`
3. `UiSlider`
4. `BgmSlider`
5. `CloseButton`
6. Tuy chon: 4 text hien `%`

Gan component `SettingsPanelController` va map:

- `Panel Root` -> `SettingsPanelRoot`
- `Master Volume Slider` -> `MasterSlider`
- `Sfx Volume Slider` -> `SfxSlider`
- `Ui Volume Slider` -> `UiSlider`
- `Bgm Volume Slider` -> `BgmSlider`
- `Close Button` -> `CloseButton`
- Cac field value text -> tuy chon

Quay lai `MainMenuController` va gan:

- `Settings Panel Controller` -> component `SettingsPanelController`

Nut `Settings` o start menu va main menu deu se mo cung panel nay.

## 5. Hanh vi Start Menu

Sau khi setup xong:

1. Vao game tu `Boot`
2. `FlowController` se load `MainMenuScene`
3. Neu chua co session save dang active, `StartMenuRoot` se hien
4. `Continue`
   - Chi bam duoc khi ton tai save file o `slot 0`
   - Se load save `slot 0`
   - Sau do chuyen qua `MainMenuRoot`
5. `New Game`
   - Tao save moi
   - Ghi de vao `slot 0`
   - Sau do chuyen qua `MainMenuRoot`
6. `Tutorial`
   - Mo panel tutorial dang la slideshow anh
7. `Settings`
   - Mo panel settings

## 6. Save slot

Code hien tai van giu kieu kien truc save slot, nhung da bi khoa ve 1 slot:

- Slot duoc su dung la `slot 0`
- Moi request slot khac deu bi redirect ve `slot 0`
- Khong can xoa code UI/save slot cu, nhung neu ban con scene UI cu cho nhieu slot thi nen an no di de tranh gay nham

## 7. Kiem tra nhanh

Kiem tra theo thu tu nay:

1. Xoa save cu trong persistent data neu muon test sach
2. Chay `Boot`
3. Xac nhan `StartMenuRoot` hien ra
4. `Continue` bi disable khi chua co save
5. Bam `New Game` -> vao `MainMenuRoot`
6. Quay lai scene `MainMenuScene` trong luc session con ton tai -> phai vao thang `MainMenuRoot`
7. Test `Tutorial` slideshow
8. Test 4 slider volume o `Settings`

## 8. Neu muon tutorial la man onboarding bat buoc

Hien tai code dang theo yeu cau moi: tutorial la mot muc trong start menu. Neu sau nay ban muon first-time player bat buoc vao tutorial truoc, khi do moi can doi lai `FlowController` hoac noi `New Game` sang scene tutorial.
