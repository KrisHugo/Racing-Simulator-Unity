using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputSettings defaultInputConfig;
    public InputSettings currentInputConfig;


    // 当前输入值的缓存
    public float SteeringInput { get; private set; }
    public float ThrottleInput { get; private set; }
    public float BrakeInput { get; private set; }

    [SerializeField] public bool StartUpInput { get; private set; }

    // [SerializeField] private InputSettings inputSettings;
    [SerializeField] public bool RespawnInput { get; private set; }
    // ��ǰ����ֵ
    // [SerializeField] public float ThrottleInput { get; private set; }
    // [SerializeField] public float SteerInput { get; private set; }
    // [SerializeField] public float BrakeInput { get; private set; }
    [SerializeField] public bool HandbrakeInput { get; private set; }


    // ������λ��������
    [SerializeField] public bool ShiftUpPressed { get; private set; }
    [SerializeField] public bool ShiftDownPressed { get; private set; }
    [SerializeField] public bool ReversePressed { get; private set; }

    [SerializeField] public float ViewVerticalInput { get; private set; }
    [SerializeField] public float ViewHorizontalInput { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize(defaultInputConfig);

        // 添加设备变更监听器
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }


    private void Update()
    {
        if (currentInputConfig == null) return;



        // 更新输入状态
        StartUpInput = currentInputConfig.IsEngineStartPressed();
        RespawnInput = currentInputConfig.IsRepawnPressed();
        SteeringInput = currentInputConfig.GetSteeringInput();
        ThrottleInput = currentInputConfig.GetThrottleInput();
        BrakeInput = currentInputConfig.GetBrakeInput();
        HandbrakeInput = currentInputConfig.IsHandbrakePressed();
        ShiftUpPressed = currentInputConfig.IsShiftUpPressed();
        ShiftDownPressed = currentInputConfig.IsShiftDownPressed();

        ViewHorizontalInput = currentInputConfig.GetViewHorizontalInput();
    }


    // 初始化输入配置
    public void Initialize(InputSettings config)
    {
        currentInputConfig = Instantiate(config);
    }
    
        
    // 响应设备变更
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log("Gamepad connected");
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log("Gamepad disconnected");
                    break;
                case InputDeviceChange.Reconnected:
                    Debug.Log("Gamepad reconnected");
                    break;
            }
        }
    }
    
    // 使用当前配置模拟输入
    public void SetActiveConfig(InputSettings newConfig)
    {
        currentInputConfig = Instantiate(newConfig);
    }

    // private void Update()
    // {
    //     //������
    //     // RespawnInput = Input.GetKey(currentInputConfig.respawnKey);

    //     // StartUpInput = Input.GetKey(currentInputConfig.startUpKey);

    //     // // ��������
    //     // float keyboardThrottle = Input.GetKey(currentInputConfig.accelerateKey) ? 1 : 0;


    //     // float keyboardBrake = Input.GetKey(currentInputConfig.brakeKey) ? 1 : 0;
    //     // float keyboardSteer =
    //     //     (Input.GetKey(currentInputConfig.leftKey) ? -1 : 0) +
    //     //     (Input.GetKey(currentInputConfig.rightKey) ? 1 : 0);

    //     // // ��Ϸ�ֱ�����
    //     // float gamepadThrottle = Mathf.Clamp01(currentInputConfig.throttleAxis);
    //     // float gamepadBrake = Mathf.Clamp01(-Input.GetAxis(currentInputConfig.throttleAxis));
    //     // float gamepadSteer = Input.GetAxis(inputSettings.steerAxis);


    //     // ��⵵λ����
    //     // ShiftUpPressed = Input.GetKeyDown(currentInputConfig.shiftUpKey);
    //     // ShiftDownPressed = Input.GetKeyDown(currentInputConfig.shiftDownKey);
    //     // ReversePressed = Input.GetKeyDown(currentInputConfig.reverseKey);


    //     // ���ת�򣨿�ѡ��

    //     // ������
    //     // ThrottleInput = Mathf.Clamp01(keyboardThrottle + gamepadThrottle);
    //     // BrakeInput = Mathf.Clamp01(keyboardBrake + gamepadBrake);
    //     // SteerInput = Mathf.Clamp(keyboardSteer + gamepadSteer, -1, 1);


    //     // HandbrakeInput = Input.GetKey(currentInputConfig.handbrakeKey);


    //     // Debug.Log($"Steer: {SteerInput}, Throttle: {ThrottleInput}, Brake: {BrakeInput}");
    // }
}
