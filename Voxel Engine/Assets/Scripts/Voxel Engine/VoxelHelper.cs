using System;
using System.Linq;
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
        private static Vector2Int TexturePosition(Direction direction, VoxelType voxelType)
        {
            return direction switch
            {
                Direction.Up => VoxelDataManager.VoxelTextureDataDictionary[voxelType].up,
                Direction.Down => VoxelDataManager.VoxelTextureDataDictionary[voxelType].down,
                _ => VoxelDataManager.VoxelTextureDataDictionary[voxelType].side
            };
        }

        #region PerVoxelMeshing
        
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

                if (neighbourBlockType == VoxelType.Nothing ||
                    VoxelDataManager.VoxelTextureDataDictionary[neighbourBlockType].IsSolid) continue;
                
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
        private static MeshData GetFaceDataInDirection(Direction direction, ChunkData chunkData, int x, int y, int z,
            MeshData meshData, VoxelType voxelType)
        {
            AddFaceVertices(direction, x, y, z, meshData, voxelType, chunkData.WorldReference.voxelScaling);
            
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
        private static void AddFaceVertices(Direction direction, int x, int y, int z, MeshData meshData, VoxelType blockType, Vector3 voxelScale)
        {
            var generatesCollider = VoxelDataManager.VoxelTextureDataDictionary[blockType].GeneratesCollider;
            //order of vertices matters for the normals and how we render the mesh
            switch (direction)
            {
                case Direction.Forward:
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Backwards:
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Right:
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Left:
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Up:
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Down:
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), voxelScale), generatesCollider);
                    break;
                default:
                    break;
            }
        }

        private static Vector2[] FaceUVs(Direction direction, VoxelType voxelType)
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
        #endregion
        
        #region Greedy Meshing

        public static MeshData GreedyMesh(ChunkData chunkData, MeshData meshData)
        {
            //BUG: Geht bis Expanding new Quad, dann passiert nix mehr
            Debug.Log("Generating xy slices");
            //Loop through all xy,yz,xz slices and greedy mesh both sides
            for (var z = 0; z < chunkData.ChunkSize; z++)
            {
                //Precompute slice data
                var sliceDataForward = new VoxelType[chunkData.ChunkSize, chunkData.ChunkHeight];
                var sliceDataBackward = new VoxelType[chunkData.ChunkSize, chunkData.ChunkHeight];
                for (var x = 0; x < chunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < chunkData.ChunkHeight; y++)
                    {
                        //TODO: If neighbor in face direction is solid, set its type to nothing, so it won't get rendered
                        var neighbourBlockCoordinates = new Vector3Int(x, y, z) + Direction.Forward.GetVector3();
                        var neighbourBlockType = Chunk.GetVoxelFromChunkCoordinates(chunkData, neighbourBlockCoordinates);

                        sliceDataForward[x, y] = neighbourBlockType == VoxelType.Nothing ||
                                                 VoxelDataManager.VoxelTextureDataDictionary[neighbourBlockType].IsSolid ? chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, x, y, z)] : VoxelType.Nothing;
                        
                        neighbourBlockCoordinates = new Vector3Int(x, y, z) + Direction.Backwards.GetVector3();
                        neighbourBlockType = Chunk.GetVoxelFromChunkCoordinates(chunkData, neighbourBlockCoordinates);
                        
                        sliceDataBackward[x, y] = neighbourBlockType == VoxelType.Nothing ||
                                                 VoxelDataManager.VoxelTextureDataDictionary[neighbourBlockType].IsSolid ? chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, x, y, z)] : VoxelType.Nothing;

                    }
                }
                
                meshData = GenerateGreedyMesh(z, Direction.Forward, sliceDataForward, chunkData, meshData);
                meshData = GenerateGreedyMesh(z, Direction.Backwards, sliceDataBackward, chunkData, meshData);
            }
            /*
            Debug.Log("Generating yz slices");
            for (var x = 0; x < chunkData.ChunkSize; x++)
            {
                var sliceData = new VoxelType[chunkData.ChunkSize, chunkData.ChunkHeight];
                for (var y = 0; x < chunkData.ChunkSize; x++)
                {
                    for (var z = 0; y < chunkData.ChunkHeight; y++)
                    {
                        sliceData[y, z] = chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, x, y, z)];
                    }
                }
                
                meshData = GenerateGreedyMesh(x, Direction.Right, sliceData, chunkData, meshData);
                meshData = GenerateGreedyMesh(x, Direction.Left, sliceData, chunkData, meshData);
            }
            
            Debug.Log("Generating xz slices");
            for (var y = 0; y < chunkData.ChunkSize; y++)
            {
                var sliceData = new VoxelType[chunkData.ChunkSize, chunkData.ChunkHeight];
                for (var x = 0; x < chunkData.ChunkSize; x++)
                {
                    for (var z = 0; y < chunkData.ChunkHeight; y++)
                    {
                        sliceData[x, z] = chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, x, y, z)];
                    }
                }
                
                meshData = GenerateGreedyMesh(y, Direction.Up, sliceData, chunkData, meshData);
                meshData = GenerateGreedyMesh(y, Direction.Down, sliceData, chunkData, meshData);
            }
            */
            
            Debug.Log("Done greedy mesh");
            return meshData;
        }

        struct GreedyQuad
        {
            public int LowX;
            public int LowY;
            public int HighX;
            public int HighY;
        }
        
        /// <summary>
        /// Create the quads for the slice by greedily expanding them
        /// </summary>
        /// <param name="sliceRow"></param>
        /// <param name="faceNormalDirection"></param>
        /// <param name="sliceData">Voxel types, indexed by lexicographic axes (i.e. slices in xz have [x,z])</param>
        /// <param name="chunkData"></param>
        /// <param name="meshData"></param>
        /// <returns></returns>
        private static MeshData GenerateGreedyMesh(int sliceRow, Direction faceNormalDirection, VoxelType[,] sliceData, ChunkData chunkData,
            MeshData meshData)
        {
            for (var x = 0; x < sliceData.Length; x++)
            {
                for (var y = 0; y < sliceData.GetLength(1); y++)
                {
                    if (sliceData[x, y] == VoxelType.Nothing)
                        continue;
                    
                    Debug.Log("Expanding new Quad");
                    var newQuad = GetNextGreedyQuad(sliceData, x, y);
                    
                    var voxelType = sliceData[newQuad.LowX, newQuad.LowY];
                    
                    meshData = CreateQuad(newQuad, sliceRow, faceNormalDirection, voxelType, meshData, chunkData.WorldReference.voxelScaling);
                    meshData.AddQuadTriangles(VoxelDataManager.VoxelTextureDataDictionary[voxelType].GeneratesCollider);
                    
                    //Remove the voxels from the slice, so they are not processed by several quads
                    for (var i = newQuad.LowX; i < newQuad.HighX; i++)
                    {
                        for (var j = newQuad.LowY; j < newQuad.HighY; j++)
                        {
                            sliceData[i, j] = VoxelType.Nothing;
                        }
                    }
                }
            }
            
            //Add quad to mesh
            
            return meshData;
        }

        private static GreedyQuad GetNextGreedyQuad(VoxelType[,] sliceData, int xStart, int yStart)
        {
            var quad = new GreedyQuad
            {
                LowX = xStart,
                LowY = yStart,
                HighX  = sliceData.GetLength(0),
                HighY = sliceData.GetLength(1)
            };

            // Make the two for loops a function
            //Expand X to max, then expand y as far as possible
            for (var x = quad.LowX; x < sliceData.Length; x++)
            {
                if ((int)sliceData[quad.LowX, quad.LowY] == (int)sliceData[x, quad.LowY])
                    continue;
                
                //Other type found
                quad.HighX = x - 1;
            }

            var firstRow = sliceData.GetRow(quad.LowY);
            
            //Expand y as long as the types match in the whole row
            for (var y = quad.LowY; y < sliceData.GetLength(1); y++)
            {
                if (firstRow.SequenceEqual(sliceData.GetRow(y)))
                    continue;

                quad.HighY = y - 1;
            }

            return quad;
        }

        private static MeshData CreateQuad(GreedyQuad quad, int sliceRow, Direction faceDirection, VoxelType voxelType, MeshData meshData, Vector3 voxelScale)
        {
            var generatesCollider = VoxelDataManager.VoxelTextureDataDictionary[voxelType].GeneratesCollider;
            Debug.Log("Adding quad");
            //order of vertices matters for the normals and how we render the mesh
            switch (faceDirection)
            {
                case Direction.Forward:
                    // br, ur, ul, bl
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.LowY - 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.HighY + 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.HighY + 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.LowY - 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Backwards:
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.LowY - 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.HighY + 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.HighY + 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.LowY - 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider); 
                    break;
                case Direction.Right:
                    // quad.x = global.z
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.LowY - 0.5f, quad.LowX - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.HighY + 0.5f, quad.LowX - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.HighY + 0.5f, quad.HighX + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.LowY - 0.5f, quad.HighX + 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Left:
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.LowY - 0.5f, quad.HighX + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.HighY + 0.5f, quad.HighX + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.HighY + 0.5f, quad.LowX - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.LowY - 0.5f, quad.LowX - 0.5f), voxelScale), generatesCollider); 
                    break;
                case Direction.Up:
                    // quad.y = global.z
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Down:
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    break;
                default:
                    break;
            }

            return meshData;
        }
        
        #endregion
    }
}