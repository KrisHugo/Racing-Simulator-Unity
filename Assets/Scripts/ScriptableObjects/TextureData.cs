using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using PimDeWitte.UnityMainThreadDispatcher;

[CreateAssetMenu(menuName = "TerrainData/TextureData")]
public class TextureData : UpdatableData
{
    float savedMinHeight;
    float savedMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        UpdateMeshHeight(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeight(Material material, float minHeight, float maxHeight)
    {
        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;

        // Debug.Log("Update Height: " + minHeight + ";" + maxHeight);

        material.SetFloat("_minHeight", minHeight);
        material.SetFloat("_maxHeight", maxHeight);

    }

}
