using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine.WorldGen.Structures
{
    public class StructureData
    {
        public StructureType Type;
        public List<Vector2Int> StructurePointsOfInterest = new List<Vector2Int>();
        public ConcurrentDictionary<Vector3Int, VoxelType> StructureVoxels = new ConcurrentDictionary<Vector3Int, VoxelType>();
    }
}