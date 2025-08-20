using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public class MapGenerator : MonoBehaviour
{

    // terrain means using terrain system to make generate easy and a lot more easier for me to modify the terrain data without care about the vertices and triangles.
    public enum DrawMode { NoiseMap, Mesh, FalloffMap, Terrain };
    public DrawMode drawMode;

    public TerrainConfig terrainConfig;
    public NoiseData noiseData;
    public TextureData textureData;
    public Material terrainMaterial;

    [Range(0, 6)]
    public int editorPreviewLOD;


    public bool autoUpdate;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();

    public void Awake()
    {

        textureData.UpdateMeshHeight(terrainMaterial, terrainConfig.MinHeight, terrainConfig.MaxHeight);
    }

    void OnValueUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    //  minus 2 from 241 to 239 because of the calculation of correct uv of border vertices
    public int MapChunkSize
    {
        get
        {
            if (drawMode == DrawMode.Terrain)
            {
                return 257;
            }
            else
            {
                if (terrainConfig.useFlatShading)
                {
                    return 95;
                }
                else
                {
                    return 239;
                }
            }
        }
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);


        MapDisplay display = GetComponent<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightmap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GeneratorTerrainMesh(mapData.heightmap, terrainConfig.meshHeightMultiplier, terrainConfig.meshHeightCurve, editorPreviewLOD, terrainConfig.useFlatShading));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(MapChunkSize)));
        }
        else if (drawMode == DrawMode.Terrain)
        {
            display.DrawTerrain(TerrainGenerator.GenerateTerrainData(mapData.heightmap, terrainConfig.meshHeightCurve, terrainConfig.uniformScale, terrainConfig.MaxHeight - terrainConfig.MinHeight), terrainMaterial);
        }
    }

    public void CreateNoisesForTesting(){
        MapData[,] testDatas = new MapData[2,2];
        for(int i = 0; i < 2; i++){
            for(int j = 0; j < 2; j++){
                print("i:" + i + " j:" + j);
                Vector2 centre = new(i * MapChunkSize, j * MapChunkSize);
                testDatas[i,j] = GenerateMapData(centre);
            }
        }
        print(testDatas[0,0].heightmap[MapChunkSize - 1, MapChunkSize - 1]);
        print(testDatas[0,1].heightmap[MapChunkSize - 1, 0]);
        print(testDatas[1,0].heightmap[0, MapChunkSize - 1]);
        print(testDatas[1,1].heightmap[0, 0]);
    }


    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        void threadStart()
        {
            MapDataThread(centre, callback);
        }

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }

    }

    public void RequestMeshData(MapData mapData, int LOD, Action<MeshData> callback)
    {
        void threadStart()
        {
            MeshDataThread(mapData, LOD, callback);
        }

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GeneratorTerrainMesh(mapData.heightmap, terrainConfig.meshHeightMultiplier, terrainConfig.meshHeightCurve, lod, terrainConfig.useFlatShading);

        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre)
    {
        // print(centre);
        // print(MapChunkSize);
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);
        // print(centre.ToString() + "start:" + noiseMap[0, MapChunkSize + 1]);
        // print(centre.ToString() + "end:" + noiseMap[MapChunkSize + 1, MapChunkSize + 1]);
        if (terrainConfig.useFalloff)
        {
            falloffMap ??= FalloffGenerator.GenerateFalloffMap(MapChunkSize);
            for (int y = 0; y < MapChunkSize; y++)
            {
                for (int x = 0; x < MapChunkSize; x++)
                {
                    if (terrainConfig.useFalloff)
                    {
                        if(drawMode == DrawMode.Terrain){
                            noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] + falloffMap[x, y]);
                        }
                        else{
                            noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                        }
                    }
                }
            }
        }

        return new MapData(noiseMap);

    }

    void OnValidate()
    {
        if (terrainConfig != null)
        {
            terrainConfig.OnValueUpdated -= OnValueUpdated;
            terrainConfig.OnValueUpdated += OnValueUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValueUpdated -= OnValueUpdated;
            noiseData.OnValueUpdated += OnValueUpdated;
        }
        if (textureData != null)
        {

            textureData.OnValueUpdated -= OnTextureValuesUpdated;
            textureData.OnValueUpdated += OnTextureValuesUpdated;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

public struct MapData
{
    public readonly float[,] heightmap;

    public MapData(float[,] noisemap)
    {
        heightmap = noisemap;
    }
}