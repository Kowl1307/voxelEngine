using System.Collections.Generic;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator
{
    public abstract class ChunkFeatureGenerator : MonoBehaviour
    {
        [SerializeField]
        protected List<BiomeType> allowedBiomes = new();
        public abstract void Handle(ChunkData chunkData);

        protected bool GeneratesInBiome(BiomeType biomeType)
        {
            return allowedBiomes.Contains(biomeType);
        }
    }
}