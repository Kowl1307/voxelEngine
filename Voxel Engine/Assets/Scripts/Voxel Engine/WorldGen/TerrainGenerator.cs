using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Voxel_Engine.WorldGen;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;
using Voxel_Engine.WorldGen.BiomeSelectors;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Structures;

namespace Voxel_Engine.WorldGen
{
    /// <summary>
    /// Handles Biome selection, generates temperature and climate data, and delegates concrete implementation of generation to biomes
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        private Dictionary<BiomeType, BiomeGenerator> _biomeGenerators = new Dictionary<BiomeType, BiomeGenerator>();

        [SerializeField] private BiomeSelector biomeSelector;
        
        [SerializeField] private GameObject structureGeneratorsHolder;
        private List<StructureGenerator> _structureGenerators = new();

        [SerializeField] private GameObject decorationGeneratorsHolder;
        private List<DecorationGenerator> _decorationGenerators = new();


        
        [Serializable]
        private struct BiomeTypeGeneratorPair
        {
            public BiomeType type;
            public BiomeGenerator generator;
        }
        [SerializeField] private List<BiomeTypeGeneratorPair> biomeTypeGeneratorPairs;

        private void Awake()
        {
            foreach (var pair in biomeTypeGeneratorPairs)
            {
                _biomeGenerators.Add(pair.type, pair.generator);
            }
            
            if(structureGeneratorsHolder != null)
                _structureGenerators = structureGeneratorsHolder.GetComponents<StructureGenerator>().ToList();
            
            if(decorationGeneratorsHolder != null)
                _decorationGenerators = decorationGeneratorsHolder.GetComponents<DecorationGenerator>().ToList();
        }

        /// <summary>
        /// Generates the VoxelType data for the whole Chunk.
        /// </summary>
        /// <param name="chunkData">The chunkData that should be filled</param>
        /// <param name="mapSeedOffset"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ChunkData GenerateChunkData(ChunkData chunkData, Vector2Int mapSeedOffset)
        {
            Parallel.For(0, chunkData.ChunkSizeInVoxel, chunkData.WorldReference.WorldParallelOptions, (x) =>
            {
                var voxelInChunk = new Vector3Int(x, 0, 0);
                for (var z = 0; z < chunkData.ChunkSizeInVoxel; z++)
                {
                    voxelInChunk.z = z;
                    var biomeGenerator = GetBiomeGeneratorAt(chunkData.WorldReference.WorldData, Chunk.GetVoxelCoordsFromChunkCoords(chunkData, voxelInChunk));
                    chunkData = biomeGenerator.ProcessChunkColumn(chunkData, x, z, mapSeedOffset);
                }
            });
            
            chunkData = GenerateStructures(chunkData);
            GenerateDecorations(chunkData);
            // Generate biome-independent structures (i.e. villages). Biome dependent structures are already handled in the Layer system above.
            
            return chunkData;
        }
        
        public ChunkData GenerateStructures(ChunkData chunkData)
        {
            foreach (var structureGenerator in _structureGenerators)
            {
                structureGenerator.Handle(chunkData);
            }
            /*
            //TODO: This seems to be broken (leaves of trees are not spawned in deserts)
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
            }*/
            
            return chunkData;
        }

        // TODO: Would be better if decorations are also voxels but are rendered differently. Creating all these objects or having insanely large objectPools seems very inefficient..
        public void GenerateDecorations(ChunkData chunkData)
        {
            Task.Run( () =>
            {
                /*Parallel.ForEach(_decorationGenerators, chunkData.WorldReference.WorldParallelOptions,
                    decorationGenerator =>
                    {
                        decorationGenerator.Handle(chunkData);
                    });
                */
                foreach (var decorationGenerator in _decorationGenerators)
                {
                    decorationGenerator.Handle(chunkData);
                }
            });
        }
        
        //TODO This will be removed due to the TODO above.
        public BiomeGenerator GetBiomeGeneratorAt(WorldData worldData, Vector3Int voxelPosition)
        {
            var biomeSelection = biomeSelector.GetBiomeTypeAt(worldData, voxelPosition);
            
            if (!_biomeGenerators.TryGetValue(biomeSelection, out var biomeGenerator))
            {
                throw new Exception("Biome not found in dictionary!");
            }

            return biomeGenerator;
        }

        public VoxelType ProcessVoxelAt(World world, Vector3Int voxelPosition)
        {
            return GetBiomeGeneratorAt(world.WorldData, voxelPosition)
                .ProcessVoxel(world, voxelPosition.x, voxelPosition.y, voxelPosition.z);
        }

        public int GetSurfaceHeightAt(World world, Vector3Int voxelPosition)
        {
            return GetBiomeGeneratorAt(world.WorldData, voxelPosition)
            //return GetBiomeGeneratorAt(world.WorldData, WorldDataHelper.GetChunkPositionFromVoxelCoords(world, voxelPosition))
                .GetSurfaceHeightNoise(voxelPosition.x, voxelPosition.z, world.WorldData);
        }

        public BiomeType GetBiomeAt(World world, Vector3Int voxelPosition)
        {
            return biomeSelector.GetBiomeTypeAt(world.WorldData, voxelPosition);
        }

        public async void InitBiomeSelector(World world, Vector3Int worldPosition)
        {
            await Task.Run(() => biomeSelector.PrecomputeData(world, worldPosition));
        }
    }
}

[Serializable]
public struct BiomeGeneratorData
{
    [Range(0, 1)] public float temperatureStartThreshold;
    [Range(0, 1)] public float temperatureEndThreshold;
    public BiomeGenerator biomeTerrainGenerator;
}