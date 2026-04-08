using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIPulseEffect : MonoBehaviour
{
    [Header("Cài đặt Nhịp thở (Pulsing)")]
    [Tooltip("Tốc độ nhấp nháy (càng to càng nhanh)")]
    public float pulseSpeed = 2.5f;
    
    [Tooltip("Độ mờ thấp nhất (0.0 đến 1.0)")]
    [Range(0f, 1f)] public float minAlpha = 0.3f;
    
    [Tooltip("Độ sáng cao nhất (0.0 đến 1.0)")]
    [Range(0f, 1f)] public float maxAlpha = 1.0f;

    private Image targetImage;
    private Color baseColor;

    void Start()
    {
        targetImage = GetComponent<Image>();
        baseColor = targetImage.color; // Lưu lại màu gốc bạn đã set trong Inspector
    }

    void Update()
    {
        // Hàm Sin chạy theo thời gian thực sẽ tạo ra đường cong mượt mà từ -1 đến 1
        // Phép tính này chuẩn hóa nó về dải từ 0 đến 1
        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // Tính toán độ mờ (Alpha) hiện tại nằm giữa khoảng Min và Max
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, wave);

        // Áp dụng Alpha mới vào màu của UI
        Color newColor = baseColor;
        newColor.a = currentAlpha;
        targetImage.color = newColor;
    }
}