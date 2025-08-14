using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;
        if(DrawDefaultInspector()){
            if(mapGen.autoUpdate){
                mapGen.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("GenerateMap")){
            mapGen.DrawMapInEditor();
        }
        if(GUILayout.Button("GenerteNoiseForTesting")){
            //get 4 adjacent chunk mapdata, and check the edge height if is the same
            mapGen.CreateNoisesForTesting();
        }
        
    } 
}
