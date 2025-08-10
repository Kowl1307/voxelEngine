using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.BiomeSelectors
{
    public interface IBiomeProvider
    {
        public BiomeType GetBiomeTypeAt(Vector3Int voxelPosition, WorldData worldData);
    }
    
    public abstract class BiomeSelector : MonoBehaviour, IBiomeProvider
    {
        public abstract BiomeType GetBiomeTypeAt(Vector3Int voxelPosition, WorldData worldData);

        public abstract void PrecomputeData(World world, Vector3Int worldPosition);
    }
}