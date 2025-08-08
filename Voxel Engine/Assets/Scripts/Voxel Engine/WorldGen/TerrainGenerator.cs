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

        public ChunkData GenerateChunkData(ChunkData chunkData, Vector2Int mapSeedOffset)
        {
            var biomeSelection = biomeSelector.GetBiomeTypeAt(chunkData.ChunkPositionInVoxel, chunkData);
            if (!_biomeGenerators.TryGetValue(biomeSelection, out var biomeGenerator))
            {
                throw new Exception("Biome not found in dictionary!");
            }
            
            Parallel.For(0, chunkData.ChunkSize, chunkData.WorldReference.WorldParallelOptions, (x) =>
            {
                for (var z = 0; z < chunkData.ChunkSize; z++)
                {
                    chunkData = biomeGenerator.ProcessChunkColumn(chunkData, x, z, mapSeedOffset);
                }
            });
            
            return chunkData;
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