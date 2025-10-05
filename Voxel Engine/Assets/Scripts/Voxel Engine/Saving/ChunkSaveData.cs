using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kowl.Utils.Saving;
using Kowl.Utils.Saving.SerializableTypes;

namespace Voxel_Engine.Saving
{
    [Serializable]
    public class ChunkSaveData : SaveData
    {
        public SerializableDictionary<int, VoxelType> modifiedVoxels;
        public SerializableVector3Int positionInVoxel;
        public SerializableVector3 positionInWorld;
        
        public ChunkSaveData(ChunkData chunkData)
        {
            modifiedVoxels = chunkData.GetModifiedVoxels();

            positionInVoxel = new SerializableVector3Int(chunkData.ChunkPositionInVoxel);
            positionInWorld = new SerializableVector3(chunkData.ChunkPositionInWorld);
        }
    }
}