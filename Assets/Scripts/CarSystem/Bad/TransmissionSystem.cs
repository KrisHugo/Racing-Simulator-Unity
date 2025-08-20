using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public enum GearState
{
    Reverse,
    Neutral,
    Drive
}
[System.Serializable]
public class GearFeatures
{

    public float finalDriveRatio = 3.8f;
    public float[] gearRatios = { 3.8f, 2.5f, 1.8f, 1.5f, 1.1f, 0.9f }; // 齿轮比数组（1-5挡）
    public float reverseRatio = 3.5f; // 倒挡比
    public float gearShiftDelay = 0.5f; // 换挡延迟
    [Range(0, 1)] public float efficiency = 0.95f;
}

public class TransmissionSystem : MonoBehaviour
{
    [Header("Gear Settings")]
    [SerializeField]
    public GearFeatures features;
    public bool isAutoShifting = false;
    private int currentGear = 1; // 当前挡位
    private float lastShiftTime = -1f; // 上次换挡时间
    public bool IsShifting { get; private set; } = false; // 是否正在换挡
    public float clutchEngagement = 1f;
    public GearState CurrentGearState
    {
        get
        {
            if (currentGear == 0) return GearState.Neutral;
            else if (currentGear < 0) return GearState.Reverse;
            else return GearState.Drive;
        }
    }
    public int CurrentGear
    {
        get
        {
            return currentGear;
        }
    }
    public float CurrentGearRatio
    {
        get
        {
            if (currentGear > 0)
            {
                return features.gearRatios[currentGear - 1];
            }
            else if (currentGear < 0)
            {
                return -features.reverseRatio;
            }
            else
            {
                return 0;
            }
        }
    }

    private OldEngineSystem engine;
    private DriveTrain drivetrain; 
    // private DifferentialSystem differentialSystem;
    // private WheelController wheelController;
    // private List<WheelGroup> wheelGroups;

    [SerializeField]
    private float testWheelTorque;
    public void Initialize()
    {
        engine = GetComponent<OldEngineSystem>();
        drivetrain = GetComponent<DriveTrain>();
    }
    public void HandleGearShifting(float deltaTime)
    {
        if (IsShifting)
        {
            lastShiftTime -= deltaTime;
            if (lastShiftTime <= 0)
            {
                IsShifting = false;
            }
            return;
        }

        // // 自动换挡逻辑
        // if (isAutoShifting && currentGear > 0 && currentGear <= features.gearRatios.Length)
        // {
        //     if (engine.CurrentRPM > engine.feature.maxRPM - 100f)
        //     {
        //         ShiftGear(1);
        //     }
        //     else if (engine.CurrentRPM < engine.feature.idleRPM + 100f)
        //     {
        //         ShiftGear(-1);
        //     }
        // }

    }

    // 有序换挡操作
    public void ShiftGearSerialized(int direction)
    {
        // 检查换挡冷却时间
        if (IsShifting || (direction > 0 && currentGear >= features.gearRatios.Length) || direction < 0 && currentGear <= -1) return;

        if (direction > 0) // 升挡
        {
            StartShift(currentGear + 1);
        }
        else if (direction < 0) // 降挡 
        {
            StartShift(currentGear = 1);
        }
    }

    public void ShiftSpecificGear(int gear)
    {
        if (gear < -1 || gear > features.gearRatios.Length) return;
        StartShift(gear);
    }

    private void StartShift(int targetGear)
    {
        lastShiftTime = features.gearShiftDelay;
        IsShifting = true;
        currentGear = targetGear;
    }
    public float GetLoadTorque(){

        // 1. 计算LoadTorque
        float loadTorque = drivetrain.CalculateDrivetrainLoad(CurrentGearRatio * features.finalDriveRatio * features.efficiency); 
        
        return Mathf.Lerp(0, loadTorque, clutchEngagement);
    }

    //更新车轮组的传动扭矩
    public void UpdateWheelTorque(float deltaTime)
    {
        if (Mathf.Abs(CurrentGearRatio) <= 0.0001f) return; // Neutral or invalid
        // 2. 计算引擎输出扭矩
        // 3. 转换到轮端
        float wheelTorque = engine.CalculateEngineTorque() * CurrentGearRatio * features.finalDriveRatio * features.efficiency;
        testWheelTorque = wheelTorque;
        // 4. 分配到各车轮组
        foreach (var axle in drivetrain.axles)
        {
            if(axle.isDriven){
                axle.ApplyDriveTorque(wheelTorque, deltaTime);
            }
        }
    }


}
