using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GearState
{
    Reverse,
    Neutral,
    Drive
}

public class GearSystem : MonoBehaviour
{
    [Header("Gear Settings")]
    [SerializeField] private int maxGears = 6;
    [SerializeField] private float[] gearRatios = { 4.17f, 3.14f, 2.11f, 1.67f, 1.28f, 1f };
    [SerializeField] private float shiftDelay = 0.5f;


    
    [Header("Reverse Settings")]
    [SerializeField] private float reverseRatio = 4f;
    [SerializeField] private float reverseMaxSpeed = 40f;

    public int currentGear { get; private set; } = 1;
    public GearState CurrentGearState { get; private set; } = GearState.Drive;
    public bool IsShifting { get; private set; }

    private CarMovement car;
    private float shiftTimer;

    private void Awake()
    {
        car = GetComponent<CarMovement>();
    }

    private void Update()
    {
        if (car.carStatus == CarMovement.Status.On){
            HandleGearInput();
            UpdateShiftTimer();
        }
    }

    private void HandleGearInput()
    {
        if (IsShifting) return;

        // 升档逻辑
        if (InputManager.Instance.ShiftUpPressed)
        {
            if(CurrentGearState == GearState.Drive){
                ShiftGear(1);
            }
            else if(CurrentGearState == GearState.Reverse){
                ToggleReverse();
                return;
            }
        }

        // 降档逻辑
        if (InputManager.Instance.ShiftDownPressed)
        {
            if(CurrentGearState == GearState.Drive && currentGear > 1){
                ShiftGear(-1);
            }
            else if(CurrentGearState == GearState.Drive && currentGear == 1){
                ToggleReverse();
                return;
            }
        }
    }

    private void ToggleReverse()
    {
        CurrentGearState = (CurrentGearState == GearState.Reverse) ? 
            GearState.Drive : 
            GearState.Reverse;
        
        currentGear = 1;
        IsShifting = true;
        shiftTimer = shiftDelay;
        car.OnGearChanged();
    }

    private void ShiftGear(int direction)
    {
        int newGear = Mathf.Clamp(currentGear + direction, 1, maxGears);
        
        if (newGear != currentGear)
        {
            currentGear = newGear;
            IsShifting = true;
            shiftTimer = shiftDelay;
            car.OnGearChanged();
        }
    }

    private void UpdateShiftTimer()
    {
        if (IsShifting)
        {
            shiftTimer -= Time.deltaTime;
            if (shiftTimer <= 0) IsShifting = false;
        }
    }

    public float GetCurrentRatio()
    {
        return CurrentGearState switch
        {
            GearState.Reverse => -reverseRatio,
            GearState.Neutral => 0,
            _ => gearRatios[currentGear - 1]
        };
    }

    public float GetMaxSpeed(){
        
        return CurrentGearState switch
        {
            GearState.Reverse => reverseMaxSpeed,
            _ => car.BaseMaxSpeed * (1f + (gearRatios.Length * 0.2f))
        };
    }

    // 在GearSystem中添加扭矩计算
    // public float CalculateTorque(AnimationCurve RPMCurve, float engineRPM, float maxRPM, float maxMotorTorque, float finalDriveRatio, float clutching)
    // {
    // }

}
