using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Serialization;
using Voxel_Engine.Saving.SerializableTypes;

namespace Voxel_Engine.Saving
{
    [Serializable]
    public class ChunkSaveData : SaveData
    {
        public SerializableDictionary<int, VoxelType> modifiedVoxels;
        public int[] positionInVoxel;
        public int[] positionInWorld;
        
        public ChunkSaveData(ChunkData chunkData)
        {
            modifiedVoxels = chunkData.GetModifiedVoxels();
            
            positionInVoxel = new int[] {
                chunkData.ChunkPositionInVoxel.x,
                chunkData.ChunkPositionInVoxel.y,
                chunkData.ChunkPositionInVoxel.z
            };
            positionInWorld = new int[] {
                chunkData.ChunkPositionInWorld.x,
                chunkData.ChunkPositionInWorld.y,
                chunkData.ChunkPositionInWorld.z
            };
        }
    }
}