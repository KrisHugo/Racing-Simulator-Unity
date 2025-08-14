using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [System.Serializable]
// public class CustomFrictionCurve
// {
//     public float ExtreumSlip = 0.4f;
//     public float ExtreumValue = 1.0f;
//     public float AsymptoteSlip = 0.8f;
//     public float AsymptoteValue = 0.5f;
//     public float stiffness = 1f;

//     public AnimationCurve GetAnimationCurve()
//     {

//         // 创建关键点（三个关键点定义摩擦曲线）
//         Keyframe[] keys = new Keyframe[3];

//         // 关键点1：原点 (0,0)
//         keys[0] = new Keyframe(0f, 0f)
//         {
//             inTangent = 0f,
//             outTangent = ExtreumValue / ExtreumSlip // 起始斜率
//         };

//         // 关键点2：最大摩擦点
//         keys[1] = new Keyframe(ExtreumSlip, ExtreumValue * stiffness)
//         {
//             inTangent = keys[0].outTangent,
//             outTangent = (AsymptoteValue * stiffness - ExtreumValue * stiffness) /
//                               (AsymptoteSlip - ExtreumSlip) // 下降斜率
//         };

//         // 关键点3：最小摩擦点
//         keys[2] = new Keyframe(AsymptoteSlip, AsymptoteValue * stiffness)
//         {
//             inTangent = keys[1].outTangent,
//             outTangent = 0f // 水平渐近线
//         };

//         // 创建曲线并设置平滑参数
//         AnimationCurve curve = new(keys);
//         curve.SmoothTangents(1, 0.5f); // 对峰值点进行平滑
//         return curve;
//     }

//     public float Evaluate(float slipRatio)
//     {
//         AnimationCurve curve = GetAnimationCurve();
//         return curve.Evaluate(slipRatio) * stiffness;
//     }
// }

[System.Serializable]
public class WheelSlipData
{
    public float slipRatio;      // 纵向打滑率
    public float slipAngle;      // 侧偏角(弧度)
    public float normalizedSlip; // 综合打滑率(0-1)


    // 计算滑移率
    public float CalculateSlipRatio(Transform car, Vector3 wheelVelocity, float tireCircumVelocity)
    {
        // 接地线速度（世界坐标系转车轮局部坐标系）
        Vector3 localVelocity = car.InverseTransformDirection(wheelVelocity);
        float groundSpeed = localVelocity.z;  // Z轴为车轮前进方向

        // 避免除零错误
        if (Mathf.Approximately(groundSpeed, 0f) && Mathf.Approximately(tireCircumVelocity, 0f))
            return 0f;

        // 滑移率公式
        return Mathf.Clamp01(Mathf.Abs(tireCircumVelocity - groundSpeed) /
                             Mathf.Max(Mathf.Abs(tireCircumVelocity), Mathf.Abs(groundSpeed)));
    }


}
// public class WheelPhysics : MonoBehaviour
// {
//     public Transform visual;
//     [Header("Wheel Data")]
//     public float mass = 20f;
//     public float radius = 0.3f;
//     public float inertia;
//     [Header("阻力参数")]
//     public float airResistance = 0.1f;     // 空气阻力系数
//     public float frictionCoefficient = 0.02f; // 基本滚动阻力系数
//     public CustomFrictionCurve currentFriction;
//     [Header("Wheel State")]
//     public bool isGround;
//     public WheelSlipData slipData;
//     [Header("Motor State")]
//     [SerializeField]
//     private float motorTorque;
//     [SerializeField]
//     private float brakeTorque;    // 刹车扭矩(N·m);
//     [SerializeField]
//     private float angularSpeed;
//     public float rpm;
//     [Header("Steer State")]
//     [SerializeField]
//     private float currentSteeringAngle;
    

//     [Header("Steering Setting")]
//     public float maxSteeringAngle = 25f;
//     public float steeringSpeed = 1f;

//     [Header("Test")]
//     [SerializeField]
//     private Rigidbody rb;
//     public float engineTorque = 200f;
//     public float maxBrakeTorque = 1500f;
//     // Start is called before the first frame update
//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         CalculateInertia();
//         motorTorque = 0f;
//         angularSpeed = 0f;
//     }

//     public void CalculateInertia()
//     {
//         inertia = 0.5f * mass * Mathf.Pow(radius, 2);
//     }

//     // 计算总角速度（不考虑方向）
//     public float GetAngularSpeed() => angularSpeed;

//     // 计算RPM（每分钟转数）
//     public float CalculateRPM()
//     {
//         // 避免除零错误
//         if (Mathf.Approximately(angularSpeed, 0f))
//             return 0f;

//         return angularSpeed * 60 / (2 * Mathf.PI);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // // 根据油门设置扭矩
//         // float throttle = Input.GetKey(KeyCode.W)? 1.0f: 0f;
//         // float wheelTorque = engineTorque * throttle;
        
//         // // 3. 获取刹车输入  
//         // float brakeInput = Input.GetKey(KeyCode.S)? 1.0f: 0f;
//         // float brakeTorque = brakeInput * maxBrakeTorque;

//         // float steerInput = Input.GetAxis("Horizontal");
//         // // 目标转向角
//         // float targetAngle = maxSteeringAngle * steerInput;
        
//         // // 平滑转向过渡
//         // currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetAngle, 
//         //                                  steeringSpeed * Time.deltaTime);

//         // ApplyTorque(wheelTorque);
//         // ApplyBrakeTorque(brakeTorque);
//         UpdateVisual();
//         // update state
//         rpm = CalculateRPM();

//     }
//     void FixedUpdate()
//     {
//         // UpdateWheelPhysics();
//     }

//     // 施加扭矩
//     public void ApplyTorque(float motorTorque)
//     {
//         this.motorTorque = motorTorque;
//     }

//     public void ApplyBrakeTorque(float brakeTorque){
//         this.brakeTorque = brakeTorque;
//     }

//     public void UpdateWheelPhysics()
//     {
//         // float normalForce = CalculateNormalForce(rb, springForce);

//         // float resistanceTorque = CalculateResistanceTorque(rb.transform, wheelVelocity, angularSpeed, normalForce);
//         // float resistanceTorque = CalculateResistanceTorque();

//         // float angularAcceleration = (motorTorque - resistanceTorque) / inertia;

//         // // 计算新角速度 ω = ω₀ + α * Δt
//         // angularSpeed += angularAcceleration * Time.fixedDeltaTime;
//     }

//     public void UpdateVisual()
//     {

//         // // 转向角度更新
//         // visual.transform.localRotation = Quaternion.Euler(
//         //     visual.transform.localEulerAngles.x,
//         //     currentSteeringAngle,
//         //     visual.transform.localEulerAngles.z);
        
//         float deltaAngle = angularSpeed * Mathf.Rad2Deg * Time.deltaTime;
//         if (angularSpeed * angularSpeed > 0.001f) // 避免零向量
//         {
//             Quaternion rotationDelta = Quaternion.AngleAxis(deltaAngle, Vector3.right);
//             visual.rotation *= rotationDelta; // 应用旋转增量
//         }
//     }

//     // public float CalculateResistanceTorque(){
//     //     return brakeTorque;
//     // }
    
// }
