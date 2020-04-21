using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMapMultiplier, AnimationCurve heightCurve_, int levelOfDetail){
        AnimationCurve heightCurve = new AnimationCurve(heightCurve_.keys); // prevent multiple threads accessing same height curve
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticiesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticiesPerLine, verticiesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y+=meshSimplificationIncrement){
            for (int x = 0; x < width; x+=meshSimplificationIncrement){

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMapMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float) height);

                if (x < width-1 && y < height-1){ // ignore right and bottom edges of map
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticiesPerLine + 1, vertexIndex + verticiesPerLine);
                    meshData.AddTriangle(vertexIndex + verticiesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }

}

public class MeshData{
    public Vector3[] vertices;
    public int[] triangles;
    int triangleIndex;
    public Vector2[] uvs;

    public MeshData(int meshWidth, int meshHeight){
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
        triangleIndex = 0;
    }

    public void AddTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals(); // assure lighting works correctly
        return mesh;
    }
}
