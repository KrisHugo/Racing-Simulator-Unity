

using UnityEngine;

public class SuspensionSystem : MonoBehaviour{

    // 悬挂系统参数
    [Header("Config")]
    public float suspensionRestDist = 0.5f;  // 悬挂自然长度
    public float springStiffness = 35000f;   // 弹簧刚度
    public float damperStiffness = 5000f;    // 阻尼系数
    public float tireRadius = 0.35f;         // 轮胎半径

    // 悬挂系统物理计算
    public void UpdateSuspension(Rigidbody rb, Wheel wheel)
    {
        Vector3 worldPos = rb.transform.TransformPoint(wheel.localPosition);
        // print(wheel.localPosition);
        // 车轮射线检测
        if (Physics.Raycast(worldPos, -transform.up, out RaycastHit hit, 
            suspensionRestDist + wheel.Radius))
        {
            // Debug.Log("Check");
            wheel.isGrounded = true;
            wheel.compression = 1f - (hit.distance - wheel.Radius) / suspensionRestDist;
            
            // 弹簧力计算
            float springForce = springStiffness * wheel.compression;
            
            // 阻尼力计算
            float velocity = Vector3.Dot(transform.up, rb.GetPointVelocity(worldPos));
            float damperForce = damperStiffness * velocity;
            
            // 应用悬挂力
            Vector3 suspensionForce = transform.up * (springForce - damperForce);
            rb.AddForceAtPosition(suspensionForce, worldPos);
            
            // 更新车轮位置
            wheel.restPosition = hit.point + transform.up * wheel.Radius;
        }
        else
        {
            // Debug.Log("Check2");
            wheel.isGrounded = false;
            wheel.compression = 0f;
            wheel.restPosition = worldPos - transform.up * suspensionRestDist;
        }
    }
    
}