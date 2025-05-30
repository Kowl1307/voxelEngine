using UnityEngine;
using Voxel_Engine.WorldGen.Biomes.BiomeRefining;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.NoiseVisualizer
{
    public class BiomeRefiningProvider : INoiseProvider
    {
        private BiomeRefining _biomeRefining = new BiomeRefining();

        public float GetNoiseValue(int x, int y, NoiseSettings noiseSettings)
        {
            return _biomeRefining.Test(x, y, noiseSettings);
        }
    }
}