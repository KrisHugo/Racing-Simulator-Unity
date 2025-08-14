// using UnityEngine;
// using System.Collections.Generic;
// using System;



// #if UNITY_EDITOR
// using UnityEditor;
// #endif


// [System.Serializable]
// public class GearTorqueData
// {
//     public int gear;
//     public List<float> speedPoints = new();
//     public List<float> rpmPoints = new();
//     public List<float> torquePoints = new();
// }

// public class GearSystemDebugger : MonoBehaviour
// {
//     public EngineSystem engine; 
//     public TransmissionSystem transimission;
//     public Wheel wheel;
//     public float maxSpeed = 200f; // 最大车速(km/h)
//     public int samples = 100; // 采样点数


//     public List<GearTorqueData> gearTorqueCurves = new();

//     public void GenerateCurves()
//     {
//         gearTorqueCurves.Clear();

//         // // 为每个挡位生成数据
//         // for (int gear = 0; gear < transimission.features.gearRatios.Length; gear++)
//         // {
//         //     GearTorqueData gearData = new()
//         //     {
//         //         gear = gear
//         //     };

//         //     // 在当前挡位下采样不同车速
//         //     for (int i = 0; i <= samples; i++)
//         //     {
//         //         float speedKPH = maxSpeed * i / samples;
//         //         float speedMPS = speedKPH / 3.6f;

//         //         // 计算当前车速对应的发动机转速
//         //         float wheelRPM = speedMPS * 60f / (2f * Mathf.PI * wheel.Radius);
//         //         float engineRPM = Mathf.Clamp(wheelRPM * transimission.features.gearRatios[gear] * transimission.features.finalDriveRatio, engine.feature.idleRPM * 0.8f, engine.feature.maxRPM);

//         //         // 获取当前转速下的发动机扭矩
//         //         float torque = engine.feature.maxTorque *
//         //                         MathF.Max(engine.feature.torqueCurve.Evaluate((engineRPM - engine.feature.idleRPM) / (engine.feature.maxRPM - engine.feature.idleRPM)), 0);

//         //         // 计算车轮扭矩
//         //         float wheelTorque = torque * transimission.features.gearRatios[gear] * transimission.features.finalDriveRatio;

//         //         gearData.speedPoints.Add(speedKPH);
//         //         gearData.rpmPoints.Add(engineRPM);
//         //         gearData.torquePoints.Add(wheelTorque);
//         //     }

//         //     gearTorqueCurves.Add(gearData);
//         // }

//         Debug.Log("扭矩曲线数据生成完成");
//         // 这里可以添加可视化代码或导出数据
//     }

//     public void ExportToCSV()
//     {
//         string path = EditorUtility.SaveFilePanel("导出扭矩曲线", "", "torque_curves", "csv");
//         if (string.IsNullOrEmpty(path)) return;

//         using (System.IO.StreamWriter file = new(path))
//         {
//             file.WriteLine("Gear,Speed(km/h),RPM,Torque(Nm)");
//             foreach (var gearData in gearTorqueCurves)
//             {
//                 for (int i = 0; i < gearData.speedPoints.Count; i++)
//                 {
//                     file.WriteLine($"{gearData.gear + 1},{gearData.speedPoints[i]},{gearData.rpmPoints[i]},{gearData.torquePoints[i]}");
//                 }
//             }
//         }

//         Debug.Log($"数据已导出到: {path}");
//     }

//     // 在TorqueCurveGenerator类中添加
//     public void OnDrawGizmos()
//     {
// #if UNITY_EDITOR
//         if (gearTorqueCurves == null || gearTorqueCurves.Count == 0) return;

//         // 设置绘制区域(屏幕右上角)
//         float graphWidth = 300f;
//         float graphHeight = 200f;
//         float margin = 20f;
//         Rect graphRect = new(Screen.width - graphWidth - margin, margin, graphWidth, graphHeight);

//         // 绘制背景
//         Handles.BeginGUI();
//         GUI.Box(graphRect, GUIContent.none);

//         // 绘制坐标轴
//         Handles.Label(new Vector2(graphRect.x, graphRect.y), "扭矩(Nm)");
//         Handles.Label(new Vector2(graphRect.x + graphRect.width - 30, graphRect.y + graphRect.height - 15), "车速(km/h)");


//         // 找到最大扭矩用于归一化
//         float maxTorque = 0;
//         foreach (var gearData in gearTorqueCurves)
//         {
//             float currentMaxTorque = Mathf.Max(gearData.torquePoints.ToArray());
//             maxTorque = Mathf.Max(maxTorque, currentMaxTorque);
//         }
        
//         // 绘制每条曲线
//         for (int g = 0; g < gearTorqueCurves.Count; g++)
//         {
//             var gearData = gearTorqueCurves[g];
//             if (gearData.speedPoints.Count != gearData.torquePoints.Count) continue;

//             Color gearColor = Color.HSVToRGB(g * 0.15f, 0.8f, 0.8f);


//             // 绘制曲线
//             for (int i = 1; i < gearData.speedPoints.Count; i++)
//             {

//                 if (gearData.torquePoints[i - 1] <= 0.1f)
//                 {
//                     break;
//                 }
//                 float x1 = graphRect.x + gearData.speedPoints[i - 1] / maxSpeed * graphRect.width;
//                 float y1 = graphRect.y + graphRect.height * (1 - gearData.torquePoints[i - 1] / maxTorque);


//                 float x2 = graphRect.x + gearData.speedPoints[i] / maxSpeed * graphRect.width;
//                 float y2 = graphRect.y + graphRect.height * (1 - gearData.torquePoints[i] / maxTorque);

//                 Handles.color = gearColor;
//                 Handles.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
//             }

//             // 添加图例
//             GUI.color = gearColor;
//             GUI.Label(new Rect(graphRect.x + 10, graphRect.y + 15 + g * 15, 50, 20), $"档位 {g + 1}");
//         }

//         Handles.EndGUI();
// #endif
//     }
// }
