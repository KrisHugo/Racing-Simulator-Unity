using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TerrainData/NoiseData")]
public class NoiseData : UpdatableData
{
    
    public Noise.NormalizeMode normalizeMode;
    public float noiseScale;
    public int octaves;

    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    #if UNITY_EDITOR
    protected override void OnValidate() {
        
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves <= 0)
        {
            octaves = 0;
        }
        base.OnValidate();
    }
    #endif
}
