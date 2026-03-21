# 🎮 Sprint 02 — Project Description

> **Tên sprint**: Visual Combat Layer  
> **Mục tiêu**: Chuyển combat logic từ trạng thái "engine-only" sang trải nghiệm người chơi có hình ảnh, âm thanh, timing và phản hồi trực quan.

---

## 1. Bài toán của Sprint 02

Sau Sprint 1, hệ thống combat đã chạy đúng về logic nhưng thiếu lớp trình bày.
Sprint 02 giải quyết khoảng cách này bằng cách bổ sung visual layer để:

- Người chơi nhìn thấy trạng thái trận đấu theo thời gian thực.
- Người chơi tương tác trực tiếp qua nút skill và timing input.
- Hành động combat có animation, âm thanh, và feedback tức thời.

---

## 2. Giá trị người chơi nhận được

- Trải nghiệm combat rõ ràng hơn nhờ HUD + turn order.
- Cảm giác "đánh trúng" tốt hơn nhờ floating damage text + hit VFX + SFX.
- Tăng chiều sâu gameplay bằng timing guard/perfect.
- Dễ hiểu tiến trình combat với telegraph trước khi enemy tấn công.

---

## 3. Phạm vi chức năng Sprint 02

### Bao gồm
- Combat UI hoàn chỉnh cho trận đấu.
- Character/Enemy view và animation hành động cơ bản.
- Timing system với phân loại Perfect/Good/Miss.
- Combat bridge giữa EventBus và visual objects.
- Audio feedback theo combat events.
- Scene-level integration để chạy được end-to-end visual combat.

### Không bao gồm
- Main menu/navigation flow hoàn chỉnh.
- Progression metagame (gacha, nâng cấp roster, inventory).
- Nội dung campaign dài hạn ngoài phạm vi combat test flow.

---

## 4. Kết quả đầu ra mong đợi

Một trận đấu trong scene test có thể:

1. Hiển thị đủ party/enemy + HUD.
2. Cho người chơi chọn skill và thấy phản hồi tức thì.
3. Chạy animation tấn công/nhận đòn/chết.
4. Kích hoạt telegraph + timing window theo enemy action.
5. Hiển thị grade timing và ảnh hưởng đúng vào damage pipeline.

---

## 5. Tiêu chí hoàn thành Sprint 02

- Tất cả module của Dev A và Dev B build được trong cùng codebase.
- Các integration points được wire bằng interface/bridge thay vì coupling cứng.
- Chạy qua checklist test Sprint02 không phát sinh blocker mới.
- Tài liệu Sprint02 đầy đủ tương đương mức mô tả của Sprint01.
