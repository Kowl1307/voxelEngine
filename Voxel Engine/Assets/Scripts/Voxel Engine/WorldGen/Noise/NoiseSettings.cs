using UnityEngine;

namespace Voxel_Engine.WorldGen.Noise
{
    [CreateAssetMenu(fileName = "Noise Settings", menuName = "Data/NoiseSettings")]
    public class NoiseSettings : ScriptableObject
    {
        public float NoiseZoom;
        public int Octaves;
        public Vector2Int Offset;
        public Vector2Int WorldOffset;
        public float Persistance;
        public float RedistributionModifier;
        public float Exponent;
    }
}
