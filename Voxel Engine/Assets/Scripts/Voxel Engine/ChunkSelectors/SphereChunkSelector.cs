using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine.ChunkSelectors
{
    public class SphereChunkSelector : IChunkSelector
    {
        public WorldGenerationData GetWorldGenerationData(WorldData worldData, Vector3Int voxelPosition)
        {
            List<Vector3Int> chunkPositionsToCreate = new();
            List<Vector3Int> chunkDataPositionsToCreate = new();
            List<Vector3Int> chunkPositionsToRemove = new();
            List<Vector3Int> chunkDataToRemove = new();
            
            return new WorldGenerationData()
            {
                ChunkPositionsToCreate = chunkPositionsToCreate,
                ChunkDataPositionsToCreate = chunkDataPositionsToCreate,
                ChunkPositionsToRemove = chunkPositionsToRemove,
                ChunkDataToRemove = chunkDataToRemove
            };
        }
    }
}