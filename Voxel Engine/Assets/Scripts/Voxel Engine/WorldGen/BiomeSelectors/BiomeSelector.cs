using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.BiomeSelectors
{
    public interface IBiomeProvider
    {
        public BiomeType GetBiomeTypeAt(WorldData worldData, Vector3Int voxelPosition);
    }
    
    public abstract class BiomeSelector : MonoBehaviour, IBiomeProvider
    {
        public abstract BiomeType GetBiomeTypeAt(WorldData worldData, Vector3Int voxelPosition);

        public abstract void PrecomputeData(WorldData worldData, Vector3 worldPosition);
    }
}