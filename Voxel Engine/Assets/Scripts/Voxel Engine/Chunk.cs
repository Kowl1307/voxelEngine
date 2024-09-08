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
            LoopThroughBlocks(chunkData, (x,y,z) => meshData = VoxelHelper.GetMeshData(chunkData, x,y,z,meshData,chunkData.Voxels[GetIndexFromPosition(chunkData, x,y,z)]));

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

        public static void SetVoxel(ChunkData chunkData, Vector3Int localPosition, VoxelType voxel)
        {
            if (InRange(chunkData, localPosition.x) && InRangeHeight(chunkData, localPosition.y) &&
                InRange(chunkData, localPosition.z))
            {
                var index = GetIndexFromPosition(chunkData, localPosition.x, localPosition.y, localPosition.z);
                chunkData.Voxels[index] = voxel;
            }
            else
            {
                WorldDataHelper.SetVoxel(chunkData.WorldReference, localPosition + chunkData.WorldPosition, voxel);
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

            return chunkData.WorldReference.GetVoxelFromChunkCoordinates(chunkData, chunkData.WorldPosition.x + x,
                chunkData.WorldPosition.y + y, chunkData.WorldPosition.z + z);
        }
        
        public static VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, Vector3Int chunkCoordinates)
        {
            return GetVoxelFromChunkCoordinates(chunkData, chunkCoordinates.x, chunkCoordinates.y, chunkCoordinates.z);
        }

        /// <summary>
        /// Get the local (in chunk coordinates) voxel coordinates 
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3Int GetVoxelInChunkCoordinates(ChunkData chunkData, Vector3Int pos)
        {
            return new Vector3Int
            {
                x = pos.x - chunkData.WorldPosition.x,
                y = pos.y - chunkData.WorldPosition.y,
                z = pos.z - chunkData.WorldPosition.z
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
        private static int GetIndexFromPosition(ChunkData chunkData, int x, int y, int z)
        {
            return x + chunkData.ChunkSize * y + chunkData.ChunkSize * chunkData.ChunkHeight * z;
        }

        public static Vector3Int ChunkPositionFromVoxelCoords(World world, int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            var pos = new Vector3Int
            {
                x = Mathf.FloorToInt(worldPositionX / (float)world.chunkSize) * world.chunkSize,
                y = Mathf.FloorToInt(worldPositionY / (float)world.chunkHeight) * world.chunkHeight,
                z = Mathf.FloorToInt(worldPositionZ / (float)world.chunkSize) * world.chunkSize
            };
            return pos;
        }

        public static bool IsOnEdge(ChunkData chunkData, Vector3Int worldPosition)
        {
            var chunkPosition = GetVoxelInChunkCoordinates(chunkData, worldPosition);
            return chunkPosition.x == 0 || chunkPosition.x == chunkData.ChunkSize - 1 ||
                   chunkPosition.y == 0 || chunkPosition.y == chunkData.ChunkHeight - 1 ||
                   chunkPosition.z == 0 || chunkPosition.z == chunkData.ChunkSize - 1;
        }

        public static List<ChunkData> GetEdgeNeighbourChunk(ChunkData chunkData, Vector3Int worldPosition)
        {
            var chunkPosition = GetVoxelInChunkCoordinates(chunkData, worldPosition);
            var neighboursToUpdate = new List<ChunkData>();
            if (chunkPosition.x == 0)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.WorldReference, worldPosition - Vector3Int.right));
            }
            if (chunkPosition.x == chunkData.ChunkSize - 1)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.WorldReference, worldPosition + Vector3Int.right));
            }
            if (chunkPosition.y == 0)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.WorldReference, worldPosition - Vector3Int.up));
            }
            if (chunkPosition.y == chunkData.ChunkHeight - 1)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.WorldReference, worldPosition + Vector3Int.up));
            }
            if (chunkPosition.z == 0)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.WorldReference, worldPosition - Vector3Int.forward));
            }
            if (chunkPosition.z == chunkData.ChunkSize - 1)
            {
                neighboursToUpdate.Add(WorldDataHelper.GetChunkData(chunkData.WorldReference, worldPosition + Vector3Int.forward));
            }
            return neighboursToUpdate;
        }
    }
}