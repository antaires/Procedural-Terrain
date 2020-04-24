using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class mapGenerator : MonoBehaviour {
    
    public enum DrawMode { NoiseMap, ColorMap, DrawMesh };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241; // dimensions of mesh are actually 240 x 240
    [Range(0, 6)]
    public int editorPreviewLevelOfDetail; // 1 if no simplification, and 2, 4, 6....12 for increaseing simplification
    public int seed;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public float meshHightMultiplier;
    public AnimationCurve meshHeightCurve; // allow user to specify how much the multiplier effects diff regions
    public Vector2 scrollOffset;
    public bool autoUpdate;

    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor(){
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.DrawMesh){
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHightMultiplier, meshHeightCurve, editorPreviewLevelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback){
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);       
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback){
        MapData mapData = GenerateMapData(center); // method executed inside thread
        lock(mapDataThreadInfoQueue){
            // when one thread reaches here, while it executes, no other thread can execute (wait turn)
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback){
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback){
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHightMultiplier, meshHeightCurve, lod);
        lock(meshDataThreadInfoQueue){
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update(){
        if (mapDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count >0){
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 center){
        // fetch 2d noise map
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + scrollOffset, normalizeMode);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++){
            for (int x = 0; x < mapChunkSize; x++){
                float currentHeight = noiseMap[x, y];
                // loop over regions and find which region fits
                for (int i = 0; i < regions.Length; i++){
                    if (currentHeight >= regions[i].height){
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    } else {
                        break; // move on to next
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }


    private void OnValidate(){
        // auto called when variable changed in inspector
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }
    }

    struct MapThreadInfo<T>{
        // generic to handle both mesh & map data
        public readonly Action<T> callback; // re)adonly (structs should be immutable
        public readonly T parameter;
        public MapThreadInfo(Action<T> callback, T parameter){
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

[System.Serializable] // so it will show in inspector
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
    public MapData(float[, ] heightMap, Color[] colorMap){
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
