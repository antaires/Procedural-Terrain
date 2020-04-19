using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {
    
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 scrollOffset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++){
            float offsetX = prng.Next(-100000, 1000000) + scrollOffset.x;
            float offsetY = prng.Next(-100000, 1000000) + scrollOffset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0) { scale = 0.0001f; }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f; // scale towards center 
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0;
                // octaves
                for (int i = 0; i < octaves; i++){
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x; // higher frequency == sample points farther apart (heigth changes more rapidl) 
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // change range to be -1 to 1 rather than 0 to 1
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance; // decreases each octave
                    frequency *= lacunarity; // frequency increases each ocatve
                }
                if (noiseHeight > maxNoiseHeight){
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight){
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        // loop over noise map and reset everything to be within 0 to 1 
        // ie normalize it
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }


}
