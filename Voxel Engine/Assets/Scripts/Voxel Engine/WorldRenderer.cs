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
        public Material chunkMaterialPerVoxel;
        public Material chunkMaterialGreedy;
        
        
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
            return newChunk;
        }

        public void RemoveChunk(ChunkRenderer chunk)
        {
            // chunk.gameObject.SetActive(false);
            foreach (var chunkDecorationObject in chunk.ChunkData.ChunkDecorations)
            {
                chunkDecorationObject.Dispose();
            }
            
            _chunkPool.ReturnObject(chunk);
        }

        public void FillChunkPool(int chunkDrawingRange)
        {
            // Whole area + surrounding edges
            var maxNumberOfChunks = chunkDrawingRange * chunkDrawingRange + chunkDrawingRange * 4;
            _chunkPool.FillTo(maxNumberOfChunks);
            _chunkPool.SetRefillAmount(chunkDrawingRange * 4);
        }
    }
}