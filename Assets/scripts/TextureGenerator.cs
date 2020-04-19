using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height){
        Texture2D texture = new Texture2D(width, height);

        texture.filterMode = FilterMode.Point; // remove blurry-ness of bi-linear
        texture.wrapMode = TextureWrapMode.Clamp; // prevent map wrapping

        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap){
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);// 2d to 1d
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }
}
