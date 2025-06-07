using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine
{
    public static class Chunk
    {
        public static MeshData GetChunkMeshData(ChunkData chunkData)
        {
            var meshData = new MeshData(true);

            //Calculate meshData from each voxel
            // LoopThroughBlocks(chunkData, (x,y,z) => meshData = VoxelHelper.GetMeshData(chunkData, x,y,z,meshData,chunkData.Voxels[GetIndexFromPosition(chunkData, x,y,z)]));
            meshData = VoxelHelper.GreedyMesh(chunkData, meshData);
            
            return meshData;
        }

        public static void LoopThroughBlocks(ChunkData chunkData, Action<int, int, int> actionToPerform)
        {
            for (var i = 0; i < chunkData.Voxels.Length; i++)
            {
                var position = GetPositionFromIndex(chunkData, i);
                actionToPerform(position.x, position.y, position.z);
            }
        }

        public static void SetVoxel(ChunkData chunkData, Vector3Int chunkCoords, VoxelType voxel)
        {
            if (InRange(chunkData, chunkCoords.x) && InRangeHeight(chunkData, chunkCoords.y) &&
                InRange(chunkData, chunkCoords.z))
            {
                var index = GetIndexFromPosition(chunkData, chunkCoords.x, chunkCoords.y, chunkCoords.z);
                chunkData.Voxels[index] = voxel;
            }
            else
            {
                WorldDataHelper.SetVoxel(chunkData.WorldReference, chunkCoords + chunkData.ChunkPositionInVoxel, voxel);
            }
        }

        /// <summary>
        /// Get VoxelType of the given local (in chunk coordinates) coordinates
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
        {
            if (InRange(chunkData, x) && InRangeHeight(chunkData, y) &&
                InRange(chunkData, z))
            {
                var index = GetIndexFromPosition(chunkData, x, y, z);
                return chunkData.Voxels[index];
            }
            
            return chunkData.WorldReference.GetVoxelFromChunkCoordinates(chunkData, x,
                y, z);
        }
        
        public static VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, Vector3Int chunkCoordinates)
        {
            return GetVoxelFromChunkCoordinates(chunkData, chunkCoordinates.x, chunkCoordinates.y, chunkCoordinates.z);
        }

        /// <summary>
        /// Get the local (in chunk coordinates) voxel coordinates 
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="voxelPosition"></param>
        /// <returns></returns>
        public static Vector3Int GetChunkCoordinateOfVoxelPosition(ChunkData chunkData, Vector3Int voxelPosition)
        {
            return new Vector3Int
            {
                x = voxelPosition.x - chunkData.ChunkPositionInVoxel.x,
                y = voxelPosition.y - chunkData.ChunkPositionInVoxel.y,
                z = voxelPosition.z - chunkData.ChunkPositionInVoxel.z
            };
        }
        
        public static Vector3Int GetVoxelCoordsFromChunkCoords(ChunkData chunkData, int chunkPositionX, int chunkPositionY, int chunkPositionZ)
        {
            return new Vector3Int
            {
                x = chunkPositionX + chunkData.ChunkPositionInVoxel.x,
                y = chunkPositionY + chunkData.ChunkPositionInVoxel.y,
                z = chunkPositionZ + chunkData.ChunkPositionInVoxel.z
            };
        }
        
        /// <summary>
        /// Get local (in chunk coordinates) position from index
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static Vector3Int GetPositionFromIndex(ChunkData chunkData, int index)
        {
            var x = index % chunkData.ChunkSize;
            var y = (index / chunkData.ChunkSize) % chunkData.ChunkHeight;
            var z = index / (chunkData.ChunkSize * chunkData.ChunkHeight);
            return new Vector3Int(x, y, z);
        }

        private static bool InRange(ChunkData chunkData, int axisCoordinate)
        {
            return axisCoordinate >= 0 && axisCoordinate < chunkData.ChunkSize;
        }

        private static bool InRangeHeight(ChunkData chunkData, int yCoordinate)
        {
            return yCoordinate >= 0 && yCoordinate < chunkData.ChunkHeight;
        }
        public static int GetIndexFromPosition(ChunkData chunkData, int x, int y, int z)
        {
            return x + chunkData.ChunkSize * y + chunkData.ChunkSize * chunkData.ChunkHeight * z;
        }

        public static bool IsOnEdge(ChunkData chunkData, Vector3Int voxelPos)
        {
            var chunkPosition = GetChunkCoordinateOfVoxelPosition(chunkData, voxelPos);
            return chunkPosition.x == 0 || chunkPosition.x == chunkData.ChunkSize - 1 ||
                   chunkPosition.y == 0 || chunkPosition.y == chunkData.ChunkHeight - 1 ||
                   chunkPosition.z == 0 || chunkPosition.z == chunkData.ChunkSize - 1;
        }

        public static List<ChunkData> GetEdgeNeighbourChunk(ChunkData chunkData, Vector3Int voxelPos)
        {
            var chunkPosition = GetChunkCoordinateOfVoxelPosition(chunkData, voxelPos);
            var neighboursToUpdate = new List<ChunkData>();
            if (chunkPosition.x == 0)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkDataFromVoxelCoords(chunkData.WorldReference, voxelPos - Vector3Int.right));
            }
            if (chunkPosition.x == chunkData.ChunkSize - 1)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkDataFromVoxelCoords(chunkData.WorldReference, voxelPos + Vector3Int.right));
            }
            if (chunkPosition.y == 0)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkDataFromVoxelCoords(chunkData.WorldReference, voxelPos - Vector3Int.up));
            }
            if (chunkPosition.y == chunkData.ChunkHeight - 1)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkDataFromVoxelCoords(chunkData.WorldReference, voxelPos + Vector3Int.up));
            }
            if (chunkPosition.z == 0)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkDataFromVoxelCoords(chunkData.WorldReference, voxelPos - Vector3Int.forward));
            }
            if (chunkPosition.z == chunkData.ChunkSize - 1)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkDataFromVoxelCoords(chunkData.WorldReference, voxelPos + Vector3Int.forward));
            }
            return neighboursToUpdate;
        }
    }
}