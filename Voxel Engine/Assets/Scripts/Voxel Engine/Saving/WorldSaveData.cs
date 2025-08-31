using System;
using UnityEngine.Serialization;

namespace Voxel_Engine.Saving
{
    [Serializable]
    public class WorldSaveData : SaveData
    {
        public int chunkSizeInVoxel;
        public int chunkHeightInVoxel;
        public int worldSeedX;
        public int worldSeedY;
        public float voxelScalingX;
        public float voxelScalingY;
        public float voxelScalingZ;

        public WorldSaveData(WorldData worldData)
        {
            chunkSizeInVoxel = worldData.ChunkSizeInVoxel;
            chunkHeightInVoxel = worldData.ChunkHeightInVoxel;
            worldSeedX = worldData.WorldSeed.x;
            worldSeedY = worldData.WorldSeed.y;
            voxelScalingX = worldData.VoxelScaling.x;
            voxelScalingY = worldData.VoxelScaling.y;
            voxelScalingZ = worldData.VoxelScaling.z;
        }
    }
}