using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinitTerrain : MonoBehaviour {

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDist;

    public Transform viewer;
    public Material mapMaterial;


    public static Vector2 viewerPosition; // static to easily access it from other classes
    Vector2 viewerPositionOld;
    static mapGenerator mapGenerator; 
    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>(); // to disable chunks that are no longer visible

    private void Start(){
        mapGenerator = FindObjectOfType<mapGenerator>();

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks(); // called once to load initially
    }

    private void Update(){
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if( (viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate){
            // only update if player moves enough
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

    }

    void UpdateVisibleChunks(){

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++){
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        // get coord of chunk containing viewer
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++){
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++){
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // track which chunks we've created already to prevent duplicates
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)){
                    // update
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    // store if visible
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible()){
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                } else {
                    // create new chunk
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lODMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material){
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            lODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++){
                lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            // passing chunk position allows us to offset texture (not all the same)
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData){
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, mapGenerator.mapChunkSize, mapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk(){
            // find point on its perimeter that is closest to viewers position
            // and find distance to disable mesh if too far away from viewer
            if (mapDataReceived){
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDist;

                if (visible){
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++){
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold){
                            lodIndex = i + 1;
                        } else {
                            // correct lod
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex){
                        LODMesh lODMesh = lODMeshes[lodIndex];
                        if (lODMesh.hasMesh){
                            meshFilter.mesh = lODMesh.mesh;
                            previousLODIndex = lodIndex;
                        } else if (!lODMesh.hasRequestedMesh){
                            lODMesh.RequestMesh(mapData);
                        }
                    }
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }

        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }

    class LODMesh {
        // level of detail
        // responsible for fetching its own mesh from map generator
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback; 

        public LODMesh(int lod, System.Action updateCallback){
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceieved(MeshData meshData){
            // callback for mapGenerator.RequestMapData
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback(); // call function to update mesh at this point
        }

        public void RequestMesh(MapData mapData){
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceieved);
        }
    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDistanceThreshold;
    }
}
