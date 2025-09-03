using UnityEngine;

public class TireFrictionModel : MonoBehaviour
{
    [Header("Pacejka 参数")]
    public float B = 8.0f;  // 刚度因子
    public float C = 1.6f;  // 形状因子
    public float D = 1.0f;  // 峰值摩擦系数
    public float E = 0.97f; // 曲率因子

    [Header("载荷敏感度")]
    [Range(0.1f, 0.5f)] public float loadSensitivity = 0.3f;

    public WheelController wheelController;

    void Start()
    {
        if (!wheelController) wheelController = GetComponent<WheelController>();
    }

    void FixedUpdate()
    {
        foreach (var wheel in wheelController.wheels)
        {
            UpdateTireFriction(wheel.collider);
        }
    }

    void UpdateTireFriction(WheelCollider wheel)
    {
        wheel.GetGroundHit(out WheelHit hit);
        
        // 获取当前载荷（垂直力）
        float verticalLoad = hit.force;
        
        // 计算滑移率
        Vector3 localVel = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
        float wheelSpeed = wheel.rpm * 2 * Mathf.PI * wheel.radius / 60;
        float slipRatio = Mathf.Abs(wheelSpeed - Mathf.Abs(localVel.z)) / Mathf.Max(Mathf.Abs(localVel.z), 0.1f);
        
        // Pacejka公式计算基础摩擦
        float x = B * slipRatio;
        float baseFriction = D * Mathf.Sin(C * Mathf.Atan(x - E * (x - Mathf.Atan(x))));
        
        // 考虑垂直载荷
        float loadEffect = 1 - (loadSensitivity * Mathf.Clamp01(verticalLoad / 5000f));
        float finalFriction = Mathf.Max(0.1f, baseFriction * loadEffect);
        
        // 应用摩擦力
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        
        forwardFriction.stiffness = finalFriction;
        sidewaysFriction.stiffness = finalFriction * 0.8f; // 侧向摩擦稍低
        
        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }
}