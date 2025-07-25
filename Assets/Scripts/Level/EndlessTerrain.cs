using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerain : MonoBehaviour
{

    const float viewerMoveThreadholdForChunkUpdate = 25f; // How much the viewer has to move before we update the chunks
    const float sqrViewerMoveThreadholdForChunkUpdate = viewerMoveThreadholdForChunkUpdate * viewerMoveThreadholdForChunkUpdate;    
    public static float maxViewDst = 450;
    public LODInfo[] detailLevels;
    public Transform viewer;
    public Material mapMaterial;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionPrevious;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
    static List<TerrainChunk> terrainChunksVisibleLastUpdated;
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        terrainChunksVisibleLastUpdated = new List<TerrainChunk>();
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        if((viewerPosition - viewerPositionPrevious).sqrMagnitude > sqrViewerMoveThreadholdForChunkUpdate)
        {
            viewerPositionPrevious = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    // Update is called once per frame
    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdated.Count; i++)
        {
            terrainChunksVisibleLastUpdated[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdated.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewerChunkCoord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                // Check if the chunk is already being generated or is visible
                if (terrainChunkDictionary.ContainsKey(viewerChunkCoord))
                {
                    terrainChunkDictionary[viewerChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewerChunkCoord, new TerrainChunk(viewerChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }

            }
        }

    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lODMeshes;
        LODMesh collisionLODMesh;
        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionVec3 = new(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk " + coord);
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionVec3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            meshObject.layer = LayerMask.NameToLayer("Ground");
    
            SetVisible(false);

            lODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lODMeshes[i];
                }
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colormap, MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }
        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lODMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            meshFilter.mesh = lodMesh.mesh;
                            previousLODIndex = lodIndex;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    if (lodIndex == 0){
                        if(collisionLODMesh.hasMesh){
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        }
                        else if(!collisionLODMesh.hasRequestedMesh){
                            collisionLODMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunksVisibleLastUpdated.Add(this);
                }

                SetVisible(visible);
            }

        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }


    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        int lod;
        Action updateCallBack;

        public LODMesh(int lod, Action updateCallBack)
        {
            this.lod = lod;
            this.updateCallBack = updateCallBack;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            // 由于update中设置了足够距离后更新地形数据，因此可能不会接收到多线程发回的meshdata, 因此需要一个callback告诉unity可以更新地形信息了。
            updateCallBack();
        }

        public void RequestMesh(MapData mapdata)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapdata, lod, OnMeshDataReceived);
        }

    }

    [Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
        public bool useForCollider;
    }
}
