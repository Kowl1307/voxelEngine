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

namespace Voxel_Engine.WorldGen
{
    /// <summary>
    /// Handles Biome selection, generates temperature and climate data, and delegates concrete implementation of generation to biomes
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        private Dictionary<BiomeType, BiomeGenerator> _biomeGenerators = new Dictionary<BiomeType, BiomeGenerator>();

        [SerializeField] private BiomeSelector biomeSelector;

        
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
            //TODO: Chunks are currently always having the same biome. The generator should be picked depending on the xyz coordinates. For this, we need to change the processing from per column to complete parallel.
            var biomeGenerator = GetBiomeGeneratorAt(chunkData.WorldReference.WorldData, chunkData.ChunkPositionInVoxel);
            
            Parallel.For(0, chunkData.ChunkSizeInVoxel, chunkData.WorldReference.WorldParallelOptions, (x) =>
            {
                for (var z = 0; z < chunkData.ChunkSizeInVoxel; z++)
                {
                    chunkData = biomeGenerator.ProcessChunkColumn(chunkData, x, z, mapSeedOffset);
                }
            });
            
            chunkData = biomeGenerator.GenerateStructures(chunkData);
            biomeGenerator.GenerateDecorations(chunkData);
            // Generate biome-independent structures (i.e. villages). Biome dependent structures are already handled in the Layer system above.
            
            return chunkData;
        }
        
        //TODO This will be removed due to the TODO above.
        public BiomeGenerator GetBiomeGeneratorAt(WorldData worldData, Vector3Int voxelPosition)
        {
            var biomeSelection = biomeSelector.GetBiomeTypeAt(voxelPosition, worldData);
            
            if (!_biomeGenerators.TryGetValue(biomeSelection, out var biomeGenerator))
            {
                throw new Exception("Biome not found in dictionary!");
            }

            return biomeGenerator;
        }

        public VoxelType ProcessVoxelAt(World world, Vector3Int voxelPosition)
        {
            return GetBiomeGeneratorAt(world.WorldData, WorldDataHelper.GetChunkPositionFromVoxelCoords(world, voxelPosition))
                .ProcessVoxel(world, voxelPosition.x, voxelPosition.y, voxelPosition.z, world.WorldData.WorldSeed);
        }

        public int GetSurfaceHeightAt(World world, Vector3Int voxelPosition)
        {
            return GetBiomeGeneratorAt(world.WorldData, voxelPosition)
            //return GetBiomeGeneratorAt(world.WorldData, WorldDataHelper.GetChunkPositionFromVoxelCoords(world, voxelPosition))
                .GetSurfaceHeightNoise(voxelPosition.x, voxelPosition.z, world.WorldData);
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