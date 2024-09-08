using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voxel_Engine
{
    public static class WorldDataHelper
    {
        public static Vector3Int ChunkPositionFromVoxelCoords(World world, Vector3Int voxelCoords)
        {
            return new Vector3Int
            {
                x = Mathf.FloorToInt(voxelCoords.x / (float)world.chunkSize) * world.chunkSize,
                y = Mathf.FloorToInt(voxelCoords.y / (float)world.chunkHeight) * world.chunkHeight,
                z = Mathf.FloorToInt(voxelCoords.z / (float)world.chunkSize) * world.chunkSize
            };
        }
        
        public static List<Vector3Int> GetChunkPositionsAroundPlayer(World world, Vector3Int playerPosition)
        {
            var startX = playerPosition.x - (world.ChunkDrawingRange) * world.chunkSize;
            var startZ = playerPosition.z - (world.ChunkDrawingRange) * world.chunkSize;
            var endX = playerPosition.x + (world.ChunkDrawingRange) * world.chunkSize;
            var endZ = playerPosition.z + (world.ChunkDrawingRange) * world.chunkSize;

            return GetPositionsAroundPlayer(world, startX, startZ, endX, endZ, playerPosition);
        }

        public static List<Vector3Int> GetDataPositionsAroundPlayer(World world, Vector3Int playerPosition)
        {
            var startX = playerPosition.x - (world.ChunkDrawingRange + 1) * world.chunkSize;
            var startZ = playerPosition.z - (world.ChunkDrawingRange + 1) * world.chunkSize;
            var endX = playerPosition.x + (world.ChunkDrawingRange + 1) * world.chunkSize;
            var endZ = playerPosition.z + (world.ChunkDrawingRange + 1) * world.chunkSize;

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
            for (var x = startX; x <= endX; x += world.chunkSize)
            {
                for (var z = startZ; z <= endZ; z += world.chunkSize)
                {
                    var chunkPos = ChunkPositionFromVoxelCoords(world, new Vector3Int(x, 0, z));
                    positions.Add(chunkPos);
                    
                    //Also add chunks below the current one, for digging down scenarios
                    if(x < playerPosition.x - world.chunkSize
                        || x > playerPosition.x + world.chunkSize
                        || z < playerPosition.z - world.chunkSize
                        || z > playerPosition.z + world.chunkSize) continue;
                    
                    for (var y = -world.chunkHeight; y >= playerPosition.y - world.chunkHeight * 2; y -= world.chunkHeight)
                    {
                        chunkPos = ChunkPositionFromVoxelCoords(world, new Vector3Int(x, y, z));
                        positions.Add(chunkPos);
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
            
            var localPos = Chunk.GetVoxelInChunkCoordinates(chunkData, worldPos);
            Chunk.SetVoxel(chunkData, localPos, voxelType);
        }

        public static ChunkData GetChunkData(World world, Vector3Int worldPos)
         {
            var chunkPos = ChunkPositionFromVoxelCoords(world, worldPos);
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