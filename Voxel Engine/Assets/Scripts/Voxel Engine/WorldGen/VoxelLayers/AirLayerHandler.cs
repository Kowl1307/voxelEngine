using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public class AirLayerHandler : VoxelLayerHandler
    {
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            if (y <= surfaceHeightNoise) return false;
            
            var pos = new Vector3Int(x, y, z);
            Chunk.SetVoxel(chunkData, pos, VoxelType.Air);
            return true;

        }
    }
}