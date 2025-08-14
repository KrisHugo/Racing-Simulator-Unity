using UnityEngine;


[CreateAssetMenu(menuName = "Car/GroundFrictions", order = 0)]
public class GroundFrictions : ScriptableObject {
    
    // 不同地面的摩擦系数
    public float asphaltStiffness = 2.0f;
    public float dirtStiffness = 1.3f;
    public float grassStiffness = 0.5f;
    public float defaultStiffness = 1.0f;

    public float GetFrictionCoeff(string tag){
        if (tag == "Asphalt") {
            return asphaltStiffness;
        } else if (tag == "Dirt") {
            return dirtStiffness;
        } else if (tag == "Grass") {
            return grassStiffness;
        } else {
            return defaultStiffness;
        }
    }
}