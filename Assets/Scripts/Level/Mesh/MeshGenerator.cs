using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GeneratorTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail){
        // Here's Threading Safe Needed.
        AnimationCurve heightCurve = new(_heightCurve.keys);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int verticePerLine = (width - 1) / meshSimplificationIncrement + 1;


        MeshData meshData = new(width, height);
        int verticeIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement)
            {
                meshData.vertices[verticeIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[verticeIndex] = new Vector2(x / (float)(width), y / (float)(height));
                if( x < width - 1 && y < height - 1){
                    int a = verticeIndex;
                    int b = verticeIndex + verticePerLine;
                    int c = verticeIndex + verticePerLine + 1;
                    int d = verticeIndex + 1;

                    meshData.AddTriangle(a, c, b);
                    meshData.AddTriangle(c, a, d);
                }

                verticeIndex ++;
            }
        }
        return meshData;
    }
}

public class MeshData{
    public Vector3[] vertices;

    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;
    public MeshData(int meshWidth, int meshHeight){
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
