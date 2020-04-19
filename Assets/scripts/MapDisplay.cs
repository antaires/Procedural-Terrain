using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture){
        textureRenderer.sharedMaterial.mainTexture = texture; // allows us to generate textures outside of runtime
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture){
        meshFilter.sharedMesh = meshData.CreateMesh(); // shared because we may make this outside of play mode
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
