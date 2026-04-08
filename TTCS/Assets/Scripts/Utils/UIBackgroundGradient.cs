using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/BackgroundGradient")]
[RequireComponent(typeof(Graphic))] // Tự động yêu cầu object phải có Image hoặc Text
public class UIBackgroundGradient : BaseMeshEffect
{
    public enum GradientDirection { Vertical, Horizontal }

    [Header("Gradient Settings")]
    [SerializeField] private GradientDirection direction = GradientDirection.Vertical;
    [SerializeField] private Color color1 = Color.white;
    [SerializeField] private Color color2 = new Color(0.8f, 0.8f, 0.8f, 1f); // Xám nhạt

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int count = vh.currentVertCount;
        if (count == 0) return;

        UIVertex vertex = new UIVertex();
        
        // Tìm tọa độ biên (Min/Max) của UI Element
        vh.PopulateUIVertex(ref vertex, 0);
        float bottomY = vertex.position.y;
        float topY = vertex.position.y;
        float leftX = vertex.position.x;
        float rightX = vertex.position.x;

        for (int i = 1; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float y = vertex.position.y;
            float x = vertex.position.x;

            if (y > topY) topY = y;
            else if (y < bottomY) bottomY = y;

            if (x > rightX) rightX = x;
            else if (x < leftX) leftX = x;
        }

        float height = topY - bottomY;
        float width = rightX - leftX;

        // Bắt đầu tô màu từng đỉnh (Vertex)
        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            Color targetColor = Color.white;

            if (direction == GradientDirection.Vertical)
            {
                // Trộn màu theo chiều dọc (Từ dưới lên trên)
                float normalizedY = (height > 0) ? (vertex.position.y - bottomY) / height : 0;
                targetColor = Color.Lerp(color2, color1, normalizedY);
            }
            else
            {
                // Trộn màu theo chiều ngang (Từ trái sang phải)
                float normalizedX = (width > 0) ? (vertex.position.x - leftX) / width : 0;
                targetColor = Color.Lerp(color1, color2, normalizedX);
            }

            // Nhân màu gradient với màu gốc của Image (nếu có)
            vertex.color *= targetColor;
            vh.SetUIVertex(vertex, i);
        }
    }
}