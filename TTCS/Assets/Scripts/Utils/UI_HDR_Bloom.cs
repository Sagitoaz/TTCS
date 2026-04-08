using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode] // Cho phép hiệu ứng chạy ngay trong lúc bạn đang edit
public class UI_HDR_Bloom : MonoBehaviour
{
    // [ColorUsage(true, true)] là dòng phép thuật bật bảng màu HDR (có dải cường độ Intensity)
    [ColorUsage(showAlpha: true, hdr: true)]
    public Color glowColor = Color.white; 

    private Image img;

    void Update()
    {
        if (img == null) img = GetComponent<Image>();
        
        // Dùng canvasRenderer.SetColor thay vì img.color để vượt qua giới hạn 0-1 của Unity UI
        img.canvasRenderer.SetColor(glowColor);
    }
}