using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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


    #region Friction Calculation

    // 改进的纵向摩擦模型
    public float CalculateLongitudinalFrictionForce(float slipRatio, float maxFrictionForce)
    {
        // 摩擦系数曲线（无量纲）
        float baseFriction = CalculateLongitudinalFrictionCoeff(slipRatio);

        // 实际摩擦力 = 摩擦系数 × 最大可用摩擦力
        float frictionForce = baseFriction * maxFrictionForce;

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
        // 实际摩擦力 = 摩擦系数 × 最大可用摩擦力
        float frictionForce = baseFriction * maxFrictionForce;

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
    // public Transform suspensionTransform;
    //need to update the wheel rotation
    public Transform wheelTransform;
    public WheelCollider collider;
    // public Vector3 contactPoint;
    // public Vector3 contactNormal; // 轮胎接触点的法线方向
    public float angularVelocity => collider.rpm * 60 / 2 * Mathf.PI; // in rad/s

    // private float currentAngle;
    // private float prevAngle;
    public float Radius => collider.radius;
    public float Mass => collider.mass;
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
    // public Vector3 LastDriveForce { get; private set; }
    // public Vector3 LastLateralForce { get; private set; }
    // public float maxLateralFriction = 0f;
    // private Vector3 velocityLS; // Local space wheel velocity, used for slip calculations
    // Vector3 LuGre_z = Vector3.zero;
    // Vector3 prevVel = Vector3.zero; // Previous relative velocity for LuGre model
    #endregion
    public void Initialize(Rigidbody rb, float radius, float mass, float springDistance, float springForce, float springDampForce, TyreType tyre)
    {
        carRB = rb;
        collider.mass = mass;
        collider.radius = radius;
        collider.suspensionDistance = springDistance;
        JointSpring jointSpring = collider.suspensionSpring;
        jointSpring.spring = springForce;
        jointSpring.damper = springDampForce;
        jointSpring.targetPosition = 0.5f;
        collider.suspensionSpring = jointSpring;

        inertia = collider.mass * Radius * Radius / 2f; // Assuming a solid cylinder for the wheel inertia

        tireFrictionCoeff = 1.0f; // Default friction coefficient, can be adjusted
        verticalLoad = 0f; // Initial vertical load
        isGrounded = false; // Initially not grounded
        this.tyre = new TyreType(tyre.peakSlip, tyre.minForwardFrictionCoeff, tyre.peakAngle, tyre.minSideFrictionCoeff);

        // LastDriveForce = Vector3.zero;
        // LastLateralForce = Vector3.zero;
    }
    public void CalculateWheelSuspensionPoint(Vector3 position)
    {
        // suspensionTransform.SetPositionAndRotation(position, rotation);
        collider.transform.position = position;
    }

    public void UpdateGroundedCheck()
    {
        isGrounded = collider.isGrounded;
        verticalLoad = collider.sprungMass;
    }

    public void UpdateSteering(float damper, float deltaTime)
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, damper * deltaTime);
        collider.steerAngle = wheelAngle;
    }

    public void UpdateVisual(){
        // 更新轮胎的可视化位置和旋转
        
        // wheelTransform.position = collider.transform.position - collider.transform.up * (collider.suspensionSpring.targetPosition * collider.suspensionDistance - collider.radius / 2);
        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;

        // Debug.Log($"Wheel Position: {wheelTransform.position}, Rotation: {wheelTransform.rotation}");
    }

    // public void UpdateGroundedCheck(bool isGrounded, float verticalLoad, float tireFrictionCoeff, Vector3 wheelPosition, float previousLength, Vector3 contactPoint, Vector3 contactNormal)
    // {
    //     this.isGrounded = isGrounded;
    //     this.verticalLoad = Mathf.Max(verticalLoad, 0.0f);
    //     this.tireFrictionCoeff = tireFrictionCoeff;
    //     lastSpringLength = previousLength;
    //     wheelTransform.SetPositionAndRotation(wheelPosition, wheelTransform.rotation);
    //     this.contactPoint = contactPoint;
    //     this.contactNormal = contactNormal;
    // }
    // public bool CalculateWheelLongitudeAndLateralForce(bool isDriven, float deltaTime)
    // {

    //     // float steerAngle = 0f;
    //     if (!isGrounded)
    //     {
    //         LastDriveForce = Vector3.zero;
    //         LastLateralForce = Vector3.zero;
    //         return false;
    //     }

    //     velocityLS = wheelTransform.InverseTransformDirection(carRB.GetPointVelocity(contactPoint));
    //     // if (carRB.velocity.magnitude < 3f && motorTorque > 0.5f)
    //     // {
    //     //     Debug.Log("低速Check");
    //     //     // 当车轮速度较低时，直接用motorTorque和brakeTorque提供向前的力
    //     //     LastDriveForce = motorTorque * deltaTime * tireFrictionCoeff * verticalLoad * wheelTransform.forward;
    //     //     LastLateralForce = Vector3.zero; // 假设在低速时不考虑侧向力
    //     //     return true;
    //     // }
    //     // else
    //     // {

    //     // }


    //     // Vector3 rotDir = Vector3.Cross(wheelTransform.right, contactNormal).normalized;
    //     // Vector3 radialVector = (contactPoint - wheelTransform.position).normalized;
    //     // Vector3 velocitySurface = Vector3.Cross(-angularVelocity * rotDir, radialVector * radius);

    //     // Vector3 relVelocity = velocityLS - velocitySurface;

    //     // LastDriveForce = UpdateLuGreForce(relVelocity, deltaTime, verticalLoad);
    //     float wheelLinearVelocity = angularVelocity * radius;

    //     float forwardSpeed = velocityLS.z;
    //     float lateralSpeed = velocityLS.x;
    //     float slipDenominator = Mathf.Max(1f, Mathf.Abs(forwardSpeed));

    //     // float frictionGivenRatioByRevSpeed = Mathf.Clamp01(slipDenominator / wheelLinearVelocity);
    //     float frictionGivenRatioByRevSpeed = 1f;
    //     float maxFrictionForce = tireFrictionCoeff * verticalLoad * frictionGivenRatioByRevSpeed;

    //     float slipRatio = Mathf.Clamp((wheelLinearVelocity - forwardSpeed) / slipDenominator, -1, 1);
    //     float longitudinalFriction = tyre.CalculateLongitudinalFrictionForce(slipRatio, maxFrictionForce);
    //     LastDriveForce = wheelTransform.forward * longitudinalFriction;

    //     float slipAngle = Mathf.Atan2(lateralSpeed, slipDenominator);
    //     float lateralFriction = tyre.CalculateLateralFrictionForce(slipAngle, maxFrictionForce);
    //     LastLateralForce = wheelTransform.right * lateralFriction;

    //     // ExponentialSmoothing()
    //     // 调试输出
    //     Debug.Log($"车轮方向: {wheelTransform.forward}" +
    //             $"角速度: {angularVelocity:F4}" +
    //             $"滑移率: {slipRatio:F4}, " +
    //             $"纵向摩擦: {longitudinalFriction:F4}, " +
    //             $"侧偏角: {slipAngle * Mathf.Rad2Deg:F4}°, " +
    //             $"侧向摩擦: {lateralFriction:F4}, " +
    //             $"车轮速度: {wheelLinearVelocity:F4}, " +
    //             $"车辆当前速度:{forwardSpeed:F4}");
    //     return true;
    // }

    // #region Wheel Angular Velocity Update

    // //give specific alxe Torque and give them goundedChec
    // //calculate the torque to apply to the left wheel and right wheel
    public void UpdateAngularVelocity(bool isDriven, float deltaTime)
    {

        // 1. 仅使用制动扭矩和发动机扭矩影响车轮转动
        float netTorque = isDriven ? motorTorque : 0f;
        // // 2. 从动轮转速计算规则
        // if ((!isDriven || carRB.velocity.magnitude < 1f && motorTorque <= 0.5f) && isGrounded && brakeTorque <= Mathf.Epsilon)
        // {
        //     // 根据车身速度计算理论线速度
        //     float forwardSpeed = velocityLS.z;
        //     // 计算理论角速度（考虑转向角度影响）
        //     float targetAngularVel = forwardSpeed / radius;

        //     // 平滑过渡到目标速度
        //     float velDiff = targetAngularVel - angularVelocity;
        //     // 低速时增大maxChange（如0.5 rad/s），抑制震荡
        //     // float maxChange = GameSystem.Instance.lowSpeedControl.IsLowSpeed(carRB.velocity.magnitude) ? 0.1f : 100f * deltaTime;
        //     // float maxChange = 100f * deltaTime; // 可调整的旋转响应速度
        //     angularVelocity += velDiff;
        // }
        // 刹车仅作用于车轮转动系统
        // netTorque -= (brakeTorque + (isDriven ? CalculateRollingResistance() : 0f)) * Mathf.Sign(angularVelocity);

        // float angularAccel = netTorque / inertia;
        // float angularDamping = isDriven && Mathf.Abs(motorTorque) > 0.5f ? 0.95f : 0.99f;
        // angularVelocity = angularVelocity * angularDamping + angularAccel * deltaTime;
        // angularVelocity = MathF.Sign(angularVelocity) * Mathf.Max(Mathf.Abs(angularVelocity), 0f); // 限制最大角速度

        float rollingResistance = CalculateRollingResistance();
        collider.motorTorque = motorTorque - (isDriven ? rollingResistance : 0f);
        collider.brakeTorque = brakeTorque;
        
        // Debug.Log(collider.motorTorque + ":" + rollingResistance);
    }
    // #endregion
    
    // #region Steering

    // public void UpdateSteering(float damper, float deltaTime)
    // {
    //     wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, damper * deltaTime);
    //     wheelTransform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    // }

    // #endregion

    #region RollingResistance
    // float rollingResistanceCoefficient = 0.015f;
    //计算rollingResistance, 以此来反向通过车辆运动影响轮胎转速
    private float CalculateRollingResistance()
    {

        // velocityLS = wheelTransform.InverseTransformDirection(carRB.GetPointVelocity(contactPoint));

        if (!isGrounded) return 0f;

        // 计算等效滚动阻力矩 (F_r * r)
        float rollingResistanceTorque = tyre.rollingResistCoeff * verticalLoad * Radius;

        // 滚动阻力
        return rollingResistanceTorque / inertia;

    }
    #endregion



    #region Reset
    public void ResetWheel()
    {
        collider.motorTorque = 0f;
        collider.brakeTorque = 0f;
        
    }
    #endregion

    #region Visual 
    public float GetCurrentWheelRPM(){
        return collider.rpm;
    }
    #endregion
    // #region suspension

    // public Vector3 UpdateSuspension(SuspensionData suspension, GroundFrictions frictions, LayerMask drivable, float deltaTime)
    // {
    //     Transform rayPoint = suspensionTransform;
    //     if (Physics.Raycast(rayPoint.position, -rayPoint.up, out RaycastHit hit, suspension.maxLength + radius, drivable))
    //     {
    //         float currentSpringLength = hit.distance - radius;

    //         currentSpringLength = Mathf.Clamp(currentSpringLength, suspension.minLength, suspension.maxLength);

    //         float springCompression = (suspension.restLength - currentSpringLength) / suspension.springTravel;

    //         float springVelocity = (currentSpringLength - lastSpringLength) / deltaTime;

    //         float dampForce = suspension.dampStiffness * springVelocity;

    //         float springForce = springCompression * suspension.springStiffness;

    //         float netForce = springForce - dampForce;

    //         float frictionCoeff = frictions.GetFrictionCoeff(hit.transform.tag);

    //         Vector3 wheelPosition = hit.point + rayPoint.up * radius;

    //         Vector3 contactPoint = hit.point;
    //         Vector3 contactNormal = hit.normal;

    //         // rb.AddForceAtPosition(netForce * rayPoint.up, hit.point, ForceMode.Force);
    //         // carRB.AddForceAtPosition(netForce * hit.normal, wheelPosition, ForceMode.Force);

    //         UpdateGroundedCheck(true, Mathf.Max(netForce, 0), frictionCoeff, wheelPosition, currentSpringLength, contactPoint, contactNormal);
    //         return netForce * hit.normal;
    //     }
    //     else
    //     {
    //         Vector3 wheelPosition = rayPoint.position + suspension.maxLength * -rayPoint.up;
    //         Vector3 contactPoint = rayPoint.position + (suspension.maxLength + radius) * -rayPoint.up;
    //         Vector3 contactNormal = -rayPoint.up;

    //         UpdateGroundedCheck(false, 0, 0f, wheelPosition, suspension.maxLength, contactPoint, contactNormal);
    //         return Vector3.zero;
    //     }
    // }

    // #endregion
}
