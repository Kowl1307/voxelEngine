using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.BiomeSelectors
{
    public interface IBiomeProvider
    {
        public BiomeGeneratorSelection GetBiomeSelection(Vector3Int voxelPosition, ChunkData chunkData);
    }
    
    public abstract class BiomeSelector : MonoBehaviour, IBiomeProvider
    {
        public abstract BiomeGeneratorSelection GetBiomeSelection(Vector3Int voxelPosition, ChunkData chunkData);

        public abstract void PrecomputeData(World world, Vector3Int worldPosition);
    }
}