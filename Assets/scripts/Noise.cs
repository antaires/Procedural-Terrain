using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {

    public enum NormalizeMode { Local, Global }; // global = estimate
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 scrollOffset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++){
            float offsetX = prng.Next(-100000, 1000000) + scrollOffset.x;
            float offsetY = prng.Next(-100000, 1000000) - scrollOffset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) { scale = 0.0001f; }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f; // scale towards center 
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0;
                // octaves
                for (int i = 0; i < octaves; i++){
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency; // higher frequency == sample points farther apart (heigth changes more rapidl) 
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // change range to be -1 to 1 rather than 0 to 1
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance; // decreases each octave
                    frequency *= lacunarity; // frequency increases each ocatve
                }
                if (noiseHeight > maxLocalNoiseHeight){
                    maxLocalNoiseHeight = noiseHeight;
                } else if (noiseHeight < minLocalNoiseHeight){
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        // loop over noise map and reset everything to be within 0 to 1 
        // ie normalize it
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                // best method for generating full mesh, but not chunk by chunk (where we need to estimate)
                if (normalizeMode == NormalizeMode.Local){
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } else {
                    // estimate max and min 
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.8f); // change last value (bigger = taller mountains)
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue); // prevent negatives
                }

            }
        }

        return noiseMap;
    }


}
