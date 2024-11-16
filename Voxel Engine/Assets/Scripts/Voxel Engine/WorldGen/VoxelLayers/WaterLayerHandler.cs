using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public class WaterLayerHandler : VoxelLayerHandler
    {
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            var voxelY = Chunk.GetVoxelCoordsFromChunkCoords(chunkData, x, y, z).y;

            if (voxelY <= surfaceHeightNoise || voxelY > biomeSettings.WaterLevel)
                return false;
            
            var pos = new Vector3Int(x, y, z);
            
            Chunk.SetVoxel(chunkData, pos, VoxelType.Water);
            
            if (voxelY != surfaceHeightNoise + 1) return true;
            //Generate the sea floor
            pos.y -= 1;
            Chunk.SetVoxel(chunkData, pos, VoxelType.Sand);
            return true;
        }
    }
}