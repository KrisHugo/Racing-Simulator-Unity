using UnityEngine;

public enum DifferentialType
{
    Open,
    LimitedSlip,
    Locked
}
[System.Serializable]
public class DifferentialSystem
{
    [SerializeField] private DifferentialType type = DifferentialType.LimitedSlip;
    [SerializeField][Range(0, 1)] private float lockFactor = 0.8f;
    [SerializeField] private float bias = 0.5f; // 扭矩分配偏置

    public void DistributeTorque(float inputTorque,
                                Wheel leftWheel,
                                Wheel rightWheel,
                                float deltaTime)
    {
        // Vector2 outputTorque;
        switch (type)
        {
            case DifferentialType.Locked:
                // 完全锁止差速器 - 均等分配
                leftWheel.ApplyDriveTorque(inputTorque * 0.5f,deltaTime);
                rightWheel.ApplyDriveTorque(inputTorque * 0.5f,deltaTime);
                break;

            case DifferentialType.LimitedSlip:
                // 限滑差速器 - 智能分配
                float leftTorque = inputTorque * bias;
                float rightTorque = inputTorque * (1 - bias);
                // TODO: WHEELSLIPDATA尚未进行实时更新
                // 防止单侧打滑
                if (leftWheel.slipData.slipRatio > 0.2f) leftTorque *= 1 - lockFactor;
                if (rightWheel.slipData.slipRatio > 0.2f) rightTorque *= 1 - lockFactor;

                leftWheel.ApplyDriveTorque(leftTorque,deltaTime);
                rightWheel.ApplyDriveTorque(rightTorque,deltaTime);
                // outputTorque = new Vector2(leftTorque, rightTorque);
                break;
            case DifferentialType.Open:// Open differential
                // // 计算两侧车轮的可用牵引力
                // float leftMaxTorque = leftWheel.CalculateMaxDriveTorque();
                // float rightMaxTorque = rightWheel.CalculateMaxDriveTorque();
                // // 开放式差速器 - 最小阻力分配
                // float totalMax = leftMaxTorque + rightMaxTorque;
                // float finalTorque = inputTorque * (leftMaxTorque / totalMax);
                // outputTorque = new Vector2(finalTorque, finalTorque);
                // if (totalMax > 0.001f)
                // {
                //     leftWheel.ApplyDriveTorque(finalTorque);
                //     rightWheel.ApplyDriveTorque(finalTorque);
                // }
                // break;
            default: 
                // 完全锁止差速器 - 均等分配
                leftWheel.ApplyDriveTorque(inputTorque * 0.5f,deltaTime);
                rightWheel.ApplyDriveTorque(inputTorque * 0.5f,deltaTime);
                // outputTorque = new Vector2(inputTorque * 0.5f, inputTorque * 0.5f);
                break;
        }
        // return outputTorque;
    }
}