using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ¥¥Ω®ScriptableObject¥Ê¥¢º¸Œª≈‰÷√
[CreateAssetMenu(menuName = "Input/InputSettings")]
public class InputSettings : ScriptableObject
{
    [Header("System Controls")]
    public KeyCode respawnKey = KeyCode.R;

    public KeyCode startUpKey = KeyCode.U;

    [Header("Basic Movement")]
    public KeyCode accelerateKey = KeyCode.W;
    public KeyCode brakeKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode handbrakeKey = KeyCode.Space;

    // ????????
    [Header("Gear Controls")]
    public KeyCode shiftUpKey = KeyCode.LeftShift;
    public KeyCode shiftDownKey = KeyCode.LeftControl;
    public KeyCode reverseKey = KeyCode.Tab; // ???????

    [Header("Mouse Settings")]
    public float mouseSteeringSensitivity = 0.5f;
    public bool isReversingVertical = false;

    [Header("Gamepad Settings")]
    public string throttleAxis = "Vertical";
    public string steerAxis = "Horizontal";
    public KeyCode gamepadBrake = KeyCode.JoystickButton0;
}