using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private List<VoxelType> allowedTreeGroundTypes = new() { VoxelType.GrassDirt };
        
        private readonly Vector2Int _treeExtentsXZ = new Vector2Int(6, 6);
        private Vector2Int TreeHalfExtentsXZ => _treeExtentsXZ / 2;

        public override void Handle(ChunkData chunkData)
        {
            var noise = GenerateTreeNoise(chunkData, treeNoiseSettings);
            var treePositions = DataProcessing.FindLocalMaxima(noise, closestTreeDistance);
            treePositions = treePositions.ConvertAll(position => position - new Vector2Int(chunkData.ChunkSize/2, chunkData.ChunkSize/2));

            // This is just for easier debugging.
            /*
            treePositions = new();
            for (var x = -10; x < chunkData.ChunkSize+10; x++)
            {
                for (var z = -10; z < chunkData.ChunkSize+10; z++)
                {
                    var voxelPos = chunkData.ChunkPositionInVoxel + new Vector3Int(x, 0, z);
                    if (voxelPos.x % 10 == 0 && voxelPos.z % 10 == 0)
                    {
                        treePositions.Add(new Vector2Int(x, z));
                    }
                }
            }
            */
            
            foreach (var treePosition2D in treePositions)
            {
                var treePosition3DVoxel = treePosition2D.AsX0Z();
                treePosition3DVoxel.y = Chunk.GetSurfaceHeight(chunkData, treePosition2D);

                var groundVoxelType = Chunk.GetVoxelTypeAt(chunkData, treePosition3DVoxel);
                
                if (allowedTreeGroundTypes.Contains(groundVoxelType))
                {
                    CreateTree(chunkData, treePosition3DVoxel);
                }
            }
        }

        private void CreateTree(ChunkData chunkData, Vector3Int treePosition3D)
        {
            foreach (var trunkOffset in _trunkPositions)
            {
                var trunkPosition = treePosition3D + trunkOffset;
                if (!Chunk.IsInsideChunkBounds(chunkData, trunkPosition))
                    continue;
                
                Chunk.SetVoxel(chunkData, trunkPosition, VoxelType.TreeTrunk);
            }

            foreach (var leafOffset in _treeLeavesStaticLayout)
            {
                var leafPosition = leafOffset + treePosition3D;
                if (!Chunk.IsInsideChunkBounds(chunkData, leafPosition))
                    continue;
                
                Chunk.SetVoxel(chunkData, leafPosition, VoxelType.TreeLeafesTransparent);
            }
        }

        private float[,] GenerateTreeNoise(ChunkData chunkData, NoiseSettings noiseSettings)
        {
            var voxelPosition = chunkData.ChunkPositionInVoxel;
            var chunkSize = chunkData.ChunkSize;
            var voxelScale = chunkData.WorldReference.WorldData.VoxelScaling;
            
            var noise = new float[chunkSize*2, chunkSize*2];
            var xMin = voxelPosition.x - chunkSize;
            var xMax = voxelPosition.x + chunkSize;
            var zMin = voxelPosition.z - chunkSize;
            var zMax = voxelPosition.z + chunkSize;
            
            var xIndex = 0;
            var zIndex = 0;
            for (var xInVoxel = xMin; xInVoxel < xMax; xInVoxel++)
            {
                for (var zInVoxel = zMin; zInVoxel < zMax; zInVoxel++)
                {
                    //noiseMax[xIndex, zIndex] = UseOctaves ? MyNoise.OctaveSimplex(x,z, noiseSettings) : MyNoise.SimplexNoise(x, z, noiseSettings);
                    //noise[xIndex, zIndex] = DomainWarping.GenerateDomainNoise(x * voxelScale.x, z * voxelScale.y, noiseSettings);
                    noise[xIndex, zIndex] = MyNoise.SimplexNoise(xInVoxel * voxelScale.x, zInVoxel * voxelScale.y, noiseSettings);
                    zIndex++;
                }

                xIndex++;
                zIndex = 0;
            }

            return noise;
        }
        
        private readonly List<Vector3Int> _trunkPositions = new List<Vector3Int>()
        {
            new Vector3Int(0,1,0),
            new Vector3Int(0,2,0),
            new Vector3Int(0,3,0),
            new Vector3Int(0,4,0),
        };
        private readonly List<Vector3Int> _treeLeavesStaticLayout = new List<Vector3Int>
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
