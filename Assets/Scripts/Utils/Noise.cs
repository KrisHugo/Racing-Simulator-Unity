using UnityEngine;
using System.Collections;

public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        float maxPossibleHeight = 0;
        float amplitude = 1;
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;

        }

        scale = Mathf.Max(scale, 0.00001f);

        // float halfWidth = mapWidth * 0.5f;
        // float halfHeight = mapHeight * 0.5f;

        float invScale = Mathf.PI / scale; // 预计算倒数避免重复除法

        float maxNoiseValue = float.MinValue;
        float minLocalNoiseValue = float.MaxValue;

        float frequency;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    // float sampleX = (x - halfWidth + octaveOffsets[i].x) * invScale * frequency;
                    // float sampleY = (y - halfHeight + octaveOffsets[i].y) * invScale * frequency;
                    float sampleX = (x + octaveOffsets[i].x) * invScale * frequency;
                    float sampleY = (y + octaveOffsets[i].y) * invScale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;

                }

                // 实时更新极值
                if (noiseHeight > maxNoiseValue) maxNoiseValue = noiseHeight;
                if (noiseHeight < minLocalNoiseValue) minLocalNoiseValue = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }
        float range = maxNoiseValue - minLocalNoiseValue;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = (noiseMap[x, y] - minLocalNoiseValue) / range;
                }
                else
                {
                    // using maxPossibleHeight to smoothe the edge.
                    noiseMap[x, y] = (noiseMap[x, y] + 1) / (2 * maxPossibleHeight / 1.75f);
                }
            }
        }

        return noiseMap;
    }

}
