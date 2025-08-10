using UnityEngine;

namespace Voxel_Engine.WorldGen.Noise
{
    [CreateAssetMenu(menuName = "Voxel Engine/WorldGen/Domain Warping", fileName = "Domain Warping")]
    public class DomainWarping : ScriptableObject
    {
        public NoiseSettings noiseDomainX, noiseDomainY;
        public int amplitudeX = 20, amplitudeY = 20;
        
        
        public float GenerateDomainNoise(float x, float z, NoiseSettings defaultNoiseSettings)
        {
            Vector2 domainOffset = GenerateDomainOffset(x, z);
            return MyNoise.OctavePerlin(x + domainOffset.x, z + domainOffset.y, defaultNoiseSettings);
        }

        public Vector2 GenerateDomainOffset(float x, float z)
        {
            var noiseX = MyNoise.OctavePerlin(x, z, noiseDomainX) * amplitudeX;
            var noiseY = MyNoise.OctavePerlin(x, z, noiseDomainY) * amplitudeY;
            return new Vector2(noiseX, noiseY);
        }

        public Vector2Int GenerateDomainOffsetInt(float x, float z)
        {
            return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
        }
    }
}