using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voxel_Engine.ChunkSelectors
{
    public class RangeChunkSelector : MonoBehaviour, IChunkSelector
    {
        public WorldGenerationData GetWorldGenerationData(WorldData worldData, Vector3Int voxelPosition)
        {
            //What needs to exist
            var allChunkPositionsNeeded = GetChunkPositionsAroundPlayer(worldData, voxelPosition);
            var allChunkDataPositionsNeeded = GetDataPositionsAroundPlayer(worldData, voxelPosition);

            //Things needed to create (do not exist yet)
            var chunkPositionsToCreate = SelectPositionsToCreate(worldData, allChunkPositionsNeeded, voxelPosition);
            var chunkDataPositionsToCreate = SelectDataPositionsToCreate(worldData, allChunkDataPositionsNeeded, voxelPosition);

            var chunkPositionsToRemove = GetUnneededChunks(worldData, allChunkPositionsNeeded);
            var chunkDataToRemove = GetUnneededData(worldData, allChunkDataPositionsNeeded);

            var data = new WorldGenerationData
            {
                ChunkPositionsToCreate = chunkPositionsToCreate,
                ChunkDataPositionsToCreate = chunkDataPositionsToCreate,
                ChunkPositionsToRemove = chunkPositionsToRemove,
                ChunkDataToRemove = chunkDataToRemove
            };
            return data;
        }
        
        public static List<Vector3Int> GetChunkPositionsAroundPlayer(WorldData worldData, Vector3Int playerVoxelPosition)
        {
            //TODO This whole thing is working with ints, even though the world coordinates are not ints...
            // This needs to be changed to calculate the voxel positions instead.
            //We calculate the chunk world position to avoid potential rounding errors in calculation
            //var playerWorldPos = GetChunkWorldPositionFromWorldCoords(world, voxelPosition);
            // var chunkWorldPos = GetChunkWorldPositionFromVoxelCoords(world, voxelPosition);
            var chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(worldData, playerVoxelPosition);
            var startX = chunkVoxelPos.x - (worldData.ChunkDrawingRange) * worldData.ChunkSizeInVoxel;
            var startZ = chunkVoxelPos.z - (worldData.ChunkDrawingRange) * worldData.ChunkSizeInVoxel;
            var endX =   chunkVoxelPos.x + (worldData.ChunkDrawingRange) * worldData.ChunkSizeInVoxel; 
            var endZ =   chunkVoxelPos.z + (worldData.ChunkDrawingRange) * worldData.ChunkSizeInVoxel;

            return GetPositionsAroundPlayer(worldData, startX, startZ, endX, endZ, playerVoxelPosition);
        }
        
        public static List<Vector3Int> GetDataPositionsAroundPlayer(WorldData worldData, Vector3Int playerVoxelPosition)
        {
            //We calculate the chunk world position to avoid potential rounding errors in calculation
            // var chunkWorldPos = GetChunkWorldPositionFromVoxelCoords(world, voxelPosition);
            var chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(worldData, playerVoxelPosition);
            var startX = chunkVoxelPos.x - (worldData.ChunkDrawingRange + 1) * worldData.ChunkSizeInVoxel;
            var startZ = chunkVoxelPos.z - (worldData.ChunkDrawingRange + 1) * worldData.ChunkSizeInVoxel;
            var endX =   chunkVoxelPos.x   + (worldData.ChunkDrawingRange + 1) *   worldData.ChunkSizeInVoxel;
            var endZ =   chunkVoxelPos.z   + (worldData.ChunkDrawingRange + 1) *   worldData.ChunkSizeInVoxel;

            return GetPositionsAroundPlayer(worldData, startX, startZ, endX, endZ, playerVoxelPosition);
        }

        /// <summary>
        /// Get Chunk positions around the player
        /// </summary>
        /// <param name="worldData"></param>
        /// <param name="startX"></param>
        /// <param name="startZ"></param>
        /// <param name="endX"></param>
        /// <param name="endZ"></param>
        /// <param name="playerVoxelPosition"></param>
        /// <returns></returns>
        private static List<Vector3Int> GetPositionsAroundPlayer(WorldData worldData, int startX, int startZ, int endX,
            int endZ, Vector3Int playerVoxelPosition)
        {
            var positions = new List<Vector3Int>();
            for (var x = startX; x <= endX; x += worldData.ChunkSizeInVoxel)
            {
                for (var z = startZ; z <= endZ; z += worldData.ChunkSizeInVoxel)
                {
                    var chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(worldData, new Vector3Int(x, playerVoxelPosition.y, z));
                    positions.Add(chunkVoxelPos);
                    
                    if(    x < playerVoxelPosition.x - worldData.ChunkSizeInVoxel
                           || x > playerVoxelPosition.x + worldData.ChunkSizeInVoxel
                           || z < playerVoxelPosition.z - worldData.ChunkSizeInVoxel
                           || z > playerVoxelPosition.z + worldData.ChunkSizeInVoxel) continue;
                    
                    //Also add chunks below the current one, for digging down scenarios
                    for (var y = -worldData.ChunkHeightInVoxel; y >= playerVoxelPosition.y - worldData.ChunkHeightInVoxel * 2; y -= worldData.ChunkHeightInVoxel)
                    {
                        if (y == 0) continue;
                        chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(worldData, new Vector3Int(x, y, z));
                        positions.Add(chunkVoxelPos);
                    }
                }
            }

            return positions;
        }

        public static List<Vector3Int> SelectPositionsToCreate(WorldData worldData, List<Vector3Int> allChunkPositionsNeeded, Vector3Int playerPositionInVoxel)
        {
            return allChunkPositionsNeeded
                .Where(pos => worldData.ChunkDictionary.ContainsKey(pos) == false)
                .OrderBy(pos => Vector3.Distance(playerPositionInVoxel, pos))
                .ToList();
        }

        public static List<Vector3Int> SelectDataPositionsToCreate(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded, Vector3Int playerPositionInVoxel)
        {
            return allChunkDataPositionsNeeded
                .Where(pos => worldData.ChunkDataDictionary.ContainsKey(pos) == false)
                .OrderBy(pos => Vector3.Distance(playerPositionInVoxel, pos))
                .ToList();
        }

        public static List<Vector3Int> GetUnneededChunks(WorldData worldData, List<Vector3Int> allChunkPositionsNeeded)
        {
            return worldData.ChunkDictionary.Keys
                .Where(pos => allChunkPositionsNeeded.Contains(pos) == false)
                .ToList();
        }

        public static List<Vector3Int> GetUnneededData(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded)
        {
            return worldData.ChunkDataDictionary.Keys
                .Where(pos => allChunkDataPositionsNeeded.Contains(pos) == false 
                              && worldData.ChunkDataDictionary[pos].IsDirty() == false
                )
                .ToList();
        }
    }
}