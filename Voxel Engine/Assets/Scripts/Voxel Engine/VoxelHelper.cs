using UnityEngine;

namespace Voxel_Engine
{
    public static class VoxelHelper
    {
        private static Direction[] _directions =
        {
            Direction.Forward,
            Direction.Backwards,
            Direction.Right,
            Direction.Left,
            Direction.Up,
            Direction.Down
        };

        //This would have to be changed for different side textures
        public static Vector2Int TexturePosition(Direction direction, VoxelType voxelType)
        {
            return direction switch
            {
                Direction.Up => VoxelDataManager.VoxelTextureDataDictionary[voxelType].up,
                Direction.Down => VoxelDataManager.VoxelTextureDataDictionary[voxelType].down,
                _ => VoxelDataManager.VoxelTextureDataDictionary[voxelType].side
            };
        }

        /// <summary>
        /// Calculates the whole meshData.
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="meshData"></param>
        /// <param name="voxelType"></param>
        /// <returns></returns>
        public static MeshData GetMeshData(ChunkData chunkData, int x, int y, int z, MeshData meshData,
            VoxelType voxelType)
        {
            if (voxelType == VoxelType.Air || voxelType == VoxelType.Nothing)
                return meshData;

            foreach (Direction direction in _directions)
            {
                var neighbourBlockCoordinates = new Vector3Int(x, y, z) + direction.GetVector3();
                var neighbourBlockType = Chunk.GetVoxelFromChunkCoordinates(chunkData, neighbourBlockCoordinates);

                if (neighbourBlockType != VoxelType.Nothing && VoxelDataManager.VoxelTextureDataDictionary[neighbourBlockType].IsSolid == false)
                {

                    if (voxelType == VoxelType.Water)
                    {
                        if (neighbourBlockType == VoxelType.Air)
                            meshData.WaterMesh = GetFaceDataInDirection(direction, chunkData, x, y, z, meshData.WaterMesh, voxelType);
                    }
                    else
                    {
                        meshData = GetFaceDataInDirection(direction, chunkData, x, y, z, meshData, voxelType);
                    }

                }
            }

            return meshData;
        }
        
        /// <summary>
        /// Adds the needed vertices, triangles and uv data for the given voxel into the meshData.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="chunkData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="meshData"></param>
        /// <param name="voxelType"></param>
        /// <returns></returns>
        public static MeshData GetFaceDataInDirection(Direction direction, ChunkData chunkData, int x, int y, int z,
            MeshData meshData, VoxelType voxelType)
        {
            GetFaceVertices(direction, x, y, z, meshData, voxelType);
            meshData.AddQuadTriangles(VoxelDataManager.VoxelTextureDataDictionary[voxelType].GeneratesCollider);
            meshData.UV.AddRange(FaceUVs(direction,voxelType));

            return meshData;
        }

        /// <summary>
        /// Adds the vertices of the voxel in the given direction into the meshData.
        /// x,y,z are the coordinates of the center of the voxel
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="meshData"></param>
        /// <param name="blockType"></param>
        public static void GetFaceVertices(Direction direction, int x, int y, int z, MeshData meshData, VoxelType blockType)
        {
            var generatesCollider = VoxelDataManager.VoxelTextureDataDictionary[blockType].GeneratesCollider;
            //order of vertices matters for the normals and how we render the mesh
            switch (direction)
            {
                case Direction.Forward:
                    meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                    break;
                case Direction.Backwards:
                    meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                    break;
                case Direction.Right:
                    meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                    break;
                case Direction.Left:
                    meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                    break;
                case Direction.Up:
                    meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                    break;
                case Direction.Down:
                    meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                    meshData.AddVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                    break;
                default:
                    break;
            }
        }

        public static Vector2[] FaceUVs(Direction direction, VoxelType voxelType)
        {
            var uvs = new Vector2[4];
            var tilePos = TexturePosition(direction, voxelType);

            uvs[0] = new Vector2(
                VoxelDataManager.TileSizeX * tilePos.x + VoxelDataManager.TileSizeX - VoxelDataManager.TextureOffset,
                VoxelDataManager.TileSizeY * tilePos.y + VoxelDataManager.TextureOffset);
            uvs[1] = new Vector2(
                VoxelDataManager.TileSizeX * tilePos.x + VoxelDataManager.TileSizeX - VoxelDataManager.TextureOffset,
                VoxelDataManager.TileSizeY * tilePos.y + VoxelDataManager.TileSizeY - VoxelDataManager.TextureOffset);
            uvs[2] = new Vector2(
                VoxelDataManager.TileSizeX * tilePos.x + VoxelDataManager.TextureOffset,
                VoxelDataManager.TileSizeY * tilePos.y + VoxelDataManager.TileSizeY - VoxelDataManager.TextureOffset);
            uvs[3] = new Vector2(
                VoxelDataManager.TileSizeX * tilePos.x + VoxelDataManager.TextureOffset,
                VoxelDataManager.TileSizeY * tilePos.y + VoxelDataManager.TextureOffset);

            return uvs;
        }
    }
}