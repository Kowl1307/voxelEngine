using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voxel_Engine.ChunkSelectors
{
    public class RangeChunkSelector : MonoBehaviour, IChunkSelector
    {
        public WorldGenerationData GetWorldGenerationData(World world, Vector3Int voxelPosition)
        {
            var worldData = world.WorldData;
            //What needs to exist
            var allChunkPositionsNeeded = GetChunkPositionsAroundPlayer(world, voxelPosition);
            var allChunkDataPositionsNeeded = GetDataPositionsAroundPlayer(world, voxelPosition);

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
        
        public static List<Vector3Int> GetChunkPositionsAroundPlayer(World world, Vector3Int playerVoxelPosition)
        {
            //TODO This whole thing is working with ints, even though the world coordinates are not ints...
            // This needs to be changed to calculate the voxel positions instead.
            //We calculate the chunk world position to avoid potential rounding errors in calculation
            //var playerWorldPos = GetChunkWorldPositionFromWorldCoords(world, voxelPosition);
            // var chunkWorldPos = GetChunkWorldPositionFromVoxelCoords(world, voxelPosition);
            var chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(world, playerVoxelPosition);
            var startX = chunkVoxelPos.x - (world.ChunkDrawingRange) * world.WorldData.ChunkSizeInVoxel;
            var startZ = chunkVoxelPos.z - (world.ChunkDrawingRange) * world.WorldData.ChunkSizeInVoxel;
            var endX =   chunkVoxelPos.x + (world.ChunkDrawingRange) * world.WorldData.ChunkSizeInVoxel; 
            var endZ =   chunkVoxelPos.z + (world.ChunkDrawingRange) * world.WorldData.ChunkSizeInVoxel;

            return GetPositionsAroundPlayer(world, startX, startZ, endX, endZ, playerVoxelPosition);
        }
        
        public static List<Vector3Int> GetDataPositionsAroundPlayer(World world, Vector3Int playerVoxelPosition)
        {
            //We calculate the chunk world position to avoid potential rounding errors in calculation
            // var chunkWorldPos = GetChunkWorldPositionFromVoxelCoords(world, voxelPosition);
            var chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(world, playerVoxelPosition);
            var startX = chunkVoxelPos.x - (world.ChunkDrawingRange + 1) * world.WorldData.ChunkSizeInVoxel;
            var startZ = chunkVoxelPos.z - (world.ChunkDrawingRange + 1) * world.WorldData.ChunkSizeInVoxel;
            var endX =   chunkVoxelPos.x   + (world.ChunkDrawingRange + 1) *   world.WorldData.ChunkSizeInVoxel;
            var endZ =   chunkVoxelPos.z   + (world.ChunkDrawingRange + 1) *   world.WorldData.ChunkSizeInVoxel;

            return GetPositionsAroundPlayer(world, startX, startZ, endX, endZ, playerVoxelPosition);
        }
        
        /// <summary>
        /// Get Chunk positions around the player
        /// </summary>
        /// <param name="world"></param>
        /// <param name="startX"></param>
        /// <param name="startZ"></param>
        /// <param name="endX"></param>
        /// <param name="endZ"></param>
        /// <param name="playerVoxelPosition"></param>
        /// <returns></returns>
        private static List<Vector3Int> GetPositionsAroundPlayer(World world, int startX, int startZ, int endX,
            int endZ, Vector3Int playerVoxelPosition)
        {
            var positions = new List<Vector3Int>();
            for (var x = startX; x <= endX; x += world.WorldData.ChunkSizeInVoxel)
            {
                for (var z = startZ; z <= endZ; z += world.WorldData.ChunkSizeInVoxel)
                {
                    var chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(world, new Vector3Int(x, playerVoxelPosition.y, z));
                    positions.Add(chunkVoxelPos);
                    
                    if(    x < playerVoxelPosition.x - world.WorldData.ChunkSizeInVoxel
                           || x > playerVoxelPosition.x + world.WorldData.ChunkSizeInVoxel
                           || z < playerVoxelPosition.z - world.WorldData.ChunkSizeInVoxel
                           || z > playerVoxelPosition.z + world.WorldData.ChunkSizeInVoxel) continue;
                    
                    //Also add chunks below the current one, for digging down scenarios
                    for (var y = -world.WorldData.ChunkHeightInVoxel; y >= playerVoxelPosition.y - world.WorldData.ChunkHeightInVoxel * 2; y -= world.WorldData.ChunkHeightInVoxel)
                    {
                        if (y == 0) continue;
                        chunkVoxelPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(world, new Vector3Int(x, y, z));
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