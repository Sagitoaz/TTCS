# Sprint 03 - DevA Service Integration Setup Guide (Flow/UI)

Muc tieu:
- Chay Flow/UI bang service that cua DevA (khong dung mock).
- Dam bao Boot/Tutorial/LevelSelect/TeamFormation lay duoc service tu MetaServiceHub.

Code da doi:
- Assets/Scripts/Flow/Common/FlowController.cs
- Assets/Scripts/Flow/LevelSelect/LevelSelectController.cs
- Assets/Scripts/Flow/TeamFormation/TeamFormationUIController.cs
- Assets/Scripts/Meta/Team/ITeamService.cs

## 1) Setup object can thiet trong scene Boot

Ban can co 2 singleton object trong Boot scene:
1. SaveManager (DevA)
2. MetaServiceHub

### 1.1 SaveManager
- Kiem tra trong Hierarchy co object chua SaveManager component.
- Neu chua co, tao empty object va add SaveManager theo implementation cua DevA.
- SaveManager phai khoi tao CurrentSave truoc khi vao flow.

### 1.2 MetaServiceHub
- Tao empty object: MetaServiceHub.
- Add component: MetaServiceHub (Assets/Scripts/Meta/MetaServiceHub.cs).
- MetaServiceHub Awake se auto InitializeServices() neu SaveManager da san sang.

Luu y thu tu khoi tao:
- SaveManager nen khoi tao truoc MetaServiceHub.
- Neu chua chac script execution order, co the dat SaveManager object len tren trong hierarchy va test log.

## 2) Setup FlowController trong Boot

- Object FlowManager co component FlowController.
- Kiem tra scene names trong inspector:
  - Boot
  - TutorialScene
  - MainMenuScene
  - TeamFormationScene
  - GachaScene
  - InventoryScene
  - LevelSelectScene
  - CombatScene

FlowController hien tai:
- Lay service tu MetaServiceHub (Progression/Team/Gacha/Inventory).
- Tutorial state uu tien ProgressionService.IsTutorialCompleted().
- Chi fallback PlayerPrefs neu service chua san sang.

## 3) Build Settings

Dam bao cac scene da add trong Build Settings va ten giong chinh xac:
- Boot
- TutorialScene
- MainMenuScene
- TeamFormationScene
- GachaScene
- InventoryScene
- LevelSelectScene
- CombatScene (neu da co)

## 4) Sanity test nhanh sau khi setup

### Test A: Boot routing
1. Play tu Boot.
2. Kiem tra log:
   - [Flow] FlowController initialized with DevA services from MetaServiceHub
3. Kiem tra route:
   - Tutorial chua complete -> vao TutorialScene
   - Tutorial da complete -> vao MainMenuScene

### Test B: Team flow dung TeamService that
1. MainMenu -> TeamFormation.
2. Chon lineup va bam Validate.
3. Kiem tra status text hien message theo TeamService (invalid/valid).
4. Bam Back -> ve MainMenu.

### Test C: LevelSelect dung ProgressionService that
1. MainMenu -> Play -> LevelSelect.
2. Level lock/unlock phan anh state tu save/progression.
3. Bam level locked thi khong vao combat.

## 5) Loi thuong gap

1. Log: MetaServiceHub not found
- Chua add MetaServiceHub vao Boot scene.

2. Team/Progression service null
- SaveManager.CurrentSave chua co khi MetaServiceHub InitializeServices.
- Kiem tra thu tu khoi tao SaveManager truoc.

3. Tutorial state khong dung
- Kiem tra Save data field tutorialCompleted trong save cua DevA.
- Neu save chua san, FlowController co fallback PlayerPrefs tam thoi.

## 6) Trang thai mock

Da bo mock khoi FlowController:
- MockProgressionService
- MockTeamService
- MockGachaService
- MockInventoryService

Neu mot module DevA chua hoan thanh:
- Flow hien tai se warning/service null-safe o mot so cho.
- Ban co the tiep tuc test flow co ban, sau do bo fallback khi DevA complete.
