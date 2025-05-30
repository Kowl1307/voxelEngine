using UnityEngine;
using Voxel_Engine.WorldGen.Noise;
using Random = System.Random;

namespace Voxel_Engine.WorldGen.Biomes.BiomeRefining
{
    public class BiomeRefining
    {
        public float Test(int x, int y, NoiseSettings noiseSettings)
        {
            var random = new Random(noiseSettings.Seed.x);
            for (var i = 0; i < x + y; i++) random.NextDouble();
            return (float)random.NextDouble();
        }
    }
}