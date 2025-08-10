using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voxel_Engine.WorldGen.Structures
{
    public abstract class StructureGenerator : MonoBehaviour
    {
        public abstract void Handle(ChunkData chunkData);
    }
}