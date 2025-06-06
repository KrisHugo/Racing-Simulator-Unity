using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GearSystem))]
public class CarMovement : MonoBehaviour
{
    public enum Status{
        Off,
        On
    }
    public enum TyreSide{
        Left,
        Right
    }

    public enum Axel
    {
        Front,
        Rear
    }
    public enum DrivingType
    {
        Front,
        Rear,
        Full
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
        public TyreSide tyreSide;
    }
    [Header("Car Status")]
    public Status carStatus = Status.Off;

    [Header("Wheels Settings")]
    public List<Wheel> wheels;

    public DrivingType drivingType = DrivingType.Rear;
    public int NumberOfDrivingWheels
    {
        get
        {
            if (drivingType == DrivingType.Full) return 4; else return 2;
        }
    }
    
    [Header("Slipping Parameters")]
    public float sidewaysSlipThreshold = 0.5f; // ��������ֵ
    public float forwardSlipThreshold = 0.75f;   // ��������ֵ
    public float minDriftSpeed = 5.0f;          // ����Ư�Ƶ�����ٶ�
    public float driftAngleThreshold = 30.0f;   // �ٶȷ����복ͷ�н���ֵ


    [Header("Engine Parameters")]

    [SerializeField] private float MaxMotorTorque = 450f;
    [SerializeField] private float differentialRatio = 2.56f;

    [SerializeField] AnimationCurve PowerCurve;
    [SerializeField] public float MinRPM;
    [SerializeField] public float MaxRPM;
    [SerializeField] public float EngineRPM;
    [SerializeField] float RPMSmoothness = 20f;
    float wheelRPM = 0f;
    private float wheelRadius;

    public float BaseMaxSpeed { get { return baseMaxSpeed; } }

    [Header("Gear Parameters")]
    // [SerializeField] public float baseMaxSpeed = 30f;

    [SerializeField] private float baseMaxSpeed = 30f;
    private GearSystem gearSystem;
    [SerializeField] private float clutching = 1;

    [Header("Movement Settings")]
    [SerializeField] private float throttleForce = 10f;
    [SerializeField] private float brakeForce = 20f;
    // [SerializeField] private float reverseSpeed = 15f;
    [SerializeField] private float airDragCoeff = 3f;

    // [SerializeField] private int throttle = 1;

    [Header("Steering Settings")]
    [SerializeField] private float steeringSpeed = 100f;
    [SerializeField] private float maxSteerAngle = 5f;


    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.5f;

    private Rigidbody rb;
    public bool isGrounded;
    public float CurrentSpeed { get; private set; }

    private float currentTorque;
    private void Awake()
    {
        carStatus = Status.Off;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.45f, 0);
        gearSystem = GetComponent<GearSystem>();

        // MinRPM = 400f;
        // MaxRPM = 6000f;

        wheelRadius = wheels[0].wheelCollider.radius;
        
    }


    private void Update()
    {
        AnimatedWheels();
    }
    private void FixedUpdate()
    {
        CheckGrounded();
        if(carStatus == Status.On){
            HandleMovement();
            HandleSteering();
            CheckDrifting();
            if(InputManager.Instance.StartUpInput){
                Stall();
            }
        }
        else{
            if(InputManager.Instance.StartUpInput){
                StartUp();
            }
        }
        ApplyDrag();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.SphereCast(
            transform.position,
            0.5f,
            -transform.up,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer
        );
    }

    private float CalculateTorque(float throttle)
    {
        float torque = 0;
        if(clutching < 0.1f){
            EngineRPM = Mathf.Lerp(EngineRPM, Mathf.Max(MinRPM, MaxRPM * throttle) + UnityEngine.Random.Range(-50f, 50f), Time.fixedDeltaTime);
        }
        else{
            wheelRPM = 0;
            foreach (var wheel in wheels)
            {
                if (drivingType != DrivingType.Full && (int)drivingType != (int)wheel.axel)
                {
                    continue;
                }
                wheelRPM += wheel.wheelCollider.rpm;
            }
            float curGearRatio = gearSystem.GetCurrentRatio();
            wheelRPM /= NumberOfDrivingWheels;
            wheelRPM *= Mathf.Abs(curGearRatio) * differentialRatio;
            EngineRPM = Mathf.Lerp(
                EngineRPM,
                Mathf.Max(MinRPM-100, wheelRPM),
                Time.fixedDeltaTime * RPMSmoothness);
            
            torque = PowerCurve.Evaluate(EngineRPM / MaxRPM) * MaxMotorTorque / EngineRPM * curGearRatio * differentialRatio * 5252f * clutching;
        }
        // return currentTorque; // 200为基准扭矩值
        return torque;
    }

    private void ApplyMotor(){
        
        float throttle = InputManager.Instance.ThrottleInput;
        currentTorque = CalculateTorque(throttle);
        foreach (var wheel in wheels)
        {
            if (drivingType != DrivingType.Full && (int)drivingType != (int)wheel.axel)
            {
                continue;
            }
            wheel.wheelCollider.motorTorque = currentTorque * throttle * throttleForce;
        }

        CurrentSpeed = rb.velocity.magnitude * 3.6f;
    }

    private void ApplyBrake(){

        float brake = InputManager.Instance.BrakeInput;

        if (brake > 0)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = 0;
                wheel.wheelCollider.brakeTorque = brake * brakeForce * Time.fixedDeltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }
    private void RecoverClutching(){
        clutching = Mathf.Lerp(clutching, 1, Time.fixedDeltaTime * 5.0f);
    }

    private void HandleMovement()
    {
        ApplyBrake();
        ApplyMotor();
        RecoverClutching();
    }
    
    private void HandleSteering()
    {
        float steer = InputManager.Instance.SteerInput;
        float speedFactor = Mathf.Clamp01(gearSystem.GetMaxSpeed() / rb.velocity.magnitude);
        float steeringMultiplier = Mathf.Lerp(0.3f, 1f, speedFactor);
        // Debug.Log("steeringMultiplier" + steeringMultiplier);
        float steerAngle = steer * maxSteerAngle * steeringMultiplier;
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                wheel.wheelCollider.steerAngle =
                    Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, steeringSpeed * Time.fixedDeltaTime);

            }
        }
    }
    private void ApplyDrag()
    {
        if (isGrounded)
        {
            rb.drag = airDragCoeff * 0.05f * 1.225f * rb.velocity.magnitude * rb.velocity.magnitude;
        }
        else
        {
            rb.drag = 0;
        }
    }
    public void Respawn(Transform respawnPoint)
    {

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = 0;
            wheel.wheelCollider.brakeTorque = 0f;
            wheel.wheelCollider.steerAngle = 0;
        }
        rb.velocity = Vector3.zero;

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
    }

    public void OnGearChanged()
    {
        clutching = 0;
        StartCoroutine(GearShiftEffect());
    }

    private IEnumerator GearShiftEffect()
    {
        float originalForce = throttleForce;
        throttleForce *= 0.5f;

        yield return new WaitForSeconds(0.3f);

        throttleForce = originalForce;
    }
    private void AnimatedWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion quo;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out quo);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = quo * Quaternion.Euler(new Vector3(-90, 0, -90));
        }
    }


    private bool CheckDrifting(){
        if(rb.velocity.magnitude < minDriftSpeed) return false;
        Vector3 carForward = transform.forward;
        Vector3 velocityDir = rb.velocity.normalized;
        float angle = Vector3.Angle(carForward, velocityDir);
        bool isAngleDrifting = angle > driftAngleThreshold;

        int slippingWheels = 0;
        foreach (var wheel in wheels)
        {
            WheelHit hit;
            if (wheel.wheelCollider.GetGroundHit(out hit))
            {
                bool isSlip = Mathf.Abs(hit.sidewaysSlip) > sidewaysSlipThreshold || 
                              Mathf.Abs(hit.forwardSlip) > forwardSlipThreshold;
                if (isSlip) slippingWheels++;
            }
        }
        return isAngleDrifting && slippingWheels >= 2;
    }
    void StartUp(){
        carStatus = Status.On;
    }

    void Stall(){
        carStatus = Status.Off;
    }
}
