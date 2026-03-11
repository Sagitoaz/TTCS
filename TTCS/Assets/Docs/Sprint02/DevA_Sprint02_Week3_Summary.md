# ✅ Sprint 02 — Week 3 Summary (Developer A)

> **Dự án:** TTCS — Those at The Crossroads of Story  
> **Developer:** Developer A — UI + Timing System  
> **Tuần:** Tuần 3 (Ngày 11–15)  
> **Hoàn thành:** 2026-03-11  
> **Trạng thái:** ✅ COMPLETE (phần độc lập) — ⏳ Chờ merge Dev B cho joint test

---

## 1. Kết quả Tuần 3

Tuần 3 **kết nối thực sự** hệ thống Timing vào combat flow — `TimingGrade` từ input player giờ ảnh hưởng trực tiếp đến lượng damage trong mỗi lượt đánh. Ngoài ra bổ sung thêm tính năng **attack timing** cho player (không có trong kế hoạch ban đầu).

| Hạng mục | Files sửa | Trạng thái |
|---------|-----------|------------|
| Guard timing tích hợp vào combat flow | `CombatFlowController.cs` | ✅ Done + In-scene verified |
| Damage modifier theo grade | `ActionResolver.cs` | ✅ Done + In-scene verified |
| Attack timing window (player tấn công) | `CombatFlowController.cs`, `ActionResolver.cs` | ✅ Done (bonus) |
| Debug: enemy damage logic, stat check | `StatCalculator.cs`, enemy/char JSON | ✅ Verified |
| Integration với Dev B | — | ⏳ Chờ merge |

---

## 2. Thay đổi code đã thực hiện

### 2.1 `ActionResolver.cs` — áp dụng grade multiplier

**Guard khi bị tấn công (target là player):**

| Grade | Multiplier | Ý nghĩa |
|-------|-----------|---------|
| Perfect | ×0.2 | Nhận 20% damage — chặn 80% |
| Good | ×0.6 | Nhận 60% damage — chặn 40% |
| Miss | ×1.0 | Nhận full damage |

**Attack timing khi player tấn công (actor là player):**

| Grade | Multiplier | Ý nghĩa |
|-------|-----------|---------|
| Perfect | ×1.5 | Gây 150% damage |
| Good | ×1.2 | Gây 120% damage |
| Miss | ×1.0 | Gây damage bình thường |

### 2.2 `CombatFlowController.cs` — mở timing window

**Enemy tấn công → player guard:**
- Sau khi AI decision xác định skill là `type == "attack"`
- Mở `TimingWindow` với `_guardWindowDuration`, `_perfectThresholdMs`, `_goodThresholdMs`
- `WaitUntil(() => gradeReceived)` — chờ player nhấn hoặc window tự đóng
- Pass `_pendingTimingGrade` → `ExecuteAction` → `SkillAction.Execute` → `ActionResolver.Resolve`

**Player tấn công → attack timing:**
- Sau khi player submit action (`SubmitPlayerAction`)
- Nếu skill là `type == "attack"` → mở `TimingWindow`
- Chờ player nhấn → `attackGrade` pass vào `ExecuteAction`

---

## 3. Bug đã phát hiện và xử lý

### Bug: Goblin gây damage quá thấp (64 khi guard miss)

**Phát hiện:** Goblin ATK=180, player DEF=180, công thức:
```
defFactor = 180 / (180 + 100) = 0.643  → 64.3% giảm
baseDmg   = 180 * 1.0 = 180
netDamage = 180 * (1 - 0.643) = 64.3  → 64
```

**Kết luận:** Đây là **kết quả đúng** — không phải bug. DEF 180 của warrior giảm hơn 64% damage. Hệ thống tính toán hoạt động theo thiết kế.

**Tham chiếu công thức:**
```
defFactor = DEF / (DEF + 100)  →  max 75%
netDamage = ATK * skillMult * (1 - defFactor) * critBonus
```

---

## 4. Skill data đã tạo

| File | Loại | Mô tả |
|------|------|-------|
| `skill_goblin_strike.json` | attack, single | ATK×1.0, timing window 150–500ms |
| `skill_knight_slash.json` | attack, single | ATK×1.2, armor pen 10%, window 300–700ms |
| `skill_knight_guard.json` | buff, self | +50% DEF, cooldown 3 |
| `skill_knight_rage.json` | attack, all | ATK×1.5 dark, armor pen 20%, cooldown 4, window 500–1000ms |

---

## 5. Còn lại (chờ Dev B)

| Việc | Điều kiện | Mô tả |
|------|-----------|-------|
| Joint test `CombatScene.unity` | Dev B merge | Spawn CharacterView/EnemyView, test full flow |
| Verify telegraph visual → timing sync | Dev B `TelegraphVisual.cs` | `CombatBridge.NotifyTelegraphComplete()` |
| Test floating numbers vị trí đúng | Dev B `RegisterEntityPosition` | `CombatUIController.RegisterEntityPosition()` |
| Turn order hiển thị đúng tên | Dev B entity setup | `TurnOrderDisplay.RegisterEntity()` |

---

## 6. Final timing system flow (Tuần 3 hoàn chỉnh)

```
[Enemy lượt]
AI Decision → skill.type == "attack"
  └── TimingSystem.OpenWindow()
        └── TimingInputHandler detects Space/Guard
              └── TimingSystem.RegisterInput()
                    └── OnTimingResult(grade)
                          └── CombatFlowController._pendingTimingGrade = grade
                                └── ExecuteAction(..., grade)
                                      └── ActionResolver.ResolveAttack()
                                            └── target.IsPlayer → damage * guardMultiplier

[Player lượt]
Player chọn skill (type == "attack")
  └── TimingSystem.OpenWindow()
        └── TimingInputHandler detects Space/Guard
              └── OnTimingResult(grade)
                    └── ExecuteAction(..., attackGrade)
                          └── ActionResolver.ResolveAttack()
                                └── actor.IsPlayer → damage * attackBonus
```
