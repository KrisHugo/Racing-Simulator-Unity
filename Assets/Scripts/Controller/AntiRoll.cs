using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRoll : MonoBehaviour
{
    private CarMovement car;
    private Rigidbody rb;

    // private float multiplier = 0.1f;
    [Header("Anti-Roll Settings")]
    [SerializeField] private float Anti_Roll_Force = 40_000;

    private WheelCollider colliderL = null;
    private WheelCollider colliderR = null;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<CarMovement>();
        rb = GetComponent<Rigidbody>();
        foreach(var wheel in car.wheels){
            if(wheel.axel == CarMovement.Axel.Rear && wheel.tyreSide == CarMovement.TyreSide.Left){
                colliderL = wheel.wheelCollider;
            }
            if(wheel.axel == CarMovement.Axel.Rear && wheel.tyreSide == CarMovement.TyreSide.Right){
                colliderR = wheel.wheelCollider;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ApplyAntiRoll();
    }

    
    private void ApplyAntiRoll(){
        WheelHit hit = new WheelHit();
        float travelL = 1.0f;
        float travelR = 1.0f;
        bool groundL = false;
        bool groundR = false;
        if(colliderL != null){
            groundL = colliderL.GetGroundHit(out hit);
            if(groundL){
                travelL = (-colliderL.transform.InverseTransformPoint(hit.point).y - colliderL.radius) / colliderL.suspensionDistance;
            }
        }
        if(colliderR != null)
            groundR = colliderR.GetGroundHit(out hit);
            if(groundR){
                travelR = (-colliderR.transform.InverseTransformPoint(hit.point).y - colliderR.radius) / colliderR.suspensionDistance;
            }
        var antiForce = (travelL - travelR) * Anti_Roll_Force;
        if(groundL){
            rb.AddForceAtPosition(colliderL.transform.up * - antiForce, colliderL.transform.position);
        }
        if(groundR){
            rb.AddForceAtPosition(colliderR.transform.up * - antiForce, colliderR.transform.position);
        }
    }
}
