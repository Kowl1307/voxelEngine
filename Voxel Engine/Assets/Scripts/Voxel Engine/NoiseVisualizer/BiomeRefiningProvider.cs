using Unity.Cinemachine;
using UnityEngine;
using Voxel_Engine.WorldGen.BiomeSelectors.BiomeRefining;
using NoiseSettings = Voxel_Engine.WorldGen.Noise.NoiseSettings;

namespace Voxel_Engine.NoiseVisualizer
{
    public class BiomeRefiningProvider : INoiseProvider
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
            return _biomeRefining.CreateRefinedMap(200, 200, 200, noiseSettings)[0];
        }
    }
}