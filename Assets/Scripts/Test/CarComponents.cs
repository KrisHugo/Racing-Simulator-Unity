using UnityEngine;
using System.Collections.Generic;

// public class AdvancedCarPhysics : MonoBehaviour
// {
//     // 引擎系统参数
//     [Header("Engine System")]
//     public float maxTorque = 600f;      // 最大扭矩（牛·米）
//     public float maxRPM = 7000f;        // 最大转速
//     public float idleRPM = 800f;        // 怠速转速
//     public AnimationCurve torqueCurve;  // 扭矩曲线（RPM->扭矩）
//     public float engineInertia = 0.3f;  // 发动机转动惯量
//     private float currentRPM;           // 当前转速
//     private float throttleInput;        // 油门输入(0-1)

//     // 变速箱参数
//     [Header("Transmission")]
//     public float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f }; // 档位齿比
//     public float finalDriveRatio = 3.2f;   // 主减速比
//     public float shiftTime = 0.3f;         // 换档时间
//     private int currentGear = 1;           // 当前档位
//     private float shiftProgress = 0f;      // 换档进度

//     // 离合器参数
//     [Header("Clutch System")]
//     public float clutchEngagement = 0f;    // 离合器接合度(0-1)
//     public float maxClutchTorque = 1000f;  // 最大传递扭矩
//     public float clutchSmoothTime = 0.2f;  // 离合器响应时间

//     // 差速器参数
//     [Header("Differential")]
//     public DifferentialType diffType = DifferentialType.LimitedSlip;
//     public float diffRatio = 3.5f;         // 差速器齿比
//     public float maxBiasTorque = 200f;     // 最大差速扭矩

//     // 悬挂系统参数
//     [Header("Suspension")]
//     public float suspensionRestDist = 0.5f;  // 悬挂自然长度
//     public float springStiffness = 35000f;   // 弹簧刚度
//     public float damperStiffness = 5000f;    // 阻尼系数
//     public float tireRadius = 0.35f;         // 轮胎半径

//     // 转向系统参数
//     [Header("Steering")]
//     public float maxSteerAngle = 35f;        // 最大转向角
//     public float steerSpeed = 3f;            // 转向速度
//     private float currentSteerAngle;         // 当前转向角

//     // 车轮配置
//     [System.Serializable]
//     public struct WheelConfig
//     {
//         public Transform wheelVisual;
//         public Vector3 localPosition;
//         public bool isSteering;
//         public bool isDriven;
//         public float tireFriction;
//     }

//     public WheelConfig[] wheels;
//     public List<WheelState> wheelStates = new List<WheelState>();

//     // 物理组件
//     private Rigidbody carRigidbody;
    
//     void Start()
//     {
//         carRigidbody = GetComponent<Rigidbody>();
//         InitializeWheels();
//     }

//     void InitializeWheels()
//     {
//         foreach(var wheel in wheels)
//         {
//             WheelState state = new()
//             {
//                 localPosition = wheel.localPosition,
//                 restPosition = transform.TransformPoint(wheel.localPosition),
//                 springVelocity = 0f,
//                 compression = 0f,
//                 steerAngle = 0f,
//                 angularVelocity = 0f,
//                 isGrounded = false
//             };
//             wheelStates.Add(state);
//         }
//     }

//     void Update()
//     {
//         // 获取玩家输入
//         float verticalInput = Input.GetAxis("Vertical");
//         float horizontalInput = Input.GetAxis("Horizontal");

//         // 引擎控制系统
//         UpdateEngine(verticalInput);
        
//         // 变速箱控制
//         HandleTransmission();
        
//         // 转向系统
//         UpdateSteering(horizontalInput);
        
//         // 更新车轮视觉旋转
//         UpdateWheelVisuals();
//     }

//     void FixedUpdate()
//     {
//         // 物理状态更新
//         UpdateSuspension();
//         ApplyTireForces();
//         ApplyEngineTorque();
//         ApplyDragForces();
//     }

//     // 引擎系统更新
//     void UpdateEngine(float throttle)
//     {
//         throttleInput = Mathf.Clamp01(throttle);
        
//         // 计算引擎负载扭矩
//         float loadTorque = CalculateDrivetrainLoad();
        
//         // 物理模拟引擎转速
//         float acceleration = (throttle * GetTorqueAtRPM(currentRPM) - loadTorque) / engineInertia;
//         currentRPM += acceleration * Time.deltaTime * 60f; // 转换为RPM
        
//         // 确保转速在合理范围内
//         currentRPM = Mathf.Clamp(currentRPM, idleRPM * 0.7f, maxRPM * 1.05f);
//     }

//     // 基于扭矩曲线获取当前转速下的扭矩
//     float GetTorqueAtRPM(float rpm)
//     {
//         float normalizedRPM = Mathf.Clamp01(rpm / maxRPM);
//         return torqueCurve.Evaluate(normalizedRPM) * maxTorque;
//     }

//     // 变速箱控制
//     void HandleTransmission()
//     {
//         if (shiftProgress > 0)
//         {
//             shiftProgress -= Time.deltaTime / shiftTime;
//             clutchEngagement = 1f - shiftProgress;
//             return;
//         }

//         clutchEngagement = Mathf.Clamp01(clutchEngagement);

//         // // 自动换挡逻辑
//         // if (currentRPM > maxRPM * 0.9f && currentGear < gearRatios.Length)
//         // {
//         //     ShiftGear(currentGear + 1);
//         // }
//         // else if (currentRPM < idleRPM * 1.5f && currentGear > 1)
//         // {
//         //     ShiftGear(currentGear - 1);
//         // }
//     }

//     void ShiftGear(int newGear)
//     {
//         currentGear = newGear;
//         shiftProgress = 1f;
//         clutchEngagement = 0f;
//     }

//     // 差速器负载计算
//     float CalculateDrivetrainLoad()
//     {
//         float totalLoad = 0f;
//         for (int i = 0; i < wheelStates.Count; i++)
//         {
//             if (!wheels[i].isDriven) continue;
            
//             // 计算单个驱动轮的传动负载
//             float wheelRatio = gearRatios[currentGear - 1] * finalDriveRatio * diffRatio;
//             float wheelInertia = carRigidbody.mass * tireRadius * 0.5f;
            
//             totalLoad += wheelStates[i].angularVelocity * wheelInertia / wheelRatio;
//         }
        
//         // 应用离合器状态
//         return Mathf.Lerp(0f, totalLoad, clutchEngagement);
//     }

//     // 转向系统更新
//     void UpdateSteering(float input)
//     {
//         float targetAngle = input * maxSteerAngle;
//         currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, steerSpeed * Time.deltaTime);
        
//         // 应用转向角度到对应车轮
//         for (int i = 0; i < wheelStates.Count; i++)
//         {
//             if (wheels[i].isSteering)
//             {
//                 wheelStates[i].steerAngle = currentSteerAngle;
//             }
//         }
//     }

//     // 悬挂系统物理计算
//     void UpdateSuspension()
//     {
//         for (int i = 0; i < wheelStates.Count; i++)
//         {
//             WheelState state = wheelStates[i];
//             Vector3 worldPos = transform.TransformPoint(state.localPosition);
            
//             // 车轮射线检测
//             if (Physics.Raycast(worldPos, -transform.up, out RaycastHit hit, 
//                 suspensionRestDist + tireRadius))
//             {
//                 state.isGrounded = true;
//                 state.compression = 1f - (hit.distance - tireRadius) / suspensionRestDist;
                
//                 // 弹簧力计算
//                 float springForce = springStiffness * state.compression;
                
//                 // 阻尼力计算
//                 float velocity = Vector3.Dot(transform.up, carRigidbody.GetPointVelocity(worldPos));
//                 float damperForce = damperStiffness * velocity;
                
//                 // 应用悬挂力
//                 Vector3 suspensionForce = transform.up * (springForce - damperForce);
//                 carRigidbody.AddForceAtPosition(suspensionForce, worldPos);
                
//                 // 更新车轮位置
//                 state.restPosition = hit.point + transform.up * tireRadius;
//             }
//             else
//             {
//                 state.isGrounded = false;
//                 state.compression = 0f;
//                 state.restPosition = worldPos - transform.up * suspensionRestDist;
//             }
//             wheelStates[i] = state;
//         }
//     }

//     // 轮胎力应用（牵引力+转向力）
//     void ApplyTireForces()
//     {
//         for (int i = 0; i < wheelStates.Count; i++)
//         {
//             WheelState state = wheelStates[i];
//             if (!state.isGrounded) continue;
            
//             Vector3 wheelWorldPos = transform.TransformPoint(state.localPosition);
//             Vector3 tireForward = Quaternion.Euler(0, state.steerAngle, 0) * transform.forward;
            
//             // 计算轮胎接触点速度
//             Vector3 contactVelocity = carRigidbody.GetPointVelocity(wheelWorldPos);
//             float forwardSpeed = Vector3.Dot(tireForward, contactVelocity);
//             float lateralSpeed = Vector3.Dot(transform.right, contactVelocity);
            
//             // 滑移率计算
//             float slipRatio = Mathf.Abs(state.angularVelocity * tireRadius - forwardSpeed) / 
//                              Mathf.Max(0.1f, Mathf.Abs(forwardSpeed));
            
//             float slipAngle = Mathf.Atan2(lateralSpeed, Mathf.Abs(forwardSpeed));
            
//             // 摩擦系数模型（Pacejka模型简化版）
//             float longitudinalFriction = wheels[i].tireFriction * Mathf.Exp(-slipRatio * 4f);
//             float lateralFriction = wheels[i].tireFriction * Mathf.Exp(-slipAngle * 10f);
            
//             // 应用牵引力
//             if (wheels[i].isDriven)
//             {
//                 Vector3 driveForce = tireForward * longitudinalFriction;
//                 carRigidbody.AddForceAtPosition(driveForce, wheelWorldPos);
//             }
            
//             // 应用转向力
//             Vector3 lateralForce = -transform.right * lateralFriction;
//             carRigidbody.AddForceAtPosition(lateralForce, wheelWorldPos);
//         }
//     }

//     // 应用引擎扭矩到驱动轮
//     void ApplyEngineTorque()
//     {
//         float drivetrainTorque = GetTorqueAtRPM(currentRPM) * gearRatios[currentGear - 1] * 
//                                 finalDriveRatio * clutchEngagement;
        
//         for (int i = 0; i < wheelStates.Count; i++)
//         {
//             if (!wheels[i].isDriven || !wheelStates[i].isGrounded) continue;
            
//             // 差速器扭矩分配（简化版）
//             float torqueSplit = (diffType == DifferentialType.Open) ? 
//                 0.5f * drivetrainTorque : 
//                 drivetrainTorque * Mathf.Clamp01(wheelStates[i].angularVelocity / currentRPM);
            
//             // 更新车轮角速度
//             float wheelInertia = carRigidbody.mass * tireRadius * 0.25f;
//             wheelStates[i].angularVelocity += torqueSplit / wheelInertia * Time.deltaTime;
//         }
//     }

//     // 应用空气阻力
//     void ApplyDragForces()
//     {
//         Vector3 localVelocity = transform.InverseTransformDirection(carRigidbody.velocity);
        
//         // 空气阻力（速度平方关系）
//         float dragForce = localVelocity.z * Mathf.Abs(localVelocity.z) * 0.3f;
//         carRigidbody.AddForce(transform.forward * -dragForce);
        
//         // 车轮滚动阻力
//         foreach (var state in wheelStates)
//         {
//             if (!state.isGrounded) continue;
//             carRigidbody.AddForceAtPosition(
//                 -transform.forward * 2f * state.compression, 
//                 transform.TransformPoint(state.localPosition));
//         }
//     }

//     // 更新车轮视觉表现
//     void UpdateWheelVisuals()
//     {
//         for (int i = 0; i < wheels.Length; i++)
//         {
//             if (wheels[i].wheelVisual == null) continue;
            
//             // 悬挂位置更新
//             wheels[i].wheelVisual.position = wheelStates[i].restPosition;
            
//             // 转向角度更新
//             wheels[i].wheelVisual.localRotation = Quaternion.Euler(
//                 wheels[i].wheelVisual.localEulerAngles.x,
//                 wheelStates[i].steerAngle,
//                 wheels[i].wheelVisual.localEulerAngles.z);
            
//             // 轮胎旋转
//             float rotation = wheels[i].wheelVisual.localEulerAngles.x;
//             rotation += wheelStates[i].angularVelocity * Time.deltaTime * Mathf.Rad2Deg;
//             wheels[i].wheelVisual.Rotate(Vector3.right, rotation);
//         }
//     }

//     // 差速器类型枚举
//     public enum DifferentialType { Open, LimitedSlip, Locked }
    
//     // 车轮状态结构
//     public class WheelState
//     {
//         public Vector3 localPosition;
//         public Vector3 restPosition;
//         public float springVelocity;
//         public float compression;
//         public float steerAngle;
//         public float angularVelocity;
//         public bool isGrounded;
//     }
// }