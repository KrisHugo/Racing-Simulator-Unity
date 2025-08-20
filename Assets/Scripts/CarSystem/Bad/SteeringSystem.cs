using UnityEngine;

public class SteeringSystem : MonoBehaviour
{
    [Header("转向参数")]
    public float maxSteeringAngle = 30f;
    public float steeringSpeed = 2f;
    public float steeringDamping = 0.5f;
    public float stabilityHelper = 0.3f; // 帮助车辆保持稳定
    // 需要更新车轮组
    // private WheelController wheelController;
    private DriveTrain drivetrain;
    private float currentSteeringInput;
    private float currentSteeringAngle;

    public void Initialize(){
        drivetrain = GetComponent<DriveTrain>();
    }

    public void SetInput(float input)
    {
        currentSteeringInput = Mathf.Clamp(input, -1f, 1f);
    }
    public void UpdateSteering(){
        // 目标转向角
        float targetAngle = maxSteeringAngle * currentSteeringInput;
        
        // 平滑转向过渡
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetAngle, 
                                         steeringSpeed * Time.fixedDeltaTime);
        // 应用转向
        ApplySteering(currentSteeringAngle);
        
        // 稳定辅助
        // ApplyStabilityAssist();

    }

    void ApplySteering(float angle)
    {
        foreach (var axle in drivetrain.axles)
        {
            if (axle.canSteer)
            {
                axle.ApplySteeringAngle(angle);
            }
        }
    }

    // void ApplyStabilityAssist()
    // {
    //     // 帮助车辆恢复稳定
    //     Vector3 localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
    //     Vector3 localAngularVel = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);
        
    //     // 侧滑补偿
    //     float slideCompensation = -localVelocity.x * stabilityHelper;
    //     GetComponent<Rigidbody>().AddForce(transform.right * slideCompensation, ForceMode.Acceleration);
        
    //     // 旋转补偿
    //     float spinCompensation = -localAngularVel.y * stabilityHelper;
    //     GetComponent<Rigidbody>().AddTorque(transform.up * spinCompensation, ForceMode.Acceleration);
    // }
}