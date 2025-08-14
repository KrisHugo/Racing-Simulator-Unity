using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadNode
{
    public Vector3 position;
    public float width;
    public RoadType type;
}

public enum RoadType { Dirt, Gravel, Paved }

[System.Serializable]
public class RoadSegment
{
    public List<RoadNode> nodes = new List<RoadNode>();
    public List<Vector3> smoothedPath = new List<Vector3>();
    public Mesh roadMesh;
}   
public class RoadGenerator : MonoBehaviour
{
    public MeshFilter terrainMeshFilter;
    public int numKeyPoints = 20;
    public float roadWidth = 3f;
    public float maxSlopeAngle = 30f;
    public float smoothRadius = 2f;
    
    public Material roadMaterial;

    private List<RoadSegment> roads = new List<RoadSegment>();
    private Dictionary<Vector2Int, List<RoadSegment>> roadGrid = new Dictionary<Vector2Int, List<RoadSegment>>();


}
