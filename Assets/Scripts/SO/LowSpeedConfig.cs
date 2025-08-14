using UnityEngine;

[CreateAssetMenu(menuName = "Car/LowSpeedCOnfig", order = 0)]
public class LowSpeedConfig : ScriptableObject{
    public float lowSpeedThreshold = 0.1f;
    public float stopDamping = 5.0f; // Damping factor for angular velocity
    public float minAngularVelocity = 0.5f; // Minimum angular velocity to prevent jittering

    public bool IsLowSpeed(float velocity){
        return Mathf.Abs(velocity) <= lowSpeedThreshold;
    }
}
