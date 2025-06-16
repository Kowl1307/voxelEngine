using System.Collections.Generic;
using UnityEngine;
using Voxel_Engine.WorldGen;
using Voxel_Engine.WorldGen.Structures;
using Voxel_Engine.WorldGen.Trees;

namespace Voxel_Engine
{
    public class ChunkData
    {
        public VoxelType[] Voxels;
        public int ChunkSize = 16;
        public int ChunkHeight = 100;
        public World WorldReference;
        public Vector3Int ChunkPositionInWorld; // Position of the transform in world space
        public Vector3Int ChunkPositionInVoxel; // Position of the chunk in voxel space
        
        public Chunk.RenderMethod RenderMethod;

        public bool ModifiedByPlayer = false;
        public List<StructureData> Structures = new List<StructureData>();

        public ChunkData(int chunkSize, int chunkHeight, World world, Vector3Int chunkPositionInWorld, Vector3Int chunkPositionInVoxel)
        {
            ChunkSize = chunkSize;
            ChunkHeight = chunkHeight;
            WorldReference = world;
            ChunkPositionInWorld = chunkPositionInWorld;
            ChunkPositionInVoxel = chunkPositionInVoxel;
            Voxels = new VoxelType[ChunkSize * ChunkHeight * chunkSize];
            //TODO: Make this dynamic
            RenderMethod = Chunk.RenderMethod.Greedy;
        }

        public void AddStructureData(StructureData data)
        {
            Structures.Add(data);
        }
    }
}
