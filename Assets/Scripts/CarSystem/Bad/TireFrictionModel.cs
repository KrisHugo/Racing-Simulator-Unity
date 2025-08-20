using System;
using UnityEngine;

public class TireFrictionModel
{
    [Header("Pacejka 参数")]
    public float B = 10.0f;  // 刚度因子
    public float C = 1.8f;  // 形状因子
    public float D = 2.5f;  // 峰值摩擦系数
    public float E = 0.97f; // 曲率因子

    [Header("载荷敏感度")]
    [Range(0.1f, 0.5f)] 
    public float loadSensitivity = 0.1f;

    [Header("摩擦缩放")]
    public float forwardFrictionScale = 1.0f;
    public float sidewaysFrictionScale = 0.8f;
    void UpdateTireFriction(Transform transform, Rigidbody rb, WheelCollider wheel)
    {
        wheel.GetGroundHit(out WheelHit hit);
        
        // 获取当前载荷（垂直力）
        float verticalLoad = Mathf.Max(hit.force, 100f);
        
        // 计算滑移率
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float wheelSpeed = wheel.rpm * 2 * Mathf.PI * wheel.radius / 60;
        float denominator = Mathf.Max(Mathf.Abs(localVel.z), 0.1f);
        float slipRatio = Mathf.Abs(wheelSpeed - Mathf.Abs(localVel.z)) / denominator;

        // 限制滑移率范围
        slipRatio = Mathf.Clamp(slipRatio, 0f, 1f);
        
        // Pacejka公式计算基础摩擦
        float x = B * slipRatio;
        float baseFriction = D * Mathf.Sin(C * Mathf.Atan(x - E * (x - Mathf.Atan(x))));
        
        // 考虑垂直载荷
        float normalizedLoad = Mathf.Clamp(verticalLoad / 5000f, 0.5f, 2f);
        float loadEffect = 1 - (loadSensitivity * (normalizedLoad - 1f));
        // 最终摩擦系数计算
        float finalFriction = Mathf.Clamp(baseFriction * loadEffect, 0.5f, 3f);
        
        // 应用摩擦力
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        
        forwardFriction.stiffness = finalFriction * forwardFrictionScale;
        sidewaysFriction.stiffness = finalFriction * sidewaysFrictionScale; // 侧向摩擦稍低
        
        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }
}