using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Voxel_Engine.Saving
{
    [Serializable]
    public class ChunkSaveData : SaveData
    {
        private Dictionary<int, VoxelType> _modifiedVoxels;
        
        public ChunkSaveData(ChunkData chunkData)
        {
            _modifiedVoxels = chunkData.GetModifiedVoxels();
        }
    }
}