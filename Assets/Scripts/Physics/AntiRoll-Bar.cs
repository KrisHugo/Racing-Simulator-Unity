// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class AntiRollBar : MonoBehaviour
// {

//     [Header("防侧翻参数")]
//     [Range(5000, 20000)] public float antiRollForce = 12000f; // 抗侧翻力基数
//     [Range(0.1f, 1f)] public float forceDistribution = 0.6f; // 前/后轴分配比
//     [Range(1, 10)] public float angleSensitivity = 4f; // 角度敏感度

//     [Header("动态调整")]
//     public bool enableDynamicAdjust = true; // 根据速度自动调整
//     public float minSpeedThreshold = 5f;  // 生效最低速度
//     public AnimationCurve speedAdjustCurve; // 速度-力度曲线

//     void StabilizeVehicle()
//     {
//         // 计算当前侧倾角度
//         float rollAngle = Vector3.Angle(Vector3.up, transform.up);

//         // 动态力计算
//         float dynamicForce = antiRollForce;
//         if (enableDynamicAdjust)
//         {
//             float speedFactor = rb.velocity.magnitude > minSpeedThreshold
//                 ? speedAdjustCurve.Evaluate(rb.velocity.magnitude / maxSpeed)
//                 : 0f;
//             dynamicForce *= speedFactor * angleSensitivity;
//         }

//         // 应用抗侧翻力矩
//         if (rollAngle > 5f)
//         {
//             Vector3 antiRollTorque = transform.up * dynamicForce * Mathf.Sin(rollAngle * Mathf.Deg2Rad);
//             rb.AddTorque(antiRollTorque * Time.fixedDeltaTime, ForceMode.VelocityChange);
//         }

//         // 悬架平衡系统
//         BalanceSuspension(Wheel_FL, Wheel_FR, dynamicForce * forceDistribution);
//         BalanceSuspension(Wheel_RL, Wheel_RR, dynamicForce * (1 - forceDistribution));
//     }
// }
