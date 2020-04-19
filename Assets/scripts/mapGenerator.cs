using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, DrawMesh };
    public DrawMode drawMode;

    const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail; // 1 if no simplification, and 2, 4, 6....12 for increaseing simplification
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

    public void GenerateMap(){
        // fetch 2d noise map
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, scrollOffset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++){
            for (int x = 0; x < mapChunkSize; x++){
                float currentHeight = noiseMap[x, y];
                // loop over regions and find which region fits
                for (int i = 0; i < regions.Length; i++){
                    if (currentHeight <= regions[i].height){
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break; // move on to next
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }  else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.DrawMesh){
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }

    }


    private void OnValidate(){
        // auto called when variable changed in inspector
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }
    }

}

[System.Serializable] // so it will show in inspector
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}
