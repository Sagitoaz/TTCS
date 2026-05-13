# Scene Transition (Iris In/Out) — Unity Editor Setup

Tính năng: khi đổi scene sẽ chạy **Iris In → panel transition (random image + random advice + progress bar) → Iris Out**.

## 1) Cách hoạt động

- `TTCS.Flow.FlowController` là object `DontDestroyOnLoad`.
- Khi gọi các hàm mở scene trong `FlowController` (Main Menu, Inventory, Character Collection, Combat, …) hệ thống sẽ dùng `TTCS.Flow.SceneTransitionController.LoadScene(sceneName)` thay vì `SceneManager.LoadScene`.
- `SceneTransitionController` tự tạo UI overlay runtime (không cần prefab).

## 2) Không cần kéo thả gì trong Inspector

`FlowController` sẽ tự `AddComponent<SceneTransitionController>()` nếu thiếu.

Nếu bạn muốn chủ động gắn sẵn:
- Chọn GameObject có `TTCS.Flow.FlowController`
- Add component: `TTCS.Flow.SceneTransitionController`

## 3) Thả ảnh random (Resources)

Tạo folder (nếu chưa có):

- `Assets/Resources/UI/SceneTransition/Images/`

Bỏ các sprite (PNG/JPG) vào folder trên.

Code sẽ load bằng:
- `Resources.LoadAll<Sprite>("UI/SceneTransition/Images")`

Nếu folder rỗng (không có ảnh), phần image sẽ ẩn (alpha=0) nhưng transition vẫn chạy bình thường.

## 4) Iris shader

Iris effect dùng shader:
- `Assets/Shaders/UI/IrisWipe.shader`

Shader name được tìm bằng `Shader.Find("UI/IrisWipe")`.

Nếu vì lý do nào đó shader không được find, overlay vẫn chạy nhưng sẽ là màn đen (không có hiệu ứng iris).

## 5) Gọi transition từ script khác (tuỳ chọn)

Nếu có script khác muốn đổi scene qua transition:

- `TTCS.Flow.FlowController.Instance.GetComponent<TTCS.Flow.SceneTransitionController>().LoadScene("SomeScene")`

Khuyến nghị: ưu tiên đi qua các hàm của `FlowController` để giữ flow state nhất quán.
