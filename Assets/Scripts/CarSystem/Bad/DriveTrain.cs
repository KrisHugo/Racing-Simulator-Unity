using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Axle
{
    public Transform alxeTrans;
    public bool isDriven;
    public bool canSteer;
    public Wheel leftWheel;
    public Wheel rightWheel;
    public DifferentialSystem differential;

    // public float GetRollingResistantForce()
    // {
    //     float left = leftWheel.GetRollingResistence();
    //     float right = rightWheel.GetRollingResistence();
    //     return left + right;
    // }

    // public float GetAvgWheelRPM(){
    //     return (leftWheel.RPM + rightWheel.RPM) / 2;
    // }

    public void Initialize(Rigidbody rb,float massPerAxle, float axleLength){
        
            Vector3 leftWheelPosition = new(alxeTrans.position.x - axleLength / 2f, alxeTrans.position.y, alxeTrans.position.z);
            Vector3 rightWheelPosition = new(alxeTrans.position.x + axleLength / 2f, alxeTrans.position.y, alxeTrans.position.z);

            leftWheel.Initialize(massPerAxle / 2f, leftWheelPosition, rb.transform.TransformPoint(leftWheelPosition));
            rightWheel.Initialize(massPerAxle / 2f, rightWheelPosition, rb.transform.TransformPoint(rightWheelPosition));
    }

    public void ApplyDriveTorque(float torque, float deltaTime)
    {
        if(!isDriven) return;

        if (differential != null)
        {
            differential.DistributeTorque(torque, leftWheel, rightWheel, deltaTime);
        }
        else
        {
            // 如果没有差速器，均等分配
            leftWheel.ApplyDriveTorque(torque * 0.5f, deltaTime);
            rightWheel.ApplyDriveTorque(torque * 0.5f, deltaTime);
        }
    }

    public void ApplySteeringAngle(float angle){
        if(!canSteer) return;
        // 实际要计算正确的角度差
        leftWheel.ApplySteeringAngle(angle);
        rightWheel.ApplySteeringAngle(angle);
    }

    public void ApplyBrakeForce(float brakeForce){
        leftWheel.ApplyBrakeTorque(brakeForce);
        rightWheel.ApplyBrakeTorque(brakeForce);
    }
}
public class DriveTrain : MonoBehaviour
{
    [SerializeField]
    public List<Axle> axles;
    public float AxleLength = 1.2f;


    private SuspensionSystem suspension;
    private Rigidbody rb;
    
    public void Initialize(){
        rb = GetComponent<Rigidbody>();
        suspension = GetComponent<SuspensionSystem>();

        

        float massPerAxle = rb.mass / axles.Count;

        foreach(var axle in axles){
            axle.Initialize(rb, massPerAxle, AxleLength);
        }
    }

    public void UpdateDrivetrain(float deltaTime)
    {

        foreach (var axle in axles)
        {
            
            bool isDriven = axle.isDriven;
            print(isDriven);

            suspension.UpdateSuspension(rb, axle.leftWheel);
            suspension.UpdateSuspension(rb, axle.rightWheel);

            axle.leftWheel.CalculateWheelToRigidbodyForces(rb, isDriven);
            axle.rightWheel.CalculateWheelToRigidbodyForces(rb, isDriven);

            axle.leftWheel.UpdateWheelVisual(deltaTime);
            axle.rightWheel.UpdateWheelVisual(deltaTime);
        }
    }

    public float CalculateDrivetrainLoad(float wheelRatio){

        float totalLoad = 0f;
        
        foreach (var axle in axles)
        {
            if (!axle.isDriven) continue;
            
            totalLoad += axle.leftWheel.AngularVelocity * axle.leftWheel.inertia / wheelRatio;
            totalLoad += axle.rightWheel.AngularVelocity * axle.rightWheel.inertia / wheelRatio;
        }
        
        // 应用离合器状态
        return totalLoad;
    }


    // public float GetLoadTorque()
    // {
    //     float loadTorque = 0;
    //     foreach (var axle in axles){
    //         if (axle.isDriven){
    //             loadTorque += axle.GetLoadTorque();
    //         }
    //     }
    //     return loadTorque;
    // }
}