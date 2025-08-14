using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
public class VehicleController2 : MonoBehaviour
{

    // Components
    [Header("Core Components")]
    public AxlePhysics drivetrain;
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
        drivetrain = GetComponent<AxlePhysics>();
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        SetInputs(deltaTime);
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

            // // 手动换挡控制
            // if (InputManager.Instance.ShiftUpPressed) // E键升挡
            // {
            //     transmission.ShiftGearSerialized(1);
            // }
            // else if (InputManager.Instance.ShiftDownPressed) // Q键降挡
            // {
            //     transmission.ShiftGearSerialized(-1);
            // }
        }
        else
        {
            respawnTimer -= deltaTime;
            if (respawnTimer <= 0)
            {
                isRespawning = false;
            }
        }

        //TODO in fact, the throttle and brake need to transverse to driveTorque and brakeTorque, likewise, the steering need to transverse to steering angle and give to drivetrain. 

        drivetrain.SetInput(throttle, brake, isHandbrakeOn, steering, deltaTime);

    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        // brakingSystem.UpdateBrakes();
        // steeringSystem.UpdateSteering();

        // engineSystem.UpdateEngine(transmission, deltaTime);
        // transmission.UpdateWheelTorque(deltaTime);
        // drivetrain.UpdateDrivetrain(deltaTime);
    }

    public void Respawn(Transform respawnPoint)
    {
        isRespawning = true;
        respawnTimer = respawnDelay;

        // reset all movement;
        transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
    }
}