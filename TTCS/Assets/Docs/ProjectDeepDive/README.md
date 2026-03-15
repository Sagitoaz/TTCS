# Bộ Tài Liệu Deep Dive Dự Án TTCS

Bộ tài liệu này dành cho người mới tham gia dự án, với mục tiêu:
- Hiểu toàn bộ cấu trúc mã nguồn.
- Nắm đầy đủ các luồng chạy trong dự án (không chỉ combat flow chính).
- Tra được class nào phụ trách việc gì, gọi hàm nào trong từng luồng.

## Mục lục
1. [01_System_Structure.md](./01_System_Structure.md)
2. [02_Runtime_Flow.md](./02_Runtime_Flow.md)
3. [03_Code_Architecture.md](./03_Code_Architecture.md)
4. [04_Combat_Pipeline.md](./04_Combat_Pipeline.md)
5. [05_Data_Save_EventBus.md](./05_Data_Save_EventBus.md)
6. [06_QA_Viva_Preparation.md](./06_QA_Viva_Preparation.md)
7. [07_Flow_Class_Method_Map.md](./07_Flow_Class_Method_Map.md)
8. [08_Full_Class_Inventory.md](./08_Full_Class_Inventory.md)
9. [09_Method_Index_By_Class.md](./09_Method_Index_By_Class.md)

## Cách đọc khuyến nghị
- Bước 1: Đọc `01` để có bản đồ module toàn dự án.
- Bước 2: Đọc `02` để nắm tất cả luồng runtime (chính + phụ).
- Bước 3: Đọc `07` để trace chính xác class và hàm theo từng luồng.
- Bước 4: Đọc `08` để tra cứu toàn bộ class/interface/enum.
- Bước 5: Đọc `09` để tra cứu nhanh method entry-point theo từng class.
- Bước 6: Đọc `03`, `04`, `05` để hiểu sâu kiến trúc, combat, dữ liệu, event.
- Bước 7: Đọc `06` để chuẩn bị vấn đáp kỹ thuật.

## Chuẩn tài liệu trong thư mục này
- Dùng tiếng Việt có dấu, trình bày theo mục lớn -> mục chi tiết.
- Mỗi luồng phải có chuỗi gọi hàm rõ ràng.
- Mỗi class phải có vai trò rõ, không mô tả chung chung.
- Khi thêm/sửa class, phải cập nhật `07`, `08` và `09`.
