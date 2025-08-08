using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;
using Voxel_Engine.WorldGen.Structures;
using Voxel_Engine.WorldGen.VoxelLayers;

namespace Voxel_Engine.WorldGen
{
    public class BiomeGenerator : MonoBehaviour
    {
        public BiomeType biomeType = BiomeType.Undefined;
        public BiomeSettingsSO BiomeSettings;

        public NoiseSettings BiomeNoiseSettings;

        public DomainWarping DomainWarping;
        public bool useDomainWarping = true;

        public VoxelLayerHandler StartLayerHandler;

        public List<VoxelLayerHandler> additionalLayerHandlers;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="x">Chunk Coord</param>
        /// <param name="z">Chunk Coord</param>
        /// <param name="mapSeedOffset"></param>
        /// <returns></returns>
        public ChunkData ProcessChunkColumn(ChunkData chunkData, int x, int z, Vector2Int mapSeedOffset)
        {
            BiomeNoiseSettings.Seed = mapSeedOffset;
            var groundPosition = GetSurfaceHeightNoise(chunkData.ChunkPositionInVoxel.x + x,chunkData.ChunkPositionInVoxel.z + z, chunkData);

            //Fill the whole chunk with voxelType data
            for (var y = 0; y < chunkData.ChunkHeight; y++)
            {
                StartLayerHandler.Handle(chunkData, x, y, z, groundPosition, mapSeedOffset, BiomeSettings);
            }
            
            /*
            foreach (var layer in additionalLayerHandlers)
            {
                layer.Handle(chunkData, x, 0, z, groundPosition, mapSeedOffset, BiomeSettings);
            }
            */

            return chunkData;
        }

        /// <summary>
        /// The function that calculates the surface height.
        /// To change terrain generation, swap the Noise function
        /// </summary>
        /// <param name="x">Voxel Coord</param>
        /// <param name="z">Voxel Coord</param>
        /// <param name="chunkData"></param>
        /// <returns></returns>
        public int GetSurfaceHeightNoise(int x, int z, ChunkData chunkData)
        {
            var voxelScale = chunkData.WorldReference.WorldData.VoxelScaling;
            var chunkHeight = chunkData.ChunkHeight;
            
            var terrainHeight = useDomainWarping ? DomainWarping.GenerateDomainNoise(x * voxelScale.x, z * voxelScale.z, BiomeNoiseSettings) : MyNoise.OctavePerlin(x * voxelScale.x, z * voxelScale.z, BiomeNoiseSettings);
            // terrainHeight /= voxelScale.y;
            //var terrainHeight = useDomainWarping ? MyNoise.OctaveSimplex(x,z,BiomeNoiseSettings) : MyNoise.SimplexNoise(x, z, BiomeNoiseSettings);
            terrainHeight = MyNoise.Redistribution(terrainHeight, BiomeNoiseSettings);
            return MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight / voxelScale.y) + BiomeSettings.MinimumHeight;
        }
    }
}
