using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public class UndergroundLayerHandler : VoxelLayerHandler
    {
        [SerializeField] private VoxelType undergroundType;
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            if (y >= surfaceHeightNoise)
                return false;
            //Needed for underground layers
            var localY = y - chunkData.ChunkPositionInVoxel.y;
            
            var pos = new Vector3Int(x, localY, z);
            Chunk.SetVoxel(chunkData, pos, undergroundType);
            return true;
        }
    }
}