using UnityEngine;
using UnityEngine.Serialization;

namespace Voxel_Engine.WorldGen.Noise
{
    [CreateAssetMenu(fileName = "Noise Settings", menuName = "Data/NoiseSettings")]
    public class NoiseSettings : ScriptableObject
    {
        public float NoiseZoom = 1;
        public int Octaves = 3;
        public Vector2Int Offset = new Vector2Int(0,0);
        [FormerlySerializedAs("WorldOffset")] public Vector2Int Seed = new Vector2Int(0,0);
        public float Persistance = 0.5f;
        public float RedistributionModifier = 0.5f;
        public float Exponent = 3;
    }
}
