using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Structures;
using Voxel_Engine.WorldGen.Noise;
using Voxel_Engine.WorldGen.VoxelLayers;
using Task = System.Threading.Tasks.Task;

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

        [SerializeField] private GameObject structureGeneratorsHolder;
        private List<StructureGenerator> _structureGenerators = new();

        [SerializeField] private GameObject decorationGeneratorsHolder;
        private List<DecorationGenerator> _decorationGenerators = new();

        private void Awake()
        {
            if(structureGeneratorsHolder != null)
                _structureGenerators = structureGeneratorsHolder.GetComponents<StructureGenerator>().ToList();
            
            if(decorationGeneratorsHolder != null)
                _decorationGenerators = decorationGeneratorsHolder.GetComponents<DecorationGenerator>().ToList();
        }

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
            var surfaceHeight = GetSurfaceHeightNoise(chunkData.ChunkPositionInVoxel.x + x,chunkData.ChunkPositionInVoxel.z + z, chunkData.WorldReference.WorldData);
            chunkData.HeightMap[x,z] = surfaceHeight;
            
            //Fill the whole chunk with voxelType data
            for (var y = 0; y < chunkData.ChunkHeightInVoxel; y++)
            {
                StartLayerHandler.Handle(chunkData, x, y, z, surfaceHeight, mapSeedOffset, BiomeSettings);
            }

            return chunkData;
        }

        /// <summary>
        /// Calculates the VoxelType of a single voxel. This should be used for non-generated chunks only!
        /// </summary>
        /// <param name="world"></param>
        /// <param name="x">Voxel Coords</param>
        /// <param name="y">Voxel Coords</param>
        /// <param name="z">Voxel Coords</param>
        /// <param name="mapSeedOffset"></param>
        /// <returns></returns>
        public VoxelType ProcessVoxel(World world, int x, int y, int z, Vector2Int mapSeedOffset)
        {
            //Create a temporary chunkData so we can use it to set the voxel
            var voxelCoords = new Vector3Int(x, y, z);
            var chunkData = new ChunkData(world.WorldData.ChunkSizeInVoxel, world.WorldData.ChunkHeightInVoxel, world, WorldDataHelper.GetChunkWorldPositionFromVoxelCoords(world, voxelCoords),
                WorldDataHelper.GetChunkPositionFromVoxelCoords(world, voxelCoords));
            var voxelInChunkCoord = Chunk.GetChunkCoordinateOfVoxelPosition(chunkData, voxelCoords);
            
            var surfaceHeight = GetSurfaceHeightNoise(x, z, chunkData.WorldReference.WorldData);
            StartLayerHandler.Handle(chunkData, voxelInChunkCoord.x, voxelInChunkCoord.y, voxelInChunkCoord.z, surfaceHeight, mapSeedOffset, BiomeSettings);
            
            return chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, voxelInChunkCoord.x, voxelInChunkCoord.y, voxelInChunkCoord.z)];
        }

        public ChunkData GenerateStructures(ChunkData chunkData)
        {
            foreach (var structureGenerator in _structureGenerators)
            {
                structureGenerator.Handle(chunkData);
            }
            
            //Also check for other generators of the neighboring chunks, as structures can go over biome boundaries.
            var adjacentChunkPositionsInVoxel = WorldDataHelper.GetAdjacentChunkPositionsInVoxel(chunkData.WorldReference, chunkData.ChunkPositionInVoxel);
            var structureTypes = _structureGenerators.ConvertAll(generator => generator.GetType());
            
            foreach (var adjacentChunkPosition in adjacentChunkPositionsInVoxel)
            {
                var adjacentStructureGenerators =
                    chunkData.WorldReference.terrainGenerator.GetBiomeGeneratorAt(chunkData.WorldReference.WorldData,
                        adjacentChunkPosition)._structureGenerators;
                
                foreach (var adjacentGenerator in adjacentStructureGenerators.Where(adjacentGenerator => !structureTypes.Contains(adjacentGenerator.GetType())))
                {
                    adjacentGenerator.Handle(chunkData);
                }
            }
            
            return chunkData;
        }

        // TODO: Would be better if decorations are also voxels but are rendered differently. Creating all these objects or having insanely large objectPools seems very inefficient..
        public void GenerateDecorations(ChunkData chunkData)
        {
            Task.Run( () =>
            {
                Parallel.ForEach(_decorationGenerators, chunkData.WorldReference.WorldParallelOptions,
                    decorationGenerator =>
                    {
                        decorationGenerator.Handle(chunkData);
                    });
            });
            /*
            foreach (var decorationGenerator in _decorationGenerators)
            {
                decorationGenerator.Handle(chunkData);
            }
            */
            
        }

        /// <summary>
        /// The function that calculates the surface height.
        /// To change terrain generation, swap the Noise function
        /// </summary>
        /// <param name="x">Voxel Coord</param>
        /// <param name="z">Voxel Coord</param>
        /// <param name="worldData"></param>
        /// <returns></returns>
        public int GetSurfaceHeightNoise(int x, int z, WorldData worldData)
        {
            var voxelScale = worldData.VoxelScaling;
            var chunkHeight = worldData.ChunkHeightInVoxel;
            
            var terrainHeight = useDomainWarping ? DomainWarping.GenerateDomainNoise(x * voxelScale.x, z * voxelScale.z, BiomeNoiseSettings) : MyNoise.OctavePerlin(x * voxelScale.x, z * voxelScale.z, BiomeNoiseSettings);
            // terrainHeight /= voxelScale.y;
            //var terrainHeight = useDomainWarping ? MyNoise.OctaveSimplex(x,z,BiomeNoiseSettings) : MyNoise.SimplexNoise(x, z, BiomeNoiseSettings);
            terrainHeight = MyNoise.Redistribution(terrainHeight, BiomeNoiseSettings);
            return MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight / voxelScale.y) + BiomeSettings.MinimumHeight;
        }
    }
}
