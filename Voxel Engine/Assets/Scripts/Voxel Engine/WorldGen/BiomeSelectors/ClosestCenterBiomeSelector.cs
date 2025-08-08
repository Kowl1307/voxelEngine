using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.WorldGen.BiomeSelectors
{
    public class ClosestCenterBiomeSelector : BiomeSelector
    {
        [SerializeField] private List<BiomeGeneratorData> biomeGeneratorsData = new List<BiomeGeneratorData>();
        
        [SerializeField] private List<BiomeCenter> _biomeCenters = new List<BiomeCenter>();
        
        [SerializeField] private NoiseSettings biomeNoiseSettings;
        public DomainWarping biomeDomainWarping;

        
        public bool doInterpolateBiomes = true;
        ///Amount of biomes to use at any point to determine the biome
        public int interpolationSize = 2;
        
        public EaseFunctions.EaseFunctionType blendFunctionType = EaseFunctions.EaseFunctionType.Quadratic;

        
        private struct BiomeSelectionHelper
        {
            public int Index;
            public float Distance;
        }

        public override void PrecomputeData(World world, Vector3Int worldPosition)
        {
            GenerateBiomePoints(worldPosition, world.ChunkDrawingRange, world.WorldData.ChunkSizeInVoxel, world.WorldData.WorldSeed);
        }

        public override BiomeType GetBiomeTypeAt(Vector3Int voxelPosition, ChunkData chunkData)
        {
            return SelectBiomeGenerator(voxelPosition, chunkData, false).BiomeGenerator.biomeType;
        }
        
        /// <summary>
        /// Selects the biome Generator for the given world position.
        /// </summary>
        /// <param name="voxelPos"></param>
        /// <param name="chunkData"></param>
        /// <param name="useDomainWarping"></param>
        /// <returns></returns>
        private BiomeGeneratorSelection SelectBiomeGenerator(Vector3Int voxelPos, ChunkData chunkData, bool useDomainWarping = true)
        {
            if (useDomainWarping)
            {
                var domainOffset = biomeDomainWarping.GenerateDomainOffsetInt(voxelPos.x, voxelPos.z);
                voxelPos += new Vector3Int(domainOffset.x, 0, domainOffset.y);
            }

            var biomeSelectionHelpers = GetBiomeSelectionLocations(voxelPos, interpolationSize);
            var closestBiomeGen = SelectBiome(biomeSelectionHelpers[0].Index);
            var biomeGenerators = biomeSelectionHelpers.Select(helper => SelectBiome(helper.Index)).ToList();
            
            var worthInterpolating = biomeGenerators.Any(gen => gen != biomeGenerators[0]);
            if (!doInterpolateBiomes || biomeSelectionHelpers[0].Distance == 0 || !worthInterpolating)
                return new BiomeGeneratorSelection(closestBiomeGen);
            var interpolatedValue = CalculateInterpolatedHeight(voxelPos, chunkData, biomeSelectionHelpers);

            
            return new BiomeGeneratorSelection(closestBiomeGen);
            /*
            var blendStrength = blendFunctionType.Function()(distances.Min() / distances.Max());

            return new BiomeGeneratorSelection(closestGen,
                Mathf.FloorToInt(terrainHeights[0] * (1-blendStrength) + interpolatedValue * blendStrength));
            */
        }
        /// <summary>
        /// Selects the biome of the given chunk
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private BiomeGenerator SelectBiome(int index)
        {
            var temp = _biomeCenters[index].Temperature;
            foreach (var data in biomeGeneratorsData.Where(data => temp >= data.temperatureStartThreshold && temp < data.temperatureEndThreshold))
            {
                return data.biomeTerrainGenerator;
            }

            throw new Exception("No Biome Generator found for the given temperature");
            //return biomeGeneratorsData[0].biomeTerrainGenerator;
        }
        
        /// <summary>
        /// Gets world positions for biome selection (always sets y=0)
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="returnLength"></param>
        /// <returns></returns>
        private List<BiomeSelectionHelper> GetBiomeSelectionLocations(Vector3Int worldPos, int returnLength)
        {
            worldPos.y = 0;
            return GetClosestBiomeIndex(worldPos, returnLength);
        }
        
        /// <summary>
        /// Gets the closest biomes by distance
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="returnLength"></param>
        /// <returns></returns>
        private List<BiomeSelectionHelper> GetClosestBiomeIndex(Vector3Int worldPos, int returnLength)
        {
            return _biomeCenters.Select((center, index) => new BiomeSelectionHelper
            {
                Index = index, Distance = Vector3.Distance(worldPos, center.Position)
            }).OrderBy(helper => helper.Distance).Take(returnLength).ToList();
        }
        
        
        /// <summary>
        /// Calculates the Biome points and calculates the biome data (temperature, etc)
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="drawRange"></param>
        /// <param name="chunkSize"></param>
        /// <param name="mapSeedOffset"></param>
        public void GenerateBiomePoints(Vector3 playerPos, int drawRange, int chunkSize, Vector2Int mapSeedOffset)
        {
            _biomeCenters = BiomeCenterFinder.CalculateBiomeCenterPositions(playerPos, drawRange, chunkSize).Select(pos => new BiomeCenter(pos)).ToList();

            // Create the biome centers
            if (biomeDomainWarping != null)
            {
                //modify biome centers with domain warping
                foreach (var biomeCenter in _biomeCenters)
                {
                    var domainWarpingOffset = biomeDomainWarping.GenerateDomainOffsetInt(biomeCenter.Position.x, biomeCenter.Position.z);
                    biomeCenter.Position += new Vector3Int(domainWarpingOffset.x, 0, domainWarpingOffset.y);
                }
            }
            
            // Fill the terrain info struct with data for the biome centers
            GenerateTerrainInfo(mapSeedOffset);
        }
        private void GenerateTerrainInfo(Vector2Int mapSeedOffset)
        {
            CalculateBiomeNoise(_biomeCenters, mapSeedOffset);
        }

        /// <summary>
        /// Calculates the list of noise values for the given positions
        /// </summary>
        /// <param name="centers"></param>
        /// <param name="mapSeedOffset"></param>
        /// <returns></returns>
        private void CalculateBiomeNoise(List<BiomeCenter> centers, Vector2Int mapSeedOffset)
        {
            biomeNoiseSettings.Seed = mapSeedOffset;
            foreach(var center in centers)
            {
                center.Temperature = MyNoise.OctavePerlin(center.Position.x, center.Position.y, biomeNoiseSettings);
            }
        }
        
        private float CalculateInterpolatedHeight(Vector3Int worldPos, ChunkData chunkData, List<BiomeSelectionHelper> biomeSelectionHelpers)
        {
            //General interpolation with Inverse Distance Weighting
            var terrainHeights = biomeSelectionHelpers.Select(helper =>
            {
                var biome = SelectBiome(helper.Index);
                var floorHeight = biome.GetSurfaceHeightNoise(worldPos.x, worldPos.z, chunkData);
                return floorHeight < biome.BiomeSettings.WaterLevel ? biome.BiomeSettings.WaterLevel : floorHeight;
            }).ToList();

            var distances = biomeSelectionHelpers.Select(helper => helper.Distance).ToList();
            var weights = distances.Select(distance => 1 / (distance * distance)).ToList();
            var totalWeight = weights.Sum();
            var normalizedWeights = weights.Select(weight => weight / totalWeight).ToList();
            float interpolatedValue = 0;
            for (var i = 0; i < interpolationSize; i++)
            {
                interpolatedValue += normalizedWeights[i] * terrainHeights[i];
            }

            return interpolatedValue;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            foreach (var biomeCenter in _biomeCenters)
            {
                Gizmos.DrawLine(biomeCenter.Position, biomeCenter.Position + Vector3.up * 255);        
            }
        }
#endif
    }
}