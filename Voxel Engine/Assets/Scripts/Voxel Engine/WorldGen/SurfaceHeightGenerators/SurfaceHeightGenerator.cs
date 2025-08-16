using UnityEngine;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.WorldGen.SurfaceHeightGenerators
{
    public class SurfaceHeightGenerator : MonoBehaviour
    {
        public int GetSurfaceHeight(int voxelX, int voxelZ, int minimumHeight, int maximumHeight, NoiseSettings noiseSettings, WorldData worldData)
        {
            var voxelScale = worldData.VoxelScaling;
            // var chunkHeight = worldData.ChunkHeightInVoxel;
            
            // useDomainWarping ? DomainWarping.GenerateDomainNoise(voxelX * voxelScale.x, voxelZ * voxelScale.z, noiseSettings) :
            var terrainHeight =  MyNoise.OctavePerlin(voxelX * voxelScale.x, voxelZ * voxelScale.z, noiseSettings);
            // terrainHeight /= voxelScale.y;
            //var terrainHeight = useDomainWarping ? MyNoise.OctaveSimplex(x,z,BiomeNoiseSettings) : MyNoise.SimplexNoise(x, z, BiomeNoiseSettings);
            terrainHeight = MyNoise.Redistribution(terrainHeight, noiseSettings);
            return MyNoise.RemapValue01ToInt(terrainHeight, 0, maximumHeight) + minimumHeight;
        }
    }
}