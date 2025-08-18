using System.Collections.Generic;
using UnityEngine;
using Voxel_Engine.WorldGen;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations;

namespace Voxel_Engine
{
    public class ChunkData
    {
        public VoxelType[] Voxels;
        public readonly List<DecorationObject> ChunkDecorations = new();
        public int[,] HeightMap; // indices x,z, value = y
        public int ChunkSizeInVoxel = 16;
        public int ChunkHeightInVoxel = 100;
        public World WorldReference;
        public Vector3Int ChunkPositionInWorld; // Position of the transform in world space
        public Vector3Int ChunkPositionInVoxel; // Position of the chunk in voxel space

        public readonly int ChunkHeightInWorld;
        
        public Chunk.RenderMethod RenderMethod;

        public bool ModifiedByPlayer = false;

        public ChunkData(int chunkSizeInVoxel, int chunkHeightInVoxel, World world, Vector3Int chunkPositionInWorld, Vector3Int chunkPositionInVoxel)
        {
            ChunkSizeInVoxel = chunkSizeInVoxel;
            ChunkHeightInVoxel = chunkHeightInVoxel;
            WorldReference = world;
            ChunkPositionInWorld = chunkPositionInWorld;
            ChunkPositionInVoxel = chunkPositionInVoxel;
            Voxels = new VoxelType[ChunkSizeInVoxel * ChunkHeightInVoxel * chunkSizeInVoxel];
            //TODO: Make this dynamic
            RenderMethod = Chunk.RenderMethod.Greedy;
            
            HeightMap = new int[ChunkSizeInVoxel,ChunkSizeInVoxel];

            ChunkHeightInWorld = Mathf.RoundToInt(ChunkHeightInVoxel * WorldReference.WorldData.VoxelScaling.y);
        }
    }
}
