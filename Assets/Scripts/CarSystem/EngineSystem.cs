using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WheelController))]
public class EngineSystem : MonoBehaviour
{
    // 新增字段
    public float CurrentRPM { get; private set; } // 当前转速（RPM）;
    public float VehicleSpeed { get; private set; } // km/h
    public int CurrentGear { get { return currentGear; } }
    public GearState CurrentGearState
    {
        get
        {
            if (currentGear == 0) return GearState.Neutral;
            else if (currentGear < 0) return GearState.Reverse;
            else return GearState.Drive;
        }
    }

    [Header("速度与RPM")]
    public float maxSpeed = 200f; // 最高时速（km/h）
    public float[] gearRatios = { 3.8f, 2.3f, 1.5f, 1.1f, 0.9f }; // 齿轮比数组（1-5挡）
    public float reverseRatio = 3.5f; // 倒挡比

    private int currentGear = 1; // 当前挡位
    private float gearShiftDelay = 0.5f; // 换挡延迟
    private float lastShiftTime = -1f; // 上次换挡时间
    [Header("差速器设置")]
    public float differentialRatio = 3.7f; // 差速比
    public float slipLimit = 0.25f; // 允许打滑限制

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
    private Rigidbody rb;
    private WheelController wheelController;
    [Header("调试信息")]
    public float currentThrottle = 0f;
    public float currentBrake = 0f;
    public bool IsShifting { get; private set; } = false; // 是否正在换挡
    // private float totalRPM = 0f;
    void Start()
    {
        // 计算车速（公里/小时）
        rb = GetComponent<Rigidbody>();
        wheelController = GetComponent<WheelController>();
    }

    public void SetInput(float throttle, float brake)
    {
        currentThrottle = Mathf.Clamp01(throttle);
        currentBrake = Mathf.Clamp01(brake);
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
        if (Time.time < lastShiftTime + gearShiftDelay)
        {
            IsShifting = true; // 正在换挡
        }
        else
        {
            IsShifting = false; // 换挡完成
        }
        // 计算引擎RPM
        if (currentGear > 0) // 前进挡
        {
            // RPM = 轮速 * 齿轮比 * 60
            CurrentRPM = Mathf.Abs(avgWheelRPM) * gearRatios[currentGear - 1];
        }
        else // 倒挡
        {
            // 倒挡单独处理
            CurrentRPM = Mathf.Abs(avgWheelRPM) * reverseRatio;
        }

        // 确保RPM在合理范围内
        CurrentRPM = Mathf.Clamp(CurrentRPM, idleRPM, maxRPM);
    }

    void FixedUpdate()
    {
        StateUpdate();
        ApplyTorque();
        ApplyBrakes();
    }

    // 换挡操作
    public void ShiftGear(int direction)
    {
        // 检查换挡冷却时间
        if (Time.time < lastShiftTime + gearShiftDelay) return;

        if (direction > 0 && currentGear < gearRatios.Length) // 升挡
        {
            currentGear++;
            lastShiftTime = Time.time;
        }
        else if (direction < 0 && currentGear > -1) // 降挡
        {
            currentGear--;
            lastShiftTime = Time.time;
        }

        // Debug.Log($"已换挡至 {CurrentGearState}挡");
    }
    void ApplyTorque()
    {

        // 根据挡位调整扭矩输出
        float gearMultiplier = 1.0f;
        if (currentGear > 0)
        {
            // 低挡位提供更高扭矩输出
            gearMultiplier = Mathf.Lerp(1.8f, 0.8f, (float)(currentGear - 1) / (gearRatios.Length - 1));
        }
        else if (currentGear < 0)
        {
            // 倒挡提供较低扭矩输出
            gearMultiplier = -0.5f;
        }
        foreach (var wheel in wheelController.wheels)
        {
            if (!wheel.isDriveWheel) continue;

            float slip = wheelController.GetWheelSlip(wheel);

            // 计算轮上可用扭矩（考虑挡位）
            float rpmNormalized = Mathf.InverseLerp(idleRPM, maxRPM, CurrentRPM);
            float torqueFactor = torqueCurve.Evaluate(rpmNormalized);
            float torque = maxTorque * torqueFactor * currentThrottle * gearMultiplier;

            // 根据驱动模式分配扭矩
            if (slip > slipLimit)
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
    void AutoShiftGears()
    {
        // 检测换挡冷却时间
        if (IsShifting) return;

        // 基于RPM的自动换挡逻辑
        if (CurrentRPM > maxRPM * 0.85f && currentGear < gearRatios.Length) // RPM过高需要升挡
        {
            currentGear++;
            lastShiftTime = Time.time;
        }
        else if (CurrentRPM < maxRPM * 0.5f && currentGear > 1) // RPM过低需要降挡
        {
            currentGear--;
            lastShiftTime = Time.time;
        }
    }

}