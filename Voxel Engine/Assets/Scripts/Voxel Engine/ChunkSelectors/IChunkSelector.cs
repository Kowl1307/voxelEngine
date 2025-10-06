using System.Threading.Tasks;
using UnityEngine;

namespace Voxel_Engine.ChunkSelectors
{
    public interface IChunkSelector
    {
        public WorldGenerationData GetWorldGenerationData(World world, Vector3Int voxelPosition);
    }
}