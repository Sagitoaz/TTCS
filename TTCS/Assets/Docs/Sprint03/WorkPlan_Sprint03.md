# Kế hoạch làm việc Sprint 3 - Hoàn thiện Core Level-based

> **Dự án**: TTCS  
> **Sprint**: Sprint 3 (14 ngày)  
> **Team**: 2 Dev  
> **Mục tiêu sprint**: Hoàn thiện toàn bộ core gameplay theo mô hình level-based để giai đoạn sau chỉ tập trung làm game design/content.

---

## 1) Mục tiêu Sprint 3

### Mục tiêu chính
- Chuyển flow gameplay chính sang: `Tutorial (new player) -> Chapter -> Level -> Combat -> Reward -> Save`.
- Giữ nguyên các cơ chế cũ: nhân vật, gacha, inventory (không phá hành vi đã có).
- Hoàn thiện pipeline dữ liệu để thêm level/chapter/chỉ số nhanh bằng JSON, không cần sửa logic lõi.
- Hoàn thiện **meta-loop ở menu chính**: đội hình, gacha, item, inventory, điều hướng scene.

### Phạm vi bắt buộc hoàn thành
- Scene hướng dẫn riêng cho người chơi mới.
- Chapter/Level Select và unlock theo cụm chapter.
- Reward bridge sang inventory/gacha progression.
- Save/Load mở rộng cho trạng thái tutorial + chapter/level.
- Skill icon pipeline (mapping, bind UI, fallback).
- Baseline balancing đủ ổn định để QA/game design tiếp quản.
- Main Menu flow đầy đủ: Home -> Team -> Gacha -> Inventory -> Level Select.
- Team Formation: chọn/tách đội hình trước khi vào level.
- Gacha flow cơ bản: roll, nhận kết quả, cập nhật inventory/character roster.
- Item & Inventory flow: xem item, dùng item hợp lệ, cập nhật save.
- Character Collection scene: xem toàn bộ nhân vật đã mở khóa/quay được, search theo tên, filter/sort theo rare và level, xem chi tiết nhân vật, feed tăng level.

---

## 2) Định nghĩa “Done” của Sprint 3

Sprint 3 được xem là hoàn thành khi thỏa tất cả điều kiện sau:
- [ ] Người chơi mới vào game chạy qua tutorial scene và được lưu trạng thái đã hoàn thành tutorial.
- [ ] Người chơi quay lại không bị ép chạy lại tutorial, đi thẳng vào flow chapter/level.
- [ ] Chapter/level unlock hoạt động đúng theo điều kiện đã cấu hình.
- [ ] Kết thúc level nhận reward và cập nhật inventory/gacha đúng.
- [ ] Save/Load khôi phục đúng tiến trình chapter/level/tutorial.
- [ ] Skill icon hiển thị đúng, có fallback khi thiếu asset.
- [ ] Có dữ liệu JSON mẫu cho chapter/level/balance để thêm content không cần sửa code.
- [ ] Full loop chạy ổn định trong scene test, không lỗi compile, không crash.
- [ ] Main menu điều hướng đúng giữa các màn hệ thống.
- [ ] Team formation lưu đúng đội hình và dùng đúng khi vào combat.
- [ ] Gacha roll cập nhật đúng roster/item, không mất dữ liệu.
- [ ] Inventory hiển thị và dùng item đúng rule, đồng bộ save/load.
- [ ] Character Collection hiển thị đúng roster đã mở khóa, có search/filter/sort, mở detail card đúng dữ liệu, feed tăng level và hồi full HP/Mana sau khi level up.

---

## 3) Chia việc theo ownership (2 Dev)

## Dev A - Core/Systems Owner
**Trách nhiệm chính**
- Progression service: unlock chapter/level, validate điều kiện mở khóa.
- Save schema migration: lưu tutorial state + chapter/level state.
- Data schema + validator: chapter/level/balance JSON.
- Reward bridge domain logic sang inventory/gacha.
- Team formation domain logic: validate đội hình, serialize lineup.
- Gacha service logic: roll result, rarity table, cập nhật dữ liệu người chơi.
- Inventory domain logic: item use rules, stack/update state.
- Logging + checklist test kỹ thuật cho core flow.

**Không phụ trách chính**
- Setup scene tutorial trực quan.
- UI layout chapter/level và icon rendering cụ thể.

## Dev B - Flow/UI Owner
**Trách nhiệm chính**
- Tutorial scene flow cho người chơi mới.
- Chapter/Level select scene + điều hướng vào combat.
- Skill icon pipeline trong UI (bind + fallback).
- Main menu UI flow và navigation state.
- Team setup UI (chọn đội hình trước trận).
- Gacha UI (roll screen + result panel).
- Inventory UI (list item, filter cơ bản, action dùng item).
- Character Collection UI (grid nhân vật đã mở khóa, search/filter/sort, detail panel, feed level).
- Wiring scene/prefab/UI để chạy full flow.
- Checklist test scene và regression UI flow.

**Không phụ trách chính**
- Thiết kế schema save/progression lõi.
- Rule reward domain và migration dữ liệu.

## Quy tắc phối hợp bắt buộc
- Mỗi dev làm trên feature branch riêng.
- Mọi PR đều review chéo.
- Không chỉnh file ngoài ownership nếu chưa có ticket/sync.
- Các điểm sync bắt buộc: Ngày 3, 7, 10, 13.

---

## 4) Branch Strategy & Merge Gate

## Branch đề xuất
- `feature/devA-core-progression`
- `feature/devA-save-reward-bridge`
- `feature/devB-tutorial-scene`
- `feature/devB-level-select-icon-flow`
- `integration/sprint3-level-core`

## Merge Gate
- [ ] Có checklist test đi kèm PR.
- [ ] Có dữ liệu JSON mẫu nếu PR liên quan data/schema.
- [ ] Không có lỗi compile.
- [ ] Không regress flow cũ (combat/gacha/inventory).
- [ ] Được dev còn lại approve.

---

## 5) Kế hoạch theo ngày (14 ngày)

## Giai đoạn 1 - Thiết kế và khóa interface (Ngày 1-2)

### Ngày 1
**Dev A**
- [ ] Định nghĩa contract progression và save state mới.
- [ ] Draft schema JSON cho chapter/level/unlock.
- [ ] Định nghĩa contract player meta-state (lineup, roster, inventory snapshot).

**Dev B**
- [ ] Draft flow tutorial scene và level select scene.
- [ ] Liệt kê các điểm UI cần icon skill + fallback.
- [ ] Draft luồng Main Menu và wireframe Team/Gacha/Inventory.

**Sync cuối ngày**
- [ ] Chốt interface giữa UI flow và progression service.

### Ngày 2
**Dev A**
- [ ] Cài validator cho schema data mới.
- [ ] Chuẩn bị migration save data.
- [ ] Draft service interfaces: `TeamService`, `GachaService`, `InventoryService`.

**Dev B**
- [ ] Setup khung scene tutorial và level select.
- [ ] Tạo UI state cơ bản (locked/unlocked/completed).
- [ ] Setup khung MainMenu scene và điều hướng đến Team/Gacha/Inventory.

**Sync cuối ngày**
- [ ] Chốt input/output contract để bắt đầu code song song.

## Giai đoạn 2 - Xây core song song (Ngày 3-7)

### Ngày 3
**Dev A**
- [ ] Implement progression service (unlock logic).
- [ ] Unit test logic unlock theo chapter.

**Dev B**
- [ ] Implement tutorial trigger lần đầu.
- [ ] Routing sau tutorial về chapter/level select.

**Sync bắt buộc**
- [ ] Verify tutorial state đọc/ghi qua save contract.

### Ngày 4
**Dev A**
- [ ] Mở rộng save/load: tutorial, chapter progress, level stars.
- [ ] Test migrate save cũ -> save mới.
- [ ] Implement save fields cho lineup/roster/inventory meta-state.

**Dev B**
- [ ] Level select UI bind dữ liệu chapter/level.
- [ ] Trạng thái khóa/mở/đã clear hiển thị đúng.
- [ ] Team Formation UI v1 (chọn nhân vật vào đội trước combat).

### Ngày 5
**Dev A**
- [ ] Reward bridge cập nhật inventory/gacha progression.
- [ ] Log đầy đủ đường đi reward.
- [ ] Implement `GachaService` v1 và rarity table config.

**Dev B**
- [ ] Skill icon mapping vào UI chiến đấu/chọn nhân vật.
- [ ] Fallback icon khi thiếu asset.
- [ ] Gacha UI v1 (nút roll + popup kết quả + cập nhật hiển thị).

### Ngày 6
**Dev A**
- [ ] Baseline balance data cho chapter/level đầu.
- [ ] Chuẩn hóa trường dữ liệu cân bằng nhanh.
- [ ] Implement `InventoryService` v1 và item-use rules cơ bản.

**Dev B**
- [ ] Hook level select -> vào combat với dữ liệu đúng.
- [ ] Hook combat end -> quay lại progression flow.
- [ ] Inventory UI v1 (list item + hành động dùng item cơ bản).
- [ ] Character Collection scene v1 (grid ô vuông avatar + rare badge góc phải trên, search name, sort rare asc/desc, sort level asc/desc, detail panel, feed button).
- [ ] Implement feed level flow cho Character Collection (consume resource, tăng level, hồi full HP/Mana khi level-up).

### Ngày 7
**Cả 2 Dev (Sync lớn)**
- [ ] Integration lần 1: tutorial + level select + combat + reward + save.
- [ ] Lập danh sách lỗi blocker và phân công fix.
- [ ] Integration lần 1 cho meta-loop: menu -> team -> gacha -> inventory -> level select.

## Giai đoạn 3 - Ổn định và hoàn thiện (Ngày 8-12)

### Ngày 8-9
**Dev A**
- [ ] Fix lỗi logic progression/reward/save.
- [ ] Tăng độ bền validator + xử lý dữ liệu lỗi.

**Dev B**
- [ ] Fix lỗi UI flow/tutorial/icon.
- [ ] Hoàn thiện trạng thái hiển thị và scene transitions.

### Ngày 10
**Cả 2 Dev (Sync bắt buộc)**
- [ ] Full flow test từ account mới và account cũ.
- [ ] Regression test với inventory/gacha/combat hiện có.
- [ ] Chốt danh sách bug cuối sprint.
- [ ] Regression test riêng cho lineup, gacha roll, inventory item-use ở menu.

### Ngày 11-12
**Dev A**
- [ ] Hoàn thiện logging/debug checkpoints cho core flow.
- [ ] Rà soát integrity dữ liệu (anti-tamper mức tối thiểu).

**Dev B**
- [ ] Hoàn thiện UX tutorial scene (rõ bước, không gây rối).
- [ ] Hoàn thiện fallback visuals/icon states.
- [ ] Polish Character Collection UX (scroll performance, trạng thái selected card, phản hồi sau feed level-up).

## Giai đoạn 4 - Core freeze (Ngày 13-14)

### Ngày 13
**Cả 2 Dev**
- [ ] Regression pass lần cuối theo checklist.
- [ ] Không thêm tính năng mới (feature freeze).
- [ ] Chỉ fix bug blocker/critical.

### Ngày 14
**Cả 2 Dev**
- [ ] Build xác nhận ổn định.
- [ ] Chốt tài liệu bàn giao cho game design phase.
- [ ] Merge vào nhánh tích hợp và tag mốc core done.

---

## 6) Ma trận phụ thuộc quan trọng

| Hạng mục | Owner | Phụ thuộc | Mốc cần có |
|---|---|---|---|
| Progression unlock logic | Dev A | Data schema | Ngày 3 |
| Tutorial first-time flow | Dev B | Save tutorial state API | Ngày 3 |
| Save schema migration | Dev A | Contract progression | Ngày 4 |
| Chapter/Level UI binding | Dev B | DataManager + schema | Ngày 4 |
| Reward bridge | Dev A | Combat end payload | Ngày 5 |
| Skill icon UI pipeline | Dev B | Skill-icon mapping data | Ngày 5 |
| Team formation flow | Dev B | TeamService contract | Ngày 4-5 |
| Gacha logic + UI | Dev A + Dev B | Rarity config + result panel | Ngày 5-6 |
| Inventory logic + UI | Dev A + Dev B | Item rules + UI action | Ngày 6-7 |
| Character collection flow | Dev B | Roster save data + character stat read/write + feed level logic | Ngày 6-10 |
| Full integration v1 | Both | Tất cả trên | Ngày 7 |

---

## 7) Checklist kiểm thử bắt buộc

## Functional
- [ ] New player: vào tutorial đúng luồng.
- [ ] Returning player: bỏ qua tutorial đúng điều kiện.
- [ ] Unlock chapter/level đúng rule.
- [ ] Reward cập nhật đúng inventory/gacha.
- [ ] Save/Load phục hồi đúng tiến trình.
- [ ] Skill icon và fallback chạy đúng.
- [ ] Main menu điều hướng đúng các màn Team/Gacha/Inventory/Level.
- [ ] Team setup chọn đội hợp lệ và dùng đúng khi vào level.
- [ ] Gacha roll trả kết quả hợp lệ và cập nhật roster/item.
- [ ] Inventory dùng item đúng điều kiện, state cập nhật đúng.
- [ ] Character collection: search theo tên, sort rare/level, mở detail đúng stat/portrait, feed level-up hồi full HP/Mana.

## Technical
- [ ] Không lỗi compile.
- [ ] Không crash trong full loop test.
- [ ] JSON validator bắt được dữ liệu sai định dạng.
- [ ] Không có hardcode secret/key nhạy cảm.
- [ ] Có log đủ để truy lỗi các điểm: tutorial start/end, level enter, combat end, reward apply, save write/read.

---

## 8) Rủi ro và cách giảm rủi ro

| Rủi ro | Mức độ | Cách giảm |
|---|---|---|
| Conflict merge do 2 dev chạm cùng flow | Cao | Ownership cứng + sync cố định + review chéo |
| Sai dữ liệu save sau migration | Cao | Test migrate + backup + verify nhiều case |
| Reward bridge làm vỡ inventory/gacha cũ | Cao | Regression checklist + gate review bắt buộc |
| UI tutorial/level flow không ổn định | Trung bình | Scene test hằng ngày + fix sớm từ ngày 8 |

---

## 9) Ghi chú AIDLC cho Sprint 3

- Tài liệu này là **WorkPlan triển khai** dựa trên requirements + user stories đã duyệt.
- Để bắt đầu code chính thức theo workflow AIDLC, cần hoàn tất tuần tự:
  1. Application Design
  2. Units Generation
  3. Functional Design / NFR Requirements / NFR Design
  4. Code Planning + Code Generation + Build and Test

Nếu team muốn, có thể dùng file này như baseline thực thi ngay, và đồng thời cập nhật tương ứng vào các artifacts của từng phase AIDLC ở `aidlc-docs/`.

---

## 10) Trạng thái theo dõi nhanh

- [x] Plan đã được team xác nhận.
- [x] `Sprint03_Standardization_Agreement.md` đã được 2 dev chốt và ký xác nhận nội bộ.
- [x] Giai đoạn 1 (khóa contract/schema/route) đã hoàn tất qua `Sprint03_Phase1_Kickoff_Package.md`.
- [ ] Ownership/branch đã tạo xong (thực hiện ngay khi bắt đầu code).
- [ ] Sync ngày 3 đã hoàn thành.
- [ ] Integration ngày 7 pass.
- [ ] Integration ngày 10 pass.
- [ ] Core freeze ngày 14 hoàn tất.

---

## 11) Hạng mục làm chung bắt buộc ngay khi bắt đầu

Trước khi code Sprint 3, cả 2 dev phải hoàn thành buổi “thống nhất chuẩn” dựa trên file:
- `Assets/Docs/Sprint03/Sprint03_Standardization_Agreement.md`

Checklist kickoff:
- [x] Chốt contract các service cốt lõi.
- [x] Chốt schema JSON v1 và quy tắc migration.
- [x] Chốt event naming + logging points.
- [x] Chốt PR gate và nhịp sync bắt buộc.
- [x] Chốt các quyết định mở (gacha pity, item-use context, lineup scope, replay tutorial).

---

## 12) Trạng thái thực thi Giai đoạn 1 (đã làm ngay để vào code)

- [x] Đã tạo gói chốt Giai đoạn 1: `Assets/Docs/Sprint03/Sprint03_Phase1_Kickoff_Package.md`
- [x] Đã khóa contract service (Progression/Team/Gacha/Inventory/Flow)
- [x] Đã khóa schema dữ liệu v1 + save meta-state
- [x] Đã khóa route scene/menu
- [x] Đã chốt các quyết định mở để không block code

Kết luận: Team có thể bắt tay triển khai code Sprint 3 ngay theo ownership đã phân công.

---

## 13) Snapshot bàn giao cho Dev còn lại (Hiện tại)

- **Trạng thái sprint**: Ready for implementation.
- **Đã xong**: Requirements, User Stories, Workflow Planning, Standardization, Phase 1 kickoff package.
- **Vào code ngay**:
  - Dev A bắt đầu từ `Progression/Save/Gacha/Inventory services`.
  - Dev B bắt đầu từ `MainMenu/Tutorial/Team/Gacha/Inventory UI flow`.
- **Việc đầu tiên của cả 2 dev**: tạo branch theo mục 4, push khung code ngày 1, và sync cuối ngày để khóa interface thật trên code.
