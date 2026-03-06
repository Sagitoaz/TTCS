# 🛠️ Hướng dẫn Setup — Developer A (Sprint 02)

> **Dành cho:** Developer A — UI + Timing System  
> **Engine:** Unity 2022 LTS+ / 2D URP  
> **Phụ thuộc:** DOTween Pro, TextMeshPro, New Input System  
> **Trạng thái Dev B:** Chưa hoàn thành — hướng dẫn này cho phép chạy độc lập không cần Dev B

---

## 📋 Mục lục

1. [Yêu cầu trước khi bắt đầu](#1-yêu-cầu-trước-khi-bắt-đầu)
2. [Cài đặt packages](#2-cài-đặt-packages)
3. [Cấu trúc Canvas](#3-cấu-trúc-canvas)
4. [Setup từng Prefab / Component](#4-setup-từng-prefab--component)
5. [Setup Scene Managers](#5-setup-scene-managers)
6. [Setup Audio](#6-setup-audio)
7. [Chạy Test Scripts](#7-chạy-test-scripts)
8. [Checklist cuối cùng](#8-checklist-cuối-cùng)

---

## 1. Yêu cầu trước khi bắt đầu

Những thứ sau phải có trong project **trước khi** setup Dev A:

| Yêu cầu | Kiểm tra tại |
|---------|-------------|
| Sprint 1 engine (EventBus, TurnManager, SkillManager, CombatFlowController...) | `Assets/Scripts/Core/`, `Assets/Scripts/Combat/` |
| DOTween / DOTween Pro | `Assets/Plugins/Demigiant/DOTween/` |
| TextMeshPro | `Window > TextMeshPro > Import TMP Essential Resources` |
| New Input System | `Window > Package Manager → Input System` |
| JSON data files (stages, skills, characters, enemies) | `Assets/Data/` |

---

## 2. Cài đặt packages

### DOTween
1. Import DOTween Pro từ Asset Store hoặc `.unitypackage`
2. **Tools > Demigiant > DOTween Utility Panel** → nhấn **Setup DOTween**
3. Confirm: `DOTWEEN_VERSION` macro được define → build sẽ không lỗi

### TextMeshPro
```
Window > TextMeshPro > Import TMP Essential Resources
```
Cần thiết vì các component TMP (`TextMeshProUGUI`) được dùng trong: `FloatingText`, `SkillButton`, `TurnOrderSlot`, `BattleHUD`, `TimingFeedbackUI`, `CombatUIController`.

### New Input System
1. `Window > Package Manager` → tìm **Input System** → Install
2. Khi Unity hỏi "Enable new input system?" → chọn **Yes**
3. `TimingInputHandler` sẽ tự fallback sang `Input.GetKeyDown(KeyCode.Space)` nếu `PlayerInput` chưa được setup.

---

## 3. Cấu trúc Canvas

Tạo một **Canvas** với render mode **Screen Space - Camera**, rồi setup hierarchy sau:

```
Canvas (Screen Space - Camera)
├── CombatUIController       ← MonoBehaviour gốc, gán tất cả refs
│
├── BattleHUD                ← HP/MP bars
│   ├── AllySlots/
│   │   ├── AllySlot_0      ← HPSlider + MPSlider + NameText + HPText (CanvasGroup)
│   │   ├── AllySlot_1
│   │   └── AllySlot_2
│   └── EnemySlots/
│       ├── EnemySlot_0
│       ├── EnemySlot_1
│       └── EnemySlot_2
│
├── SkillButtonPanel         ← 4 skill buttons, ẩn/hiện theo lượt
│   ├── SkillButton_0
│   ├── SkillButton_1
│   ├── SkillButton_2
│   └── SkillButton_3
│
├── TurnOrderDisplay         ← Queue visualization (pool 8 slots)
│   └── SlotContainer/      ← Horizontal Layout Group gắn vào đây
│
├── ActionResultDisplay      ← Pool của FloatingText
│
├── TimingFeedbackUI         ← Flash overlay + grade text
│   ├── FlashOverlay         ← CanvasGroup (alpha start = 0)
│   └── GradeText            ← TextMeshProUGUI, center screen
│
└── ResultPanel              ← Victory/Defeat screen (start inactive)
    ├── ResultText           ← TextMeshProUGUI
    └── ReturnButton         ← Button
```

---

## 4. Setup từng Prefab / Component

### 4.1 FloatingText Prefab
**Path:** `Assets/Prefabs/UI/FloatingText.prefab`

1. Tạo UI GameObject → thêm `FloatingText` component
2. Thêm `TextMeshProUGUI` child → gán vào `_label`
3. Cấu hình Inspector:

| Field | Giá trị gợi ý |
|-------|--------------|
| `_label` | TextMeshProUGUI child |
| `_floatHeight` | `80` (pixels) |
| `_duration` | `1.0` giây |
| `_criticalScale` | `1.5` |

### 4.2 ActionResultDisplay
1. Thêm `ActionResultDisplay` component lên một GameObject trong Canvas
2. Gán Inspector:

| Field | Giá trị |
|-------|---------|
| `_floatingTextPrefab` | FloatingText Prefab (bước 4.1) |
| `_poolSize` | `10` |
| `_spawnOffset` | `(0, 1.2, 0)` |

### 4.3 TurnOrderSlot Prefab
**Path:** `Assets/Prefabs/UI/TurnOrderSlot.prefab`

1. Tạo UI GameObject → thêm `TurnOrderSlot` component
2. Cấu trúc con:

```
TurnOrderSlot (TurnOrderSlot.cs)
├── Portrait           ← Image (_portrait)
├── NameText           ← TextMeshProUGUI (_nameText)
└── HighlightOutline   ← Image, màu vàng (_highlightOutline)
```

### 4.4 TurnOrderDisplay
1. Thêm `TurnOrderDisplay` component
2. Gán Inspector:

| Field | Giá trị |
|-------|---------|
| `_slotPrefab` | TurnOrderSlot Prefab |
| `_slotContainer` | Transform cha của slot list (nên có HorizontalLayoutGroup) |
| `_previewCount` | `8` |

### 4.5 SkillButton Prefab
**Path:** `Assets/Prefabs/UI/SkillButton.prefab`

```
SkillButton (SkillButton.cs + Button)
├── Icon               ← Image (_icon)
├── CooldownOverlay    ← Image, Image Type = Filled, FillMethod = Radial360 (_cooldownOverlay)
├── ManaCostText       ← TextMeshProUGUI (_manaCostText)
└── CooldownText       ← TextMeshProUGUI (_cooldownText)
```

| Field | Giá trị |
|-------|---------|
| `_button` | Button component trên root |
| `_icon` | Image child |
| `_cooldownOverlay` | Image (Filled/Radial) child |
| `_manaCostText` | TMP child |
| `_cooldownText` | TMP child |
| `_canvasGroup` | CanvasGroup trên root |

### 4.6 SkillButtonPanel
1. Thêm `SkillButtonPanel` component lên Panel GameObject
2. Thêm `CanvasGroup` component lên cùng GameObject
3. Gán Inspector:

| Field | Giá trị |
|-------|---------|
| `_buttons` | Array 4 phần tử → gán 4 SkillButton GameObjects |
| `_canvasGroup` | CanvasGroup trên cùng GameObject |

### 4.7 BattleHUD
BattleHUD dùng **serialized struct `HUDSlot`** với các field public. Mỗi slot là một group UI.

**Mỗi `HUDSlot` cần:**

| Field | Component |
|-------|-----------|
| `HPSlider` | Slider (min=0, max=1) |
| `MPSlider` | Slider (min=0, max=1) |
| `NameText` | TextMeshProUGUI |
| `HPText` | TextMeshProUGUI |
| `SlotGroup` | CanvasGroup trên root của slot |

Setup:
1. Tạo 3 GameObject cho `AllySlots` và 3 cho `EnemySlots`
2. Mỗi GameObject có: Slider (HP), Slider (MP), TMP (name), TMP (HP value), CanvasGroup
3. Gán lần lượt vào `_allySlots[0..2]` và `_enemySlots[0..2]` trong Inspector

### 4.8 TimingFeedbackUI
1. Thêm `TimingFeedbackUI` component
2. Gán Inspector:

| Field | GameObject |
|-------|-----------|
| `_flashOverlay` | CanvasGroup, full-screen Image (alpha = 0 ban đầu) |
| `_gradeText` | TextMeshProUGUI ở giữa màn hình |
| `_perfectColor` | `(1, 0.85, 0.1, 0.35)` — Gold |
| `_goodColor` | `(0.3, 0.7, 1, 0.25)` — Blue |
| `_missColor` | `(0.8, 0.2, 0.2, 0.2)` — Red |

### 4.9 CombatUIController
Gán tất cả sub-panels trên CombatUIController root GameObject:

| Field | Gán gì |
|-------|--------|
| `_battleHUD` | BattleHUD component |
| `_skillButtonPanel` | SkillButtonPanel component |
| `_turnOrderDisplay` | TurnOrderDisplay component |
| `_actionResultDisplay` | ActionResultDisplay component |
| `_timingFeedbackUI` | TimingFeedbackUI component |
| `_resultPanel` | ResultPanel GameObject |
| `_resultText` | ResultPanel > ResultText TMP |
| `_resultButton` | ResultPanel > ReturnButton |

---

## 5. Setup Scene Managers

### 5.1 CombatSceneManager
Tạo một **empty GameObject** tên `CombatSceneManager` trong scene, thêm component `CombatSceneManager`.

| Field | Giá trị |
|-------|---------|
| `_defaultStageId` | `"stage_01_tutorial"` (hoặc ID có trong data) |
| `_defaultSeed` | `0` |
| `_autoStartOnPlay` | `true` (để test tự động) |
| `_defaultPartyIds` | `["char_warrior", "char_mage"]` |

### 5.2 CombatBridge
Tạo empty GameObject `CombatBridge`, thêm `CombatBridge` component.  
Không có SerializeField — tự khởi tạo qua Awake singleton.

### 5.3 TimingSystem + TimingInputHandler
Tạo empty GameObject `TimingSystem`, thêm:
- `TimingSystem` component
- `TimingInputHandler` component

**TimingInputHandler cần Input Action (nếu dùng New Input System):**
1. Tạo `InputActionAsset` mới: `Assets/Input/CombatInput.inputactions`
2. Thêm Action Map `Combat`, Action tên `Guard`, binding: `<Keyboard>/space`
3. Gán `PlayerInput` component lên cùng GameObject → assign Asset vào
4. Hoặc để `_useNewInputSystem = false` nếu chưa có → dùng Space key trực tiếp

---

## 6. Setup Audio

Tạo **empty GameObject** tên `AudioController` (nên ở Root scene để `DontDestroyOnLoad` hoạt động đúng), thêm `AudioController` component.

Thêm **2 AudioSource** component lên cùng GameObject:

| AudioSource | Gán vào field | Cấu hình |
|-------------|--------------|---------|
| AudioSource #1 | `_sfxSource` | Play On Awake = OFF, Loop = OFF |
| AudioSource #2 | `_bgmSource` | Play On Awake = OFF, Loop = ON |

**Gán AudioClip (optional — bỏ trống nếu chưa có audio):**

| Field | Âm thanh |
|-------|---------|
| `_hitSFX` | Tiếng đánh/chém |
| `_critHitSFX` | Tiếng đánh critical |
| `_healSFX` | Tiếng hồi máu |
| `_deathSFX` | Tiếng chết |
| `_skillCastSFX` | Tiếng cast skill |
| `_perfectSFX` | Timing Perfect |
| `_goodSFX` | Timing Good |
| `_missSFX` | Timing Miss |
| `_turnStartSFX` | Bắt đầu lượt |
| `_victorySFX` | Chiến thắng |
| `_defeatSFX` | Thua cuộc |
| `_bgmClip` | Nhạc nền battle |

> **Lưu ý**: Tất cả clip null sẽ bị bỏ qua an toàn — không crash.

---

## 7. Chạy Test Scripts

Các test script nằm tại `Assets/Scripts/Debug/Test/`. Cách chạy:

1. Tạo một **empty scene** mới (hoặc dùng scene test riêng)
2. Đảm bảo Managers cần thiết có trong scene (EventBus, TurnManager, SkillManager, DataManager...)
3. Tạo empty GameObject, gán test component muốn chạy
4. Nhấn **Play** → xem Console

### Thứ tự chạy test gợi ý

| Bước | Test file | Cần trong scene |
|------|-----------|----------------|
| 1 | `TimingWindowTest.cs` | Không cần gì thêm |
| 2 | `TimingSystemTest.cs` | `TimingSystem` MonoBehaviour |
| 3 | `CombatBridgeTest.cs` | `CombatBridge`, `TimingSystem` (optional) |
| 4 | `CombatUIControllerTest.cs` | `CombatUIController` (với refs) |
| 5 | `AudioControllerTest.cs` | `AudioController` (với 2 AudioSource) |
| 6 | `TurnOrderDisplayTest.cs` | `TurnManager`, `TurnOrderDisplay` |
| 7 | `BattleHUDTest.cs` | `BattleHUD` (Canvas), `CombatUIController` (optional) |
| 8 | `SkillButtonPanelTest.cs` | `SkillManager`, `SkillButtonPanel` (Canvas) |
| 9 | `FloatingTextPoolTest.cs` | `ActionResultDisplay` |
| 10 | `CombatSceneManagerTest.cs` | `CombatSceneManager`, `DataManager`, `EntityFactory` |

### Đọc kết quả test

```
✅ Tên test — detail    → PASS
❌ Tên test — detail    → FAIL (cần debug)
[TestName] SKIP — lý do → test bị bỏ qua do dependency chưa có
[TestName] WARN — lý do → test không xác định được kết quả
```

---

## 8. Checklist cuối cùng

Trước khi bàn giao cho Dev B integration:

- [ ] DOTween setup đã chạy (Tools > Demigiant > Setup DOTween)
- [ ] TextMeshPro Essential Resources đã import
- [ ] Canvas hierarchy đã tạo theo mục 3
- [ ] Tất cả SerializeField đã gán (không có reference null trong Inspector)
- [ ] `CombatSceneManager._autoStartOnPlay = true` để test
- [ ] `AudioController` có 2 AudioSource được gán
- [ ] `TimingInputHandler` có thể nhận Space key (không crash khi PlayerInput chưa setup)
- [ ] Chạy `TimingWindowTest` → tất cả ✅
- [ ] Chạy `TimingSystemTest` → không có ❌
- [ ] Chạy `CombatBridgeTest` → mock callbacks hoạt động
- [ ] 0 compile errors (kiểm tra bằng `Ctrl+Shift+B` trong VS / xem Console Unity)

---

## 9. Khi Dev B hoàn thành

Sau khi Dev B xong `CharacterView`, `EnemyView`, `CharacterViewFactory`:

1. **Dev B implement** `CombatBridge.ICharacterAnimatorBridge` trong `CharacterView`/`EnemyView`:

```csharp
public class CharacterView : MonoBehaviour, CombatBridge.ICharacterAnimatorBridge
{
    [SerializeField] private CharacterAnimator _animator;

    public void PlayAttack()  => _animator.PlayAttack();
    public void PlayHurt()    => _animator.PlayHurt();
    public void PlayDeath()   => _animator.PlayDeath();
    public void PlayVictory() => _animator.PlayVictory();
    public Transform GetWorldTransform() => transform;
}
```

2. **Dev B đăng ký** sau khi spawn (gọi từ `CharacterViewFactory` hoặc trong `CombatSceneManager` hook):

```csharp
CombatBridge.Instance.RegisterView(entityId, myView);
CombatUIController.Instance.RegisterEntityPosition(entityId, myView.transform);
```

3. **Dev B gọi** khi telegraph animation kết thúc:

```csharp
CombatBridge.Instance.NotifyTelegraphComplete(enemyId, telegraphDuration);
```

4. `CombatSceneManager.InitializeCombat()` có sẵn `yield return null` tại bước 3 — Dev B spawn views trong frame đó.
