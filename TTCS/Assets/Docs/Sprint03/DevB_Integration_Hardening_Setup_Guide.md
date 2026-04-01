# Sprint 03 - Integration Hardening Setup Guide (SaveManager + MetaServiceHub)

Muc tieu:
- Tranh service null o frame dau khi vao Boot.
- Dam bao Flow route dung ngay ca khi thu tu Awake/Start khac nhau.

Code da harden:
- Assets/Scripts/Meta/MetaServiceHub.cs
- Assets/Scripts/Flow/Common/FlowController.cs
- Assets/Scripts/Flow/LevelSelect/LevelSelectController.cs

## 1) Bat buoc co 3 object trong Boot scene

Trong scene Boot, dam bao co:
1. SaveManager object (component SaveManager)
2. MetaServiceHub object (component MetaServiceHub)
3. FlowManager object (component FlowController)

Khuyen nghi:
- Dat ten object ro rang: SaveManager, MetaServiceHub, FlowManager.
- Ca 3 object deu duoc DontDestroyOnLoad.

## 2) Script Execution Order (khuyen nghi)

Mo Project Settings -> Script Execution Order:
- SaveManager: -100
- MetaServiceHub: -50
- FlowController: 0

Ly do:
- SaveManager dung san truoc.
- MetaServiceHub se bind service khi SaveManager da co.
- FlowController route sau khi service san sang (co retry).

## 3) Kiem tra Build Settings

Dam bao co scene Boot va scene dich da add vao Build Settings:
- Boot
- TutorialScene
- MainMenuScene
- TeamFormationScene
- LevelSelectScene
- GachaScene
- InventoryScene
- CombatScene (neu da san)

## 4) Cac hanh vi hardening moi

1. MetaServiceHub
- Co EnsureInitialized() va retry theo frame den khi tao du service.
- Co IsReady de check readiness.

2. FlowController
- Start dung coroutine BootstrapAndRoute().
- Cho toi da 120 frame de bind ProgressionService truoc khi route.
- Neu van null thi warning va fallback tutorial state.

3. LevelSelectController
- Start dung DeferredPopulate().
- Cho toi da 60 frame de bind ProgressionService truoc khi render level buttons.

## 5) Test checklist nhanh

Test A - Startup
1. Play tu Boot.
2. Kiem tra Console co log init service.
3. Khong co null-reference o frame dau.

Test B - Tutorial routing
1. New save: Boot -> TutorialScene.
2. Sau complete tutorial -> MainMenuScene.
3. Restart voi tutorial completed -> skip tutorial.

Test C - LevelSelect
1. MainMenu -> LevelSelect.
2. Khong bi empty do service null frame dau.
3. Level lock/unlock hien thi theo progression service.

## 6) Neu van thay service null

1. Kiem tra SaveManager co trong Boot scene khong.
2. Kiem tra SaveManager co bi duplicate object khong.
3. Kiem tra Script Execution Order da set nhu muc 2 chua.
4. Kiem tra co scene nao load truc tiep bo qua Boot khong.

Neu co scene load truc tiep khi test:
- Them SaveManager + MetaServiceHub vao scene test do,
hoac
- Luon bat dau test bang Boot scene.
