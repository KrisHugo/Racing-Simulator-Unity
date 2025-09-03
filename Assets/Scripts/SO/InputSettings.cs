using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ����ScriptableObject�洢��λ����
[CreateAssetMenu(menuName = "Input/InputSettings")]
public class InputSettings : ScriptableObject
{
    // [Header("System Controls")]
    // public KeyCode respawnKey = KeyCode.R;

    // public KeyCode startUpKey = KeyCode.U;

    // [Header("Basic Movement")]
    // public KeyCode accelerateKey = KeyCode.W;
    // public KeyCode brakeKey = KeyCode.S;
    // public KeyCode leftKey = KeyCode.A;
    // public KeyCode rightKey = KeyCode.D;
    // public KeyCode handbrakeKey = KeyCode.Space;

    // ????????
    // [Header("Gear Controls")]
    // public KeyCode shiftUpKey = KeyCode.LeftShift;
    // public KeyCode shiftDownKey = KeyCode.LeftControl;
    // public KeyCode reverseKey = KeyCode.Tab; // ???????


    // [Header("Gamepad Settings")]
    // public string throttleAxis = "Accelerator";
    // public string steerAxis = "Horizontal";
    // public KeyCode gamepadBrake = KeyCode.JoystickButton0;



    [Header("Steering Controls")]
    [Tooltip("主转向控制")] public InputActionReference steeringAxis;
    [Tooltip("转向灵敏度曲线")] public AnimationCurve steeringSensitivity = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0.01f, 0.5f)] public float steeringDeadZone = 0.1f;

    [Header("Acceleration Controls")]
    [Tooltip("油门控制")] public InputActionReference throttleAxis;
    [Tooltip("刹车控制")] public InputActionReference brakeAxis;
    [Tooltip("油门/刹车灵敏度曲线")] public AnimationCurve accelerationSensitivity = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0.01f, 0.3f)] public float triggerDeadZone = 0.05f;

    [Header("Misc Controls")]
    [Tooltip("手刹控制")] public InputActionReference handbrakeAction;
    [Tooltip("启动引擎按钮")] public InputActionReference engineStartAction;
    [Tooltip("切换视角按钮")] public InputActionReference cameraSwitchAction;
    [Tooltip("视角转向灵敏度曲线")] public AnimationCurve viewSensitivity = AnimationCurve.Linear(0, 0, 1, 1);
    public InputActionReference viewSteeringAxis;
    public bool isReversingVertical = false;

    [Header("Gear Controls")]
    [Tooltip("升档控制")] public InputActionReference gearUpAction;
    [Tooltip("降档控制")] public InputActionReference gearDownAction;
    [Tooltip("换挡模式 (自动/手动)")] public InputActionReference gearModeSwitch;

    [Header("Haptics")]
    [Tooltip("发动机震动强度")][Range(0, 1)] public float engineVibrationStrength = 0.3f;
    [Tooltip("碰撞震动强度")][Range(0, 1)] public float collisionVibrationStrength = 0.8f;

    [Header("Respawn Settings")]
    [Tooltip("复活按键")] public InputActionReference respawnAction;
    [Tooltip("复活延迟")] public float respawnDelay = 3f;

    // 获取处理后的输入值
    public float GetSteeringInput()
    {
        float rawValue = steeringAxis.action.ReadValue<float>();
        if (Mathf.Abs(rawValue) < steeringDeadZone) return 0f;
        return Mathf.Sign(rawValue) * steeringSensitivity.Evaluate(Mathf.Abs(rawValue));
    }

    public float GetThrottleInput()
    {
        float rawValue = throttleAxis.action.ReadValue<float>();
        // if (rawValue < triggerDeadZone) return 0f;
        return accelerationSensitivity.Evaluate(rawValue);
    }

    public float GetBrakeInput()
    {
        float rawValue = brakeAxis.action.ReadValue<float>();
        if (rawValue < triggerDeadZone) return 0f;
        return accelerationSensitivity.Evaluate(rawValue);
    }

    public float GetViewHorizontalInput()
    {
        float rawValue = viewSteeringAxis.action.ReadValue<float>();
        if (Mathf.Abs(rawValue) < steeringDeadZone) return 0f;
        return Mathf.Sign(rawValue) * viewSensitivity.Evaluate(Mathf.Abs(rawValue));
    }
    // 简化Action检查的方法
    public bool IsHandbrakePressed() => handbrakeAction.action.ReadValue<float>() > 0.5f;
    public bool IsEngineStartPressed() => engineStartAction.action.triggered;
    public bool IsRepawnPressed() => respawnAction.action.triggered;
    public bool IsCameraSwitchPressed() => cameraSwitchAction.action.triggered;

    public bool IsShiftUpPressed() => gearUpAction.action.triggered;
    public bool IsShiftDownPressed() => gearDownAction.action.triggered;

    // 力反馈控制
    public void SetGamepadVibration(float leftMotor, float rightMotor)
    {
        Gamepad.current?.SetMotorSpeeds(leftMotor, rightMotor);
    }
    
}