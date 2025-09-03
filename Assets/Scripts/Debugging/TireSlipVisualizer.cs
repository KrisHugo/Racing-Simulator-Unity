using UnityEngine;

public class TireSlipVisualizer : MonoBehaviour
{
    public WheelController slipCalculator;
    public float vectorScale = 1.0f; // 向量缩放比例
    public float maxVectorLength = 2.0f; // 最大向量长度
    public bool showSlipVectors = true;
    public bool showSlipCircles = true;
    public bool showSlipText = true;
    public bool showTireMarks = true;
    
    [Header("调试设置")]
    public Material slipMaterial;
    public float circleRadius = 0.3f;
    public float textOffset = 0.8f;
    public float markLength = 1.0f;
    public float markWidth = 0.1f;

    void Start()
    {
        slipCalculator = GetComponent<WheelController>();
    }

    void OnDrawGizmos()
    {
        if (slipCalculator == null || slipCalculator.wheels == null) 
            return;
        
        foreach (var data in slipCalculator.wheels)
        {
            if (data.collider == null) continue;
            
            // 获取车轮位置
            data.collider.GetWorldPose(out Vector3 wheelPos, out Quaternion wheelRot);
            data.collider.GetGroundHit(out WheelHit hit);
            Vector3 actualDirection = hit.forwardDir;
            if (showSlipVectors)
            {
                DrawSlipVector(wheelPos, actualDirection, data.slipData);
            }
            
            if (showSlipCircles)
            {
                DrawSlipCircle(wheelPos, data.slipData);
            }
            
            if (showSlipText)
            {
                DrawSlipText(wheelPos, data.slipData);
            }
            
            if (showTireMarks && Application.isPlaying)
            {
                DrawTireMarks(wheelPos, wheelRot, data.slipData);
            }
        }
    }
    
    void DrawSlipVector(Vector3 position, Vector3 actualDirection, WheelSlipData data)
    {
        if (data.slipMagnitude > 0.01f)
        {
            // 计算向量方向和长度
            Vector3 direction = data.slipDirection.normalized;
            float length = Mathf.Min(data.slipMagnitude * vectorScale, maxVectorLength);
            Vector3 endPoint = position + direction * length;
            
            // 绘制滑移向量
            Gizmos.color = data.slipColor;
            Gizmos.DrawLine(position, endPoint);
            Gizmos.DrawSphere(endPoint, 0.05f);
            
            // 绘制参考线（理想方向）
            Gizmos.color = Color.blue;
            Vector3 wheelForward = actualDirection;
            Gizmos.DrawLine(position, position + wheelForward * 0.5f);
        }
    }
    
    void DrawSlipCircle(Vector3 position, WheelSlipData data)
    {
        // 创建圆环表示滑移状态
        Gizmos.color = data.slipColor;
        
        // 绘制圆环
        float radius = circleRadius;
        int segments = 36;
        Vector3 prevPoint = position + Vector3.forward * radius;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 newPoint = position + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
        
        // 绘制滑移方向指示线
        if (data.slipMagnitude > 0.01f)
        {
            Vector3 slipDir = data.slipDirection.normalized;
            Gizmos.DrawLine(position, position + slipDir * radius);
        }
    }
    
    void DrawSlipText(Vector3 position, WheelSlipData data)
    {
        #if UNITY_EDITOR
        // 显示滑移数据文本
        string text = $"Angle: {data.slipAngle:F1}°\nRatio: {data.slipRatio:F2}";
        UnityEditor.Handles.Label(position + Vector3.up * textOffset, text, new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = data.slipColor },
            fontSize = 12
        });
        #endif
    }
    
    void DrawTireMarks(Vector3 position, Quaternion rotation, WheelSlipData data)
    {
        if (data.slipRatio > 0.1f || Mathf.Abs(data.slipAngle) > 5f)
        {
            // 计算滑移方向
            Vector3 slipDirection = data.slipDirection.normalized;
            
            // 创建轮胎痕迹
            Vector3 startPoint = position;
            Vector3 endPoint = position + slipDirection * markLength;
            
            // 使用GL绘制线段
            GL.Begin(GL.QUADS);
            slipMaterial.SetPass(0);
            GL.Color(data.slipColor);
            
            Vector3 perpendicular = Vector3.Cross(slipDirection, Vector3.up).normalized * markWidth;
            
            GL.Vertex(startPoint - perpendicular);
            GL.Vertex(startPoint + perpendicular);
            GL.Vertex(endPoint + perpendicular);
            GL.Vertex(endPoint - perpendicular);
            
            GL.End();
        }
    }
}