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
        public Vector3Int WorldPosition;

        public bool ModifiedByPlayer = false;
        public List<StructureData> Structures = new List<StructureData>();

        public ChunkData(int chunkSize, int chunkHeight, World world, Vector3Int worldPosition)
        {
            ChunkSize = chunkSize;
            ChunkHeight = chunkHeight;
            WorldReference = world;
            WorldPosition = worldPosition;
            Voxels = new VoxelType[ChunkSize * ChunkHeight * chunkSize];
        }

        public void AddStructureData(StructureData data)
        {
            Structures.Add(data);
        }
    }
}
