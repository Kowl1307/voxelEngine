using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public abstract class VoxelLayerHandler : MonoBehaviour
    {
        [SerializeField] 
        private VoxelLayerHandler nextLayer;

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="x">ChunkCoords</param>
        /// <param name="y">ChunkCoords</param>
        /// <param name="z">ChunkCoords</param>
        /// <param name="surfaceHeightNoise"></param>
        /// <param name="mapSeedOffset"></param>
        /// <param name="biomeSettings"></param>
        /// <returns></returns>
        public bool Handle(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            if (TryHandling(chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset, biomeSettings))
                return true;
            if (nextLayer != null)
                return nextLayer.Handle(chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset, biomeSettings);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="x">ChunkCoords</param>
        /// <param name="y">ChunkCoords</param>
        /// <param name="z">ChunkCoords</param>
        /// <param name="surfaceHeightNoise"></param>
        /// <param name="mapSeedOffset"></param>
        /// <param name="biomeSettings"></param>
        /// <returns></returns>
        protected abstract bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise,
            Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings);
    }
}
