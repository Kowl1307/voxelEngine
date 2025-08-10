using UnityEngine;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.BiomeRefiningVisualizer
{
    public class DomainWarpingProviderWrapper : INoiseProvider
    {
        private DomainWarping _domainWarping = ScriptableObject.CreateInstance<DomainWarping>();
        
        public Color GetNoiseValue(int x, int y, NoiseSettings noiseSettings)
        {
            _domainWarping.noiseDomainX = noiseSettings;
            _domainWarping.noiseDomainY = noiseSettings;
            
            var noise =  _domainWarping.GenerateDomainNoise(x, y, noiseSettings);
            return new Color(noise, noise, noise);
        }
    }
}