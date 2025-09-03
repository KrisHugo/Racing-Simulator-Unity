using UnityEngine;

public class SteeringSystem : MonoBehaviour
{
    [Header("转向参数")]
    public float maxSteeringAngle = 30f;
    public float steeringSpeed = 2f;
    public float steeringDamping = 0.5f;
    public float stabilityHelper = 0.3f; // 帮助车辆保持稳定
    
    private WheelController wheelController;
    public float currentSteeringInput;
    private float currentSteeringAngle;

    void Start()
    {
        wheelController = GetComponent<WheelController>();
    }

    public void SetSteeringInput(float input)
    {
        currentSteeringInput = Mathf.Clamp(input, -1f, 1f);
    }

    void FixedUpdate()
    {
        // 目标转向角
        float targetAngle = maxSteeringAngle * currentSteeringInput;
        
        // 平滑转向过渡
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetAngle, 
                                         steeringSpeed * Time.fixedDeltaTime);
        
        // 应用转向
        ApplySteering(currentSteeringAngle);
        
        // 稳定辅助
        ApplyStabilityAssist();
    }

    void ApplySteering(float angle)
    {
        foreach (var wheel in wheelController.wheels)
        {
            if (wheel.isSteerWheel)
            {
                wheel.collider.steerAngle = Mathf.Abs(angle) > 0.03f ? angle : 0f; // 小于0.3时不转向
            }
        }
    }

    void ApplyStabilityAssist()
    {
        // 帮助车辆恢复稳定
        Vector3 localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
        Vector3 localAngularVel = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);
        
        // 侧滑补偿
        float slideCompensation = -localVelocity.x * stabilityHelper;
        GetComponent<Rigidbody>().AddForce(transform.right * slideCompensation, ForceMode.Acceleration);
        
        // 旋转补偿
        float spinCompensation = -localAngularVel.y * stabilityHelper;
        GetComponent<Rigidbody>().AddTorque(transform.up * spinCompensation, ForceMode.Acceleration);
    }
}