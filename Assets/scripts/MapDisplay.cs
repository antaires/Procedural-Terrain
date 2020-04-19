using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;


    public void DrawNoiseMap(float[,] noiseMap){
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);// 2d to 1d
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture; // allows us to generate textures outside of runtime
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }

}
