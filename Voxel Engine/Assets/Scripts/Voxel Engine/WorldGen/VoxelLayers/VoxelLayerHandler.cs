using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public abstract class VoxelLayerHandler : MonoBehaviour
    {
        [SerializeField] 
        private VoxelLayerHandler nextLayer;

        public bool Handle(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            if (TryHandling(chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset, biomeSettings))
                return true;
            if (nextLayer != null)
                return nextLayer.Handle(chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset, biomeSettings);
            return false;
        }

        protected abstract bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise,
            Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings);
    }
}
