using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Core Components")]
    public OldEngineSystem engineSystem;
    public TransmissionSystem transmission;
    public SteeringSystem steeringSystem;
    public BrakingSystem brakingSystem;
    public DriveTrain drivetrain;

    public CarSoundSystem soundSystem;
    // public SuspensionSystem suspension;
    private Rigidbody rb;


    [Header("Basic Car Data")]
    public float mass = 1500f;
    public Vector3 centerOfMass = new Vector3(0, -0.5f, 0);
    // 新增字段
    // public float VehicleSpeed { get; private set; } // km/h

    // for debugging and visualization
    [Header("Car Status")]
    private float throttle = 0;
    private float brake = 0;
    private float steering = 0;
    public bool isEngineOn = true;
    public bool isHandbrakeOn = false;
    public bool isBraking = false;
    public bool isSteering = false;

    private readonly float respawnDelay = 2f;
    private float respawnTimer = 0f;
    private bool isRespawning = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.mass = mass;
        rb.centerOfMass = centerOfMass;

        engineSystem = GetComponent<OldEngineSystem>();
        engineSystem.Initialize();
        transmission = GetComponent<TransmissionSystem>();
        transmission.Initialize();
        steeringSystem = GetComponent<SteeringSystem>();
        steeringSystem.Initialize();
        brakingSystem = GetComponent<BrakingSystem>();
        brakingSystem.Initialize();
        drivetrain = GetComponent<DriveTrain>();
        drivetrain.Initialize();
        soundSystem = GetComponent<CarSoundSystem>();
        soundSystem.Initialize(engineSystem.feature.idleRPM, engineSystem.feature.maxRPM);
        // suspension = GetComponent<SuspensionSystem>();


    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        SetInputs(deltaTime);
        transmission.HandleGearShifting(Time.deltaTime);
        soundSystem.UpdateEngineSound(isEngineOn, engineSystem.CurrentRPM);
    }

    void SetInputs(float deltaTime){

        // VehicleSpeed = rb.velocity.magnitude * 3.6f; // m/s to km/h

        isEngineOn = true;
        isHandbrakeOn = false;
        isBraking = false;
        isSteering = false;

        throttle = 0;
        brake = 0;
        steering = 0;
        if (!isRespawning)
        {
            // 获取玩家输入: 油门、刹车、转向
            throttle = InputManager.Instance.ThrottleInput;
            throttle = Mathf.Max(0, throttle); // 确保油门为正

            // 刹车输入分为手刹和常规刹车
            // 手刹和常规刹车可以同时使用
            // 优先级：手刹 > 常规刹车
            brake = 0;
            if (InputManager.Instance.HandbrakeInput)
            {
                brake = 0;
                isHandbrakeOn = true; // 手刹开启
            }
            else if (InputManager.Instance.BrakeInput > 0)
            {
                brake = InputManager.Instance.BrakeInput; // 常规刹车
                isBraking = true; // 常规刹车开启
            }

            // 转向输入
            steering = InputManager.Instance.SteerInput;

            // 手动换挡控制
            if (InputManager.Instance.ShiftUpPressed) // E键升挡
            {
                transmission.ShiftGearSerialized(1);
            }
            else if (InputManager.Instance.ShiftDownPressed) // Q键降挡
            {
                transmission.ShiftGearSerialized(-1);
            }
        }
        else
        {
            respawnTimer -= deltaTime;
            if (respawnTimer <= 0)
            {
                isRespawning = false;
            }
        }

        // 设置输入
        engineSystem.SetInput(throttle, brake);
        steeringSystem.SetInput(steering);
        brakingSystem.SetInput(brake, isHandbrakeOn);
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        brakingSystem.UpdateBrakes();
        steeringSystem.UpdateSteering();

        engineSystem.UpdateEngine(transmission, deltaTime);
        transmission.UpdateWheelTorque(deltaTime);
        drivetrain.UpdateDrivetrain(deltaTime);
    }

    public void Respawn(Transform respawnPoint)
    {
        isRespawning = true;
        respawnTimer = respawnDelay;

        // reset all movement;
        rb.linearVelocity = Vector3.zero;
        transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
    }

    public float GetSpeedKMH()
    {
        return rb.linearVelocity.magnitude * 3.6f;
    }
    // 轮胎力应用（牵引力+转向力）
    void ApplyDragForces()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        
        // 空气阻力（速度平方关系）
        float dragForce = localVelocity.z * Mathf.Abs(localVelocity.z) * 0.3f;
        rb.AddForce(transform.forward * -dragForce);
        
        // 车轮滚动阻力
        foreach (var axle in drivetrain.axles)
        {
            if (axle.isDriven){
                rb.AddForceAtPosition(
                    2f * axle.leftWheel.compression * -transform.forward, 
                    transform.TransformPoint(axle.leftWheel.localPosition));
                rb.AddForceAtPosition(
                    2f * axle.rightWheel.compression * -transform.forward, 
                    transform.TransformPoint(axle.rightWheel.localPosition));
            }
        }
    }

    // 调试信息
    void OnGUI()
    {
        if (engineSystem != null && steeringSystem != null)
        {
            string info = $"Engine On: {isEngineOn}\n";
            info += $"Throttle: {throttle:F2}\n";
            info += $"Brake: {brake:F2}\n";
            info += $"Steering: {steering:F2}\n";
            info += $"Speed: {GetSpeedKMH():F1} km/h | Torque: {engineSystem.CalculateEngineTorque():F0}\n";
            info += $"Gear: {transmission.CurrentGear} | RPM: {engineSystem.CurrentRPM:F0}\n";
            GUI.Label(new Rect(10, 10, 200, 120), info);
        }
    }
}