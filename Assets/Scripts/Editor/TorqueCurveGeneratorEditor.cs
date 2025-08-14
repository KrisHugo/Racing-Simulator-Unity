// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;

// [CustomEditor(typeof(GearSystemDebugger))]
// public class TorqueCurveGeneratorEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
        
//         GearSystemDebugger generator = (GearSystemDebugger)target;
        
//         if (GUILayout.Button("生成扭矩曲线"))
//         {
//             generator.GenerateCurves();
//             generator.ExportToCSV();
//         }
        
//         if (GUILayout.Button("清除数据"))
//         {
//             generator.gearTorqueCurves.Clear();
//         }

//     }
// }
// #endif