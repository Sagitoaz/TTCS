# 🧪 Ngày 5 — Test + Polish BattleHUD (Tuần 1 Dev A)

> **Sprint:** Sprint 02 — Visual Combat Layer  
> **Developer:** Developer A  
> **Scene làm việc:** `TestCombat` (đã có từ Sprint 1)  
> **Mục tiêu:** Kết nối CombatUIController vào scene, xác minh HP bars và skill buttons hoạt động đúng

---

## Checklist Ngày 5

- [ ] Tắt CombatTestLoader autostart, thêm CombatSceneManager
- [ ] Tạo Canvas UI hierarchy đầy đủ
- [ ] Tạo prefabs FloatingText, TurnOrderSlot, SkillButton
- [ ] Gán tất cả Inspector references
- [ ] Verify HP bars cập nhật đúng khi nhận damage
- [ ] Verify skill buttons enable/disable theo state

---

## Tổng quan: Tại sao thay CombatTestLoader?

`CombatTestLoader` (Sprint 1) chỉ tạo entities và khởi động battle engine — **không gọi `CombatUIController.Initialize()`**. Vì vậy UI không biết entities nào tồn tại.

`CombatSceneManager` (Sprint 2) làm đúng cả hai: tạo entities **và** wire vào UI. Pipeline:

```
CombatSceneManager.Start()
  → DataManager load stage_01_tutorial
  → EntityFactory.CreateParty() + CreateWave()
  → CombatUIController.Initialize(playerTeam, enemyTeam)   ← điểm mới
  → yield return null (1 frame gap)
  → CombatFlowController.StartBattle()
```

---

## BƯỚC 1 — Mở TestCombat scene và kiểm tra Managers hiện có

1. **File > Open Scene** → `Assets/Scenes/TestCombat.unity` (hoặc `Assets/TestScenes/TestCombat.unity`)

2. Trong **Hierarchy**, kiểm tra các Manager sau phải có sẵn từ Sprint 1:

   | GameObject | Component | Ghi chú |
   |-----------|-----------|---------|
   | `EventBus` | `EventBus.cs` | DontDestroyOnLoad |
   | `DataManager` | `DataManager.cs` | DontDestroyOnLoad, tự load Assets/Data/ |
   | `TurnManager` | `TurnManager.cs` | DontDestroyOnLoad |
   | `SkillManager` | `SkillManager.cs` | DontDestroyOnLoad |
   | `CombatFlowController` | `CombatFlowController.cs` | DontDestroyOnLoad |

   Nếu thiếu bất kỳ Manager nào → tạo **empty GameObject** đặt tên tương ứng và gắn component vào.

---

## BƯỚC 2 — Tắt CombatTestLoader

`CombatTestLoader` đang có `_autoStartOnPlay = true` → nó sẽ gọi `CombatFlowController.StartBattle()` trước khi UI được khởi tạo.

1. Tìm GameObject có `CombatTestLoader` component trong Hierarchy
2. Trong **Inspector**, tìm field **Auto Start On Play** → **bỏ tick** (uncheck)

> **Không xóa CombatTestLoader** — vẫn giữ để so sánh hoặc dùng OnGUI debug panel.

---

## BƯỚC 3 — Thêm CombatSceneManager vào scene

1. Trong Hierarchy, nhấn **Create Empty** → đặt tên `CombatSceneManager`
2. Gán component: **Add Component → `CombatSceneManager`**
3. Cấu hình Inspector:

   | Field | Giá trị | Ghi chú |
   |-------|---------|---------|
   | `Default Stage Id` | `stage_01_tutorial` | Đúng với file `Assets/Data/Stages/stage_01_tutorial.json` |
   | `Default Seed` | `42` | Bất kỳ số nào, dùng cố định để debug |
   | **`Auto Start On Play`** | ✅ **true** | Tự khởi động khi Play |
   | `Default Party Ids` → Size | `2` | |
   | `Default Party Ids` → Element 0 | `char_warrior` | Đúng với `Assets/Data/Characters/char_warrior.json` |
   | `Default Party Ids` → Element 1 | `char_mage` | Đúng với `Assets/Data/Characters/char_mage.json` |

4. Thêm component: **Add Component → `AudioController`**
5. Thêm **2 AudioSource** vào cùng GameObject này:
   - **Add Component → Audio Source** (lần 1) → `Play On Awake = OFF`, `Loop = OFF` → đây là `_sfxSource`
   - **Add Component → Audio Source** (lần 2) → `Play On Awake = OFF`, `Loop = ON` → đây là `_bgmSource`
6. Gán AudioSource vào **AudioController** Inspector:
   - `Sfx Source` → gán AudioSource #1
   - `Bgm Source` → gán AudioSource #2

   > Tất cả clip âm thanh (sfxHit, perfectSFX...) để trống cũng không crash.

---

## BƯỚC 4 — Tạo Canvas hierarchy

### 4.1 Tạo Canvas gốc

1. Hierarchy → **Create > UI > Canvas**
2. Đặt tên `CombatCanvas`
3. Trong Inspector, **Canvas** component:
   - **Render Mode** → `Screen Space - Camera`
   - **Render Camera** → gán **Main Camera**
   - **Sorting Layer** → `UI` (hoặc Default)
4. Thêm **Canvas Scaler** nếu chưa có: Reference Resolution = `1920 x 1080`, UI Scale Mode = `Scale With Screen Size`
5. Tạo **empty child** trên Canvas, đặt tên `CombatUIController`, gán component **`CombatUIController`**

> Mỗi UI sub-panel bên dưới là **child của `CombatCanvas`**, ngang hàng với `CombatUIController`.

---

### 4.2 Tạo BattleHUD

1. Dưới `CombatCanvas`, tạo **Create Empty** → đặt tên `BattleHUD` → gán component **`BattleHUD`**
2. Tạo 2 empty con: `AllySlots` và `EnemySlots`
3. Trong `AllySlots`, tạo 3 child: `AllySlot_0`, `AllySlot_1`, `AllySlot_2`
4. Trong `EnemySlots`, tạo 3 child: `EnemySlot_0`, `EnemySlot_1`, `EnemySlot_2`
5. Với **mỗi slot** (6 slot tổng), thêm các child sau:

   ```
   AllySlot_0 (CanvasGroup component)
   ├── NameText     ← UI > Text - TextMeshPro
   ├── HPText       ← UI > Text - TextMeshPro
   ├── HPSlider     ← UI > Slider  (Min=0, Max=1, Value=1)
   └── MPSlider     ← UI > Slider  (Min=0, Max=1, Value=1)
   ```

6. Gán `CanvasGroup` component lên root của mỗi slot (`AllySlot_0`, không phải child)
7. Quay lại **BattleHUD** Inspector, gán từng slot:

   | Field | Phần tử | Gán component |
   |-------|---------|---------------|
   | `Ally Slots` → Element 0 → `HP Slider` | `AllySlot_0/HPSlider` | Slider |
   | `Ally Slots` → Element 0 → `MP Slider` | `AllySlot_0/MPSlider` | Slider |
   | `Ally Slots` → Element 0 → `Name Text` | `AllySlot_0/NameText` | TextMeshProUGUI |
   | `Ally Slots` → Element 0 → `HP Text` | `AllySlot_0/HPText` | TextMeshProUGUI |
   | `Ally Slots` → Element 0 → `Slot Group` | `AllySlot_0` | CanvasGroup |
   | *(lặp lại cho Element 1, 2 và toàn bộ Enemy Slots)* | | |

---

### 4.3 Tạo SkillButton prefab và SkillButtonPanel

#### Tạo SkillButton prefab

1. Hierarchy → tạo **UI > Button - TextMeshPro** → đặt tên `SkillButton`
2. Gán component **`SkillButton`** vào GameObject này
3. Thêm **CanvasGroup** component vào `SkillButton`
4. Cấu trúc con:
   ```
   SkillButton (Button + SkillButton.cs + CanvasGroup)
   ├── Icon              ← Create > UI > Image  (đặt tên "Icon")
   ├── CooldownOverlay   ← Create > UI > Image  (đặt tên "CooldownOverlay")
   │                        Image Type = Filled | Fill Method = Radial 360 | Fill Origin = Top
   │                        Màu = (0,0,0,0.6) đen mờ | Fill Amount = 0
   ├── ManaCostText      ← TextMeshPro (con mặc định của Button, đổi tên)
   └── CooldownText      ← Create > UI > Text - TextMeshPro (đặt tên "CooldownText")
   ```
5. Gán vào Inspector của **SkillButton**:
   - `Button` → component Button trên root
   - `Icon` → Image "Icon"
   - `Cooldown Overlay` → Image "CooldownOverlay"
   - `Mana Cost Text` → ManaCostText
   - `Cooldown Text` → CooldownText
   - `Canvas Group` → CanvasGroup trên root
6. **Kéo `SkillButton` từ Hierarchy vào `Assets/Prefabs/UI/`** → tạo prefab. Sau đó xóa instance khỏi scene.

#### Tạo SkillButtonPanel

1. Dưới `CombatCanvas` → **Create Empty** → đặt tên `SkillButtonPanel`
2. Gán component **`SkillButtonPanel`** và **`CanvasGroup`**
3. Tạo 4 instance của `SkillButton` prefab làm child: `SkillBtn_0`, `SkillBtn_1`, `SkillBtn_2`, `SkillBtn_3`
4. Gán vào Inspector của **SkillButtonPanel**:
   - `Buttons` → Size = 4, lần lượt gán 4 SkillBtn
   - `Canvas Group` → CanvasGroup trên SkillButtonPanel

---

### 4.4 Tạo TurnOrderSlot prefab và TurnOrderDisplay

#### Tạo TurnOrderSlot prefab

1. Hierarchy → **Create Empty** (dưới Canvas) → đặt tên `TurnOrderSlot`
2. Gán component **`TurnOrderSlot`**
3. Cấu trúc:
   ```
   TurnOrderSlot (TurnOrderSlot.cs)
   ├── Portrait          ← Create > UI > Image
   ├── NameText          ← Create > UI > Text - TextMeshPro
   └── HighlightOutline  ← Create > UI > Image (màu vàng #FFD91A, alpha=0 ban đầu)
   ```
4. Gán Inspector:
   - `Portrait` → Image "Portrait"
   - `Name Text` → TextMeshProUGUI "NameText"
   - `Highlight Outline` → Image "HighlightOutline"
5. **Kéo vào `Assets/Prefabs/UI/`** → tạo prefab. Xóa instance khỏi scene.

#### Tạo TurnOrderDisplay

1. Dưới `CombatCanvas` → **Create Empty** → đặt tên `TurnOrderDisplay`
2. Gán component **`TurnOrderDisplay`**
3. Tạo 1 child empty → đặt tên `SlotContainer`, thêm **Horizontal Layout Group** component
4. Gán Inspector của **TurnOrderDisplay**:
   - `Slot Prefab` → `TurnOrderSlot` prefab
   - `Slot Container` → Transform của `SlotContainer`
   - `Preview Count` → `8`

---

### 4.5 Tạo ActionResultDisplay (FloatingText pool)

#### Tạo FloatingText prefab

1. Hierarchy → **Create Empty** (dưới Canvas) → đặt tên `FloatingText`
2. Gán component **`FloatingText`**
3. Tạo child: **Create > UI > Text - TextMeshPro** → đặt tên `Label`
   - Font Size = 32, Font Style = Bold, Color = White
   - Alignment = Center Middle
4. Gán Inspector **FloatingText**:
   - `Label` → TextMeshProUGUI "Label"
   - `Float Height` → `80`
   - `Duration` → `1`
   - `Critical Scale` → `1.5`
5. **Kéo vào `Assets/Prefabs/UI/`** → tạo prefab. Xóa instance khỏi scene.

#### Tạo ActionResultDisplay

1. Dưới `CombatCanvas` → **Create Empty** → đặt tên `ActionResultDisplay`
2. Gán component **`ActionResultDisplay`**
3. Gán Inspector:
   - `Floating Text Prefab` → `FloatingText` prefab
   - `Pool Size` → `10`
   - `Spawn Offset` → `X=0, Y=1.2, Z=0`

---

### 4.6 Tạo TimingFeedbackUI

1. Dưới `CombatCanvas` → **Create Empty** → đặt tên `TimingFeedbackUI`
2. Gán component **`TimingFeedbackUI`**
3. Cấu trúc:
   ```
   TimingFeedbackUI (TimingFeedbackUI.cs)
   ├── FlashOverlay   ← UI > Image, Stretch full screen, Color=(0,0,0,0), CanvasGroup alpha=0
   └── GradeText      ← UI > Text - TextMeshPro, Font Size=72, Center Middle
   ```
4. Gán Inspector:
   - `Flash Overlay` → CanvasGroup của `FlashOverlay` (thêm CanvasGroup vào FlashOverlay)
   - `Grade Text` → TextMeshProUGUI "GradeText"
   - `Perfect Color` → `R=1, G=0.85, B=0.1, A=0.35` (vàng gold mờ)
   - `Good Color` → `R=0.3, G=0.7, B=1, A=0.25` (xanh mờ)
   - `Miss Color` → `R=0.8, G=0.2, B=0.2, A=0.20` (đỏ mờ)

---

### 4.7 Tạo ResultPanel

1. Dưới `CombatCanvas` → **Create > UI > Panel** → đặt tên `ResultPanel`
2. **SetActive = false** (tắt ban đầu — sẽ hiện khi combat kết thúc)
3. Thêm **CanvasGroup** component vào `ResultPanel`
4. Tạo 2 child:
   - `ResultText` → **UI > Text - TextMeshPro**, Font Size = 60, Center Middle
   - `ReturnButton` → **UI > Button - TextMeshPro**, đặt text con = "Return"

---

### 4.8 Thêm TimingSystem

1. Hierarchy → **Create Empty** → đặt tên `TimingSystem`
2. **Add Component → `TimingSystem`**
3. **Add Component → `TimingInputHandler`**
4. Trong `TimingInputHandler` Inspector:
   - `Use New Input System` → **❌ false** (dùng Space key trực tiếp, chưa cần setup InputAction)

---

## BƯỚC 5 — Gán references vào CombatUIController

Chọn GameObject `CombatUIController` trong Hierarchy. Gán **tất cả** trong Inspector:

| Field | Gán GameObject/Component |
|-------|--------------------------|
| `Battle H U D` | Component `BattleHUD` trên GameObject `BattleHUD` |
| `Skill Button Panel` | Component `SkillButtonPanel` trên `SkillButtonPanel` |
| `Turn Order Display` | Component `TurnOrderDisplay` trên `TurnOrderDisplay` |
| `Action Result Display` | Component `ActionResultDisplay` trên `ActionResultDisplay` |
| `Timing Feedback U I` | Component `TimingFeedbackUI` trên `TimingFeedbackUI` |
| `Result Panel` | GameObject `ResultPanel` |
| `Result Text` | TextMeshProUGUI con `ResultPanel/ResultText` |
| `Result Button` | Button con `ResultPanel/ReturnButton` |

---

## BƯỚC 6 — Kiểm tra Hierarchy cuối cùng trước khi Play

Hierarchy sau khi setup xong:

```
TestCombat (Scene)
│
├── EventBus
├── DataManager
├── TurnManager
├── SkillManager
├── CombatFlowController
│
├── CombatTestLoader         ← ⚠️ Auto Start On Play = FALSE
│
├── CombatSceneManager       ← ✅ Auto Start On Play = TRUE
│   └── [AudioController + 2x AudioSource]
│
├── TimingSystem
│   └── [TimingSystem + TimingInputHandler]
│
└── CombatCanvas (Canvas, CanvasScaler, GraphicRaycaster)
    ├── CombatUIController   ← gắn tất cả refs đã gán
    ├── BattleHUD
    │   ├── AllySlots/       ← AllySlot_0..2 (Slider HP, Slider MP, TMP Name, TMP HP, CanvasGroup)
    │   └── EnemySlots/      ← EnemySlot_0..2
    ├── SkillButtonPanel     ← CanvasGroup (alpha=0 ban đầu)
    │   ├── SkillBtn_0
    │   ├── SkillBtn_1
    │   ├── SkillBtn_2
    │   └── SkillBtn_3
    ├── TurnOrderDisplay
    │   └── SlotContainer    ← HorizontalLayoutGroup
    ├── ActionResultDisplay
    ├── TimingFeedbackUI
    │   ├── FlashOverlay
    │   └── GradeText
    └── ResultPanel          ← SetActive = false
        ├── ResultText
        └── ReturnButton
```

---

## BƯỚC 7 — Nhấn Play và xác minh

### 7.1 Console kỳ vọng khi Play

```
[DataManager] Loading all data...
[DataManager] Load complete — Characters:2 Skills:2 Enemies:2 Stages:1
[CombatSceneManager] Initializing stage 'stage_01_tutorial'...
[CombatSceneManager] 2 players, 2 enemies.
CombatUIController: Initializing UI...
CombatUIController: UI initialized.
[CombatSceneManager] Combat started.
Turn 1 started — Actor: char_mage   (SPD=140, đi trước char_warrior SPD=120)
```

### 7.2 Verify HP bars

Quan sát Hierarchy **trong khi Play**: mở `BattleHUD > AllySlots > AllySlot_0`
- `NameText` phải hiển thị `"Character_Warrior"` (hoặc tên từ nameKey)
- `HPSlider.value` = 1.0 (đầy)
- `MPSlider.value` ≈ 0.8 (80% theo STARTING_MANA_RATIO)

**Test thủ công — nhận damage:** thêm script test tạm hoặc chạy từ Console unityscripting:

```csharp
// Dán vào bất kỳ MonoBehaviour test nào, gọi từ Update()
if (Input.GetKeyDown(KeyCode.H))
{
    EventBus.Instance.Publish(new TTCS.Core.Events.DamageTakenEvent(
        targetId: "char_warrior",
        sourceId: "enemy_goblin",
        damageAmount: 500,
        isCrit: false,
        damageType: "Physical"
    ));
}
```

Nhấn **H** khi đang Play → `char_warrior` HP bar (AllySlot_0) phải:
- ✅ Animate lerp xuống trong 0.4s
- ✅ HPText hiển thị số mới ngay lập tức (ví dụ: `2500/3000`)
- ✅ Không bị nhảy giật (HP warrior = 3000, damage = 500 → HPPercent = 0.833)

**Test Heal:**

```csharp
if (Input.GetKeyDown(KeyCode.Y))
{
    EventBus.Instance.Publish(new TTCS.Core.Events.HealingReceivedEvent(
        targetId: "char_warrior",
        healAmount: 300
    ));
}
```

### 7.3 Verify Skill Buttons

Khi đến lượt `char_warrior` (TurnStartedEvent phát):

- ✅ `SkillButtonPanel` hiện ra (alpha từ 0 → 1)
- ✅ **Chú ý:** chỉ `skill_warrior_slash` có JSON data → button 0 sáng, buttons 1–3 bị ẩn (`SkillButton.Hide()` vì `_skillModels[i] == null`)
  - `skill_warrior_slash`: mana=0, cooldown=0 → button fully enabled, ManaCostText = "—"
  - `skill_warrior_power_strike`, `skill_warrior_guard`, `skill_warrior_taunt`: chưa có JSON → ẩn
- ✅ Click button `skill_warrior_slash` → Console log ghi nhận action
- ✅ Button có bounce scale animation khi click

> **Đây là hành vi đúng.** Muốn test đủ 4 button → tạo thêm 3 file JSON skill còn lại.

Khi đến lượt `char_mage`:
- `skill_mage_fireball`: mana=30, cooldown=2 — button sáng nếu mana ≥ 30
- Sau khi dùng 1 lần: cooldown overlay `fillAmount = 1.0` (2/2), CooldownText = "2"
- Sau 1 lượt: `fillAmount = 0.5` (1/2), CooldownText = "1"

### 7.4 Verify TurnOrderDisplay

- ✅ Hàng icon hiển thị tên entity theo thứ tự lượt (dựa trên SPD)
- ✅ Entity đang đi lượt có gold outline highlight
- ✅ Rebuild mỗi khi TimelineUpdatedEvent phát

### 7.5 Verify khi entity chết

Publish `EntityDeathEvent` cho `char_warrior`:
- ✅ HPSlider animate về 0
- ✅ HPText hiển thị `0/3000`
- ✅ Slot fade alpha về 0.4 (trong 0.5s, sau delay 0.2s)

### 7.6 Verify Result Screen

```csharp
if (Input.GetKeyDown(KeyCode.V))
    EventBus.Instance.Publish(new TTCS.Core.Events.CombatEndedEvent(victory: true));
```

- ✅ `ResultPanel` hiện ra với fade in 0.5s
- ✅ Text = "VICTORY!"
- ✅ Click ReturnButton → reload scene (TestCombat tải lại)

---

## BƯỚC 8 — Xử lý lỗi thường gặp

| Triệu chứng | Nguyên nhân | Cách fix |
|------------|------------|---------|
| `NullReferenceException` ở `CombatUIController.Initialize` | Một sub-panel field chưa gán | Kiểm tra từng field trong Inspector CombatUIController |
| `[DataManager] Load complete — Characters:0` | Không tìm thấy JSON | Kiểm tra `Assets/Data/Characters/` có 2 file `.json` |
| `CombatSceneManager: Party rỗng!` | `char_warrior` không load được | Kiểm tra `Default Party Ids` đúng với ID trong JSON (không có khoảng trắng) |
| HP bar không animate | `InitializeSlots()` chưa được gọi | Đảm bảo `CombatSceneManager._autoStartOnPlay = true` và `CombatTestLoader._autoStartOnPlay = false` |
| Tất cả skill buttons bị ẩn | Skill JSON không tồn tại | Chỉ có `skill_warrior_slash.json` và `skill_mage_fireball.json` — hành vi đúng |
| SkillButtonPanel không hiện | `_currentEntityId` không khớp | Kiểm tra `CombatUIController.Initialize()` chạy trước `StartBattle()` |
| FloatingText không hiện | `RegisterEntityPosition()` chưa gọi | Dev B chưa làm CharacterView; tạm thời: `CombatUIController.Instance.RegisterEntityPosition("char_warrior", someTransform)` |
| DOTween error khi Play | DOTween chưa setup | **Tools > Demigiant > DOTween Utility Panel → Setup DOTween** |

---

## BƯỚC 9 — Lưu scene và Polish checklist

1. **File > Save** (Ctrl+S) để lưu scene
2. Đánh dấu các polish item:

- [ ] HP bar lerp 0.4s, Ease.OutCubic — không giật
- [ ] HPText cập nhật ngay khi `AnimateHP()` gọi (không đợi tween)
- [ ] Slot entity chết: HP về 0, text `0/MaxHP`, fade alpha 0.4
- [ ] SkillButtonPanel ẩn khi không phải lượt player (`CanvasGroup.blocksRaycasts = false`)
- [ ] Skill cooldown overlay `fillAmount` giảm đúng theo lượt (BUG-4 đã fix)
- [ ] ResultPanel fade in 0.5s, ReturnButton reload scene (MISSING-5 đã fix)
- [ ] Không có `NullReferenceException` trong Console
- [ ] Không có DOTween warning

---

## Sau Ngày 5

- **Ngày 6–7:** `CombatBridge.cs` — subscribe events để gọi `ICharacterAnimatorBridge` (Dev B)
- Khi Dev B hoàn thành `CharacterView`: gọi `CombatUIController.Instance.RegisterEntityPosition(id, transform)` → FloatingText sẽ hiện đúng vị trí
- Xem `DevA_Setup_Guide.md` mục 9 để biết API Dev B cần gọi
