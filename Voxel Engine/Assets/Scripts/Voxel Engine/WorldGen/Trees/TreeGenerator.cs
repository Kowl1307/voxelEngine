using Unity.VisualScripting;
using UnityEngine;
using Voxel_Engine.WorldGen.Noise;
using Voxel_Engine.WorldGen.Structures;

namespace Voxel_Engine.WorldGen.Trees
{
    public class TreeGenerator : StructureGenerator
    {
        public NoiseSettings treeNoiseSettings;
        public DomainWarping DomainWarping;

        public int closestTreeDistance = 3;

        public bool UseOctaves = false;

        public override StructureData GenerateData(ChunkData chunkData, Vector2Int mapSeedOffset)
        {
            treeNoiseSettings.WorldOffset = mapSeedOffset;
            var treeData = new StructureData();
            var noiseData = GenerateTreeNoise(chunkData, treeNoiseSettings);
            var positions =
                DataProcessing.FindLocalMaxima(noiseData, chunkData.WorldPosition.x, chunkData.WorldPosition.z, closestTreeDistance);
            foreach (var pos in positions)
            {
                treeData.StructurePointsOfInterest.Add(pos);
            }

            treeData.Type = StructureType.Tree;
            return treeData;
        }

        private float[,] GenerateTreeNoise(ChunkData chunkData, NoiseSettings noiseSettings)
        {
            var noiseMax = new float[chunkData.ChunkSize, chunkData.ChunkSize];
            var xMin = chunkData.WorldPosition.x;
            var xMax = xMin + chunkData.ChunkSize;
            var zMin = chunkData.WorldPosition.z;
            var zMax = zMin + chunkData.ChunkSize;
            var xIndex = 0;
            var zIndex = 0;

            for (var x = xMin; x < xMax; x++)
            {
                for (var z = zMin; z < zMax; z++)
                {
                    noiseMax[xIndex, zIndex] = UseOctaves ? MyNoise.OctaveSimplex(x,z, noiseSettings) : MyNoise.SimplexNoise(x, z, noiseSettings);
                    //noiseMax[xIndex, zIndex] = DomainWarping.GenerateDomainNoise(x, z, noiseSettings);
                    zIndex++;
                }

                xIndex++;
                zIndex = 0;
            }

            return noiseMax;
        }
    }
}
