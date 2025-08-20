using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public Terrain terrain;

    public void DrawTexture(Texture2D texture){
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        
        meshFilter.transform.localScale = Vector3.one * GetComponent<MapGenerator>().terrainConfig.uniformScale;

    }

    // using terrain component to build map instead of draw it by myself.
    public void DrawTerrain(TerrainData terrainData, Material material)
    {
        terrain.terrainData = terrainData;
        terrain.materialTemplate = material;
        if (!terrain.TryGetComponent<TerrainCollider>(out var terrainCollider))
        {
            terrainCollider = terrain.gameObject.AddComponent<TerrainCollider>();
        }
        terrainCollider.terrainData = terrainData;
    }
}
