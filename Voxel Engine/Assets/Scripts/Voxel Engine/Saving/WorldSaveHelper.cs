using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Voxel_Engine.Saving
{
    public static class WorldSaveHelper
    {
        private const string WorldSettingsFileName = "/settings";
        private const string WorldSuffix = ".world";
        private const string ChunkSuffix = ".chunk";

        public static void SaveWorld(World world)
        {
            var worldSaveData = new WorldSaveData(world.WorldData);

            var worldName = world.name;
            
            SaveSystem.SaveData(worldSaveData, worldName+WorldSettingsFileName+WorldSuffix);

            foreach (var chunkDataKvP in world.WorldData.ChunkDataDictionary)
            {
                if (!chunkDataKvP.Value.IsDirty())
                {
                    continue;
                }
                var chunkSaveData = new ChunkSaveData(chunkDataKvP.Value);
                SaveSystem.SaveData(chunkSaveData, worldName+"/"+chunkDataKvP.Key+ChunkSuffix);
            }
        }

        /// <summary>
        /// Loads the given world by its name (gameObject name has to be set beforehand!)
        /// </summary>
        /// <param name="world"></param>
        public static void LoadWorld(World world)
        {
            var worldName = world.name;
            try
            {
                var worldSaveData = SaveSystem.LoadData<WorldSaveData>(worldName+WorldSettingsFileName+WorldSuffix);
                
                world.WorldData = new WorldData()
                {
                    ChunkHeightInVoxel = worldSaveData.chunkHeightInVoxel,
                    ChunkSizeInVoxel = worldSaveData.chunkSizeInVoxel,
                    ChunkDataDictionary = new ConcurrentDictionary<Vector3Int, ChunkData>(),
                    ChunkDictionary = new ConcurrentDictionary<Vector3Int, ChunkRenderer>(),
                    WorldSeed = new Vector2Int(worldSaveData.worldSeedX, worldSaveData.worldSeedY),
                    VoxelScaling = new Vector3(worldSaveData.voxelScalingX, worldSaveData.voxelScalingY,
                        worldSaveData.voxelScalingZ),
                };

                foreach (var fileName in SaveSystem.GetAllFileNamesWithSuffix(world.name, ChunkSuffix))
                {
                    var chunkSaveData = SaveSystem.LoadData<ChunkSaveData>(fileName);
                    var positionInWorld = new Vector3Int(chunkSaveData.positionInWorld[0], chunkSaveData.positionInWorld[1], chunkSaveData.positionInWorld[2]);
                    world.ChunkSaveCache.TryAdd(positionInWorld, chunkSaveData);
                }
            }
            catch (Exception e)
            {
                // World has no save data, so no need to do anything
                Debug.LogWarning("Tried loading world with no save data!", world);
            }
        }
    }
}