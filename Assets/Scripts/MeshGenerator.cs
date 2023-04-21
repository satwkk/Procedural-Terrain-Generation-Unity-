using UnityEngine;

namespace VoidTerrainGenerator {
    
    public class MeshData
    {
        public Vector3[] vertices;
        public Vector2[] textureCoords;
        public int[] indices;

        public int indicesIndex = 0;

        public MeshData(int width, int height)
        {
            vertices = new Vector3[width * height];
            textureCoords = new Vector2[width * height];
            indices = new int[(width - 1) * (height - 1) * 6];
        }


        public void AddTriangle(int a, int b, int c)
        {
            indices[indicesIndex] = a;
            indices[indicesIndex+1] = b;
            indices[indicesIndex+2] = c;
            indicesIndex += 3;
        }

        public Mesh GenerateMeshFromData()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = textureCoords;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }

    public static class MeshGenerator
    {
        public static MeshData GenerateMeshData(float[,] heightMap, float heightMultiplier, AnimationCurve animationCurve, int levelOfDetail)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;
            int vertexIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2; // Doubles the level of detail provided
            int nVerticesToIterate = ((width - 1) / vertexIncrement) + 1;

            MeshData meshData = new MeshData(nVerticesToIterate, nVerticesToIterate);
            int vertexIndex = 0;

            for (int y = 0; y < height; y += vertexIncrement)
            {
                for (int x = 0; x < width; x += vertexIncrement)
                {
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, animationCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                    meshData.textureCoords[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                    if (x < width - 1 && y < height - 1)
                    {
                        // Adding the indices in clockwise manner excluding the last vertex of width and height
                        meshData.AddTriangle(vertexIndex, vertexIndex + nVerticesToIterate + 1, vertexIndex + nVerticesToIterate);
                        meshData.AddTriangle(vertexIndex + nVerticesToIterate + 1, vertexIndex, vertexIndex + 1);
                    }
                    vertexIndex++;
                }
            }
            return meshData;
        }

        public static Mesh GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve animationCurve, int levelOfDetail)
        {
            var meshData = GenerateMeshData(heightMap, heightMultiplier, animationCurve, levelOfDetail);
            return meshData.GenerateMeshFromData();
        }
    }
}