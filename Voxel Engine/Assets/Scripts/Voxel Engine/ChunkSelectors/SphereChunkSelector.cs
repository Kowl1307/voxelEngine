using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics.Geometry;
using UnityEngine;
using Math = System.Math;

namespace Voxel_Engine.ChunkSelectors
{
    public class SphereChunkSelector : MonoBehaviour, IChunkSelector
    {
        [SerializeField] private int _renderRadius = 6;
        
        public WorldGenerationData GetWorldGenerationData(WorldData worldData, Vector3Int voxelPosition)
        {
            var allPositionsInSphere = GetAllChunkPositionsInSphere(worldData, voxelPosition);
            var existingPositions = worldData.ChunkDictionary.Keys;
            var existingDataPositions = worldData.ChunkDataDictionary.Keys;
            var chunkPosition = WorldDataHelper.GetChunkPositionFromVoxelCoords(worldData, voxelPosition);
            
            var chunkPositionsToCreate = allPositionsInSphere.Where(position => !existingPositions.Contains(position) && IsInSphere(chunkPosition, position, _renderRadius * worldData.ChunkSizeInVoxel)).ToList();
            var chunkDataPositionsToCreate = allPositionsInSphere.Where(position => !existingDataPositions.Contains(position) && IsInSphere(chunkPosition, position, (_renderRadius+1) * worldData.ChunkSizeInVoxel)).ToList();
            var chunkPositionsToRemove = worldData.ChunkDictionary.Keys.Where(pos => !allPositionsInSphere.Contains(pos)).ToList();
            var chunkDataToRemove = worldData.ChunkDataDictionary.Keys.Where(pos => !allPositionsInSphere.Contains(pos) && !worldData.ChunkDataDictionary[pos].IsDirty()).ToList();
            
            return new WorldGenerationData()
            {
                ChunkPositionsToCreate = chunkPositionsToCreate,
                ChunkDataPositionsToCreate = chunkDataPositionsToCreate,
                ChunkPositionsToRemove = chunkPositionsToRemove,
                ChunkDataToRemove = chunkDataToRemove
            };
        }

        public List<Vector3Int> GetAllChunkPositionsInSphere(WorldData worldData, Vector3Int voxelPosition)
        {
            var chunkPositions = new List<Vector3Int>();
            
            var chunkVoxelPosition = WorldDataHelper.GetChunkPositionFromVoxelCoords(worldData, voxelPosition);

            for (var x = -_renderRadius; x <= _renderRadius; x++)
            {
                for (var y = -_renderRadius; y <= _renderRadius; y++)
                {
                    for (var z = -_renderRadius; z <= _renderRadius; z++)
                    {
                        if (new Vector3Int(x,y,z).magnitude > _renderRadius) continue;
                        
                        var offset = new Vector3Int(x * worldData.ChunkSizeInVoxel, y * worldData.ChunkHeightInVoxel, z * worldData.ChunkSizeInVoxel);
                        chunkPositions.Add(chunkVoxelPosition + offset);
                    }
                }
            }
            
            return chunkPositions;
        }

        private bool IsInSphere(Vector3Int sphereCenter, Vector3Int voxelPosition, int radius)
        {
            var distance = (sphereCenter - voxelPosition).magnitude;
            return distance <= radius;
        }
    }
}