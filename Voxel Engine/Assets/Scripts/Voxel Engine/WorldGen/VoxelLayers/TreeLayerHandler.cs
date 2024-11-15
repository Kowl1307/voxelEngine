using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Structures;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    public class TreeLayerHandler : VoxelLayerHandler
    {
        public float heightThreshold = 0f;
        public float heightLimit = 25f;
        
        [SerializeField]
        private List<VoxelType> allowedTreeGroundTypes = new List<VoxelType> { VoxelType.GrassDirt };
        
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            if (chunkData.ChunkPositionInVoxel.y < 0) return false;
            //if (y < heightThreshold || y > heightLimit) return false;

            var treeStructures = chunkData.Structures.Where(structureData => structureData.Type == StructureType.Tree).ToList();

            foreach (var treeStructure in treeStructures)
            {
                var poiPositions = treeStructure.StructurePointsOfInterest;

                if (!(surfaceHeightNoise < heightLimit) ||
                    !poiPositions.Contains(new Vector2Int(chunkData.ChunkPositionInVoxel.x + x,
                        chunkData.ChunkPositionInVoxel.z + z))) return false;

                var chunkCoords = new Vector3Int(x, surfaceHeightNoise, z);
                var groundType = Chunk.GetVoxelFromChunkCoordinates(chunkData, chunkCoords);

                if (!allowedTreeGroundTypes.Contains(groundType)) return false;

                Chunk.SetVoxel(chunkData, chunkCoords, VoxelType.Dirt);
                //Create Trunk
                for (var i = 1; i < 5; i++)
                {
                    chunkCoords.y = surfaceHeightNoise + i;
                    Chunk.SetVoxel(chunkData, chunkCoords, VoxelType.TreeTrunk);
                }

                //Add Leaves data
                foreach (var leafPos in _treeLeavesStaticLayout)
                {
                    var leafInChunk = new Vector3Int(x + leafPos.x,
                        surfaceHeightNoise + 5 + leafPos.y, z + leafPos.z);
                    //Leaves of trees can overlap, only add once
                    if(!treeStructure.StructureVoxels.ContainsKey(leafInChunk))
                        treeStructure.StructureVoxels.TryAdd(leafInChunk, VoxelType.TreeLeafsSolid);
                }
            }

            return false;
        }
        
        private List<Vector3Int> _treeLeavesStaticLayout = new List<Vector3Int>
        {
            new(-2, 0, -2),
            new(-2, 0, -1),
            new(-2, 0, 0),
            new(-2, 0, 1),
            new(-2, 0, 2),
            new(-1, 0, -2),
            new(-1, 0, -1),
            new(-1, 0, 0),
            new(-1, 0, 1),
            new(-1, 0, 2),
            new(0, 0, -2),
            new(0, 0, -1),
            new(0, 0, 0),
            new(0, 0, 1),
            new(0, 0, 2),
            new(1, 0, -2),
            new(1, 0, -1),
            new(1, 0, 0),
            new(1, 0, 1),
            new(1, 0, 2),
            new(2, 0, -2),
            new(2, 0, -1),
            new(2, 0, 0),
            new(2, 0, 1),
            new(2, 0, 2),
            new(-1, 1, -1),
            new(-1, 1, 0),
            new(-1, 1, 1),
            new(0, 1, -1),
            new(0, 1, 0),
            new(0, 1, 1),
            new(1, 1, -1),
            new(1, 1, 0),
            new(1, 1, 1),
            new(0, 2, 0)
        };
    }
}