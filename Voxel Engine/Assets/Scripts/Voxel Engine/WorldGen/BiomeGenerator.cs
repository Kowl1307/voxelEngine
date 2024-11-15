using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;
using Voxel_Engine.WorldGen.Structures;
using Voxel_Engine.WorldGen.Trees;
using Voxel_Engine.WorldGen.VoxelLayers;

namespace Voxel_Engine.WorldGen
{
    public class BiomeGenerator : MonoBehaviour
    {
        public BiomeSettingsSO BiomeSettings;

        public NoiseSettings BiomeNoiseSettings;

        public DomainWarping DomainWarping;
        public bool useDomainWarping = true;

        public VoxelLayerHandler StartLayerHandler;

        public List<VoxelLayerHandler> additionalLayerHandlers;

        [SerializeField] private List<StructureGenerator> StructureGenerators = new List<StructureGenerator>();

        private void Awake()
        {
            //StructureGenerators.AddRange(GetComponents<StructureGenerator>());
        }

        public ChunkData ProcessChunkColumn(ChunkData chunkData, int x, int z, Vector2Int mapSeedOffset,
            int? terrainHeightNoise)
        {
            BiomeNoiseSettings.WorldOffset = mapSeedOffset;
            var groundPosition = terrainHeightNoise ?? GetSurfaceHeightNoise(chunkData.ChunkPositionInVoxel.x + x,chunkData.ChunkPositionInVoxel.z + z, chunkData.ChunkHeight, BiomeSettings);

            //Fill the whole chunk with voxelType data
            for (var y = chunkData.ChunkPositionInVoxel.y; y < chunkData.ChunkHeight + chunkData.ChunkPositionInVoxel.y; y++)
            {
                StartLayerHandler.Handle(chunkData, x, y, z, groundPosition, mapSeedOffset, BiomeSettings);
            }

            foreach (var layer in additionalLayerHandlers)
            {
                layer.Handle(chunkData, x, chunkData.ChunkPositionInVoxel.y, z, groundPosition, mapSeedOffset, BiomeSettings);
            }

            return chunkData;
        }

        /// <summary>
        /// The function that calculates the surface height.
        /// To change terrain generation, swap the Noise function
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="chunkHeight"></param>
        /// <returns></returns>
        public int GetSurfaceHeightNoise(int x, int z, int chunkHeight, BiomeSettingsSO biomeSettings)
        {
            var terrainHeight = useDomainWarping ? DomainWarping.GenerateDomainNoise(x, z, BiomeNoiseSettings) : MyNoise.OctavePerlin(x, z, BiomeNoiseSettings);
            //var terrainHeight = useDomainWarping ? MyNoise.OctaveSimplex(x,z,BiomeNoiseSettings) : MyNoise.SimplexNoise(x, z, BiomeNoiseSettings);
            terrainHeight = MyNoise.Redistribution(terrainHeight, BiomeNoiseSettings);
            return MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight) + biomeSettings.MinimumHeight;
        }

        public List<StructureData> GetStructureData(ChunkData chunkData, Vector2Int mapSeedOffset)
        {
            return StructureGenerators.Select(structureGenerator => structureGenerator.GenerateData(chunkData, mapSeedOffset)).ToList();
        }
    }
}
