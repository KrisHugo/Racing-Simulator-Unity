using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] public bool StartUpInput{get; private set;}

    [SerializeField] private InputSettings inputSettings;
    [SerializeField] public bool RespawnInput {get; private set;}
    // 当前输入值
    [SerializeField] public float ThrottleInput { get; private set; }
    [SerializeField] public float SteerInput { get; private set; }
    [SerializeField] public float BrakeInput { get; private set; }
    [SerializeField] public bool HandbrakeInput { get; private set; }


    // 新增档位输入属性
    [SerializeField] public bool ShiftUpPressed { get; private set; }
    [SerializeField] public bool ShiftDownPressed { get; private set; }
    [SerializeField] public bool ReversePressed { get; private set; }

    [SerializeField] public float ViewVerticalInput {get; private set; }
    [SerializeField] public float ViewHorizontalInput {get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        //最优先
        RespawnInput = Input.GetKey(inputSettings.respawnKey);

        StartUpInput = Input.GetKey(inputSettings.startUpKey);

        // 键盘输入
        float keyboardThrottle = Input.GetKey(inputSettings.accelerateKey) ? 1 : 0;
        

        float keyboardBrake = Input.GetKey(inputSettings.brakeKey) ? 1 : 0;
        float keyboardSteer =
            (Input.GetKey(inputSettings.leftKey) ? -1 : 0) +
            (Input.GetKey(inputSettings.rightKey) ? 1 : 0);

        // 游戏手柄输入
        float gamepadThrottle = Mathf.Clamp01(Input.GetAxis(inputSettings.throttleAxis));
        float gamepadBrake = Mathf.Clamp01(-Input.GetAxis(inputSettings.throttleAxis));
        float gamepadSteer = Input.GetAxis(inputSettings.steerAxis);
        

        // 检测档位输入
        ShiftUpPressed = Input.GetKeyDown(inputSettings.shiftUpKey);
        ShiftDownPressed = Input.GetKeyDown(inputSettings.shiftDownKey);
        ReversePressed = Input.GetKeyDown(inputSettings.reverseKey);


        // 鼠标转向（可选）
        float mouseSteer = Input.GetAxis("Mouse X") * inputSettings.mouseSteeringSensitivity;

        // 输入混合
        ThrottleInput = Mathf.Clamp01(keyboardThrottle + gamepadThrottle);
        BrakeInput = Mathf.Clamp01(keyboardBrake + gamepadBrake);
        SteerInput = Mathf.Clamp(keyboardSteer + gamepadSteer, -1, 1);

        ViewHorizontalInput = Mathf.Clamp(mouseSteer, -1, 1);

        HandbrakeInput = Input.GetKey(inputSettings.handbrakeKey) ||
                        Input.GetKey(inputSettings.gamepadBrake);


        // Debug.Log($"Steer: {SteerInput}, Throttle: {ThrottleInput}, Brake: {BrakeInput}");
    }
}
