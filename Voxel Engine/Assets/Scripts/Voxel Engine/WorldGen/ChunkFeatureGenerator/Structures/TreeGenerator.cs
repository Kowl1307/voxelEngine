using System.Collections.Generic;
using Kowl.Utils;
using UnityEngine;
using Voxel_Engine.WorldGen.Biomes;
using Voxel_Engine.WorldGen.Noise;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Structures
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
            var treePositionCandidates = DataProcessing.FindLocalMaxima(noise, closestTreeDistance);
            treePositionCandidates = treePositionCandidates.ConvertAll(position => position - new Vector2Int(chunkData.ChunkSizeInVoxel/2, chunkData.ChunkSizeInVoxel/2));

            // This is just for easier debugging.
            /*
            treePositionCandidates = new();
            for (var x = -10; x < chunkData.ChunkSizeInVoxel+10; x++)
            {
                for (var z = -10; z < chunkData.ChunkSizeInVoxel+10; z++)
                {
                    var voxelPos = chunkData.ChunkPositionInVoxel + new Vector3Int(x, 0, z);
                    if (voxelPos.x % 10 == 0 && voxelPos.z % 10 == 0)
                    {
                        treePositionCandidates.Add(new Vector2Int(x, z));
                    }
                }
            }
            */
            
            foreach (var treePosition2DInChunk in treePositionCandidates)
            {
                var treePosition3DInChunk = treePosition2DInChunk.AsX0Z();
                treePosition3DInChunk.y = Chunk.GetSurfaceHeight(chunkData, treePosition2DInChunk);
                //Make the y position in chunk coords
                treePosition3DInChunk.y = Chunk.GetChunkCoordinateOfVoxelPosition(chunkData, treePosition3DInChunk).y;
                if (!IsValidTreePosition(chunkData, treePosition3DInChunk)) continue;

                CreateTree(chunkData, treePosition3DInChunk);
            }
        }

        private bool IsValidTreePosition(ChunkData chunkData, Vector3Int treePosition3DVoxel)
        {
            //TODO: y position may be out of bounds. Convert the position into a valid one.
            var candidateBiome = WorldDataHelper.GetBiomeAt(chunkData.WorldReference,
                Chunk.GetVoxelCoordsFromChunkCoords(chunkData, treePosition3DVoxel.x, treePosition3DVoxel.y,
                    treePosition3DVoxel.z));
            if (!GeneratesInBiome(candidateBiome))
                return false;
                
            var groundVoxelType = Chunk.GetVoxelTypeAt(chunkData, treePosition3DVoxel);
            return allowedTreeGroundTypes.Contains(groundVoxelType);
        }

        private void CreateTree(ChunkData chunkData, Vector3Int treePositionInChunk)
        {
            foreach (var trunkOffset in _trunkPositions)
            {
                var trunkPosition = treePositionInChunk + trunkOffset;
                if (!Chunk.IsInsideChunkBounds(chunkData, trunkPosition))
                    continue;
                
                Chunk.SetVoxel(chunkData, trunkPosition, VoxelType.TreeTrunk);
            }
                
            foreach (var leafOffset in _treeLeavesStaticLayout)
            {
                var leafPosition = treePositionInChunk + leafOffset;
                if (!Chunk.IsInsideChunkBounds(chunkData, leafPosition))
                    continue;
                
                Chunk.SetVoxel(chunkData, leafPosition, VoxelType.TreeLeafesTransparent);
            };
        }

        private float[,] GenerateTreeNoise(ChunkData chunkData, NoiseSettings noiseSettings)
        {
            var voxelPosition = chunkData.ChunkPositionInVoxel;
            var chunkSize = chunkData.ChunkSizeInVoxel;
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
