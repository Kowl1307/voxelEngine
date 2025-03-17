using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
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
        [SerializeField] private BiomeSelector _biomeSelector;
        
        public ChunkData GenerateChunkData(ChunkData chunkData, Vector2Int mapSeedOffset)
        {
            var biomeSelection = _biomeSelector.GetBiomeSelection(chunkData.ChunkPositionInVoxel, chunkData);
            var structureDataList = biomeSelection.BiomeGenerator.GetStructureData(chunkData, mapSeedOffset);
            foreach(var structureData in structureDataList)
                chunkData.AddStructureData(structureData);
            
            
            Parallel.For(0, chunkData.ChunkSize, (x) =>
            {
                for (var z = 0; z < chunkData.ChunkSize; z++)
                {
                    //Need to re-define as this the biomeSelection is out of scope for parallel
                    var biomeGeneratorSelection = _biomeSelector.GetBiomeSelection(new Vector3Int(chunkData.ChunkPositionInVoxel.x + x, 0, chunkData.ChunkPositionInVoxel.z + z), chunkData);
                    
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

        public void InitBiomeSelector(World world, Vector3Int worldPosition)
        {
            _biomeSelector.PrecomputeData(world, worldPosition);
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