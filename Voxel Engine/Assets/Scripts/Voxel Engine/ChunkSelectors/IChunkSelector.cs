using System.Threading.Tasks;
using UnityEngine;

namespace Voxel_Engine.ChunkSelectors
{
    public interface IChunkSelector
    {
        public WorldGenerationData GetWorldGenerationData(WorldData worldData, Vector3Int voxelPosition);
    }
}