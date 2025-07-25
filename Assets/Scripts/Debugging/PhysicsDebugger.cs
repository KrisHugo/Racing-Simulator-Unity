using UnityEngine;

public class PhysicsDebugger : MonoBehaviour
{
    public WheelController wheelController;
    public bool showWheelSlip = true;
    public bool showFrictionValues = true;
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || wheelController == null) return;
        
        foreach (var wheel in wheelController.wheels)
        {
            if (wheel.collider == null) continue;
            
            // 显示车轮位置和方向
            wheel.collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            
            // 车轮半径方向
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + rot * Vector3.up * wheel.collider.radius);
            
            // 显示滑移信息
            if (showWheelSlip)
            {
                wheel.collider.GetGroundHit(out WheelHit hit);
                float slip = wheelController.GetWheelSlip(wheel);
                
                GUIStyle style = new();
                style.normal.textColor = slip > wheelController.slipThreshold ? Color.red : Color.green;
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(pos, $"Slip: {slip:F2}", style);
                #endif
            }
            
            // 显示摩擦值
            if (showFrictionValues)
            {
                GUIStyle style = new();
                style.normal.textColor = Color.yellow;
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(pos + Vector3.up * 0.4f, 
                                         $"F: {wheel.collider.forwardFriction.stiffness:F2}\n" +
                                         $"S: {wheel.collider.sidewaysFriction.stiffness:F2}", style);
                #endif
            }
        }
        
        // 显示速度向量
        Rigidbody rb = GetComponent<Rigidbody>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        
        // 显示重心
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(rb.centerOfMass + transform.position, 0.1f);
    }
}