using System;
using System.Collections.Generic;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Structures;
using Voxel_Engine.WorldGen.Structures.Trees;

namespace Voxel_Engine.WorldGen.VoxelLayers
{
    [RequireComponent(typeof(TreeGenerator))]
    public class TreeLayerHandler : VoxelLayerHandler
    {
        private TreeGenerator _treeGenerator;

        private void Awake()
        {
            _treeGenerator = GetComponent<TreeGenerator>();
        }

        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset, BiomeSettingsSO biomeSettings)
        {
            var voxelPos = Chunk.GetVoxelCoordsFromChunkCoords(chunkData, x, y, z);
            if (!_treeGenerator.IsInPoiRange(voxelPos, chunkData.WorldReference))
                return false;
            
            var placeType = _treeGenerator.GetStructureVoxelAt(voxelPos, chunkData.WorldReference);
            if (placeType == VoxelType.Nothing)
                return false;

            Chunk.SetVoxel(chunkData, new Vector3Int(x, y, z), placeType);
            
            return true;
        }
    }
}