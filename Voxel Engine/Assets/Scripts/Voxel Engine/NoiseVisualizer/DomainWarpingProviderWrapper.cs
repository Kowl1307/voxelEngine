using UnityEngine;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.NoiseVisualizer
{
    public class DomainWarpingProviderWrapper : INoiseProvider
    {
        private DomainWarping _domainWarping = ScriptableObject.CreateInstance<DomainWarping>();
        
        public float GetNoiseValue(int x, int y, NoiseSettings noiseSettings)
        {
            _domainWarping.noiseDomainX = noiseSettings;
            _domainWarping.noiseDomainY = noiseSettings;
            
            return _domainWarping.GenerateDomainNoise(x, y, noiseSettings);
        }
    }
}