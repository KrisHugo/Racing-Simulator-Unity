using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class TyreType
{
    #region friction

    public float peakSlip = 0.15f;      // 峰值摩擦出现的滑移率位置
    public float minForwardFrictionCoeff = 0.5f;     // 打滑时的最小摩擦系数
    public float peakAngle = 8f * Mathf.Deg2Rad; // 峰值侧偏角（8度）
    public float minSideFrictionCoeff = 0.3f;



    // Pacejka魔术公式参数
    public float B_lat = 10.0f;         // 侧向刚度因子
    public float C_lat = 1.9f;           // 侧向形状因子
    public float D_lat = 1.0f;           // 侧向峰值因子
    public float E_lat = 0.97f;
    public float B_long = 12.0f;        // 纵向刚度因子
    public float C_long = 2.2f;          // 纵向形状因子
    public float D_long = 1.1f;          // 纵向峰值因子
    public float E_long = 0.97f;
    public float loadSensitivity = 0.1f;
    // 在TyreType类中添加静摩擦参数
    // public float staticFrictionCoeff = 1.2f; // 静摩擦系数（大于动摩擦峰值）
    [Range(0.01f, 0.03f)]
    public float rollingResistCoeff = 0.015f; // 滚动阻力系数

    #endregion

    public TyreType(float peakSlip, float minForwardFrictionCoeff, float peakAngle, float minSideFrictionCoeff)
    {
        this.peakSlip = peakSlip;
        this.minForwardFrictionCoeff = minForwardFrictionCoeff;
        this.peakAngle = peakAngle;
        this.minSideFrictionCoeff = minSideFrictionCoeff;

        B_lat = 10.0f;         // 侧向刚度因子
        C_lat = 1.9f;           // 侧向形状因子
        D_lat = 1.0f;           // 侧向峰值因子
        E_lat = 0.97f; // 曲率因子

        B_long = 12.0f;        // 纵向刚度因子
        C_long = 2.2f;          // 纵向形状因子
        D_long = 1.1f;          // 纵向峰值因子
        E_long = 0.97f; // 曲率因子
        
        loadSensitivity = 0.1f;
        
        rollingResistCoeff = 0.015f; // 滚动阻力系数
    }

    // public TyreType(float peakSlip, float minForwardFrictionCoeff, float peakDegree, float minSideFrictionCoeff)
    // {
    //     this.peakSlip = peakSlip;
    //     this.minForwardFrictionCoeff = minForwardFrictionCoeff;
    //     this.peakAngle = peakDegree * Mathf.Deg2Rad;
    //     this.minSideFrictionCoeff = minSideFrictionCoeff;

    //     B_lat = 10.0f;         // 侧向刚度因子
    //     C_lat = 1.9f;           // 侧向形状因子
    //     D_lat = 1.0f;           // 侧向峰值因子

    //     B_long = 12.0f;        // 纵向刚度因子
    //     C_long = 2.2f;          // 纵向形状因子
    //     D_long = 1.1f;          // 纵向峰值因子
    //     rollingResistCoeff = 0.015f; // 滚动阻力系数
    // }


    #region Friction Calculation

    // 改进的纵向摩擦模型
    public float CalculateLongitudinalFrictionForce(float slipRatio, float maxFrictionForce)
    {
        // 摩擦系数曲线（无量纲）
        float baseFriction = CalculateLongitudinalFrictionCoeff(slipRatio);
        // 考虑垂直载荷
        float normalizedLoad = Mathf.Clamp(maxFrictionForce / 5000f, 0.5f, 2f);
        float loadEffect = 1 - (loadSensitivity * (normalizedLoad - 1f));
        // 最终摩擦系数计算
        float frictionCoeff = Mathf.Clamp(baseFriction * loadEffect, 0.5f, 3f);

        // 实际摩擦力 = 摩擦系数 × 最大可用摩擦力
        float frictionForce = frictionCoeff * maxFrictionForce;

        // 根据滑移方向确定力方向
        return frictionForce * Mathf.Sign(slipRatio);
    }
    //Get Abs slipRatio and Return FrictionCoeff
    float CalculateLongitudinalFrictionCoeff(float slipRatio)
    {
        float absSlip = Mathf.Abs(slipRatio);



        float x = B_long * absSlip;
        float baseFriction = D_long * Mathf.Sin(C_long * Mathf.Atan(x - E_long * (x - Mathf.Atan(x))));
        return baseFriction;

    }

    // 改进的侧向摩擦模型
    public float CalculateLateralFrictionForce(float slipAngle, float maxFrictionForce)
    {
        // 摩擦系数曲线（无量纲）
        float baseFriction = CalculateLateralFrictionCoeff(slipAngle);
        // float frictionCoeff = D_lat * Mathf.Sin(C_lat * Mathf.Atan(B_lat * Mathf.Abs(slipAngle)));
        // 考虑垂直载荷
        float normalizedLoad = Mathf.Clamp(maxFrictionForce / 5000f, 0.5f, 2f);
        float loadEffect = 1 - (loadSensitivity * (normalizedLoad - 1f));
        // 最终摩擦系数计算
        float frictionCoeff = Mathf.Clamp(baseFriction * loadEffect, 0.5f, 3f);

        // 实际摩擦力 = 摩擦系数 × 最大可用摩擦力
        float frictionForce = frictionCoeff * maxFrictionForce;

        // 实际摩擦力 = 摩擦系数 × 最大可用摩擦力
        return frictionForce * -Mathf.Sign(slipAngle);
    }
    //Get Abs slipRatio and Return FrictionCoeff

    float CalculateLateralFrictionCoeff(float slipAngle)
    {
        float absAngle = Mathf.Abs(slipAngle);

        float x = B_lat * absAngle;
        float baseFriction = D_lat * Mathf.Sin(C_lat * Mathf.Atan(x - E_lat * (x - Mathf.Atan(x))));
        return baseFriction;

    }

    #endregion
}

public enum WheelType
{
    FrontLeft = 0,
    FrontRight = 1,
    RearLeft = 2,
    RearRight = 3
}


[Serializable]
public class WheelPhysics
{
    [NonSerialized]
    public Rigidbody carRB;
    public Transform suspensionTransform;
    //need to update the wheel rotation
    public Transform wheelTransform;
    public float angularVelocity; // in rad/s
    public float radius;
    public float mass;
    public float inertia;

    public float verticalLoad;
    private float tireFrictionCoeff;
    [NonSerialized]
    public float lastSpringLength;
    public bool isGrounded;
    // public LowSpeedConfig lowSpeedSetting = GameSystem.Instance.lowSpeedControl; // Reference to the low speed control settings

    public float motorTorque;
    public float brakeTorque;
    public float steerAngle;
    private float wheelAngle;

    private TyreType tyre;

    #region PUBLIC FOR OUTPUT Get
    public Vector3 LastDriveForce { get; private set; }
    public Vector3 LastLateralForce { get; private set; }
    public float maxLateralFriction = 0f;
    private Vector3 wheelVelocityLS; // Local space wheel velocity, used for slip calculations
    #endregion
    public void Initialize(Rigidbody rb, float radius, float mass, TyreType tyre)
    {
        carRB = rb;
        this.radius = radius;
        this.mass = mass;
        inertia = mass * radius * radius / 2f; // Assuming a solid cylinder for the wheel inertia
        angularVelocity = 0f;
        tireFrictionCoeff = 1.0f; // Default friction coefficient, can be adjusted
        verticalLoad = 0f; // Initial vertical load
        isGrounded = false; // Initially not grounded
        this.tyre = new TyreType(tyre.peakSlip, tyre.minForwardFrictionCoeff, tyre.peakAngle, tyre.minSideFrictionCoeff);

        LastDriveForce = Vector3.zero;
        LastLateralForce = Vector3.zero;
    }
    public void CalculateWheelSuspensionPoint(Vector3 position)
    {
        // suspensionTransform.SetPositionAndRotation(position, rotation);
        suspensionTransform.position = position;
    }

    public void UpdateGroundedCheck(bool isGrounded, float verticalLoad, float tireFrictionCoeff, Vector3 wheelPosition, float previousLength)
    {
        this.isGrounded = isGrounded;
        this.verticalLoad = Mathf.Max(verticalLoad, 0.0f);
        this.tireFrictionCoeff = tireFrictionCoeff;
        this.lastSpringLength = previousLength;


        // VisualUpdate
        // Quaternion steeringRotation = Quaternion.Euler(0, steerAngle, 0);
        wheelTransform.SetPositionAndRotation(wheelPosition, wheelTransform.rotation);
        // wheelTransform.position = wheelPosition; // Update wheel position directly
    }
    // only works on a car with a steady normal speed
    // BUG causing jittering on a sightly speed!
    // problem finding:
    // 1. once the speed is very low, a littlt speed change will make the slipRatio very high, causing the friction force to be very high.
    // 2. the slipRatio is calculated by the difference between the wheel's linear velocity and the forward speed of the car, which is not very accurate at low speeds.
    
    public bool CalculateWheelLongitudeAndLateralForce(bool isDriven, float deltaTime)
    {
        LastDriveForce = Vector3.zero;
        LastLateralForce = Vector3.zero;

        if (!isGrounded)
        {
            return false;
        }

        // float steerAngle = 0f;
        wheelVelocityLS = wheelTransform.InverseTransformDirection(carRB.GetPointVelocity(wheelTransform.position));
        float forwardSpeed = wheelVelocityLS.z;
        //若是接触点速度在rb的正右方!=0,意味着轮胎需要阻止车辆向该方向移动。
        float lateralSpeed = wheelVelocityLS.x;
        float wheelLinearVelocity = angularVelocity * radius;

        float slipDenominator = Mathf.Max(0.1f, Mathf.Abs(forwardSpeed));
        // jittering at very low speed! it will caused by the very slightly velocity diff / very slightly baseSpeed
        // in theroratical, it doesnt do any thing wrong, but it just caused big problem.
        float slipRatio =
            Mathf.Clamp((wheelLinearVelocity - forwardSpeed) / slipDenominator, -1, 1);

        float slipAngle;
        if (Mathf.Abs(forwardSpeed) > 0.05f)
        {
            slipAngle = Mathf.Atan2(lateralSpeed, slipDenominator);
        }
        else
        {
            slipAngle = (Mathf.Abs(lateralSpeed) > 0.5f) ?
                Mathf.Sign(lateralSpeed) * Mathf.PI / 2 : 0f;
        }

        float maxFrictionForce = tireFrictionCoeff * verticalLoad;
        float longitudinalFriction = tyre.CalculateLongitudinalFrictionForce(slipRatio, maxFrictionForce);

        float lateralFriction = tyre.CalculateLateralFrictionForce(slipAngle, maxFrictionForce);
        LastDriveForce = wheelTransform.forward * longitudinalFriction;
        // 应用阻碍车辆向对应角度移动的侧向力
        LastLateralForce = wheelTransform.right * lateralFriction;

        // 调试输出
        Debug.Log($"车轮方向: {wheelTransform.forward}" +
                  $"角速度: {angularVelocity:F4}" +
                  $"滑移率: {slipRatio:F4}, " +
                  $"纵向摩擦: {longitudinalFriction:F4}, " +
                  $"侧偏角: {slipAngle * Mathf.Rad2Deg:F4}°, " +
                  $"侧向摩擦: {lateralFriction:F4}, " +
                  $"车轮速度: {wheelLinearVelocity:F4}, " +
                  $"车辆当前速度:{forwardSpeed:F4}");
        // Debug.Log($"最大侧向力:{maxLateralFriction}");

        return true;
    }


    #region Wheel Angular Velocity Update

    //give specific alxe Torque and give them goundedChec
    //calculate the torque to apply to the left wheel and right wheel
    public void UpdateAngularVelocity(bool isDriven, float deltaTime)
    {
        // 1. 仅使用制动扭矩和发动机扭矩影响车轮转动
        float netTorque = isDriven ? motorTorque : 0f;

        // 2. 从动轮转速计算规则
        if (!isDriven && isGrounded && brakeTorque <= Mathf.Epsilon)
        {
            // 根据车身速度计算理论线速度
            float forwardSpeed = wheelVelocityLS.z;
            // 计算理论角速度（考虑转向角度影响）
            float targetAngularVel = forwardSpeed / radius;

            // 平滑过渡到目标速度
            float velDiff = targetAngularVel - angularVelocity;
            // 低速时增大maxChange（如0.5 rad/s），抑制震荡
            // float maxChange = GameSystem.Instance.lowSpeedControl.IsLowSpeed(carRB.velocity.magnitude) ? 0.1f : 100f * deltaTime;
            // float maxChange = 100f * deltaTime; // 可调整的旋转响应速度
            angularVelocity += velDiff;
        }
        // 刹车仅作用于车轮转动系统
        netTorque -= (brakeTorque + (isDriven ? CalculateRollingResistance() : 0f)) * Mathf.Sign(angularVelocity);

        //滚动阻力

        // 2. 计算角加速度 (α = τ / I)
        float angularAccel = netTorque / inertia;

        // CHECK 这个有意义吗 => 更新角速度时添加阻尼, 对非驱动轮减少影响?
        float angularDamping = Mathf.Abs(motorTorque) > 0.5f ? 0.95f : 0.99f;
        angularVelocity = angularVelocity * angularDamping + angularAccel * deltaTime;
    }
    #endregion
    // 在WheelPhysics类中添加低速判断
    #region Steering

    public void UpdateSteering(float damper, float deltaTime)
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, damper * deltaTime);
        wheelTransform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    #endregion

    #region RollingResistance
    // float rollingResistanceCoefficient = 0.015f;
    //计算rollingResistance, 以此来反向通过车辆运动影响轮胎转速
    private float CalculateRollingResistance()
    {
        float forwardSpeed = wheelVelocityLS.z;

        if (!isGrounded || Mathf.Abs(forwardSpeed) < 0.1f) return 0f;

        // 计算等效滚动阻力矩 (F_r * r)
        float rollingResistanceTorque = tyre.rollingResistCoeff * verticalLoad * radius;

        // 滚动阻力
        return rollingResistanceTorque / inertia;
    }
    #endregion



    #region resistance Related
    public void StopWheel()
    {
        if (brakeTorque > 0.5f || motorTorque < 0.5f)
        {
            angularVelocity = 0f;
            LastDriveForce = Vector3.zero;
            LastLateralForce = Vector3.zero;
        }
    }
    #endregion
}


[Serializable]
public class CarAxle
{
    public Transform transform;
    public Vector3 Position => transform.position;

    public WheelPhysics leftWheelPhysics;
    public WheelPhysics rightWheelPhysics;

    //trackWidth: 轮距
    public void CalculateWheelPosition(float trackWidth)
    {
        // visual rotation need to avoid effection to the wheel's physics check, so while we update wheel's rotation, we need to do it independently.
        leftWheelPhysics.CalculateWheelSuspensionPoint(Position + transform.right * trackWidth / 2);
        rightWheelPhysics.CalculateWheelSuspensionPoint(Position - transform.right * trackWidth / 2);
    }


    public (float innerAngle, float outerAngle) CalculateAckermanAngles(
        float steerInput,
        float turnRaidus,
        float wheelbase,      // 轴距（前后轮距离）
        float trackWidth)     // 轮距（左右轮距离）
    {
        // 计算阿克曼几何
        float innerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRaidus + trackWidth / 2)) * steerInput;
        float outerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRaidus - trackWidth / 2)) * steerInput;

        // 根据转向方向调整
        if (steerInput < 0)
            return (innerAngle, outerAngle);
        else if (steerInput > 0)
            return (outerAngle, innerAngle);
        else
        {
            return (0f, 0f);
        }
    }

    public void UpdateWheelAngle(float steerInput, float turnRaidus, float wheelbase, float trackWidth)
    {
        (leftWheelPhysics.steerAngle, rightWheelPhysics.steerAngle) = CalculateAckermanAngles(steerInput, turnRaidus, wheelbase, trackWidth);
    }


}
public enum DrivenType
{
    FrontWheel,
    RearWheel,
    FullWheel
}

[Serializable]
public class SuspensionData
{
    public float springStiffness;

    [Range(0.2f, 1f)] public float zeta;
    [SerializeField]
    public float dampStiffness;
    public float restLength;
    public float springTravel;
    [Header("高级设置")]
    public float maxForceAngle = 0.5f;     // 最大力角度偏差(度)
    public float forceSmoothing = 0.1f; // 力平滑系数
    public float maxLength;
    public float minLength;

    public SuspensionData(float springStiffness, float zeta, float carMass, float restLength, float springTravel, float maxForceAngle, float forceSmoothing)
    {
        this.springStiffness = springStiffness;
        this.zeta = zeta;
        this.dampStiffness = zeta * (2 * Mathf.Sqrt(springStiffness * carMass));
        this.restLength = restLength;
        this.springTravel = springTravel;

        this.maxForceAngle = maxForceAngle;
        this.forceSmoothing = forceSmoothing;
        maxLength = restLength + springTravel;
        minLength = restLength - springTravel;
    }


}

public class AxlePhysics : MonoBehaviour
{
    [Header("Core Components")]
    public CarAxle frontAxle;
    public CarAxle rearAxle;
    public Rigidbody rb;

    [Header("Car Settings")]
    public DrivenType drivenType = DrivenType.FullWheel;
    public TyreType tyreType;
    public float wheelRadius = 0.25f;
    public float wheelMass = 20f;
    // public float inertia;
    [SerializeField]
    public SuspensionData suspension;
    public float Wheelbase => (frontAxle.Position - rearAxle.Position).magnitude;
    public readonly float TrackWidth = 1.6f;

    [Header("Steering Settings")]
    public float turnRaidus = 10.8f;
    public float steeringDamper = 4;

    [Header("Physics Settings")]
    [NonSerialized]
    WheelPhysics[] wheels;
    [SerializeField]
    public GroundFrictions frictionSetting;

    [SerializeField]
    public LayerMask drivable;


    bool FrontDriven => drivenType == DrivenType.FrontWheel || drivenType == DrivenType.FullWheel;
    bool RearDriven => drivenType == DrivenType.RearWheel || drivenType == DrivenType.FullWheel;
    // Start is called before the first frame update
    bool isBraking = false;
    float isSuspending = 4;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0, 0, 0);

        suspension = new SuspensionData(suspension.springStiffness, suspension.zeta, rb.mass, suspension.restLength, suspension.springTravel, suspension.maxForceAngle, suspension.forceSmoothing);

        wheels = new WheelPhysics[4];
        wheels[0] = frontAxle.leftWheelPhysics;
        wheels[1] = frontAxle.rightWheelPhysics;
        wheels[2] = rearAxle.leftWheelPhysics;
        wheels[3] = rearAxle.rightWheelPhysics;
        foreach (var wheel in wheels)
        {
            wheel.Initialize(rb, wheelRadius, wheelMass, tyreType);
        }
        // wheelTorques = new float[4];
        // Debug.Log($"Ray Points Count: {rayPoints.Length}");
        // inertia = wheelMass * wheelRadius * wheelRadius / 2f; // Assuming a solid cylinder for the wheel inertia
        frontAxle.CalculateWheelPosition(TrackWidth);
        rearAxle.CalculateWheelPosition(TrackWidth);

    }

    // Update is called once per frame
    void Update()
    {
        // float deltaTime = Time.deltaTime;

        // float forwardAccelerate = Input.GetAxis("Vertical");
        // //
        // float drivenTorque = forwardAccelerate * 10f;
        // calculate net Torque

        // before update Torque, we need calculate the ratio and using the differential to set differentTorque to wheel,
        // but we do it later, and just simpliy split it to half.
        // foreach(var wheel in wheels){
        //     Debug.Log(rb.angularVelocity.magnitude + ":" + rb.GetPointVelocity(wheel.wheelTransform.position).magnitude);
        // }
    }
    public void SetInput(float throttle, float brake, bool isHandbrakeOn, float steer, float deltaTime)
    {
        float drivenTorque = throttle * 500f;
        float brakeTorque = (isHandbrakeOn ? 1.2f : brake) * 100f;

        isBraking = brakeTorque > 0.5f || drivenTorque < 0.5f;

        // TODO need differential system
        // Apply driven torque to the wheels
        for (int i = 0; i < wheels.Count(); i++)
        {
            var wheel = wheels[i];
            if (IsWheelDriven(i))
            {
                wheel.motorTorque = drivenTorque / 2f; // Split torque evenly between left and right wheels

            }
            wheel.brakeTorque = brakeTorque;
        }


        frontAxle.UpdateWheelAngle(steer, turnRaidus, Wheelbase, TrackWidth);
        // rearAxle.UpdateWheelAngle(-steer, turnRaidus, Wheelbase, TrackWidth);
    }

    void FixedUpdate()
    {

        float deltaTime = Time.fixedDeltaTime;
        // frontAxle.UpdateWheelPosition(AxleLength);
        // rearAxle.UpdateWheelPosition(AxleLength);
        Suspension(deltaTime);
        // bool isTryingStopping = isSuspending < 1 && isBraking && GameSystem.Instance.lowSpeedControl.IsLowSpeed(rb.velocity.magnitude);
        // if (isTryingStopping)
        // {
        //     ForceStopAtLowSpeed(deltaTime);
        // }
        for (int i = 0; i < wheels.Count(); i++)
        {
            var wheel = wheels[i];
            wheel.UpdateAngularVelocity(IsWheelDriven(i), deltaTime);
            wheel.UpdateSteering(steeringDamper, deltaTime);
            if (wheel.CalculateWheelLongitudeAndLateralForce(IsWheelDriven(i), deltaTime))
            {
                Debug.Log((wheel.LastLateralForce + wheel.LastDriveForce));
                rb.AddForceAtPosition(wheel.LastLateralForce + wheel.LastDriveForce, wheel.wheelTransform.position - wheel.wheelTransform.up * wheel.radius, ForceMode.Force);
            }
        }
        rb.AddForce(CalculateAirDrag(), ForceMode.Force);
    }

    #region Resistance Forces

    //但是怎么样突破阻力向后滚动呢？
    //BUG 低速时横向斜面时可能会存在力矩抖动
    public void ForceStopAtLowSpeed(float deltaTime)
    {
        // Debug.Log("Check");
        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, GameSystem.Instance.lowSpeedControl.stopDamping * deltaTime);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, GameSystem.Instance.lowSpeedControl.stopDamping * deltaTime);
        // rb.velocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;
        foreach (var wheel in wheels)
        {
            wheel.StopWheel();
        }
    }
    public Vector3 CalculateAirDrag()
    {
        // if(rb.velocity.sqrMagnitude <= 0.01f) return Vector3.zero;
        // 计算空气阻力
        Vector3 airDragForce = GameSystem.Instance.airDragCoefficient * rb.velocity.sqrMagnitude * -rb.velocity.normalized;
        return airDragForce;
    }



    #endregion


    #region Suspension Functions
    // BUG 车辆侧翻时会出现碰撞问题，感觉可能是car collider太小了。
    public void Suspension(float deltaTime)
    {
        isSuspending = wheels.Count();
        for (int i = 0; i < wheels.Count(); i++)
        {
            Transform rayPoint = wheels[i].suspensionTransform;
            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out RaycastHit hit, suspension.maxLength + wheelRadius, drivable))
            {
                float currentSpringLength = hit.distance - wheelRadius;

                currentSpringLength = Mathf.Clamp(currentSpringLength, suspension.minLength, suspension.maxLength);

                float springCompression = (suspension.restLength - currentSpringLength) / suspension.springTravel;

                float springChangeVelocity = (currentSpringLength - wheels[i].lastSpringLength) / deltaTime;

                float dampForce = suspension.dampStiffness * springChangeVelocity;

                float springForce = springCompression * suspension.springStiffness;

                float netForce = springForce - dampForce;

                float frictionCoeff = frictionSetting.GetFrictionCoeff(hit.transform.tag);

                Vector3 wheelPosition = hit.point + rayPoint.up * wheelRadius;

                rb.AddForceAtPosition(netForce * rayPoint.up, hit.point, ForceMode.Force);

                wheels[i].UpdateGroundedCheck(true, Mathf.Max(netForce, 0), frictionCoeff, wheelPosition, currentSpringLength);

                isSuspending--;
            }
            else
            {
                Vector3 wheelPosition = rayPoint.position + suspension.maxLength * -rayPoint.up;

                wheels[i].UpdateGroundedCheck(false, 0, 0f, wheelPosition, suspension.maxLength);
            }
        }
    }
    #endregion

    #region transform check function
    private void OnDrawGizmos()
    {
        if (frontAxle != null && rearAxle != null && wheels != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(frontAxle.Position, rearAxle.Position);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wheels[0].suspensionTransform.position, wheels[1].suspensionTransform.position);
            Gizmos.DrawLine(wheels[2].suspensionTransform.position, wheels[3].suspensionTransform.position);

            foreach (var wheel in wheels)
            {
                DrawWheelSuspension(wheel);

                Debug.DrawRay(wheel.wheelTransform.position, wheel.LastDriveForce / wheel.verticalLoad, Color.red);    // 纵向力
                Debug.DrawRay(wheel.wheelTransform.position, wheel.LastLateralForce / wheel.verticalLoad, Color.blue);
            }

            Debug.DrawRay(rb.transform.TransformPoint(rb.centerOfMass), rb.velocity, Color.gray);


            // Gizmos.color = Color.green;
            // Gizmos.DrawSphere(frontAxle.leftWheel.position, wheelRadius);
            // Gizmos.DrawSphere(frontAxle.rightWheel.position, wheelRadius);
            // Gizmos.DrawSphere(rearAxle.leftWheel.position, wheelRadius);
            // Gizmos.DrawSphere(rearAxle.rightWheel.position, wheelRadius);

            // Gizmos.color = Color.black;
            // float rayLength = 10f;
            // Gizmos.DrawRay(frontAxle.leftWheel.position, -frontAxle.leftWheel.up * rayLength);
            // Gizmos.DrawRay(frontAxle.rightWheel.position, -frontAxle.rightWheel.up * rayLength);
            // Gizmos.DrawRay(rearAxle.leftWheel.position, -rearAxle.leftWheel.up * rayLength);
            // Gizmos.DrawRay(rearAxle.rightWheel.position, -rearAxle.rightWheel.up * rayLength);

            // foreach(var w in wheels) {
            //         Debug.DrawRay(w.wheelTransform.position, w.wheelTransform.forward * 2, Color.blue);
            //         Debug.DrawRay(w.wheelTransform.position, w.wheelTransform.right * 2, Color.red);
            //     }
        }
    }

    private void DrawWheelSuspension(WheelPhysics wheel)
    {
        if (wheel.suspensionTransform == null || wheel.wheelTransform == null)
        {
            Debug.LogError("Wheel suspension or wheel transform is not assigned.");
            return;
        }

        if (wheel.isGrounded)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;
        Gizmos.DrawLine(wheel.suspensionTransform.position, wheel.wheelTransform.position);
    }
    #endregion

    private bool IsWheelDriven(int index)
    {
        if (index < 2)
        {
            return FrontDriven;
        }
        else if (index < 4)
        {
            return RearDriven;
        }
        else
        {
            Debug.LogError("Index out of range for wheel driven check.");
            return false;
        }
    }
    private bool IsWheelSterring(int index)
    {
        return index < 2;
    }

    #region Car Status

    public float GetVehicleKMH()
    {
        return rb.velocity.magnitude * Mathf.Sign(Vector3.Dot(rb.transform.forward, rb.velocity)) * 3.6f;
    }

    #endregion
}
