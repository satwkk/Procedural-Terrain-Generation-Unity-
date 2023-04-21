using UnityEngine;

namespace VoidTerrainGenerator {

    public enum PaintType
    {
        NOISE,
        COLOR,
        MESH,
        FALLOFMAP
    };

    [System.Serializable]
    public struct TerrainType
    {
        public string terrainName;
        public float terrainHeight;
        public Color terrainColor;
    }

    public class Noise : MonoBehaviour
    {
        // Paint type enum to generate different textures based on value
        public PaintType m_PaintType;

        // Seed for random number generator
        public int m_Seed = 1928371289;

        // Size of the quad
        // Width and height and same
        // public int MapChunkSize = 100;
        // public Vector2 size;

        public int MapChunkSize = 241;

        // LOD Value to reduce the number of vertices
        [Range(0, 6)]
        public int m_LevelOfDetail;

        // To scale the noise, larger value means zooms in and smaller value means zooms out.
        public float m_Scale;

        // Offset to scroll the noise on X and Y axis
        public Vector2 m_Offset;

        // Noise is generated in between 0 to 1 which isn't noticeable for a mesh 
        // so the noise value will be multiplied with this value.
        public int m_HeightMultiplier;

        // Number of layers of noise we will generate
        // One octave (layer) of noise will be too smooth and won't be enough for terrain generation
        [Range(1f, 100f)]
        public int m_Octaves;

        // Indicates how much each octaves contribute to the final noise
        // It adjusts the amplitude of the noise with every iteration
        // amplitude decreases with each iteration by 0.5f.
        [Range(0, 1)]
        public float m_Persistence = 0.5f;

        // measures how data will fill the space 
        // it adjusts the FREQUENCY of the noise with every iteration.
        // frequency increased with each iteration by 1.6f.
        public float m_Lacunarity = 1.6f;

        /// The quad mesh renderer to draw the noise at
        public Renderer m_QuadRenderer;

        // Auto update inspector
        // DEBUG ONLY
        public bool autoUpdate = false;

        // All type of terrain and their heightmap to match with our generated noise
        public TerrainType[] m_TerrainRegions;

        // The procedural mesh filter to hold all data of mesh
        public MeshFilter m_MeshFilter;

        // The procedural mesh renderer
        public MeshRenderer m_MeshRenderer;

        // Animation curve to adjust height map
        public AnimationCurve m_AnimationCurve;

        // Bool to check whether to use falloff map 
        public bool m_UseFallOffMap;

        public float[,] m_FallOffMap;

        private void Start() 
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            m_FallOffMap = FalloffMap.GenerateFallOffMap(MapChunkSize);

            float[,] heightMap = GetHeightMap();

            // Adds fall of map to the height map based on the boolean
            if (m_UseFallOffMap)
                IncludeFallOffNoise(ref heightMap);

            Color[] colorMap = GetColorMap(heightMap);

            switch (m_PaintType)
            {
                case PaintType.NOISE:
                    DrawTexture(GenerateTextureFromHeightMap(heightMap));
                    break;

                case PaintType.COLOR:
                    DrawTexture(GenerateTextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
                    break;

                case PaintType.MESH:
                    DrawMesh(MeshGenerator.GenerateMeshData(heightMap, m_HeightMultiplier, m_AnimationCurve, m_LevelOfDetail), GenerateTextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
                    break;

                case PaintType.FALLOFMAP:
                    DrawTexture(GenerateTextureFromHeightMap(m_FallOffMap));
                    break;

                default:
                    Debug.Log("Invalid Paint Type");
                    break;
            }
        }

        public float[,] GetHeightMap() 
        {
            return GenerateNoise();
        }

        public void IncludeFallOffNoise(ref float[,] heightMap)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                for (int x = 0; x < heightMap.GetLength(0); x++)
                {
                    heightMap[x,y] = Mathf.Clamp01(heightMap[x,y] - m_FallOffMap[x, y]);
                }
            }
        }

        public Color[] GetColorMap(float[,] heightMap)
        {
            Color[] colorMap = new Color[MapChunkSize * MapChunkSize];
            for (int y = 0; y < MapChunkSize; y++)
            {
                for (int x = 0; x < MapChunkSize; x++) 
                {
                    foreach (var terrain in m_TerrainRegions)
                    {
                        if (heightMap[x,y] <= terrain.terrainHeight)
                        {
                            colorMap[y * MapChunkSize + x] = terrain.terrainColor;
                            break;
                        }
                    }
                }
            }
            return colorMap;
        }

        public void DrawMesh(MeshData meshData, Texture2D texture)
        {
            Mesh mesh = meshData.GenerateMeshFromData();
            m_MeshFilter.sharedMesh = mesh;
            // m_MeshRenderer.material.mainTexture = texture;
            m_MeshRenderer.sharedMaterial.mainTexture = texture;
        }

        public void DrawTexture(Texture2D texture)
        {
            // m_QuadRenderer.material.mainTexture = texture;
            m_QuadRenderer.sharedMaterial.mainTexture = texture;
            m_QuadRenderer.transform.localScale = new Vector3(texture.width, 1f, texture.height);
        }

        public Texture2D GenerateTextureFromColorMap(Color[] colorMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colorMap);
            texture.Apply();
            return texture;
        }

        public Texture2D GenerateTextureFromHeightMap(float[,] noiseMap) 
        {
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);

            Color[] colorMap = new Color[width * height];

            for (int y = 0; y < width; y++) 
            {
                for (int x = 0; x < width; x++) 
                {

                    float noise = noiseMap[x, y];
                    colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                }
            }

            Texture2D texture = GenerateTextureFromColorMap(colorMap, width, height);
            return texture;
        }

        public float[,] GenerateNoise() {
            float[,] noiseMap = new float[MapChunkSize, MapChunkSize];

            System.Random prng = new System.Random(m_Seed);
            Vector2[] octaveOffsets = new Vector2[m_Octaves];
            for (int i = 0; i < m_Octaves; i++) 
            {
                float offsetX = prng.Next(-100000, 100000) + m_Offset.x;
                float offsetY = prng.Next(-100000, 100000) + m_Offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if (m_Scale <= 0.0f)
                m_Scale = 0.0001f;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            // To scale towards the center 
            float halfWidth = MapChunkSize / 2f;
            float halfHeight = MapChunkSize / 2f;

            for (int y = 0; y < MapChunkSize; y++) 
            {
                for (int x = 0; x < MapChunkSize; x++) 
                {
                    float frequency = 1f;
                    float amplitude = 1f;
                    float noiseHeight = 0f;

                    // For each pixel we go over all octaves
                    for (int i = 0; i < m_Octaves; i++) 
                    {
                        // Multiply the frequency to control the wave distance (frequency)
                        float sampleX = (x - halfWidth) / m_Scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / m_Scale * frequency + octaveOffsets[i].y;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Returns the value in -1 and 1
                        // Multiply the amplitude to control the wave height (amplitude)
                        noiseHeight += perlinValue * amplitude;
                        amplitude *= m_Persistence;
                        frequency *= m_Lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                        maxNoiseHeight = noiseHeight;
                    else if (noiseHeight < minNoiseHeight)
                        minNoiseHeight = noiseHeight;

                    noiseMap[x,y] = noiseHeight;
                }
            }

            // Normalizing the noise map
            for (int y = 0; y < MapChunkSize; y++) 
            {
                for (int x = 0; x < MapChunkSize; x++) 
                {
                    noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
                }
            }
            return noiseMap;
        }
    }
}
