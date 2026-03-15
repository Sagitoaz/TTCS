# 06 - Bộ Câu Hỏi Vấn Đáp Kỹ Thuật

## 1. Câu hỏi kiến trúc tổng quan

1. Vì sao combat dùng CTB thay vì luân phiên cứng?
2. Vì sao tách `SkillManager` và `ActionResolver`?
3. Vì sao dùng EventBus thay vì gọi trực tiếp từ combat sang UI?
4. Vì sao cần `CleanupDeadEntities` sau mỗi lượt?

## 2. Câu hỏi trace code theo luồng

1. Từ click skill đến damage hiển thị trên HUD đi qua class nào?
2. Enemy attack có timing/parry đi qua class nào?
3. Một event `DamageTakenEvent` được xử lý bởi các class nào?
4. Khi battle end thì class nào chịu trách nhiệm UI và class nào chịu trách nhiệm audio?

## 3. Câu hỏi mở rộng hệ thống

1. Thêm hiệu ứng Poison mới thì sửa những class nào?
2. Thêm target rule `lowest_mana_ally` thì sửa những class nào?
3. Thêm SFX riêng cho từng skill thì bắt đầu từ class nào?

## 4. Checklist tự đánh giá trước buổi vấn đáp

- [ ] Trình bày được bản đồ module toàn dự án.
- [ ] Trình bày được ít nhất 10 luồng runtime khác nhau.
- [ ] Trace được call chain của player turn và enemy turn.
- [ ] Giải thích được event fan-out sang UI/Visual/Audio.
- [ ] Nắm được vị trí class trong full inventory.
