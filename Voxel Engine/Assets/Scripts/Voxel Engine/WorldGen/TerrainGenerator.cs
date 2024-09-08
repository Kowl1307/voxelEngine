using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Voxel_Engine.WorldGen;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.WorldGen
{
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private List<Vector3Int> biomeCenters;
        private List<float> biomeTemperatureNoise = new List<float>();

        [SerializeField] private NoiseSettings biomeNoiseSettings;
        
        public DomainWarping biomeDomainWarping;

        [SerializeField] private List<BiomeData> biomeGeneratorsData = new List<BiomeData>();

        public bool DoInterpolateBiomes = true;
        ///Amount of biomes to use at any point to determine the biome
        public int interpolationSize = 2;

        public EaseFunctions.EaseFunctionType blendFunctionType = EaseFunctions.EaseFunctionType.Quadratic;

        private struct BiomeSelectionHelper
        {
            public int Index;
            public float Distance;
        }
        
        public ChunkData GenerateChunkData(ChunkData chunkData, Vector2Int mapSeedOffset)
        {
            var biomeSelection = SelectBiomeGenerator(chunkData.WorldPosition, chunkData, false);
            //var treeData = BiomeGenerator.GetTreeData(chunkData, mapSeedOffset);
            var structureDataList = biomeSelection.BiomeGenerator.GetStructureData(chunkData, mapSeedOffset);
            //var treeData = biomeSelection.BiomeGenerator.GetTreeData(chunkData, mapSeedOffset);
            foreach(var structureData in structureDataList)
                chunkData.AddStructureData(structureData);
            
            
            Parallel.For(0, chunkData.ChunkSize, (x) =>
            {
                for (var z = 0; z < chunkData.ChunkSize; z++)
                {
                    //Need to re-define as this the biomeSelection is out of scope for parallel
                    var biomeGeneratorSelection = SelectBiomeGenerator(new Vector3Int(chunkData.WorldPosition.x + x, 0, chunkData.WorldPosition.z + z), chunkData);
                    
                    chunkData = biomeGeneratorSelection.BiomeGenerator.ProcessChunkColumn(chunkData, x, z, mapSeedOffset, biomeGeneratorSelection.TerrainSurfaceNoise);
                }
            });
            
            
            /*
            //Main Thread for loop
            for (var x = 0; x < chunkData.ChunkSize; x++)
            {
                for (var z = 0; z < chunkData.ChunkSize; z++)
                {
                    chunkData = BiomeGenerator.ProcessChunkColumn(chunkData, x, z, mapSeedOffset);
                }
            }
            */

            return chunkData;
        }

        private BiomeGeneratorSelection SelectBiomeGenerator(Vector3Int worldPos, ChunkData chunkData, bool useDomainWarping = true)
        {
            if (useDomainWarping)
            {
                var domainOffset = biomeDomainWarping.GenerateDomainOffsetInt(worldPos.x, worldPos.z);
                worldPos += new Vector3Int(domainOffset.x, 0, domainOffset.y);
            }

            var biomeSelectionHelpers = GetBiomeSelectionHelpers(worldPos, interpolationSize);
            
            var closestGen = SelectBiome(biomeSelectionHelpers[0].Index);
            
            var biomeGenerators = biomeSelectionHelpers.Select(helper => SelectBiome(helper.Index)).ToList();
            var worthInterpolating = biomeGenerators.Any(gen => gen != biomeGenerators[0]);
            
            if (!DoInterpolateBiomes || biomeSelectionHelpers[0].Distance == 0 || !worthInterpolating)
                return new BiomeGeneratorSelection(closestGen,
                    closestGen.GetSurfaceHeightNoise(worldPos.x, worldPos.z, chunkData.ChunkHeight, closestGen.BiomeSettings));

            var interpolatedValue = CalculateInterpolatedHeight(worldPos, chunkData, biomeSelectionHelpers);

            return new BiomeGeneratorSelection(closestGen, Mathf.FloorToInt(interpolatedValue));
            /*
            var blendStrength = blendFunctionType.Function()(distances.Min() / distances.Max());
            
            return new BiomeGeneratorSelection(closestGen,
                Mathf.FloorToInt(terrainHeights[0] * (1-blendStrength) + interpolatedValue * blendStrength));
            */
        }

        private float CalculateInterpolatedHeight(Vector3Int worldPos, ChunkData chunkData, List<BiomeSelectionHelper> biomeSelectionHelpers)
        {
            //General interpolation with Inverse Distance Weighting
            //If one is exactly on the given position, return
            var terrainHeights = biomeSelectionHelpers.Select(helper =>
                SelectBiome(helper.Index).GetSurfaceHeightNoise(worldPos.x, worldPos.z, chunkData.ChunkHeight,
                    SelectBiome(helper.Index).BiomeSettings)).ToList();

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

        private BiomeGenerator SelectBiome(int index)
        {
            var temp = biomeTemperatureNoise[index];
            foreach (var data in biomeGeneratorsData.Where(data => temp >= data.temperatureStartThreshold && temp < data.tmperatureEndThreshold))
            {
                return data.biomeTerrainGenerator;
            }

            throw new Exception("No Biome Generator found for the given temperature");
            //return biomeGeneratorsData[0].biomeTerrainGenerator;
        }
        
        private List<BiomeSelectionHelper> GetBiomeSelectionHelpers(Vector3Int worldPos, int returnLength)
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
            return biomeCenters.Select((center, index) => new BiomeSelectionHelper
            {
                Index = index, Distance = Vector3.Distance(worldPos, center)
            }).OrderBy(helper => helper.Distance).Take(returnLength).ToList();
        }

        public void GenerateBiomePoints(Vector3 playerPos, int drawRange, int chunkSize, Vector2Int mapSeedOffset)
        {
            biomeCenters = BiomeCenterFinder.CalculateBiomeCenters(playerPos, drawRange, chunkSize);

            if (biomeDomainWarping != null)
            {
                //modify biome centers with domain warping
                for (var i = 0; i < biomeCenters.Count; i++)
                {
                    var domainWarpingOffset = biomeDomainWarping.GenerateDomainOffsetInt(biomeCenters[i].x, biomeCenters[i].z);
                    biomeCenters[i] += new Vector3Int(domainWarpingOffset.x, 0, domainWarpingOffset.y);
                }
            }

            biomeTemperatureNoise = CalculateBiomeNoise(biomeCenters, mapSeedOffset);

        }

        private List<float> CalculateBiomeNoise(List<Vector3Int> vector3Ints, Vector2Int mapSeedOffset)
        {
            biomeNoiseSettings.WorldOffset = mapSeedOffset;
            return biomeCenters.Select(center => MyNoise.OctavePerlin(center.x, center.y, biomeNoiseSettings)).ToList();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            foreach (var biomeCenterPoint in biomeCenters)
            {
                Gizmos.DrawLine(biomeCenterPoint, biomeCenterPoint + Vector3.up * 255);        
            }
        }
        #endif
    }
}

[Serializable]
public struct BiomeData
{
    [Range(0, 1)] public float temperatureStartThreshold, tmperatureEndThreshold;
    public BiomeGenerator biomeTerrainGenerator;
}