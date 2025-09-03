using UnityEngine;
public class TransmissionSystem : MonoBehaviour
{
    [System.Serializable]
    public class Gear
    {
        public float ratio; // 传动比
        public float minSpeed; // 最低生效速度(km/h)
        public float maxSpeed; // 最高速度(km/h)
    }

    [Header("变速箱设置")]
    public Gear[] gears = {
        new Gear { ratio = 4.0f, minSpeed = 0, maxSpeed = 50 },   // 1档
        new Gear { ratio = 2.5f, minSpeed = 30, maxSpeed = 80 },  // 2档
        new Gear { ratio = 1.7f, minSpeed = 60, maxSpeed = 120 }, // 3档
        new Gear { ratio = 1.2f, minSpeed = 80, maxSpeed = 160 }, // 4档
        new Gear { ratio = 0.9f, minSpeed = 110, maxSpeed = 220 } // 5档
    };

    public float finalDriveRatio = 3.5f; // 主减速比
    public float reverseRatio = 3.8f;   // 倒车档比率

    [Header("当前状态")]
    public int currentGear = 1;
    public float drivetrainLoss = 0.15f; // 传动系统能量损耗百分比
    public bool IsShifting { get { return Time.time < lastShiftTime + gearShiftDelay; } } // 是否正在换挡
    private float gearShiftDelay = 0.5f; // 换挡延迟
    private float lastShiftTime = -1f; // 上次换挡时间

    // 计算引擎转速
    public float CalculateEngineRPM(float avgWheelRPM)
    {
        return Mathf.Abs(avgWheelRPM) * GetCurrentRatio() * finalDriveRatio;
    }
    // 计算车轮扭矩（考虑传动损耗）
    public float GetWheelTorque(float engineTorque)
    {
        if (currentGear > gears.Length) return 0;
        return engineTorque * GetGearMultiplier() * GetCurrentRatio() * finalDriveRatio * (1 - drivetrainLoss);
    }


    // 换挡操作
    public void ShiftGear(int direction)
    {
        // 检查换挡冷却时间
        if (IsShifting) return;

        if (direction > 0 && currentGear < gears.Length) // 升挡
        {
            currentGear++;
            lastShiftTime = Time.time;
        }
        else if (direction < 0 && currentGear > -1) // 降挡
        {
            currentGear--;
            lastShiftTime = Time.time;
        }

        // Debug.Log($"已换挡至 {CurrentGearState}挡");
    }

    public float GetGearMultiplier()
    {
        if (currentGear == 0 || currentGear > gears.Length) return 0;

        float gearMultiplier = 1.0f;
        if (currentGear > 0)
        {
            // 低挡位提供更高扭矩输出
            gearMultiplier = Mathf.Lerp(1.8f, 0.8f, (float)(currentGear - 1) / (gears.Length - 1));
        }
        else if (currentGear < 0)
        {
            // 倒挡提供较低扭矩输出
            gearMultiplier = -0.5f;
        }
        return gearMultiplier;
    }
    public float GetCurrentRatio()
    {
        if (currentGear > gears.Length) return 0;

        if (currentGear > 0)
        {
            // 低挡位提供更高扭矩输出
            return gears[currentGear - 1].ratio;
        }
        else if (currentGear < 0)
        {
            // 倒挡提供较低扭矩输出
            return reverseRatio;
        }
        else
        {
            return 0; // 空挡
        }
    }
}