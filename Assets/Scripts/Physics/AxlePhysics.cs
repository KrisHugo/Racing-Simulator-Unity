using System;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using UnityEngine;


[Serializable]
public class CarAxle
{
    public Transform transform;
    public Vector3 Position => transform.position;

    public WheelPhysics leftWheel;
    public WheelPhysics rightWheel;

    //trackWidth: 轮距
    public void CalculateWheelPosition(float trackWidth)
    {
        // visual rotation need to avoid effection to the wheel's physics check, so while we update wheel's rotation, we need to do it independently.
        leftWheel.CalculateWheelSuspensionPoint(Position + transform.right * trackWidth / 2);
        rightWheel.CalculateWheelSuspensionPoint(Position - transform.right * trackWidth / 2);
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
        (leftWheel.steerAngle, rightWheel.steerAngle) = CalculateAckermanAngles(steerInput, turnRaidus, wheelbase, trackWidth);
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
    [Header("EngineTorqueTest:")]
    public float maxEngineTorque = 300f;
    public float maxBrakeTorque = 1500f;

    [Header("Core Components")]
    public CarAxle frontAxle;
    public CarAxle rearAxle;
    public Rigidbody rb;
    public EngineSystem engine;

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

    #region car State
    bool FrontDriven => drivenType == DrivenType.FrontWheel || drivenType == DrivenType.FullWheel;
    bool RearDriven => drivenType == DrivenType.RearWheel || drivenType == DrivenType.FullWheel;
    // Start is called before the first frame update
    // bool isLowSpeedMode = false;
    // float isSuspending = 4;
    // 私有状态变量
    // private Vector3 velocity;
    // private Vector3 angularVelocity;
    // private Vector3 prevPosition;
    // private Quaternion prevRotation;

    private bool isDriveTraing = false;
    #endregion

    void Start()
    {
        // Time.fixedDeltaTime = 0.02f;
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0, 0, 0);
        // rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        engine = GetComponent<EngineSystem>();
        if(engine == null){
            engine = gameObject.AddComponent<EngineSystem>();
        }

        suspension = new SuspensionData(suspension.springStiffness, suspension.zeta, rb.mass, suspension.restLength, suspension.springTravel, suspension.maxForceAngle, suspension.forceSmoothing);

        wheels = new WheelPhysics[4];
        wheels[0] = frontAxle.leftWheel;
        wheels[1] = frontAxle.rightWheel;
        wheels[2] = rearAxle.leftWheel;
        wheels[3] = rearAxle.rightWheel;
        foreach (var wheel in wheels)
        {
            wheel.Initialize(rb, wheelRadius, wheelMass, suspension.maxLength, suspension.springStiffness, suspension.dampStiffness, tyreType);
        }
        // wheelTorques = new float[4];
        // Debug.Log($"Ray Points Count: {rayPoints.Length}");
        // inertia = wheelMass * wheelRadius * wheelRadius / 2f; // Assuming a solid cylinder for the wheel inertia
        frontAxle.CalculateWheelPosition(TrackWidth);
        rearAxle.CalculateWheelPosition(TrackWidth);

        // prevPosition = transform.position;
        // prevRotation = transform.rotation;

    }

    public void Update()
    {
        foreach(var wheel in wheels){
            wheel.UpdateVisual();
        }
        
    }

    public void SetInput(float throttle, float brake, bool isHandbrakeOn, float steer, float deltaTime)
    {
        float drivenTorque = throttle * maxEngineTorque;
        float brakeTorque = (isHandbrakeOn ? 1.2f : brake) * maxBrakeTorque;
        isDriveTraing = throttle != 0;
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
    }

    void FixedUpdate()
    {

        float deltaTime = Time.fixedDeltaTime;

        for (int i = 0; i < wheels.Count(); i++)
        {
            var wheel = wheels[i];
            wheel.UpdateGroundedCheck();
            wheel.UpdateAngularVelocity(IsWheelDriven(i), deltaTime);
            wheel.UpdateSteering(steeringDamper, deltaTime);
        }
        rb.AddForce(CalculateAirDrag(), ForceMode.Force);
        //测试一档rpm
        engine.UpdateEngineRPMbyWheelAvgRpm(GetAvgWheelRPM(), 3.5f, 4.0f, deltaTime);


    }

    // public void ForceCarStop(){

    //     rb.velocity = Vector3.Lerp(rb.velocity,Vector3.zero, GameSystem.Instance.lowSpeedControl.stopDamping* Time.fixedDeltaTime) ;
    //     rb.angularVelocity = Vector3.Lerp(rb.angularVelocity,Vector3.zero, GameSystem.Instance.lowSpeedControl.stopDamping* Time.fixedDeltaTime);
    //     foreach (var wheel in wheels){
    //         wheel.StopWheel();
    //     }
    // }


    #region Resistance Forces

    public Vector3 CalculateAirDrag()
    {
        // if(rb.velocity.sqrMagnitude <= 0.01f) return Vector3.zero;
        // 计算空气阻力
        Vector3 airDragForce = GameSystem.Instance.airDragCoefficient * rb.linearVelocity.sqrMagnitude * -rb.linearVelocity.normalized;
        return airDragForce;
    }

    #endregion


    #region Suspension Functions
    // BUG 车辆侧翻时会出现碰撞问题，感觉可能是car collider太小了。
    // public void Suspension(SuspensionData suspension, float deltaTime)
    // {
    //     isSuspending = wheels.Count();
    //     for (int i = 0; i < wheels.Count(); i++)
    //     {
    //         Transform rayPoint = wheels[i].suspensionTransform;
    //         if (Physics.Raycast(rayPoint.position, -rayPoint.up, out RaycastHit hit, suspension.maxLength + wheelRadius, drivable))
    //         {
    //             float currentSpringLength = hit.distance - wheelRadius;

    //             currentSpringLength = Mathf.Clamp(currentSpringLength, suspension.minLength, suspension.maxLength);

    //             float springCompression = (suspension.restLength - currentSpringLength) / suspension.springTravel;

    //             float springVelocity = (currentSpringLength - wheels[i].lastSpringLength) / deltaTime;

    //             float dampForce = suspension.dampStiffness * springVelocity;

    //             float springForce = springCompression * suspension.springStiffness;

    //             float netForce = springForce - dampForce;

    //             float frictionCoeff = frictionSetting.GetFrictionCoeff(hit.transform.tag);

    //             Vector3 wheelPosition = hit.point + rayPoint.up * wheelRadius;

    //             // rb.AddForceAtPosition(netForce * rayPoint.up, hit.point, ForceMode.Force);
    //             rb.AddForceAtPosition(netForce * hit.normal, wheelPosition, ForceMode.Force);

    //             wheels[i].UpdateGroundedCheck(true, Mathf.Max(netForce, 0), frictionCoeff, wheelPosition, currentSpringLength);

    //             isSuspending--;
    //         }
    //         else
    //         {
    //             Vector3 wheelPosition = rayPoint.position + suspension.maxLength * -rayPoint.up;

    //             wheels[i].UpdateGroundedCheck(false, 0, 0f, wheelPosition, suspension.maxLength);
    //         }
    //     }
    // }
    #endregion

    #region transform check function
    private void OnDrawGizmos()
    {
        if (frontAxle != null && rearAxle != null && wheels != null)
        {
            // Gizmos.color = Color.red;
            // Gizmos.DrawLine(frontAxle.Position, rearAxle.Position);
            // Gizmos.color = Color.blue;
            // Gizmos.DrawLine(wheels[0].suspensionTransform.position, wheels[1].suspensionTransform.position);
            // Gizmos.DrawLine(wheels[2].suspensionTransform.position, wheels[3].suspensionTransform.position);

            foreach (var wheel in wheels)
            {
                // DrawWheelSuspension(wheel);

                // Debug.DrawRay(wheel.wheelTransform.position, wheel.LastDriveForce / wheel.verticalLoad, Color.red);    // 纵向力
                // Debug.DrawRay(wheel.wheelTransform.position, wheel.LastLateralForce / wheel.verticalLoad, Color.blue);
            }

            Debug.DrawRay(rb.transform.TransformPoint(rb.centerOfMass), rb.linearVelocity, Color.gray);
        }
    }

    // private void DrawWheelSuspension(WheelPhysics wheel)
    // {
    //     if (wheel.suspensionTransform == null || wheel.wheelTransform == null)
    //     {
    //         Debug.LogError("Wheel suspension or wheel transform is not assigned.");
    //         return;
    //     }

    //     if (wheel.isGrounded)
    //         Gizmos.color = Color.red;
    //     else
    //         Gizmos.color = Color.green;
    //     Gizmos.DrawLine(wheel.suspensionTransform.position, wheel.wheelTransform.position);
    // }
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
        return rb.linearVelocity.magnitude * Mathf.Sign(Vector3.Dot(rb.transform.forward, rb.linearVelocity)) * 3.6f;
    }

    public float GetEngineRPM(){
        return engine.CurrentRPM;
    }

    public float GetAvgWheelRPM()
    {
        float avgWheelRPM = 0;
        foreach(var wheel in wheels){
            avgWheelRPM += wheel.GetCurrentWheelRPM();
        }
        return avgWheelRPM / wheels.Length;
    }

    public void Respawn(Transform respawnPoint){
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = respawnPoint.position;
        rb.rotation = respawnPoint.rotation;

        // Reset wheel states
        foreach (var wheel in wheels)
        {
            wheel.ResetWheel();
        }
    }

    #endregion
}
