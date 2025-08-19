using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voxel_Engine.Saving;
using Voxel_Engine.WorldGen;
using Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations;

namespace Voxel_Engine
{
    public class ChunkData
    {
        private readonly VoxelType[] _voxels;
        private readonly ConcurrentDictionary<int, VoxelType> _modifiedVoxels = new();
        public List<DecorationObject> ChunkDecorations { get; } =  new();
        public int[,] HeightMap { get; } // indices x,z, value = y
        public int ChunkSizeInVoxel { get; }
        public int ChunkHeightInVoxel { get; }
        public World WorldReference{ get; }
        public Vector3Int ChunkPositionInWorld { get; } // Position of the transform in world space
        public Vector3Int ChunkPositionInVoxel { get; } // Position of the chunk in voxel space

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
            _voxels = new VoxelType[ChunkSizeInVoxel * ChunkHeightInVoxel * chunkSizeInVoxel];
            //TODO: Make this dynamic
            RenderMethod = Chunk.RenderMethod.Greedy;
            
            HeightMap = new int[ChunkSizeInVoxel,ChunkSizeInVoxel];

            ChunkHeightInWorld = Mathf.RoundToInt(ChunkHeightInVoxel * WorldReference.WorldData.VoxelScaling.y);
        }

        public void SetVoxel(int index, VoxelType voxel)
        {
            _voxels[index] = voxel;
        }

        public void MarkDirty(int index, VoxelType voxel)
        {
            _voxels[index] = voxel;
            ModifiedByPlayer = true;
            _modifiedVoxels[index] = voxel;
        }
        
        public Dictionary<int, VoxelType> GetModifiedVoxels() => _modifiedVoxels.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public VoxelType GetVoxel(Vector3Int chunkPosition)
        {
            return _voxels[Chunk.GetIndexFromPosition(this, chunkPosition.x, chunkPosition.y, chunkPosition.z)];
        }

        public VoxelType GetVoxel(int index)
        {
            return _voxels[index];
        }

        public int GetNumberOfVoxels()
        {
            return _voxels.Length;
        }

        public void SaveToFile()
        {
            if (!ModifiedByPlayer)
                return;
            
            var saveData = new ChunkSaveData(this);
            saveData.Save("chunks/"+ChunkPositionInVoxel.x + "-" + ChunkPositionInVoxel.y + "-" + ChunkPositionInVoxel.z + ".chunk");
        }
    }
}
