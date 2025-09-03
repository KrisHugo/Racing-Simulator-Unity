using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WheelController))]
public class EngineSystem : MonoBehaviour
{
    // 新增字段
    public float CurrentRPM { get; private set; } // 当前转速（RPM）;
    public float VehicleSpeed { get; private set; } // km/h
    public GearState CurrentGearState
    {
        get
        {
            if (transmission.currentGear == 0) return GearState.Neutral;
            else if (transmission.currentGear < 0) return GearState.Reverse;
            else return GearState.Drive;
        }
    }


    [Header("差速器设置")]
    public float differentialRatio = 3.7f; // 差速比
    [Range(0.1f, 0.4f)] public float slipThreshold = 0.3f;
    [Header("引擎特性")]
    public float maxTorque = 2000f;
    public float idleRPM = 800f;
    public float maxRPM = 7000f;
    public float brakeForce = 3000f;
    // 优化后的扭矩曲线
    public AnimationCurve torqueCurve = new AnimationCurve(
        new Keyframe(0, 0.2f),    // 低转速扭矩
        new Keyframe(0.3f, 0.9f),  // 提高爬坡能力
        new Keyframe(0.6f, 1.0f),  // 峰值扭矩
        new Keyframe(0.85f, 0.8f), // 高转区保持
        new Keyframe(1.0f, 0.5f)   // 红区下降
    );

    [Header("驱动模式")]
    public DriveType driveType = DriveType.RearWheelDrive;

    public enum DriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }
    [Header("重量分布")]
    public float frontWeightDistribution = 0.5f; // 前后重量分布比例（0-1）
    public float rearWeightDistribution => frontWeightDistribution -  1f; // 后部重量分布比例

    [Header("必备组件")]
    private Rigidbody rb;
    public TransmissionSystem transmission;
    private WheelController wheelController;
    [Header("调试信息")]
    public float currentThrottle = 0f;
    public float currentBrake = 0f;

    void Start()
    {
        // 计算车速（公里/小时）
        rb = GetComponent<Rigidbody>();
        wheelController = GetComponent<WheelController>();
        transmission = GetComponent<TransmissionSystem>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on EngineSystem.");
        }
        if (wheelController == null)
        {
            Debug.LogError("WheelController component is missing on EngineSystem.");
        }
        //重心分布设置

        rb.centerOfMass = new Vector3(0, -0.5f, rearWeightDistribution);
    }

    public void SetInput(float throttle, float brake, bool handbrake)
    {
        currentThrottle = Mathf.Clamp01(throttle);
        currentBrake = Mathf.Clamp01(brake);
        if (handbrake)
        {
            currentBrake = 1f; // 手刹时强制刹车
        }
    }


    // 计算车速和RPM
    void StateUpdate()
    {
        // 计算车速（公里/小时）
        VehicleSpeed = rb.velocity.magnitude * 3.6f; // m/s to km/h
        float avgWheelRPM = 0f;
        foreach (var wheel in wheelController.wheels)
        {
            if (wheel.isDriveWheel)
            {
                avgWheelRPM += Mathf.Abs(wheel.collider.rpm);
            }
        }
        avgWheelRPM /= wheelController.DriveWheelCount;
        // 确保RPM在合理范围内
        CurrentRPM = Mathf.Clamp(transmission.CalculateEngineRPM(avgWheelRPM), idleRPM, maxRPM);
    }

    void FixedUpdate()
    {
        StateUpdate();
        ApplyTorque();
        ApplyBrakes();
    }

    void ApplyTorque()
    {

        // 根据挡位调整扭矩输出
        foreach (var wheel in wheelController.wheels)
        {
            if (!wheel.isDriveWheel) continue;

            float slip = wheelController.GetWheelSlip(wheel).slipRatio;

            // 计算轮上可用扭矩（考虑挡位）
            float rpmNormalized = Mathf.InverseLerp(idleRPM, maxRPM, CurrentRPM);
            float torqueFactor = torqueCurve.Evaluate(rpmNormalized);
            float torque = transmission.GetWheelTorque(maxTorque * torqueFactor * currentThrottle);

            // 根据驱动模式分配扭矩
            if (slip > slipThreshold)
            {
                // 如果打滑超过限制，减少扭矩
                torque *= Mathf.Clamp01(1 - slip);
            }
            switch (driveType)
            {
                case DriveType.AllWheelDrive:
                    wheel.collider.motorTorque = torque / 2;
                    break;

                case DriveType.FrontWheelDrive:
                    if (wheel.isDriveWheel)
                        wheel.collider.motorTorque = torque;
                    break;

                case DriveType.RearWheelDrive:
                    if (wheel.isDriveWheel)
                        wheel.collider.motorTorque = torque;
                    break;
            }

        }
    }

    void ApplyBrakes()
    {
        foreach (var wheel in wheelController.wheels)
        {
            wheel.collider.brakeTorque = currentBrake * brakeForce;
        }
    }



    // 新增：自动换挡功能
    public void AutoShiftGears()
    {
        // 检测换挡冷却时间
        if (transmission.IsShifting) return;

        // 基于RPM的自动换挡逻辑
        if (CurrentRPM > maxRPM * 0.85f && transmission.currentGear < transmission.gears.Length && VehicleSpeed >= transmission.gears[transmission.currentGear - 1].maxSpeed * 0.9f) // RPM过高需要升挡
        {
            transmission.ShiftGear(1);
        }
        else if (CurrentRPM < maxRPM * 0.5f && transmission.currentGear > 1 && VehicleSpeed < transmission.gears[transmission.currentGear - 1].minSpeed * 1.05f) // RPM过低需要降挡
        {
            transmission.ShiftGear(-1);
        }
    }
    
    
    // void CalculateDownforce()
    // {
    //     float speed = rb.velocity.magnitude;
    //     // 增加后轮气动下压力
    //     float downforce = 0.1f * speed * speed; // 平方速度关系
    //     rb.AddForceAtPosition(-transform.up * downforce * 0.6f, 
    //                         rearAxlePosition); // 60%下压力到后轴
    // }
}