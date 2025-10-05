using System;
using System.Collections;
using System.Collections.Generic;
using Kowl.Utils;
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
        
        public ChunkRenderer RenderChunk(World world, WorldData worldData, Vector3 worldPosition, MeshData meshData)
        {
            var newChunk = _chunkPool.GetObject();
            
            newChunk.transform.position = worldPosition;
            newChunk.InitializeChunk(worldData.ChunkDataDictionary[WorldDataHelper.GetVoxelPositionFromWorldPosition(world, worldPosition)]);
            newChunk.UpdateChunk(meshData);
            return newChunk;
        }

        public void RemoveChunk(ChunkRenderer chunk)
        {
            /*
            foreach (var chunkDecorationObject in chunk.ChunkData.ChunkDecorations)
            {
                chunkDecorationObject.Dispose();
            }
            chunk.ChunkData.ChunkDecorations.Clear();
            */
            
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