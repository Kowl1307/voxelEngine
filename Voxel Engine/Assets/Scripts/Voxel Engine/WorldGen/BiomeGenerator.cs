using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Structures;
using Voxel_Engine.WorldGen.Noise;
using Voxel_Engine.WorldGen.SurfaceHeightGenerators;
using Voxel_Engine.WorldGen.VoxelLayers;
using Task = System.Threading.Tasks.Task;

namespace Voxel_Engine.WorldGen
{
    [RequireComponent(typeof(SurfaceHeightGenerator))]
    public class BiomeGenerator : MonoBehaviour
    {
        public BiomeType biomeType = BiomeType.Undefined;
        public BiomeSettingsSO BiomeSettings;

        public NoiseSettings BiomeNoiseSettings;

        public DomainWarping DomainWarping;
        public bool useDomainWarping = true;

        public VoxelLayerHandler StartLayerHandler;
        
        private SurfaceHeightGenerator _surfaceHeightGenerator;

        private void Awake()
        {
            _surfaceHeightGenerator = GetComponent<SurfaceHeightGenerator>();
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
        /// <returns></returns>
        public VoxelType ProcessVoxel(World world, int x, int y, int z)
        {
            //Create a temporary chunkData so we can use it to set the voxel
            var voxelCoords = new Vector3Int(x, y, z);
            var chunkData = new ChunkData(world.WorldData.ChunkSizeInVoxel, world.WorldData.ChunkHeightInVoxel, world, WorldDataHelper.GetChunkWorldPositionFromVoxelCoords(world, voxelCoords),
                WorldDataHelper.GetChunkPositionFromVoxelCoords(world, voxelCoords));
            var voxelInChunkCoord = Chunk.GetChunkCoordinateOfVoxelPosition(chunkData, voxelCoords);
            
            var surfaceHeight = GetSurfaceHeightNoise(x, z, chunkData.WorldReference.WorldData);
            StartLayerHandler.Handle(chunkData, voxelInChunkCoord.x, voxelInChunkCoord.y, voxelInChunkCoord.z, surfaceHeight, world.WorldData.WorldSeed, BiomeSettings);
            
            return chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, voxelInChunkCoord.x, voxelInChunkCoord.y, voxelInChunkCoord.z)];
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
            var chunkHeight = worldData.ChunkHeightInVoxel;
            return _surfaceHeightGenerator.GetSurfaceHeight(x, z, BiomeSettings.MinimumHeight, chunkHeight, BiomeNoiseSettings, worldData);
        }
    }
}
