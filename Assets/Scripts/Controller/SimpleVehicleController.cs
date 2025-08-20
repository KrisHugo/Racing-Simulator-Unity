using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController3 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float enginePower = 30f;
    public float maxSpeed = 15f;
    public float steeringPower = 2f;
    public float brakePower = 25f;
    
    public Transform[] raycastPoints;
    public LayerMask groundLayer;
    public float raycastDistance = 1f;
    public float hoverHeight = 0.5f;
    public float suspensionForce = 50f;
    public float suspensionDamping = 5f;
    
    private Rigidbody rb;
    private float verticalInput;
    private float horizontalInput;
    private bool isBraking;
    private float currentSpeed;
    
    [Header("Performance Factors")]
    public float performanceFactor = 1f;
    private float previousSpeed;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.down * 0.5f;
    }
    
    void Update()
    {
        GetPlayerInput();
    }
    
    void FixedUpdate()
    {
        ApplySuspension();
        ApplyMovement();
        CalculateWear();
    }
    
    private void GetPlayerInput()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.Space);
    }
    
    private void ApplySuspension()
    {
        foreach (var point in raycastPoints)
        {
            if (Physics.Raycast(point.position, -transform.up, out RaycastHit hit, raycastDistance, groundLayer))
            {
                float currentDistance = Vector3.Distance(point.position, hit.point);
                float forceDirection = 1 - (currentDistance / hoverHeight);
                
                Vector3 forceVector = transform.up * (forceDirection * suspensionForce);
                Vector3 dampingVector = -rb.GetPointVelocity(point.position) * suspensionDamping;
                
                rb.AddForceAtPosition(forceVector + dampingVector, point.position);
            }
        }
    }
    
    private void ApplyMovement()
    {
        // 向前/向后移动
        if (verticalInput != 0)
        {
            float adjustedPower = enginePower * performanceFactor * verticalInput;
            rb.AddForce(transform.forward * adjustedPower, ForceMode.Acceleration);
        }
        
        // 转向
        if (horizontalInput != 0 && rb.linearVelocity.magnitude > 0.1f)
        {
            float adjustedSteering = steeringPower * performanceFactor * Mathf.Sign(verticalInput);
            rb.AddTorque(adjustedSteering * horizontalInput * Vector3.up, ForceMode.Acceleration);
        }
        
        // 限制最大速度
        if (rb.linearVelocity.magnitude > maxSpeed * performanceFactor)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed * performanceFactor);
        }
        
        // 刹车
        if (isBraking)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                rb.AddForce(-rb.linearVelocity.normalized * brakePower, ForceMode.Acceleration);
            }
        }
        
        // 计算速度变化
        currentSpeed = rb.linearVelocity.magnitude;
    }
    
    private void CalculateWear()
    {
        float speedDifference = Mathf.Abs(currentSpeed - previousSpeed);
        previousSpeed = currentSpeed;
        
        // 获取地面类型
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, -Vector3.up, out groundHit, raycastDistance, groundLayer))
        {
            
            float wearMultiplier = GetWearMultiplier();
            VehicleDamageSystem damageSystem = GetComponent<VehicleDamageSystem>();
            if (damageSystem != null)
            {
                float baseWear = Mathf.Abs(verticalInput) * 0.1f + 
                                Mathf.Abs(horizontalInput) * 0.05f +
                                (isBraking ? 0.05f : 0f) +
                                speedDifference * 0.1f;
                
                damageSystem.ApplyTravelWear(baseWear * wearMultiplier * Time.fixedDeltaTime);
            }
        }
    }
    
    private float GetWearMultiplier()
    {
        
        return 1.0f;
    }
}