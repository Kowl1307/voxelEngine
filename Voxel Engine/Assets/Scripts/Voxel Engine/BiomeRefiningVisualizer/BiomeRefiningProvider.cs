using UnityEngine;
using Voxel_Engine.WorldGen.BiomeSelectors.BiomeRefining;
using NoiseSettings = Voxel_Engine.WorldGen.Noise.NoiseSettings;

namespace Voxel_Engine.BiomeRefiningVisualizer
{
    public class BiomeRefiningProvider : IBiomeRefiningNoiseProvider
    {
        private BiomeRefining _biomeRefining;

        public BiomeRefiningProvider(GameObject gameObjectToAddBiomeRefining)
        {
            _biomeRefining = gameObjectToAddBiomeRefining.AddComponent<BiomeRefining>();
        }

        public Color GetNoiseValue(int x, int y, NoiseSettings noiseSettings)
        {
            var noise = _biomeRefining.GetBiomeAt(x, y, noiseSettings);
            return noise;
        }

        public Color[,] GetNoiseValues(int width, int height, NoiseSettings noiseSettings)
        {
            return _biomeRefining.CreateRefinedMap(4096, width, height, noiseSettings);
        }

        public BiomeRefiningHistory GetBiomeRefiningHistory(int resolution, int width, int height, NoiseSettings noiseSettings)
        {
            return _biomeRefining.CreateRefineHistory(resolution, width, height, noiseSettings);
        }
    }
}