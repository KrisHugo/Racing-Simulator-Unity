using UnityEngine;

public class BrakingSystem : MonoBehaviour{

    public float brakingRate = 0.7f;
    // private     
    public float brakeForce = 3000f;
    private float currentBrake;
    private bool isHandbrakeOn;

    private DriveTrain driveTrain;

    public void Initialize(){
        driveTrain = GetComponent<DriveTrain>();

    }

    public void UpdateBrakes(){

        ApplyBrakes();
        if(isHandbrakeOn){
            ApplyHandBrakes();
        }
    }

    public void SetInput(float brake, bool handbrake){
        currentBrake = brake;
        isHandbrakeOn = handbrake;
    }

    void ApplyBrakes()
    {
        foreach (var group in driveTrain.axles)
        {  
            if(group.canSteer){
                group.ApplyBrakeForce(currentBrake * brakeForce * brakingRate);
            }
            else{
                
                group.ApplyBrakeForce(currentBrake * brakeForce * (1-brakingRate));
            }
        }
    }
    void ApplyHandBrakes(){

        foreach (var group in driveTrain.axles)
        {   
            group.ApplyBrakeForce(currentBrake * brakeForce * 1.2f);
        }
    }

    void ClearBrakes(){
        foreach (var group in driveTrain.axles)
        { 
            group.ApplyBrakeForce(0);
        }
    }

}