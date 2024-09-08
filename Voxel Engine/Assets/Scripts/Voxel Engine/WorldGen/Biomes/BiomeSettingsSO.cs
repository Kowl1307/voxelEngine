using UnityEngine;

namespace Voxel_Engine.WorldGen.Biomes
{
    [CreateAssetMenu(fileName = "Biome Settings", menuName = "Data/Biome Settings")]
    public class BiomeSettingsSO : ScriptableObject
    {
        public int MinimumHeight = 0;
        public int WaterLevel = 10;
    }
}