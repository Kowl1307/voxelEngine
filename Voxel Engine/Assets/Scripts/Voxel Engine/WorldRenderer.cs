using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Voxel_Engine
{
    public class WorldRenderer : MonoBehaviour
    {
        public ChunkRenderer chunkPrefab;
        private ObjectPool<ChunkRenderer> _chunkPool;

        private void Awake()
        {
            _chunkPool = new ObjectPool<ChunkRenderer>(chunkPrefab);
        }

        public void Clear(WorldData worldData)
        {
            foreach (var chunkRenderer in worldData.ChunkDictionary.Values)
            {
                Destroy(chunkRenderer.gameObject);
            }
            _chunkPool.Clear();
        }
        
        public ChunkRenderer RenderChunk(WorldData worldData, Vector3Int position, MeshData meshData, Vector3 voxelScaling)
        {
            var newChunk = _chunkPool.GetObject();
            
            newChunk.transform.position = position;

            newChunk.InitializeChunk(worldData.ChunkDataDictionary[position]);
            newChunk.UpdateChunk(meshData);
            newChunk.gameObject.SetActive(true);

            return newChunk;
        }

        public void RemoveChunk(ChunkRenderer chunk)
        {
            chunk.gameObject.SetActive(false);
            _chunkPool.ReturnObject(chunk);
        }

        public void FillChunkPool(int chunkDrawingRange)
        {
            // Whole area + surrounding edges
            _chunkPool.FillTo(chunkDrawingRange * chunkDrawingRange + chunkDrawingRange * 4);
        }
    }
}