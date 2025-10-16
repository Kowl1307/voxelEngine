using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;
using Random = System.Random;

namespace Voxel_Engine.WorldGen.BiomeSelectors.BiomeRefining
{
    public class BiomeRefining : BiomeSelector
    {
        private Dictionary<Color, BiomeType> _biomeColorDictionary = new Dictionary<Color, BiomeType>();

        /// <summary>
        /// Noise settings used for pseudo-randomization
        /// </summary>
        [SerializeField] private NoiseSettings _randomNoiseSettings;
        
        public override BiomeType GetBiomeTypeAt(WorldData worldData, Vector3Int voxelPosition)
        {
            var biomeColor = GetBiomeAt(voxelPosition.x, voxelPosition.y, _randomNoiseSettings);
            if(_biomeColorDictionary.TryGetValue(biomeColor, out var biomeType))
                return biomeType;
            
            throw new Exception("No biome found for this biome color");
        }

        public override void PrecomputeData(WorldData worldData, Vector3 worldPosition)
        {
            
        }
        
        public float Test(int x, int y, NoiseSettings noiseSettings)
        {
            var random = new Random(noiseSettings.Seed.x);
            for (var i = 0; i < x + y; i++) random.NextDouble();
            return (float)random.NextDouble();
        }

        public Color GetBiomeAt(int x, int y, NoiseSettings noiseSettings)
        {
            var creationHistory = CreateRefinedMap(4096, 200, 200, noiseSettings);
            return creationHistory[x, y];
        }

        /// <summary>
        /// Creates a 1:1 map for biomes
        /// Returns an array of size startWidth * startZoom, startHeight * startZoom
        /// </summary>
        /// <param name="startResolution">How many blocks one pixel refers to</param>
        /// <param name="startWidth">Width in pixels</param>
        /// <param name="startHeight">Height in pixels</param>
        /// <param name="noiseSettings"></param>
        /// <returns>A list of the creation history</returns>
        public Color[,] CreateRefinedMap(int startResolution, int startWidth, int startHeight, NoiseSettings noiseSettings)
        {
            var history = CreateRefineHistory(startResolution, startWidth, startHeight, noiseSettings);
            return history.GetResolutionMapByIndex(history.GetHistoryDepth() - 1).Map;
        }

        public BiomeRefiningHistory CreateRefineHistory(int startResolution, int startWidth, int startHeight, NoiseSettings noiseSettings)
        {
            var history = new BiomeRefiningHistory();
            var landOrOcean = new ResolutionMap()
                { Map = new Color[startWidth, startHeight], Resolution = startResolution };
            
            // Pseudorandomly set land or water
            var landProbabilty = 0.33;
            for (var x = 0; x < startWidth; x++)
            {
                for (var y = 0; y < startHeight; y++)
                {
                    if (SamplePseudoRandom(x, y, noiseSettings) <= landProbabilty)
                    {
                        landOrOcean.Map[x, y] = Color.green;
                    }
                    else
                    {
                        landOrOcean.Map[x, y] = Color.blue;
                    }
                }
            }
            
            history.AddResolutionMap(landOrOcean);

            var zoomedMap = landOrOcean;
            for (var i = 0; i < 12; i++)
            {
                zoomedMap = (i % 3) switch
                {
                    0 => zoomedMap.ImperfectZoom(2, noiseSettings),
                    1 => zoomedMap.NearestNeighborZoom(2),
                    2 => zoomedMap.IncreaseLandmass(),
                    _ => zoomedMap
                };
                history.AddResolutionMap(zoomedMap);
            }
            return history;
        }

        private float SamplePseudoRandom(int x, int y, NoiseSettings noiseSettings)
        {
            var combinedX = x * 73856093 ^ y * 19349663 ^ noiseSettings.Seed.x * 83492791 ^ noiseSettings.Seed.y * 49979539;
            var combinedY = y * 19349663 ^ x * 73856093 ^ noiseSettings.Seed.y * 49979539 ^ noiseSettings.Seed.x * 83492791;

            var randX = new Random(combinedX);
            var randY = new Random(combinedY);

            var xPrime = (float)randX.NextDouble();
            var yPrime = (float)randY.NextDouble();
            
            return Mathf.PerlinNoise(xPrime, yPrime);
        }
    }

    public struct ResolutionMap
    {
        public int Resolution;
        public Color[,] Map;
    }
}