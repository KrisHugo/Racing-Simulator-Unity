using UnityEngine;


[System.Serializable]
public class EngineFeature
{
    public float idleRPM = 800f;
    public float redlineRPM = 6500f;
    public float maxRPM = 7000f;
    public float maxTorque = 350f; // Nm
    public float inertia = 0.3f; // kg·m²
    public AnimationCurve torqueCurve = new(
        new Keyframe(0, 0.2f),    // 低转速扭矩
        new Keyframe(0.3f, 0.9f),  // 提高爬坡能力
        new Keyframe(0.6f, 1.0f),  // 峰值扭矩
        new Keyframe(0.85f, 0.8f), // 高转区保持
        new Keyframe(1.0f, 0.5f),   // 红区下降
        new Keyframe(1.1f, 0.1f)
    );

    // 根据扭矩曲线获取扭矩系数
    public float GetRPMFactor(float currentRPM)
    {
        return (currentRPM - idleRPM) / maxRPM - idleRPM;
    }

    public float GetRPMClamp(float currentRPM)
    {
        return Mathf.Clamp(currentRPM, idleRPM * 0.5f, maxRPM);
    }

    public float GetInverseLerpRPM(float currentRPM){
        return Mathf.InverseLerp(idleRPM, maxRPM, currentRPM);
    }

    public float GetTorqueAtRPM(float currentRPM)
    {

        return maxTorque * torqueCurve.Evaluate(GetInverseLerpRPM(currentRPM));
    }
}

public class EngineSystem : MonoBehaviour
{

    [Header("Engine Status")]
    public float CurrentRPM {get; private set;}
    public float rpmDamper = 4.0f;
    [SerializeField]
    public EngineFeature feature;

    public void Initialize()
    {
        // 初始化引擎状态
        CurrentRPM = 0f;
    }

    public void UpdateEngineRPMbyWheelAvgRpm(float wheelAvgRpm, float currentGearRatio, float finalGearRatio, float deltaTime){
        //smoothing
        CurrentRPM = feature.GetRPMClamp(Mathf.Lerp(CurrentRPM * currentGearRatio * finalGearRatio, wheelAvgRpm, deltaTime * rpmDamper));
    }

    public float CalculateEngineTorque(float currentRPM, int currentGearRatio, int finalGearRatio){
        // 计算当前引擎输出扭矩
        float outputTorque = feature.GetTorqueAtRPM(currentRPM);
        // 计算净扭矩 (输出扭矩 * 当前档位比 * 最终传动比)
        return outputTorque * currentGearRatio * finalGearRatio;
    }


}
