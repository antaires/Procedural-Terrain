using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, DrawMesh };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public int seed; 
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public Vector2 scrollOffset;

    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap(){
        // fetch 2d noise map
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, scrollOffset);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                float currentHeight = noiseMap[x, y];
                // loop over regions and find which region fits
                for (int i = 0; i < regions.Length; i++){
                    if (currentHeight <= regions[i].height){
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break; // move on to next
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }  else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        } else if (drawMode == DrawMode.DrawMesh){
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }

    }


    private void OnValidate(){
        // auto called when variable changed in inspector
        if (mapWidth < 1) { mapWidth = 1; }
        if (mapHeight < 1) { mapHeight = 1; }
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
