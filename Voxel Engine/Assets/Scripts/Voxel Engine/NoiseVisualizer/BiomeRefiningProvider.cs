using Unity.Cinemachine;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes.BiomeRefining;
using NoiseSettings = Voxel_Engine.WorldGen.Noise.NoiseSettings;

namespace Voxel_Engine.NoiseVisualizer
{
    public class BiomeRefiningProvider : INoiseProvider
    {
        private BiomeRefining _biomeRefining = new BiomeRefining();

        public Color GetNoiseValue(int x, int y, NoiseSettings noiseSettings)
        {
            var noise = _biomeRefining.GetBiomeAt(x, y, noiseSettings);
            return noise;
        }
    }
}