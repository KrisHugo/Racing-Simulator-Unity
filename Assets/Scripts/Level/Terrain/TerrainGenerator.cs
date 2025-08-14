using UnityEngine;

public static class TerrainGenerator
{

    private static float[,] GenerateOptimizedHeightMap(float[,] noiseMap, AnimationCurve heightCurve)
    {
        int size = noiseMap.GetLength(0);
        float[,] resultHeightMap = new float[size, size];
        for (int y = 0; y < size; y += 1)
        {
            for (int x = 0; x < size; x += 1)
            {
                float height = 1f + -1f * Mathf.Abs(noiseMap[x, y]);
                resultHeightMap[size-y-1, x] = heightCurve.Evaluate(height);
            }
        }
        return resultHeightMap;
    }

    public static TerrainData GenerateTerrainData(float[,] noiseMap, AnimationCurve heightCurve, float uniformScale, float maxHeight)
    {
        float[,] OptimizedHeightMap = GenerateOptimizedHeightMap(noiseMap, heightCurve);
        TerrainData terrainData = new()
        {
            heightmapResolution = OptimizedHeightMap.GetLength(0),
            size = new Vector3(noiseMap.GetLength(0) - 1, maxHeight, noiseMap.GetLength(1) - 1) * uniformScale,
        };
        terrainData.SetHeights(0, 0, OptimizedHeightMap);
        return terrainData;
    }
}