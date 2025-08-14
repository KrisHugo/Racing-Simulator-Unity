using UnityEngine;
// 存储每个车轮的打滑状态
[System.Serializable]
public class Wheel
{
    [Header("Wheel Settings")]
    // public WheelCollider collider;
    public Transform visual;
    public WheelSlipData slipData;
    public float tireFriction;
    public float inertia = 0.5f; // kg·m²
    // 运行时状态
    public float AngularVelocity { get; private set; } // rad/s
    public bool isGrounded;
    [SerializeField]
    public float RPM => AngularVelocity * 60 / (2 * Mathf.PI); // rad/s to RPM
    public float Radius = 0.3f;
    public Vector3 localPosition;
    public Vector3 restPosition;

    public float springVelocity;
    public float compression;
    public float steerAngle;

    // private float theta = 1;



    public void Initialize(float mass, Vector3 _localPosition, Vector3 _restPosition)
    {

        inertia = mass * Radius;
        localPosition = _localPosition;
        restPosition = _restPosition;
    }

    public void UpdateWheelVisual(float deltaTime)
    {
        if (visual == null) return;
        
        // 悬挂位置更新
        visual.position = restPosition;
        // 转向角度更新
        visual.transform.localRotation = Quaternion.Euler(
            visual.transform.localEulerAngles.x,
            steerAngle,
            visual.transform.localEulerAngles.z);


        // 轮胎旋转
        float rotation = visual.localEulerAngles.x;
        rotation += AngularVelocity * deltaTime * Mathf.Rad2Deg;
        visual.Rotate(Vector3.right, rotation);
    }

    public void CalculateWheelToRigidbodyForces(Rigidbody rb, bool isDriven)
    {
        if (!isGrounded) return;

        Vector3 wheelWorldPos = rb.transform.TransformPoint(localPosition);
        Vector3 tireForward = Quaternion.Euler(0, steerAngle, 0) * rb.transform.forward;

        // 计算轮胎接触点速度
        Vector3 contactVelocity = rb.GetPointVelocity(wheelWorldPos);
        float forwardSpeed = Vector3.Dot(tireForward, contactVelocity);
        float lateralSpeed = Vector3.Dot(rb.transform.right, contactVelocity);

        // 滑移率计算
        float slipRatio = Mathf.Abs(AngularVelocity * Radius - forwardSpeed) /
                            Mathf.Max(0.1f, Mathf.Abs(forwardSpeed));
        float slipAngle = Mathf.Atan2(lateralSpeed, Mathf.Abs(forwardSpeed));

        // 摩擦系数模型（Pacejka模型简化版）
        float longitudinalFriction = tireFriction * Mathf.Exp(-slipRatio * 4f);
        float lateralFriction = tireFriction * Mathf.Exp(-slipAngle * 10f);

        // return new Vector3(longitudinalFriction, lateralFriction, wheelWorldPos);
        // 应用牵引力
        if (isDriven)
        {
            Vector3 driveForce = tireForward * longitudinalFriction;
            rb.AddForceAtPosition(driveForce, wheelWorldPos);
        }

        // 应用转向力
        Vector3 lateralForce = -rb.transform.right * lateralFriction;
        rb.AddForceAtPosition(lateralForce, wheelWorldPos);
    }

    public void ApplyDriveTorque(float torque, float deltaTime)
    {    
        AngularVelocity += torque / inertia * deltaTime;
    }

    public void ApplyBrakeTorque(float torque)
    {
        // brakeTorque = torque;
    }

    public void ApplySteeringAngle(float angle)
    {
        steerAngle = angle;
    }
    // 计算阻力矩（滚动阻力+制动）
    public float GetRollingResistence()
    {
        // 计算单个驱动轮的传动负载
        // wheelRatio = gearRatios[currentGear - 1] * finalDriveRatio * diffRatio;
        // float wheelInertia = rbMass * Radius * 0.5f;

        return AngularVelocity * inertia;
    }



    // public float GetRollingResistentForce(){
    //     return RollingResistentCoefficient * LoadMass * gravity * Mathf.Cos(theta);
    // }

    // private float CalculateSuspensionForce()
    // {
    //     // 简化悬挂模型
    //     float targetCompression = travel * 0.5f; // 正常负载下的压缩量
    //     float compressionForce = springRate * targetCompression;
    //     return compressionForce;
    // }

    // private float CalculateFrictionForce(Rigidbody rb)
    // {
    //     // 计算滑移率
    //     Vector3 velocity = rb.GetPointVelocity(collider.transform.position);
    //     float longitudinalSpeed = Vector3.Dot(velocity, collider.transform.forward);
    //     SlipRatio = Mathf.Abs(1 - AngularVelocity * radius / (longitudinalSpeed + 0.0001f));

    //     // 基于滑移率选择摩擦系数
    //     float slipFactor = frictionCurve.Evaluate(SlipRatio);
    //     float frictionCoeff = Mathf.Lerp(staticFrictionCoeff, kineticFrictionCoeff, slipFactor);

    //     // 法向力（悬挂压缩量）
    //     float normalForce = suspensionCompression;

    //     return frictionCoeff * normalForce;
    // }

    // public Vector3 WheelSideWayDir(){
    //     collider.GetGroundHit(out WheelHit hit); // 获取地面碰撞数据
    //     return hit.sidewaysDir;
    // }
}

// public class WheelController : MonoBehaviour
// {
//     [Header("Physics Parameters")]
//     public float radius = 0.3f;
//     public float mass = 20f;
//     public float inertia = 0.5f; // kg·m²

//     [Header("Suspension")]
//     public float springRate = 35000f;
//     public float damperRate = 4500f;
//     public float travel = 0.2f;

//     [Header("Friction")]
//     public AnimationCurve frictionCurve;
//     public float staticFrictionCoeff = 0.9f;
//     public float kineticFrictionCoeff = 0.75f;

//     [Header("Params")]
//     public float minSpeed = 0.01f; // 防止除以零的最小速度
//     public float slipSmoothing = 0.1f; // 打滑值平滑系数
//     [SerializeField]
//     public Wheel[] wheels;
//     [Range(0, 1)] public float slipThreshold = 0.3f;
//     //get set
//     public int DriveWheelCount { get; private set; } = 0;
//     [SerializeField]
//     public GroundFrictions groundFrictions;

//     public float WheelRadius {
//         get {
//             if( wheels.Length <= 0) return 0.3f;
//             return wheels[0].collider.radius;
//         }
//     }

//     private Rigidbody rb;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         // 计算驱动轮数量
//         DriveWheelCount = 0;
//         // wheelSlipData = new WheelSlipData[wheels.Length];
//         foreach (var wheel in wheels)
//         {
//             if (wheel.isDriveWheel)
//             {
//                 DriveWheelCount++;
//             }
//             wheel.wheelSlipData = new WheelSlipData();
//         }
//     }

//     // 用于调试的可视化
//     private void FixedUpdate()
//     {
//         foreach(var wheel in wheels)
//         {
//             if (wheel.visual && wheel.collider)
//             {
//                 wheel.collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
//                 wheel.visual.transform.SetPositionAndRotation(position, rotation);
//             }
//             wheel.isGround = wheel.collider.GetGroundHit(out WheelHit hit);

//             CalculateWheelPhysics(wheel.collider, ref wheel.wheelSlipData);
//         }
//     }    

//     void CalculateWheelPhysics(WheelCollider wheel, ref WheelSlipData data)
//     {
//         // 获取车轮世界空间速度
//         Vector3 wheelWorldVel = rb.GetPointVelocity(wheel.transform.position);

//         // 转换为车轮局部空间速度
//         Vector3 wheelLocalVel = wheel.transform.InverseTransformDirection(wheelWorldVel);

//         // float speed = wheel.rpm * Mathf.PI * wheel.radius / 60;

//         float longitudinalVel = wheelLocalVel.z;
//         float lateralVel = wheelLocalVel.x;

//         // 计算车轮圆周速度
//         float wheelSpeed = wheel.rpm * 2 * Mathf.PI * wheel.radius / 60f;

//         // 1. 计算纵向打滑率
//         float denominator = Mathf.Max(Mathf.Abs(longitudinalVel), minSpeed);
//         float rawSlipRatio = (wheelSpeed - longitudinalVel) / denominator;

//         // 2. 计算侧偏角(弧度)
//         float rawSlipAngle = Mathf.Atan2(lateralVel, Mathf.Max(Mathf.Abs(longitudinalVel), minSpeed));

//         // 3. 计算综合打滑率(归一化值)
//         float rawNormalizedSlip = Mathf.Clamp01(Mathf.Sqrt(rawSlipRatio * rawSlipRatio + rawSlipAngle * rawSlipAngle));

//         // 应用平滑处理
//         data.slipRatio = Mathf.Lerp(data.slipRatio, rawSlipRatio, slipSmoothing);
//         data.slipAngle = Mathf.Lerp(data.slipAngle, rawSlipAngle, slipSmoothing);
//         data.normalizedSlip = Mathf.Lerp(data.normalizedSlip, rawNormalizedSlip, slipSmoothing);
//     }

//     // 获取车轮基础数据
//     public float GetWheelSlip(Wheel wheel)
//     {
//         wheel.isGround = wheel.collider.GetGroundHit(out WheelHit hit);
//         return Mathf.Abs(hit.forwardSlip);
//         // return Mathf.Max(Mathf.Abs(hit.forwardSlip), Mathf.Abs(hit.sidewaysSlip));
//         // return wheel.wheelSlipData.slipRatio;
//     }

//     // 物理更新
//     public void UpdateAllWheelsPhysics(float deltaTime)
//     {
//         foreach(var wheel in wheels){

//             // 1. 计算悬挂力
//             wheel.suspensionCompression = CalculateSuspensionForce(wheel);

//             // 2. 计算轮胎摩擦力
//             float frictionForce = CalculateFrictionForce(wheel);

//             // 3. 计算角加速度
//             float angularAcceleration = (driveTorque - brakeTorque - frictionForce * radius) / inertia;

//             // 4. 更新角速度
//             AngularVelocity += angularAcceleration * deltaTime;

//             // 5. 重置扭矩
//             driveTorque = 0;
//             brakeTorque = 0;
//         }
//     }

//     private float CalculateFrictionForce(Wheel wheel)
//     {
//         // 计算滑移率
//         Vector3 velocity = rb.GetPointVelocity(wheel.collider.transform.position);
//         float longitudinalSpeed = Vector3.Dot(velocity, wheel.collider.transform.forward);

//         wheel.wheelSlipData.slipRatio = Mathf.Abs(1 - (wheel.AngularVelocity * radius) / (longitudinalSpeed + 0.0001f));
//         // 基于滑移率选择摩擦系数
//         float slipFactor = frictionCurve.Evaluate(wheel.wheelSlipData.slipRatio);
//         float frictionCoeff = Mathf.Lerp(staticFrictionCoeff, kineticFrictionCoeff, slipFactor);

//         // 法向力（悬挂压缩量）
//         float normalForce = wheel.suspensionCompression;

//         return frictionCoeff * normalForce;
//     }
// }