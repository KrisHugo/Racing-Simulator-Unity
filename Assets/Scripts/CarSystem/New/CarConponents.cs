using UnityEngine;

[System.Serializable]
public class VehicleComponent
{
    public string componentName;
    public float maxDurability = 100f;
    public float currentDurability = 100f;
    public float failureThreshold = 20f;
    public float performanceImpact = 1f;
    
    public bool IsFailed => currentDurability <= failureThreshold;
    public float HealthPercentage => Mathf.Clamp01(currentDurability / maxDurability);
    
    // 应用磨损
    public void ApplyWear(float amount)
    {
        currentDurability = Mathf.Clamp(currentDurability - amount, 0, maxDurability);
    }
    
    // 修复部件
    public void Repair(float amount, bool useResources = false)
    {
        currentDurability = Mathf.Clamp(currentDurability + amount, 0, maxDurability);
    }
    
    // 计算性能影响系数 (0.0-1.0)
    public float GetPerformanceFactor()
    {
        if (IsFailed) return 0f;
        
        float healthPercentage = HealthPercentage;
        // 健康高于40%时性能完全发挥，之后线性下降
        return healthPercentage > 0.4f ? 1f : Mathf.Lerp(0f, 1f, healthPercentage / 0.4f);
    }
}