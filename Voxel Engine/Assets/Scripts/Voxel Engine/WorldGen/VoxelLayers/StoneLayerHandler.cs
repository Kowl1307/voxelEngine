using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public class StoneLayerHandler : VoxelLayerHandler
    {
        [Range(0,1f)]
        public float stoneThreshold = .5f;

        [SerializeField] private NoiseSettings stoneNoiseSettings;

        public DomainWarping DomainWarping;
        
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            var voxelY = Chunk.GetVoxelCoordsFromChunkCoords(chunkData, x, y, z).y;
            if (voxelY > surfaceHeightNoise)
                return false;

            stoneNoiseSettings.WorldOffset = mapSeedOffset;
            //var stoneNoise = MyNoise.OctavePerlin(chunkData.WorldPosition.x + x, chunkData.WorldPosition.z + z, stoneNoiseSettings);
            var stoneNoise = DomainWarping.GenerateDomainNoise(chunkData.ChunkPositionInVoxel.x + x, chunkData.ChunkPositionInVoxel.z + z, stoneNoiseSettings);

            var endPos = surfaceHeightNoise;
            
            if (chunkData.ChunkPositionInVoxel.y < 0)
            {
                endPos = chunkData.ChunkPositionInVoxel.y + chunkData.ChunkHeight;
            }

            if (!(stoneNoise > stoneThreshold)) return false;
            
            for (var i = chunkData.ChunkPositionInVoxel.y; i <= endPos; i++)
            {
                var pos = new Vector3Int(x, i, z);
                Chunk.SetVoxel(chunkData, pos, VoxelType.Stone);
            }

            return true;

        }
    }
}
