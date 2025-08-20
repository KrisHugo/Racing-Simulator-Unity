using System.Collections.Generic;
using UnityEngine;

public class VehicleDamageSystem : MonoBehaviour
{
    private List<VehicleComponent> components = new();
    
    public VehicleComponent engine;
    public VehicleComponent wheels;
    public VehicleComponent suspension;
    public VehicleComponent transmission;
    
    private ResourceInventory resourceInventory;
    
    void Start()
    {
        resourceInventory = GetComponent<ResourceInventory>();
        
        // 初始化部件列表
        if (engine != null) components.Add(engine);
        if (wheels != null) components.Add(wheels);
        if (suspension != null) components.Add(suspension);
        if (transmission != null) components.Add(transmission);
    }
    
    public void ApplyTravelWear(float baseWear)
    {
        // 应用磨损
        engine?.ApplyWear(baseWear * 1.2f);
        wheels?.ApplyWear(baseWear * 0.8f);
        suspension?.ApplyWear(baseWear * 0.9f);
        transmission?.ApplyWear(baseWear * 1.1f);
    }
    
    public float GetOverallPerformanceFactor()
    {
        // 如果引擎损坏，性能为0
        if (engine != null && engine.IsFailed) return 0f;
        
        // 取所有部件性能系数的最小值
        float minFactor = 1f;
        foreach (var component in components)
        {
            float factor = component.GetPerformanceFactor();
            if (factor < minFactor) minFactor = factor;
        }
        
        return minFactor;
    }
    
    // 修理指定部件
    public bool RepairComponent(string componentName, float repairAmount = 25f)
    {
        VehicleComponent componentToRepair = components.Find(c => c.componentName == componentName);
        if (componentToRepair != null && !componentToRepair.IsFailed)
        {
            componentToRepair.Repair(repairAmount);
            return true;
        }
        return false;
    }
    
    // 使用资源修复损坏的部件
    public bool FixBrokenComponent(string componentName)
    {
        if (resourceInventory == null) return false;
        
        VehicleComponent componentToFix = components.Find(c => c.componentName == componentName && c.IsFailed);
        if (componentToFix != null)
        {
            // 根据部件类型决定需要的资源
            string requiredResource = "";
            int resourceCost = 0;
            
            switch(componentName.ToLower())
            {
                case "engine":
                    requiredResource = "electronics";
                    resourceCost = 3;
                    break;
                case "wheels":
                    requiredResource = "rubber";
                    resourceCost = 2;
                    break;
                case "suspension":
                    requiredResource = "scrap_metal";
                    resourceCost = 4;
                    break;
                case "transmission":
                    requiredResource = "mechanical_parts";
                    resourceCost = 5;
                    break;
            }
            
            // 检查资源是否足够并完成维修
            if (!string.IsNullOrEmpty(requiredResource) && 
                resourceInventory.HasResources(requiredResource, resourceCost))
            {
                resourceInventory.UseResources(requiredResource, resourceCost);
                componentToFix.currentDurability = componentToFix.failureThreshold + 1; // 刚好修复到故障阈值之上
                return true;
            }
        }
        return false;
    }
}