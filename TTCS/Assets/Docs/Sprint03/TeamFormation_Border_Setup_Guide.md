# Team Formation UI - Border Setup Guide

## Tổng quan
Phần hiển thị player trong team info picker giờ hỗ trợ border image thay đổi theo rarity. Có ba nơi sử dụng border:
1. **Quick Info Panel** (bên trái picker) - hiển thị thông tin nhanh của nhân vật được chọn
2. **Slot Views** (3 slot team) - hiển thị nhân vật đã chọn trên team panel
3. **Picker Cell** (từng dòng trong danh sách) - hiển thị từng nhân vật trong danh sách picker

---

## Cấu trúc thư mục Border

Tất cả border image phải được lưu trong: `Assets/Resources/UI/Border/`

Các file cầnkhi:
- `ssr_border.png` (hoặc .jpg) - border cho SSR
- `sr_border.png` - border cho SR  
- `r_border.png` - border cho R

---

## Setup trong Unity Inspector

### 1. Quick Info Panel (trong Picker Panel)

Tìm `TeamFormationUIController` trên scene:

**Thêm vào Header "Quick Info (left panel in picker)":**
- Image component: `Quick Info Border Image`
  - Gán vào field `_quickInfoBorderImage`
  - Layout: đặt nó sau `_quickInfoPortrait` (hoặc overlay nó để làm khung)

### 2. Slot Views (Team Panel)

Trên prefab hoặc scene cho mỗi slot (3 slots):

**Thêm vào SlotView struct:**
- Image component cho border (đặt nó xung quanh portrait)
  - Gán vào field `borderImage` của SlotView
  - Tên field: `border_image` hoặc tương tự trong prefab

Cấu trúc hierarchy ví dụ:
```
SlotButton
├── Portrait (Image)
├── BorderImage (Image)  ← thêm vào đây
├── NameText
├── HPSlider
└── ...
```

### 3. Picker Cell View (trong Picker List)

Tìm `TeamFormationPickerCellView` prefab:

**Thêm vào:**
- Image component: `Border Image`
  - Gán vào field `_borderImage` trong script
  - Layout: đặt nó xung quanh portrait hoặc làm khung

Cấu trúc hierarchy ví dụ:
```
PickerCell
├── Portrait (Image)
├── BorderImage (Image)  ← thêm vào đây
├── RarityText
├── NameText
├── LevelText
├── Highlight
└── Button
```

---

## Quy tắc tô màu Border

Border images **không** được tô màu theo code vì các ảnh đã được phân loại riêng theo rarity:
- **ssr_border.png** - border cho SSR (đã có màu vàng/gold sẵn trong ảnh)
- **sr_border.png** - border cho SR (đã có màu hồng sẵn trong ảnh)
- **r_border.png** - border cho R (đã có màu xanh cyan sẵn trong ảnh)

Code sẽ tự động load file đúng theo rarity mà không thay đổi màu:
- Controller sẽ load sprite từ `UI/Border/{rarity_thường_chữ}_border`
- Ví dụ: `UI/Border/ssr_border`, `UI/Border/sr_border`, etc.
- Nếu file không tồn tại, border sẽ ẩn (transparent)

---

## Hướng dẫn tạo Border Image

Border image nên:
1. Có kích thước hợp lý (ví dụ 400x400 cho full border frame)
2. Là PNG hoặc JPG với transparency nếu cần
3. Có chiều rộng viền phù hợp (thường 20-50 pixels)
4. Được lưu trực tiếp vào `Assets/Resources/UI/Border/` folder

Sau khi import:
- Unity sẽ tự động nhận diện nó từ `Resources.Load`
- Đảm bảo canvas scale UI phù hợp để border nằm vừa

---

## Testing

Để test border:
1. Mở **TeamFormationScene**
2. Click vào team button để mở picker
3. Chọn nhân vật khác rarity (SSR, SR, R) từ danh sách
4. Xem Quick Info Panel, Slot Views, và Picker Cell cùng cập nhật border color

---

## Ghi chú

- Nếu border không hiển thị, kiểm tra:
  - File border tồn tại ở `Assets/Resources/UI/Border/`
  - Tên file đúng format: `{rarity_thường_chữ}_border.png`
  - Image field được gán đúng trong Inspector
- Border tự động fade out nếu slot rỗng (không có nhân vật)
