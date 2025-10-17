using UnityEngine;

namespace Voxel_Engine.WorldGen
{
    public static class WorldInteractionHelper
    {
        public static void SetVoxel(World world, RaycastHit hit, VoxelType voxelType)
        {
            var chunk = hit.collider.GetComponent<ChunkRenderer>();
            if (chunk == null) return;

            var pos = GetVoxelPosOfRaycastHit(world, hit);

            WorldDataHelper.SetVoxel(chunk.ChunkData.WorldReference, pos, voxelType);
            //TODO: This should be a function of some sort
            var chunkPos = Chunk.GetChunkCoordinateOfVoxelPosition(chunk.ChunkData, pos);
            var index = Chunk.GetIndexFromPosition(chunk.ChunkData, chunkPos.x, chunkPos.y, chunkPos.z);
            chunk.ChunkData.SetVoxelMarkDirty(index, voxelType);
            
            //If block is on edge, update neighbour chunk
            if (Chunk.IsOnEdge(chunk.ChunkData, pos))
            {
                var neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
                foreach (var neighbourData in neighbourDataList)
                {
                    var chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.WorldReference, neighbourData.ChunkPositionInVoxel);
                    if (chunkToUpdate != null)
                        chunkToUpdate.GetMeshDataAndUpdate();
                }
            }
            
            chunk.GetMeshDataAndUpdate();
        }
        public static Vector3Int GetVoxelPosOfRaycastHit(World world, RaycastHit hit)
        {
            var voxelScaling = world.WorldData.VoxelScaling;
            var hitPos = hit.point;
            hitPos -= Vector3.Scale(hit.normal, voxelScaling / 2);
            return WorldDataHelper.GetVoxelPositionFromWorldPosition(world, hitPos);
        }
    }
}