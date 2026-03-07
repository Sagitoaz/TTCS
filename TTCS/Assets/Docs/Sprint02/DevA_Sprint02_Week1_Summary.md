# ✅ Sprint 02 — Week 1 Summary (Developer A)

> **Dự án:** TTCS — Those at The Crossroads of Story  
> **Developer:** Developer A — UI + Timing System  
> **Tuần:** Tuần 1 (Ngày 1–5)  
> **Hoàn thành:** 2026-03-07  
> **Trạng thái:** ✅ COMPLETE — Đã build, review, fix bugs, và test thành công

---

## 1. Kết quả Tuần 1

Tuần 1 xây dựng **toàn bộ UI tĩnh của combat scene** — các thanh HP/MP, skill buttons, hàng đợi lượt, và số nổi thiệt hại.

| Hệ thống | Files | Trạng thái |
|---------|-------|------------|
| Combat UI Controller | `CombatUIController.cs` | ✅ Done |
| Battle HUD (HP/MP bars) | `BattleHUD.cs` | ✅ Done + Bug-fixed |
| Skill Buttons | `SkillButton.cs`, `SkillButtonPanel.cs` | ✅ Done + Bug-fixed |
| Turn Order Display | `TurnOrderDisplay.cs`, `TurnOrderSlot.cs` | ✅ Done |
| Floating Damage Numbers | `ActionResultDisplay.cs`, `FloatingText.cs` | ✅ Done |
| Scene Orchestration | `CombatSceneManager.cs` | ✅ Done + Debug-fixed |
| Audio | `AudioController.cs` | ✅ Done |

**Tổng:** 9 files UI + 1 scene manager + 1 audio = **11 files** (7 file framework + 4 file hỗ trợ)

---

## 2. Files đã tạo

```
Assets/Scripts/UI/Combat/
  CombatUIController.cs    — Root singleton, điều phối toàn bộ UI panels
  BattleHUD.cs             — HP/MP animated bars, 3 ally + 3 enemy slots
  SkillButtonPanel.cs      — 4 skill buttons, auto target resolve
  SkillButton.cs           — Single button: icon, cooldown overlay, mana cost
  TurnOrderDisplay.cs      — Pool 8 TurnOrderSlot, rebuild on TimelineUpdatedEvent
  TurnOrderSlot.cs         — Single slot: avatar + name, gold outline cho current actor
  ActionResultDisplay.cs   — Pool 10 FloatingText, subscribe damage/heal events
  FloatingText.cs          — Poolable damage number, DOTween animate up + fade

Assets/Scripts/Combat/Managers/
  CombatSceneManager.cs    — Scene init coroutine, replaces CombatTestLoader

Assets/Scripts/Audio/
  AudioController.cs        — DontDestroyOnLoad, event-driven SFX + BGM
```

---

## 3. Bugs đã phát hiện và fix

Sau khi build xong, code review phát hiện **4 bugs + 1 missing feature**. Tất cả đã được fix.

### [BUG-1] BattleHUD — HP text hiển thị giá trị sai sau damage

**File:** `BattleHUD.cs` → `HUDSlot.AnimateHP()`  
**Mô tả:** `SetHPText()` đọc `HPSlider.value` hiện tại thay vì dùng `newPercent` được truyền vào. DOTween animate slider không cập nhật ngay lập tức, nên text luôn hiển thị giá trị cũ.  
**Fix:**
```csharp
// Trước (sai):
HPSlider.DOValue(newPercent, 0.4f);
SetHPText(HPSlider.value); // đọc giá trị cũ

// Sau (đúng):
HPSlider.DOValue(newPercent, 0.4f);
SetHPText(newPercent); // dùng giá trị mới được truyền vào
```

---

### [BUG-2] BattleHUD — HP text không cập nhật khi Heal

**File:** `BattleHUD.cs` → `OnHealingReceived()`  
**Mô tả:** Dùng `HPSlider.DOValue()` trực tiếp thay vì `AnimateHP()`, do đó HP text không được cập nhật.  
**Fix:**
```csharp
// Trước (sai):
slot.HPSlider.DOValue(newPercent, 0.4f);

// Sau (đúng):
slot.AnimateHP(newPercent); // cập nhật cả bar + text
```

---

### [BUG-3] BattleHUD — HP bar không về 0 khi entity chết

**File:** `BattleHUD.cs` → `HUDSlot.SetDead()`  
**Mô tả:** `SetDead()` chỉ fade alpha slot mà không animate HP về 0 và cập nhật text.  
**Fix:**
```csharp
// Sau (đúng): animate về 0 trước khi fade
HPSlider.DOValue(0f, 0.3f).SetEase(Ease.OutCubic);
SetHPText(0f);
SlotGroup.DOFade(0.4f, 0.5f).SetDelay(0.2f);
```

---

### [BUG-4] SkillButton — Cooldown overlay không bao giờ được set

**File:** `SkillButton.cs` → `Refresh()`  
**Mô tả:** `_cooldownOverlay.fillAmount` chỉ được ẩn/hiện nhưng không bao giờ tính toán fillAmount theo cooldown còn lại.  
**Fix:** Thêm parameter `maxCooldown` vào `Refresh()`:
```csharp
// Trước: không có maxCooldown, fillAmount không được tính
public void Refresh(string skillId, int remaining, bool hasEnoughMana)

// Sau: truyền maxCooldown, tính fillAmount
public void Refresh(string skillId, int remaining, int maxCooldown, bool hasEnoughMana)
{
    float fill = maxCooldown > 0 ? (float)remaining / maxCooldown : 0f;
    _cooldownOverlay.fillAmount = fill;
}
```

---

### [MISSING-5] CombatUIController — Result button không có listener

**File:** `CombatUIController.cs`  
**Mô tả:** `_resultButton` được gán trong Inspector nhưng không có `onClick` listener → nhấn nút kết thúc không làm gì.  
**Fix:** Thêm `OnResultButtonClicked()` trong `Awake()`:
```csharp
private void Awake()
{
    // ...existing code...
    _resultButton?.onClick.AddListener(OnResultButtonClicked);
}

private void OnResultButtonClicked()
{
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
}
```

---

### [DEBUG-6] CombatSceneManager — Test event publish sai thứ tự

**File:** `CombatSceneManager.cs` → `Update()`  
**Phát hiện khi:** Test phím `H` (damage) không cập nhật HP bar.  
**Nguyên nhân:** Test code publish `DamageTakenEvent` trực tiếp mà không gọi `HealthComponent.TakeDamage()` trước → `_currentHP` chưa thay đổi → `GetEntityHPPercent()` trả về giá trị cũ.  
**Fix:**
```csharp
// Trước (sai): publish event thủ công
EventBus.Instance.Publish(new DamageTakenEvent("char_warrior", "enemy_goblin", 500, ...));

// Sau (đúng): gọi trực tiếp vào HealthComponent
CombatEntity target = _playerTeam.Find(e => e.ID == "char_warrior");
target?.Health.TakeDamage(500, "enemy_goblin");
// HealthComponent.TakeDamage() tự cập nhật _currentHP rồi publish event
```

---

## 4. Test Results — Ngày 5

### Setup Scene (TestCombat)
- CombatSceneManager với `_autoStartOnPlay = true` ✅
- Canvas hierarchy đầy đủ (BattleHUD, SkillButtonPanel, TurnOrderDisplay, ActionResultDisplay) ✅
- AudioController với 2 AudioSource ✅
- Tất cả Inspector references đã gán ✅

### Test Cases Passed

| Test | Phím | Kết quả |
|------|------|---------|
| HP bar giảm khi damage | `H` | HP bar animate giảm + số đỏ nổi + text cập nhật ✅ |
| HP bar tăng khi heal | `Y` | HP bar animate tăng + số xanh nổi + text cập nhật ✅ |
| Result panel khi combat kết thúc | `V` | Result panel hiện + nút reload hoạt động ✅ |
| Entity death | (gọi TakeDamage liên tục) | Slot fade + HP = 0 ✅ |

### Lỗi còn lại sau Tuần 1: **Không có**

---

## 5. Bài học rút ra

1. **Thứ tự update trong event-driven UI:** Phải cập nhật data model trước rồi mới publish event. Nếu publish event trực tiếp (bypass data update), UI sẽ đọc giá trị cũ.

2. **DOTween lazy evaluation:** `Slider.value` không thay đổi ngay khi gọi `DOValue()` — chỉ thay đổi sau mỗi frame. Không đọc `Slider.value` ngay sau khi bắt đầu tween; hãy cache giá trị target.

3. **Cooldown overlay UI pattern:** Khi UI cần hiển thị tỷ lệ (fillAmount), cần truyền cả giá trị hiện tại lẫn giá trị tối đa — không dựa vào context implicit.

4. **Unity event button listeners:** Listeners gán qua code (`onClick.AddListener`) không hiển thị trong Inspector — cần review code để xác nhận, không chỉ nhìn vào Inspector.

---

## 6. Pending cho Tuần 2

| Hạng mục | Lý do chưa làm trong Tuần 1 |
|---------|---------------------------|
| `CombatBridge` integration với animated visuals | Dev B chưa có CharacterView → không test được animation trigger |
| `TimingFeedbackUI` in-scene test | TimingSystem cần setup thêm trong scene |
| SkillButton trigger real combat action | CombatFlowController cần full init với Dev B views |

Xem `DevA_Week2_Setup_Guide.md` để tiếp tục.

---

## 7. Files liên quan

| File | Vị trí |
|------|--------|
| Setup Guide Tuần 1 | `Docs/Sprint02/DevA_Day5_BattleHUD_Test.md` |
| Setup Guide Tuần 2 | `Docs/Sprint02/DevA_Week2_Setup_Guide.md` |
| API Reference đầy đủ | `Docs/Sprint02/Dev_A_Sprint02_Summary.md` |
| Tiến độ Sprint tổng thể | `Docs/Sprint02/WorkPlan_Sprint02_Progress.md` |
