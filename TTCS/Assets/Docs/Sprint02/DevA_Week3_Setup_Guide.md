# 🔧 Tuần 3 — Setup & Integration Guide (Dev A)

> **Sprint:** Sprint 02 — Visual Combat Layer  
> **Developer:** Developer A  
> **Tuần:** Tuần 3 (Ngày 11–15)  
> **Scene làm việc:** `TestCombat` (tiếp từ Tuần 2) + `CombatScene` (khi Dev B sẵn sàng)  
> **Mục tiêu:** Wire TimingSystem vào combat flow, modify ActionResolver, full integration test với Dev B

---

## Tổng quan Tuần 3

Tuần 1–2 đã xây dựng và test độc lập toàn bộ UI + Timing Systems.  
Tuần 3 **kết nối thực sự** — `TimingGrade` ảnh hưởng đến lượng damage player nhận khi guard.

| Hạng mục | Mô tả | Files cần sửa |
|---------|-------|--------------|
| Wire TimingSystem → CombatFlowController | Enemy attack → mở window → nhận grade | `CombatFlowController.cs` |
| Modify ActionResolver | Nhân hệ số damage theo grade | `ActionResolver.cs` |
| Verify AudioController events | SFX timing kết nối | `AudioController.cs` |
| Integration với Dev B | Spawn views, full scene test | Joint task |

---

## Checklist Tuần 3

- [ ] Modify `ActionResolver.Resolve()` — thêm `TimingGrade` parameter
- [ ] Modify `ActionResolver.ResolveAttack()` — áp dụng grade multiplier
- [ ] Modify `CombatFlowController.EnemyTurnRoutine()` — mở window trước khi attack resolve
- [ ] Modify `CombatFlowController.ExecuteAction()` — nhận và pass `TimingGrade`
- [ ] Thêm `using TTCS.UI.Combat;` và `using TTCS.Combat.Timing;` vào CombatFlowController
- [ ] Test: enemy attack → timing window mở → Perfect guard → damage giảm 80%
- [ ] Test: enemy attack → timing window mở → Miss (không không nhấn) → full damage
- [ ] Verify AudioController phát SFX đúng khi timing result
- [ ] Khi Dev B xong: setup `CombatScene.unity` joint test (xem BƯỚC 5)

---

## BƯỚC 1 — Modify ActionResolver.cs

> **File:** `Assets/Scripts/Combat/Actions/ActionResolver.cs`  
> **Namespace:** `TTCS.Combat.Actions`  
> **Sprint 1 file — cần thêm using + parameter**

### 1.1 Thêm using directive

Thêm vào đầu file (sau các using đã có):

```csharp
using TTCS.UI.Combat; // TimingGrade
```

### 1.2 Sửa signature của Resolve()

```csharp
// TRƯỚC:
public static void Resolve(
    CombatEntity       actor,
    List<CombatEntity> targets,
    SkillDataModel     skill)

// SAU:
public static void Resolve(
    CombatEntity       actor,
    List<CombatEntity> targets,
    SkillDataModel     skill,
    TimingGrade        guard = TimingGrade.Miss)
```

### 1.3 Pass grade vào ResolveAttack()

Tìm vòng `foreach` trong `Resolve()`, sửa case `"attack"`:

```csharp
// TRƯỚC:
case "attack":
    ResolveAttack(actor, target, skill, multiplier);
    break;

// SAU:
case "attack":
    ResolveAttack(actor, target, skill, multiplier, guard);
    break;
```

Và trường hợp `default`:

```csharp
// TRƯỚC:
default:
    ResolveAttack(actor, target, skill, multiplier);
    break;

// SAU:
default:
    ResolveAttack(actor, target, skill, multiplier, guard);
    break;
```

### 1.4 Sửa signature + logic của ResolveAttack()

```csharp
// TRƯỚC:
private static void ResolveAttack(
    CombatEntity   actor,
    CombatEntity   target,
    SkillDataModel skill,
    float          multiplier)
{
    var elementEnum = ParseElement(skill.damage?.element);
    var (damage, isCrit) = StatCalculator.CalculateDamage(
        actor, target, multiplier, elementEnum);
    target.TakeDamage(damage, actor.ID);
    Log($"  → Attack: '{actor.ID}' → '{target.ID}' dmg={damage}{(isCrit ? " [CRIT]" : "")}",
        LogCategory.Combat);

// SAU:
private static void ResolveAttack(
    CombatEntity   actor,
    CombatEntity   target,
    SkillDataModel skill,
    float          multiplier,
    TimingGrade    guard = TimingGrade.Miss)
{
    var elementEnum = ParseElement(skill.damage?.element);
    var (damage, isCrit) = StatCalculator.CalculateDamage(
        actor, target, multiplier, elementEnum);

    // Áp dụng guard multiplier lên target là player
    if (target.IsPlayer)
    {
        float guardMultiplier = guard switch
        {
            TimingGrade.Perfect => 0.2f,
            TimingGrade.Good    => 0.6f,
            _                   => 1.0f   // Miss
        };
        damage = Mathf.RoundToInt(damage * guardMultiplier);
        Log($"  → Guard: grade={guard}, multiplier={guardMultiplier:F1}x → final dmg={damage}",
            LogCategory.Combat);
    }

    target.TakeDamage(damage, actor.ID);
    Log($"  → Attack: '{actor.ID}' → '{target.ID}' dmg={damage}{(isCrit ? " [CRIT]" : "")}",
        LogCategory.Combat);
```

> **Lưu ý:** `damage` từ `StatCalculator.CalculateDamage` trả về `int`. Nếu return type là `float`, dùng `Mathf.RoundToInt()`. Kiểm tra signature của `StatCalculator.CalculateDamage` để xác nhận.

---

## BƯỚC 2 — Modify CombatFlowController.cs

> **File:** `Assets/Scripts/Combat/Managers/CombatFlowController.cs`  
> **Namespace:** `TTCS.Combat.Managers`  
> **Sprint 1 file — thêm using + sửa EnemyTurnRoutine + ExecuteAction**

### 2.1 Thêm using directives

Thêm vào đầu file:

```csharp
using TTCS.Combat.Timing;  // TimingSystem, TimingWindow
using TTCS.UI.Combat;      // TimingGrade
```

### 2.2 Thêm Inspector field cho TimingWindow config

Trong vùng `[Header("Turn Timing")]`, thêm sau `_playerTurnTimeout`:

```csharp
[Header("Guard Timing Window")]
[Tooltip("Duration của timing window khi enemy tấn công (giây)")]
[SerializeField] private float _guardWindowDuration = 1.5f;

[Tooltip("Perfect threshold (ms) — input trong khoảng này = Perfect")]
[SerializeField] private float _perfectThresholdMs = 50f;

[Tooltip("Good threshold (ms)")]
[SerializeField] private float _goodThresholdMs    = 150f;
```

### 2.3 Thêm `_pendingTimingGrade` field

Trong vùng private fields (gần `_lastActionCost`):

```csharp
private TimingGrade _pendingTimingGrade = TimingGrade.Miss;
```

### 2.4 Sửa EnemyTurnRoutine() — mở timing window

Tìm `EnemyTurnRoutine()`, sửa đoạn sau khi có `decision.IsValid`:

```csharp
// TRƯỚC:
if (decision.IsValid)
{
    Log($"CombatFlowController: AI '{enemy.ID}' → skill='{decision.skillId}' reason: {decision.reason}",
        LogCategory.Combat);
    yield return ExecuteAction(enemy, decision.skillId, decision.targetIds);
}

// SAU:
if (decision.IsValid)
{
    Log($"CombatFlowController: AI '{enemy.ID}' → skill='{decision.skillId}' reason: {decision.reason}",
        LogCategory.Combat);

    // Mở timing window cho player guard nếu skill là attack
    var skillData = DataManager.Instance?.LoadSkill(decision.skillId);
    bool isAttack = skillData?.type == "attack";

    if (isAttack && TimingSystem.Instance != null)
    {
        _pendingTimingGrade = TimingGrade.Miss; // reset
        bool gradeReceived  = false;

        void OnGrade(TimingGrade grade)
        {
            _pendingTimingGrade = grade;
            gradeReceived       = true;
        }

        TimingSystem.Instance.OnTimingResult += OnGrade;

        var window = new TimingWindow(
            openTime:           Time.time,
            duration:           _guardWindowDuration,
            perfectThresholdMs: _perfectThresholdMs,
            goodThresholdMs:    _goodThresholdMs);

        TimingSystem.Instance.OpenWindow(window);

        // Chờ grade (window tự đóng sau Duration)
        yield return new WaitUntil(() => gradeReceived);

        TimingSystem.Instance.OnTimingResult -= OnGrade;

        Log($"CombatFlowController: Guard grade = {_pendingTimingGrade}", LogCategory.Combat);
    }
    else
    {
        _pendingTimingGrade = TimingGrade.Miss;
    }

    yield return ExecuteAction(enemy, decision.skillId, decision.targetIds, _pendingTimingGrade);
}
```

### 2.5 Sửa ExecuteAction() — thêm TimingGrade parameter

```csharp
// TRƯỚC:
private IEnumerator ExecuteAction(CombatEntity actor, string skillId, List<string> targetIds)

// SAU:
private IEnumerator ExecuteAction(CombatEntity actor, string skillId, List<string> targetIds,
                                  TimingGrade guard = TimingGrade.Miss)
```

Và trong body của `ExecuteAction()`, sửa dòng gọi `action.Execute()`:

```csharp
// TRƯỚC:
action.Execute(actor, targets, SkillManager.Instance);

// SAU:
action.Execute(actor, targets, SkillManager.Instance, guard);
```

### 2.6 Sửa PlayerTurnRoutine() — pass Miss grade (player không bị guard)

Trong `PlayerTurnRoutine()`, sửa dòng gọi `ExecuteAction`:

```csharp
// TRƯỚC:
yield return ExecuteAction(player, _pendingSkillId, _pendingTargetIds);

// SAU:
yield return ExecuteAction(player, _pendingSkillId, _pendingTargetIds, TimingGrade.Miss);
```

> `TimingGrade.Miss` ở đây nghĩa là "không có guard" — enemy không thể guard lại đòn của player (trong thiết kế hiện tại).

---

## BƯỚC 3 — Sửa SkillAction.Execute()

> **File:** `Assets/Scripts/Combat/Actions/SkillAction.cs`

Kiểm tra signature của `Execute()`:

```csharp
// Tìm:
public void Execute(CombatEntity actor, List<CombatEntity> targets, SkillManager skillManager)

// Sửa thành:
public void Execute(CombatEntity actor, List<CombatEntity> targets, SkillManager skillManager,
                    TimingGrade guard = TimingGrade.Miss)
```

Và trong body, tìm dòng gọi `ActionResolver.Resolve()`:

```csharp
// TRƯỚC:
ActionResolver.Resolve(actor, targets, _skillData);

// SAU:
ActionResolver.Resolve(actor, targets, _skillData, guard);
```

---

## BƯỚC 4 — Test trong TestCombat Scene

> **Lưu ý:** Sau khi sửa code, build lại project (0 errors mới test được)

### 4.1 Inspector setup CombatFlowController

Sau khi thêm Inspector fields ở Bước 2.2, trong Unity Inspector chọn `CombatFlowController` GameObject:

| Field | Giá trị đề xuất |
|-------|----------------|
| `Guard Window Duration` | `1.5` |
| `Perfect Threshold Ms` | `50` |
| `Good Threshold Ms` | `150` |

### 4.2 Test flow timing guard thực tế

**Test case 1 — Perfect guard:**
1. Play scene
2. Chờ đến lượt enemy tấn công
3. Nhìn Console: `"TimingSystem: Window opened..."`
4. Ngay lập tức nhấn `Space`
5. Console nên hiện:
   ```
   [Combat] TimingSystem: Input registered at t=X.XXXs.
   [Combat] TimingSystem: Grade=Perfect (offset=XXms).
   [Combat] CombatFlowController: Guard grade = Perfect
   [Combat]   → Guard: grade=Perfect, multiplier=0.2x → final dmg=XX
   ```
6. HP bar giảm ~20% so với normal

**Test case 2 — Miss (không guard):**
1. Play scene
2. Chờ enemy tấn công → window mở
3. **Không nhấn gì** → đợi 1.5 giây
4. Full damage nhận
5. HP bar giảm 100% lượng damage

**Test case 3 — Good guard:**
1. Play scene
2. Chờ enemy tấn công → window mở
3. Nhấn `Space` sau ~0.3 giây
4. Grade = Good → 60% damage

### 4.3 Hotkey reference đầy đủ sau Tuần 3

| Phím | Hành động | Khanh vọng |
|------|-----------|-----------|
| `H` | Damage 500 cho `char_warrior` | HP bar giảm (Tuần 1) |
| `Y` | Heal 300 cho `char_warrior` | HP bar tăng (Tuần 1) |
| `V` | Kết thúc combat | Result panel (Tuần 1) |
| `T` | Mở timing window test | Window mở (Tuần 2) |
| `F` / `G` / `M` | Direct flash test | Perfect/Good/Miss visual (Tuần 2) |
| (auto) | Enemy tấn công → window mở | Guard window từ combat flow (Tuần 3) |

---

## BƯỚC 5 — Integration với Dev B (Khi Dev B hoàn thành)

> **Điều kiện:** Dev B đã tạo `CharacterView.cs`, `EnemyView.cs`, `ActionAnimationController.cs` và `CombatScene.unity`

### 5.1 Đăng ký views với CombatBridge

Dev B cần gọi sau khi instantiate prefabs:

```csharp
// Trong CharacterViewFactory hoặc CombatSceneManager
CombatBridge.Instance.RegisterView("char_warrior", warriorView);
CombatBridge.Instance.RegisterView("char_mage",    mageView);
CombatBridge.Instance.RegisterView("enemy_goblin", goblinView);

// Đăng ký world position cho floating numbers
CombatUIController.Instance.RegisterEntityPosition("char_warrior", warriorTransform);
```

### 5.2 Joint test checklist (Ngày 15)

| # | Test | Mong đợi |
|---|------|---------|
| 1 | Scene load → entities spawn | Character sprites hiện + Idle animation |
| 2 | Player lượt → click skill | Attack animation → damage number nổi |
| 3 | Enemy lượt → tấn công | Telegraph visual → timing window mở |
| 4 | Player guard Perfect | Short/small damage + gold flash |
| 5 | Player guard Miss | Full damage + red flash + screen shake |
| 6 | Entity chết | Death animation → slot fade |
| 7 | Combat end | Victory / Defeat panel |

---

## Vấn đề thường gặp

| Triệu chứng | Nguyên nhân | Giải pháp |
|------------|-------------|-----------|
| Compile error: `TimingGrade` không nhận ra | Thiếu `using TTCS.UI.Combat;` | Thêm using ở đầu file |
| Window không mở khi enemy attack | `TimingSystem.Instance` null | Kiểm tra `TimingSystem` component có trong scene |
| Damage không bị giảm khi Perfect | `guard` parameter không được pass đúng | Kiểm tra chuỗi call: `EnemyTurnRoutine → ExecuteAction → action.Execute → ActionResolver.Resolve` |
| `WaitUntil` bị treo | `gradeReceived` không được set | Kiểm tra `OnTimingResult` event subscription, thêm timeout fallback |
| Ambiguous `TimingGrade` | Có 2 enum cùng tên | Dùng `TTCS.UI.Combat.TimingGrade` tường minh, hoặc remove `TimingInputEvent.TimingGrade` |

### Fallback nếu WaitUntil bị treo

Thêm timeout safety:

```csharp
float waitStart = Time.time;
yield return new WaitUntil(() => gradeReceived || (Time.time - waitStart) > _guardWindowDuration + 0.5f);

if (!gradeReceived)
{
    Log("CombatFlowController: Guard window timeout — defaulting to Miss.", LogCategory.Combat);
    _pendingTimingGrade = TimingGrade.Miss;
    TimingSystem.Instance.OnTimingResult -= OnGrade;
}
```

---

## Cấu trúc thay đổi code tóm tắt

```
ActionResolver.cs (Sprint 1)
  + using TTCS.UI.Combat
  + Resolve(..., TimingGrade guard = Miss)
  + ResolveAttack(..., TimingGrade guard = Miss)
  + guardMultiplier: Perfect=0.2, Good=0.6, Miss=1.0

SkillAction.cs (Sprint 1)
  + Execute(..., TimingGrade guard = Miss)
  + ActionResolver.Resolve(..., guard)

CombatFlowController.cs (Sprint 1)
  + using TTCS.Combat.Timing, TTCS.UI.Combat
  + _pendingTimingGrade field
  + SerializeField: _guardWindowDuration, _perfectThresholdMs, _goodThresholdMs
  + EnemyTurnRoutine: mở window → WaitUntil → pass grade
  + ExecuteAction(..., TimingGrade guard = Miss)
  + PlayerTurnRoutine: pass TimingGrade.Miss
```

---

## Sau khi hoàn thành Tuần 3

Sprint 02 Dev A sẽ **100% hoàn thành** với:
- ✅ 15 files code
- ✅ Timing guard tích hợp vào combat flow
- ✅ Full integration test với Dev B's visual system
- ✅ `CombatScene.unity` chạy được từ đầu đến cuối

Xem `WorkPlan_Sprint02_Progress.md` để cập nhật tiến độ tổng thể.
