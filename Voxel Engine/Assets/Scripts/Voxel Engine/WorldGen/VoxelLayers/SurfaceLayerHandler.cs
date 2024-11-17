using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public class SurfaceLayerHandler : VoxelLayerHandler
    {
        [SerializeField] private VoxelType surfaceType;
        
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            var voxelY = Chunk.GetVoxelCoordsFromChunkCoords(chunkData, x, y, z).y;
            if (voxelY != surfaceHeightNoise)
                return false;

            if(chunkData.ChunkPositionInVoxel.y < 0)
                Debug.Log("???");
            
            var pos = new Vector3Int(x, y, z);
            Chunk.SetVoxel(chunkData, pos, surfaceType);
            return true;
        }
    }
}