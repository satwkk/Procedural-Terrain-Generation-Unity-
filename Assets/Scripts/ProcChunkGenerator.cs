using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoidTerrainGenerator {
    public class ProcChunkGenerator : MonoBehaviour
    {
        public Transform m_Viewer;
        public Noise m_Noise;
        public int m_ChunkSize; // Each chunk size
        public Vector3 m_ViewerPosition;
        public int m_ViewerViewDistance;
        public int m_ChunksVisible;

        void Update()
        {
            m_Noise = GetComponent<Noise>();
            m_ChunkSize = m_Noise.MapChunkSize - 1;
            m_ViewerPosition = new Vector3(m_Viewer.position.x, 0f, m_Viewer.position.z);
            m_ChunksVisible = Mathf.RoundToInt(m_ViewerViewDistance / m_ChunkSize);

            Vector3 viewerCoordPosition = ConvertViewerPositionToCoordSpace(m_ViewerPosition);

            for (var zOffset = -m_ChunksVisible; zOffset <= m_ChunksVisible; zOffset++) 
            {
                for (var xOffset = -m_ChunksVisible; xOffset <= m_ChunksVisible; xOffset++)
                {
                    Vector3 chunkCoordPosition = new Vector3(viewerCoordPosition.x + xOffset, 0f, viewerCoordPosition.z + zOffset);
                    Chunk chunk = new Chunk(chunkCoordPosition, m_ChunkSize);
                    chunk.Spawn();
                }
            }
        }

        public Vector3 ConvertViewerPositionToCoordSpace(Vector3 _viewerPosition) 
        {
            return new Vector3(Mathf.RoundToInt(_viewerPosition.x) / m_ChunkSize, 0f, Mathf.RoundToInt(_viewerPosition.z / m_ChunkSize));
        }
    }

    public class Chunk 
    {
        GameObject m_MeshObject;
        public Vector3 m_CoordPosition;
        public Vector3 m_WorldPosition;
        public int m_Size;

        public Chunk(Vector3 coordPosition, int size)
        {
            m_CoordPosition = coordPosition;
            m_WorldPosition = new Vector3(m_CoordPosition.x * size, 0f, m_CoordPosition.z * size);
            m_Size = size;
        }

        public void Spawn()
        {
            m_MeshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_MeshObject.transform.position = m_WorldPosition;
            m_MeshObject.transform.localScale = Vector3.one * m_Size; // / 10f
            m_MeshObject.SetActive(true);
        }

        public void DeSpawn()
        {
            m_MeshObject.SetActive(false);
        }

    }
}