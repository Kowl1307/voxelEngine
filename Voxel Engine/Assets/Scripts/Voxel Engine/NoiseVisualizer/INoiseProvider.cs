using UnityEngine;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.NoiseVisualizer
{
    public interface INoiseProvider
    {
        public Color GetNoiseValue(int x, int y, NoiseSettings noiseSettings);

        public Color[,] GetNoiseValues(int width, int height, NoiseSettings noiseSettings)
        {
            var noise = new Color[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    noise[x, y] = GetNoiseValue(x, y, noiseSettings);
                }
            }

            return noise;
        }
    }
}