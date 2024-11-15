using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Voxel_Engine
{
    public class WorldRenderer : MonoBehaviour
    {
        public ChunkRenderer chunkPrefab;
        public Queue<ChunkRenderer> chunkPool = new Queue<ChunkRenderer>();

        public void Clear(WorldData worldData)
        {
            foreach (var chunkRenderer in worldData.ChunkDictionary.Values)
            {
                Destroy(chunkRenderer.gameObject);
            }
            chunkPool.Clear();
        }
        
        public ChunkRenderer RenderChunk(WorldData worldData, Vector3Int position, MeshData meshData, Vector3 voxelScaling)
        {
            ChunkRenderer newChunk;
            if (chunkPool.Count > 0)
            {
                newChunk = chunkPool.Dequeue();
                newChunk.transform.position = position;
            }
            else
            {
                newChunk = Instantiate(chunkPrefab, position, Quaternion.identity);
            }

            newChunk.InitializeChunk(worldData.ChunkDataDictionary[position]);
            newChunk.UpdateChunk(meshData);
            newChunk.gameObject.SetActive(true);

            return newChunk;
        }

        public void RemoveChunk(ChunkRenderer chunk)
        {
            chunk.gameObject.SetActive(false);
            chunkPool.Enqueue(chunk);
        }
    }
}