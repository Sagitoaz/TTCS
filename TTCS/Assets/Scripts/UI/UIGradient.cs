using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient : BaseMeshEffect
{
    [Header("Colors")]
    public Color leftColor = new Color(0.04f, 0.07f, 0.17f);  // Màu tối ở bên trái (Vùng trống)
    public Color rightColor = new Color(0.16f, 0.25f, 0.35f); // Màu sáng ở bên phải (Dưới banner)

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        // Lấy giới hạn trục X của RectTransform
        Rect bounds = GetComponent<RectTransform>().rect;
        float left = bounds.xMin;
        float width = bounds.width;

        UIVertex vertex = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            
            // Tính toán tỷ lệ dựa trên trục X (từ trái sang phải)
            float normalizedX = (vertex.position.x - left) / width;
            
            // Trộn màu: X=0 (trái) lấy leftColor, X=1 (phải) lấy rightColor
            vertex.color = Color.Lerp(leftColor, rightColor, normalizedX);
            
            vh.SetUIVertex(vertex, i);
        }
    }
}