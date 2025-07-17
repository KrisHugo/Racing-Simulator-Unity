using UnityEngine;

[System.Serializable]
public class Wheel
{
    public WheelCollider collider;
    public GameObject visual;
    public bool isDriveWheel;
    public bool isSteerWheel;
}

public class WheelController : MonoBehaviour
{
    public Wheel[] wheels;
    [Range(0, 1)] public float slipThreshold = 0.3f;
    //get set
    public int DriveWheelCount { get; private set; } = 0;
    
    

    void Start()
    {
        // 计算驱动轮数量
        DriveWheelCount = 0;
        foreach (var wheel in wheels)
        {
            if (wheel.isDriveWheel)
            {
                DriveWheelCount++;
            }
        }
    }

    // 用于调试的可视化
    private void Update()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.visual && wheel.collider)
            {
                wheel.collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
                wheel.visual.transform.position = position;
                wheel.visual.transform.rotation = rotation;
            }
        }
    }
    
    // 获取车轮基础数据
    public float GetWheelSlip(Wheel wheel)
    {
        wheel.collider.GetGroundHit(out WheelHit hit);
        return Mathf.Max(Mathf.Abs(hit.forwardSlip), Mathf.Abs(hit.sidewaysSlip));
    }
}