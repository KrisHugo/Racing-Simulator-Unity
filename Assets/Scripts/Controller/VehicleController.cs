using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public EngineSystem engineSystem;
    public SteeringSystem steeringSystem;
    [Header("Vehicle Settings")]
    // 新增字段
    public bool automaticTransmission = true; // 是否自动挡

    // for debugging and visualization
    [Header("Car Status")]
    public bool isEngineOn = true;
    public bool isHandbrakeOn = false;
    public bool isBraking = false;
    public bool isSteering = false;
    // public float CurrentSpeed => engineSystem.VehicleSpeed; // 车速（km/h）
    // public float EngineRPM => engineSystem.CurrentRPM;
    // public GearState CurrentGearState => engineSystem.CurrentGearState; // 当前挡位状态
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on VehicleController.");
        }
    }
    void Start()
    {
        engineSystem ??= GetComponent<EngineSystem>();
        steeringSystem ??= GetComponent<SteeringSystem>();

        // 确保引擎系统和转向系统已正确初始化
        if (engineSystem == null || steeringSystem == null)
        {
            Debug.LogError("EngineSystem or SteeringSystem is not assigned in VehicleController.");
        }
    }

    void Update()
    {
        isEngineOn = true;
        isHandbrakeOn = false;
        isBraking = false;
        isSteering = false;
        // 获取玩家输入: 油门、刹车、转向


        float throttle = InputManager.Instance.ThrottleInput;
        throttle = Mathf.Max(0, throttle); // 确保油门为正

        // 刹车输入分为手刹和常规刹车
        // 手刹和常规刹车可以同时使用
        // 优先级：手刹 > 常规刹车
        float brake = 0;
        if (InputManager.Instance.HandbrakeInput)
        {
            throttle = 0; // 手刹时油门无效
            brake = 1; // 手刹时刹车为最大
            isHandbrakeOn = true; // 手刹开启
        }
        else if (InputManager.Instance.BrakeInput > 0)
        {
            isHandbrakeOn = false;
            brake = InputManager.Instance.BrakeInput; // 常规刹车
            isBraking = true; // 常规刹车开启
        }

        // 转向输入

        float steering = InputManager.Instance.SteerInput;

        // 设置输入
        engineSystem.SetInput(throttle, brake);
        steeringSystem.SetSteeringInput(steering);

        // 挡位控制
        if (!automaticTransmission)
        {
            // 手动换挡控制
            if (InputManager.Instance.ShiftUpPressed) // E键升挡
            {
                engineSystem.ShiftGear(1);
            }
            else if (InputManager.Instance.ShiftDownPressed) // Q键降挡
            {
                engineSystem.ShiftGear(-1);
            }
        }

    }


    public void Respawn(Transform respawnPoint)
    {
        engineSystem.SetInput(0, 0); // 重置输入
        steeringSystem.SetSteeringInput(0); // 重置转向输入

        transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
    }

    // 调试信息
    void OnGUI()
    {
        if (engineSystem != null && steeringSystem != null)
        {
            string info = $"Throttle: {engineSystem.currentThrottle:F2}\n";
            info += $"Brake: {engineSystem.currentBrake:F2}\n";
            info += $"Steering: {steeringSystem.currentSteeringInput:F2}\n";
            info += $"Gear: {engineSystem.CurrentGear} | RPM: {engineSystem.CurrentRPM:F0}\n";
            info += $"Speed: {engineSystem.VehicleSpeed:F1} km/h \n";
            info += $"Engine On: {isEngineOn}\n";
            GUI.Label(new Rect(10, 10, 200, 120), info);
        }
    }
}