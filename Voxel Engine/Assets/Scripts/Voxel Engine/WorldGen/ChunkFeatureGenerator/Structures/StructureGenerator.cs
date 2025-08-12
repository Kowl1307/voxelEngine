using UnityEngine;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Structures
{
    /// <summary>
    /// A structure changes the voxels inside of the grid. So buildings etc adhere to the voxel grid.
    /// </summary>
    public abstract class StructureGenerator : MonoBehaviour, IChunkFeatureGenerator
    {
        public abstract void Handle(ChunkData chunkData);
    }
}