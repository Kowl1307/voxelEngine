using System.Collections.Generic;
using UnityEngine;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.WorldGen.Structures.Trees
{
    public class TreeGenerator : StructureGenerator
    {
        public NoiseSettings treeNoiseSettings;
        public DomainWarping DomainWarping;

        public int closestTreeDistance = 3;
        public float heightThreshold = 0f;
        public float heightLimit = 25f;
        
        [SerializeField]
        private List<VoxelType> allowedTreeGroundTypes = new List<VoxelType> { VoxelType.GrassDirt };
        
        
        private List<Vector3Int> _cachedTreePositions;


        public override Vector3Int GetClosestPointOfInterest(Vector3Int voxelPosition, World world)
        {
            var closestDistance = Mathf.Infinity;
            var closestPosition = Vector3Int.zero;
            foreach (var treePos in _cachedTreePositions)
            {
                var distance = Vector3Int.Distance(treePos, voxelPosition);
                if (!(distance <= closestDistance)) continue;
                
                closestDistance = distance;
                closestPosition = treePos;
            }

            return closestPosition;
        }

        protected override List<Vector3Int> GetClosePointsOfInterest(Vector3Int voxelPosition, World world)
        {
            treeNoiseSettings.Seed = world.WorldData.WorldSeed;
            
            var noiseData = GenerateTreeNoise(voxelPosition, treeNoiseSettings);
            var positions =
                DataProcessing.FindLocalMaxima(noiseData, closestTreeDistance);
            
            var voxelPositionPois = positions.ConvertAll(localPos =>
            {
                var voxelXZ = (localPos - structureExtents.XZ() / 2) + voxelPosition.XZ();
                var chunkData = WorldDataHelper.GetChunkDataFromVoxelCoords(world, voxelXZ.AsX0Z());
                var biomeGen = world.terrainGenerator.GetBiomeGeneratorAt(voxelXZ.AsX0Z(), chunkData);
                var surfaceHeight = biomeGen.GetSurfaceHeightNoise(voxelXZ.x, voxelXZ.y, chunkData);
                return new Vector3Int(voxelXZ.x, surfaceHeight, voxelXZ.y);
            });
            
            foreach (var worldPositionPoi in voxelPositionPois)
            {
                CachedBounds.Add(new Bounds(worldPositionPoi, structureExtents));
            }
            
            _cachedTreePositions.AddRange(voxelPositionPois);

            return voxelPositionPois;
        }

        public override VoxelType GetStructureVoxelAt(Vector3Int voxelPosition, World world)
        {
            foreach (var treeCandidatePoi in _cachedTreePositions)
            {
                var worldPositionInLocalSpace = voxelPosition - treeCandidatePoi;
                
                if (_trunkPositions.Contains(worldPositionInLocalSpace))
                    return VoxelType.TreeTrunk;

                if (_treeLeavesStaticLayout.Contains(worldPositionInLocalSpace))
                    return VoxelType.TreeLeafesTransparent;
            }

            return VoxelType.Nothing;
        }

        /// <summary>
        /// Generates a 2D array of noise with the worldPosition represented by the center pixel.
        /// </summary>
        /// <param name="voxelPosition"></param>
        /// <param name="noiseSettings"></param>
        /// <returns></returns>
        private float[,] GenerateTreeNoise(Vector3Int voxelPosition, NoiseSettings noiseSettings)
        {
            var noise = new float[structureExtents.x, structureExtents.z];
            var xMin = voxelPosition.x - structureExtents.x;
            var xMax = voxelPosition.x + structureExtents.x;
            var zMin = voxelPosition.z - structureExtents.z;
            var zMax = voxelPosition.z + structureExtents.z;
            var xIndex = 0;
            var zIndex = 0;

            for (var x = xMin; x < xMax; x++)
            {
                for (var z = zMin; z < zMax; z++)
                {
                    //noiseMax[xIndex, zIndex] = UseOctaves ? MyNoise.OctaveSimplex(x,z, noiseSettings) : MyNoise.SimplexNoise(x, z, noiseSettings);
                    noise[xIndex, zIndex] = DomainWarping.GenerateDomainNoise(x, z, noiseSettings);
                    zIndex++;
                }

                xIndex++;
                zIndex = 0;
            }

            return noise;
        }
        
        private List<Vector3Int> _trunkPositions = new List<Vector3Int>()
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,2,0),
            new Vector3Int(0,3,0),
            new Vector3Int(0,4,0),
        };
        private List<Vector3Int> _treeLeavesStaticLayout = new List<Vector3Int>
        {
            new(-2, 5, -2),
            new(-2, 5, -1),
            new(-2, 5, 0),
            new(-2, 5, 1),
            new(-2, 5, 2),
            new(-1, 5, -2),
            new(-1, 5, -1),
            new(-1, 5, 0),
            new(-1, 5, 1),
            new(-1, 5, 2),
            new(0, 5, -2),
            new(0, 5, -1),
            new(0, 5, 0),
            new(0, 5, 1),
            new(0, 5, 2),
            new(1, 5, -2),
            new(1, 5, -1),
            new(1, 5, 0),
            new(1, 5, 1),
            new(1, 5, 2),
            new(2, 5, -2),
            new(2, 5, -1),
            new(2, 5, 0),
            new(2, 5, 1),
            new(2, 5, 2),
            new(-1, 6, -1),
            new(-1, 6, 0),
            new(-1, 6, 1),
            new(0, 6, -1),
            new(0, 6, 0),
            new(0, 6, 1),
            new(1, 6, -1),
            new(1, 6, 0),
            new(1, 6, 1),
            new(0, 7, 0)
        };
    }
}
