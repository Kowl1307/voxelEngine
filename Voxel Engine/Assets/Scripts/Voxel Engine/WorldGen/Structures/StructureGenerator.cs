using UnityEngine;

namespace Voxel_Engine.WorldGen.Structures
{
    public abstract class StructureGenerator : MonoBehaviour
    {
        public abstract StructureData GenerateData(ChunkData chunkData, Vector2Int mapSeedOffset);
    }
}