using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voxel_Engine
{
    public static class WorldDataHelper
    {
        public static Vector3Int GetChunkPositionFromVoxelCoords(World world, Vector3Int voxelCoords)
        {
            return new Vector3Int
            {
                x = Mathf.FloorToInt(voxelCoords.x / (float)world.chunkSizeInVoxel) * world.chunkSizeInVoxel,
                y = Mathf.FloorToInt(voxelCoords.y / (float)world.chunkHeightInVoxel) * world.chunkHeightInVoxel,
                z = Mathf.FloorToInt(voxelCoords.z / (float)world.chunkSizeInVoxel) * world.chunkSizeInVoxel
            };
        }

        public static Vector3Int GetChunkWorldPositionFromWorldCoords(World world, int worldX, int worldY, int worldZ) => GetChunkWorldPositionFromWorldCoords(world, new Vector3Int(worldX, worldY, worldZ));
        
        public static Vector3Int GetChunkWorldPositionFromWorldCoords(World world, Vector3Int worldCoords)
        {
            return new Vector3Int
            {
                x = Mathf.FloorToInt(worldCoords.x / (float)world.chunkSizeInWorld) * world.chunkSizeInWorld,
                y = Mathf.FloorToInt(worldCoords.y / (float)world.chunkHeightInWorld) * world.chunkSizeInWorld,
                z = Mathf.FloorToInt(worldCoords.z / (float)world.chunkSizeInWorld) * world.chunkSizeInWorld
            };
        }
        
        public static List<Vector3Int> GetChunkPositionsAroundPlayer(World world, Vector3Int playerPosition)
        {
            var startX = playerPosition.x - (world.ChunkDrawingRange) * world.chunkSizeInWorld;
            var startZ = playerPosition.z - (world.ChunkDrawingRange) * world.chunkSizeInWorld;
            var endX = playerPosition.x + (world.ChunkDrawingRange) * world.chunkSizeInWorld; 
            var endZ = playerPosition.z + (world.ChunkDrawingRange) * world.chunkSizeInWorld;

            return GetPositionsAroundPlayer(world, startX, startZ, endX, endZ, playerPosition);
        }

        public static List<Vector3Int> GetDataPositionsAroundPlayer(World world, Vector3Int playerPosition)
        {
            var startX = playerPosition.x - (world.ChunkDrawingRange + 1) * world.chunkSizeInWorld;
            var startZ = playerPosition.z - (world.ChunkDrawingRange + 1) * world.chunkSizeInWorld;
            var endX = playerPosition.x + (world.ChunkDrawingRange + 1) * world.chunkSizeInWorld;
            var endZ = playerPosition.z + (world.ChunkDrawingRange + 1) * world.chunkSizeInWorld;

            return GetPositionsAroundPlayer(world, startX, startZ, endX, endZ, playerPosition);
        }

        /// <summary>
        /// Get Chunk positions around the player
        /// </summary>
        /// <param name="world"></param>
        /// <param name="startX"></param>
        /// <param name="startZ"></param>
        /// <param name="endX"></param>
        /// <param name="endZ"></param>
        /// <param name="playerPosition">In world space</param>
        /// <returns></returns>
        private static List<Vector3Int> GetPositionsAroundPlayer(World world, int startX, int startZ, int endX,
            int endZ, Vector3Int playerPosition)
        {
            var positions = new List<Vector3Int>();
            for (var x = startX; x <= endX; x += world.chunkSizeInWorld)
            {
                for (var z = startZ; z <= endZ; z += world.chunkSizeInWorld)
                {
                    //var chunkPos = GetChunkPositionFromVoxelCoords(world, new Vector3Int(x, 0, z));
                    var chunkPos = GetChunkWorldPositionFromWorldCoords(world, new Vector3Int(x,0,z));
                    positions.Add(chunkPos);
                    //positions.Add(Vector3Int.FloorToInt(Vector3.Scale(chunkPos, world.voxelScaling)));
                    
                    //Also add chunks below the current one, for digging down scenarios
                    if(x < playerPosition.x - world.chunkSizeInWorld
                        || x > playerPosition.x + world.chunkSizeInWorld
                        || z < playerPosition.z - world.chunkSizeInWorld
                        || z > playerPosition.z + world.chunkSizeInWorld) continue;
                    
                    for (var y = -world.chunkHeightInWorld; y >= playerPosition.y - world.chunkHeightInWorld * 2; y -= world.chunkHeightInWorld)
                    {
                        chunkPos = GetChunkWorldPositionFromWorldCoords(world, new Vector3Int(x, y, z));
                        //chunkPos = GetChunkPositionFromVoxelCoords(world, new Vector3Int(x, y, z));
                        positions.Add(chunkPos);
                        //positions.Add(Vector3Int.FloorToInt(Vector3.Scale(chunkPos, world.voxelScaling)));
                    }
                }
            }

            return positions;
        }

        public static List<Vector3Int> SelectPositionsToCreate(WorldData worldData, List<Vector3Int> allChunkPositionsNeeded, Vector3Int playerPosition)
        {
            return allChunkPositionsNeeded
                .Where(pos => worldData.ChunkDictionary.ContainsKey(pos) == false)
                .OrderBy(pos => Vector3.Distance(playerPosition, pos))
                .ToList();
        }

        public static List<Vector3Int> SelectDataPositionsToCreate(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded, Vector3Int playerPosition)
        {
            return allChunkDataPositionsNeeded
                .Where(pos => worldData.ChunkDataDictionary.ContainsKey(pos) == false)
                .OrderBy(pos => Vector3.Distance(playerPosition, pos))
                .ToList();
        }

        public static List<Vector3Int> GetUnneededChunks(WorldData worldData, List<Vector3Int> allChunkPositionsNeeded)
        {
            return worldData.ChunkDictionary.Keys
                .Where(pos => allChunkPositionsNeeded.Contains(pos) == false)
                .Where(pos => worldData.ChunkDictionary.ContainsKey(pos))
                .ToList();
        }

        public static List<Vector3Int> GetUnneededData(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded)
        {
            return worldData.ChunkDataDictionary.Keys
                .Where(pos => allChunkDataPositionsNeeded.Contains(pos) == false && worldData.ChunkDataDictionary[pos].ModifiedByPlayer == false)
                .ToList();
        }

        public static void RemoveChunk(World world, Vector3Int pos)
        {
            ChunkRenderer chunk = null;
            if (!world.WorldData.ChunkDictionary.TryGetValue(pos, out chunk)) return;
            world.WorldRenderer.RemoveChunk(chunk);
            world.WorldData.ChunkDictionary.TryRemove(pos, out _);
        }

        public static void RemoveChunkData(World world, Vector3Int pos)
        {
            world.WorldData.ChunkDataDictionary.TryRemove(pos, out _);
        }

        public static void SetVoxel(World world, Vector3Int worldPos, VoxelType voxelType)
        {
            var chunkData = GetChunkData(world, worldPos);
            if (chunkData == null) return;
            
            var localPos = Chunk.GetChunkCoordinateOfVoxelPosition(chunkData, worldPos);
            Chunk.SetVoxel(chunkData, localPos, voxelType);
        }

        public static ChunkData GetChunkData(World world, Vector3Int worldPos)
         {
            var chunkPos = GetChunkPositionFromVoxelCoords(world, worldPos);
            ChunkData containerChunk = null;

            world.WorldData.ChunkDataDictionary.TryGetValue(chunkPos, out containerChunk);

            return containerChunk;
        }

        public static ChunkRenderer GetChunk(World worldReference, Vector3Int worldPosition)
        {
            return worldReference.WorldData.ChunkDictionary.ContainsKey(worldPosition) ? worldReference.WorldData.ChunkDictionary[worldPosition] : null;
        }
    }
}