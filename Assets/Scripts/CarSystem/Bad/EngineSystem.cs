using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


public class OldEngineSystem : MonoBehaviour
{
    // 新增字段
    public float CurrentRPM { get; private set; } // 当前转速（RPM）;
    private float angularVelocity; // rad/s

    [SerializeField]
    public EngineFeature feature;
    public float engineBrakeTorque = 25f;
    private float currentThrottle = 0f;
    // some car need to stop throttle while braking.
    private float currentBrake = 0f;

    public void Initialize(){
        
    }

    public void SetInput(float throttle, float brake)
    {
        currentThrottle = Mathf.Clamp01(throttle);
        currentBrake = Mathf.Clamp01(brake);
    }

    public float CalculateEngineTorque()
    {
        return GetTorqueAtRPM(CurrentRPM);
    }


    public void UpdateEngine(TransmissionSystem transmission, float deltaTime)
    {
        // 1. 计算当前引擎输出扭矩
        float outputTorque = GetTorqueAtRPM(CurrentRPM);
        float loadTorque = transmission.GetLoadTorque();
        Debug.Log(outputTorque + " " + loadTorque);
        // 2. 计算净扭矩 (输出扭矩 - 负载扭矩 - 引擎制动)
        float NetTorque = outputTorque - loadTorque;
        // 2.1引擎制动效果（当油门松开时）
        if (currentThrottle < 0.01f)
        {
            float engineBrake = engineBrakeTorque * feature.GetInverseLerpRPM(CurrentRPM);
            NetTorque -= engineBrake;
        }

        // 3. 计算角加速度 (α = τ / I)

        float angularAcceleration = NetTorque / feature.inertia;
        // 4. 更新角速度 (ω = ω0 + α * dt)
        angularVelocity += angularAcceleration * deltaTime;
        // 5. 转换为RPM
        CurrentRPM = angularVelocity * 60 / (2 * Mathf.PI);
        // 6. 限制RPM范围
        CurrentRPM = feature.GetRPMClamp(CurrentRPM);

        // 6.1 防止RPM低于怠速时直接归零
        if (CurrentRPM < feature.idleRPM * 0.5f && NetTorque > 0)
        {
            CurrentRPM = feature.idleRPM * 0.5f;
            angularVelocity = CurrentRPM * 2 * Mathf.PI / 60;
        }
    }

    // void ApplyDownforce()
    // {
    //     Rigidbody rb = GetComponent<Rigidbody>();
    //     float speed = rb.velocity.magnitude;
    //     float downforce = speed * speed * 0.1f; // 系数根据需求调整

    //     // 将下压力分配到驱动轮
    //     foreach (Wheel wheel in wheelController.wheels)
    //     {
    //         if (wheel.isGround)
    //         {

    //             wheel.collider.attachedRigidbody.AddForceAtPosition(
    //                 -transform.up * downforce,
    //                 wheel.collider.transform.position
    //             );
    //         }

    //     }
    // }

    private float GetTorqueAtRPM(float rpm){
        return feature.GetTorqueAtRPM(rpm) * currentThrottle;
    }


    void EngineBlown()
    {
        // Debug.Log("发动机因超转损坏!");
        CurrentRPM = 0;
        // 可以在这里添加更多效果(冒烟、声音等)
    }

}