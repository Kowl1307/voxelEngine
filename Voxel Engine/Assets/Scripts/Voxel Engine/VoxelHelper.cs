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
            FillMesh(chunkData, meshData, (currentType, neighbourType) => 
                currentType is not (VoxelType.Air or VoxelType.Nothing) 
                && neighbourType is not VoxelType.Nothing 
                && !VoxelDataManager.VoxelTextureDataDictionary[neighbourType].IsSolid 
                && currentType is not VoxelType.Water);

            FillMesh(chunkData, meshData.WaterMesh, (currentType, neighbourType) => 
                currentType is not  (VoxelType.Air or VoxelType.Nothing) 
                && neighbourType is not VoxelType.Nothing
                && !VoxelDataManager.VoxelTextureDataDictionary[neighbourType].IsSolid
                && currentType is VoxelType.Water
                && neighbourType == VoxelType.Air);
            
            return meshData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkData"></param>
        /// <param name="meshData"></param>
        /// <param name="shouldRenderBlock">(currentType, neighbourType) => true if should be rendered, false if not</param>
        private static void FillMesh(ChunkData chunkData, MeshData meshData, Func<VoxelType, VoxelType, bool> shouldRenderBlock)
        {
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
                        sliceDataForward[x, y] =
                            GetVoxelTypeOrNone(chunkData, x, y, z, Direction.Forward, shouldRenderBlock);

                        sliceDataBackward[x, y] = GetVoxelTypeOrNone(chunkData, x,y,z, Direction.Backwards, shouldRenderBlock);
                    }
                }
                
                meshData = GenerateGreedyMesh(z, Direction.Forward, sliceDataForward, chunkData, meshData);
                meshData = GenerateGreedyMesh(z, Direction.Backwards, sliceDataBackward, chunkData, meshData);
            }
            
            
            for (var x = 0; x < chunkData.ChunkSize; x++)
            {
                //Precompute slice data
                var sliceDataLeft = new VoxelType[chunkData.ChunkHeight, chunkData.ChunkSize];
                var sliceDataRight = new VoxelType[chunkData.ChunkHeight, chunkData.ChunkSize];
                for (var y = 0; y < chunkData.ChunkHeight; y++)
                {
                    for (var z = 0; z < chunkData.ChunkSize; z++)
                    {
                        sliceDataLeft[y, z] = GetVoxelTypeOrNone(chunkData, x, y, z, Direction.Left, shouldRenderBlock);

                        sliceDataRight[y, z] = GetVoxelTypeOrNone(chunkData, x, y, z, Direction.Right, shouldRenderBlock);
                    }
                }
                
                meshData = GenerateGreedyMesh(x, Direction.Left, sliceDataRight, chunkData, meshData);
                meshData = GenerateGreedyMesh(x, Direction.Right, sliceDataLeft, chunkData, meshData);
            }
            
            for (var y = 0; y < chunkData.ChunkHeight; y++)
            {
                //Precompute slice data
                var sliceDataUp = new VoxelType[chunkData.ChunkSize, chunkData.ChunkSize];
                var sliceDataDown = new VoxelType[chunkData.ChunkSize, chunkData.ChunkSize];
                for (var x = 0; x < chunkData.ChunkSize; x++)
                {
                    for (var z = 0; z < chunkData.ChunkSize; z++)
                    {
                        sliceDataUp[x, z] = GetVoxelTypeOrNone(chunkData, x, y, z, Direction.Up, shouldRenderBlock);

                        sliceDataDown[x, z] = GetVoxelTypeOrNone(chunkData, x, y, z, Direction.Down, shouldRenderBlock);
                    }
                }
                
                meshData = GenerateGreedyMesh(y, Direction.Up, sliceDataUp, chunkData, meshData);
                meshData = GenerateGreedyMesh(y, Direction.Down, sliceDataDown, chunkData, meshData);
            }
        }

        private static VoxelType GetVoxelTypeOrNone(ChunkData chunkData, int x, int y, int z, Direction direction, Func<VoxelType, VoxelType, bool> shouldRenderBlock)
        {
            var neighbourBlockCoordinates = new Vector3Int(x, y, z) + direction.GetVector3();
            var neighbourBlockType = Chunk.GetVoxelFromChunkCoordinates(chunkData, neighbourBlockCoordinates);

            var currentType = chunkData.Voxels[Chunk.GetIndexFromPosition(chunkData, x, y, z)];

            return shouldRenderBlock(currentType, neighbourBlockType)
                ? currentType
                : VoxelType.Nothing;
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
            for (var y = 0; y < sliceData.GetLength(1); y++)
            {
                for (var x = 0; x < sliceData.GetLength(0); x++)
                {
                    if (sliceData[x, y] == VoxelType.Nothing)
                        continue;
                    
                    var newQuad = GetNextGreedyQuad(sliceData, x, y);
                    
                    var voxelType = sliceData[newQuad.LowX, newQuad.LowY];
                    
                    meshData = CreateQuad(newQuad, sliceRow, faceNormalDirection, voxelType, meshData, chunkData.WorldReference.voxelScaling);
                    meshData.AddQuadTriangles(VoxelDataManager.VoxelTextureDataDictionary[voxelType].GeneratesCollider);
                    meshData = GetFaceUVsGreedy(faceNormalDirection, voxelType, newQuad, meshData);
                    
                    //Remove the voxels from the slice, so they are not processed by several quads
                    for (var i = newQuad.LowX; i < newQuad.HighX; i++)
                    {
                        for (var j = newQuad.LowY; j < newQuad.HighY; j++)
                        {
                            sliceData[i, j] = VoxelType.Nothing;
                        }
                    }

                    x = newQuad.HighX - 1;
                }
            }
            
            //Add quad to mesh
            
            return meshData;
        }

        private static GreedyQuad GetNextGreedyQuad(VoxelType[,] sliceData, int xStart, int yStart)
        {
            var type = sliceData[xStart, yStart];
            var maxX = sliceData.GetLength(0);
            var maxY = sliceData.GetLength(1);

            // Expand x
            var width = 1;
            for (var x = xStart + 1; x < maxX; x++)
            {
                if (sliceData[x, yStart] != type)
                    break;
                width++;
            }

            // Expand y
            var height = 1;
            for (var y = yStart + 1; y < maxY; y++)
            {
                var rowMatches = true;
                for (var x = xStart; x < xStart + width; x++)
                {
                    if (sliceData[x, y] == type) continue;
                    
                    rowMatches = false;
                    break;
                }
                
                if (!rowMatches)
                    break;
                height++;
            }

            return new GreedyQuad
            {
                LowX = xStart,
                LowY = yStart,
                HighX = xStart + width,
                HighY = yStart + height
            };
        }

        private static MeshData CreateQuad(GreedyQuad quad, int sliceRow, Direction faceDirection, VoxelType voxelType, MeshData meshData, Vector3 voxelScale)
        {
            var generatesCollider = VoxelDataManager.VoxelTextureDataDictionary[voxelType].GeneratesCollider;

            // High values are exclusive, so we need to subtract 1
            quad.HighX -= 1;
            quad.HighY -= 1;
            //order of vertices matters for the normals and how we render the mesh
            switch (faceDirection)
            {
                case Direction.Forward:
                    // bl, br, ur, ul
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.LowY - 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.HighY + 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.HighY + 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.LowY - 0.5f, sliceRow + 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Backwards:
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.LowY - 0.5f, sliceRow - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, quad.HighY + 0.5f, sliceRow - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.HighY + 0.5f, sliceRow - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, quad.LowY - 0.5f, sliceRow - 0.5f), voxelScale), generatesCollider); 
                    break;
                case Direction.Left:
                    // quad.x = global.y
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.LowX - 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.HighX + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.HighX + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow + 0.5f, quad.LowX - 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Right:
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow - 0.5f, quad.LowX - 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow - 0.5f, quad.HighX + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow - 0.5f, quad.HighX + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(sliceRow - 0.5f, quad.LowX - 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider); 
                    break;
                case Direction.Up:
                    // quad.y = global.z
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow + 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow + 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    break;
                case Direction.Down:
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow - 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow - 0.5f, quad.LowY - 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.HighX + 0.5f, sliceRow - 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    meshData.AddVertex(Vector3.Scale(new Vector3(quad.LowX - 0.5f, sliceRow - 0.5f, quad.HighY + 0.5f), voxelScale), generatesCollider);
                    break;
                default:
                    break;
            }

            return meshData;
        }
        
        private static MeshData GetFaceUVsGreedy(Direction direction, VoxelType voxelType, GreedyQuad quad, MeshData meshData)
        {
            var uvs = new Vector2[4];
            var uv2s = new Vector2[4];
            var uv3s = new Vector2[4];
            var tilePos = TexturePosition(direction, voxelType);

            var tileSizeX = VoxelDataManager.TileSizeX;
            var tileSizeY = VoxelDataManager.TileSizeY;

            var quadWidth = quad.HighX - quad.LowX;
            var quadHeight = quad.HighY - quad.LowY;

            uvs[0] = new Vector2(
                tileSizeX * tilePos.x + tileSizeX - VoxelDataManager.TextureOffset,
                tileSizeY * tilePos.y + VoxelDataManager.TextureOffset);
            uvs[1] = new Vector2(
                tileSizeX * tilePos.x + tileSizeX - VoxelDataManager.TextureOffset,
                tileSizeY * tilePos.y + tileSizeY - VoxelDataManager.TextureOffset);
            uvs[2] = new Vector2(
                tileSizeX * tilePos.x + VoxelDataManager.TextureOffset,
                tileSizeY * tilePos.y + tileSizeY - VoxelDataManager.TextureOffset);
            uvs[3] = new Vector2(
                tileSizeX * tilePos.x + VoxelDataManager.TextureOffset,
                tileSizeY * tilePos.y + VoxelDataManager.TextureOffset);
            
            
            // Offset und Skalierung für den Atlas im Shader!
            // Im Mesh: Gib NUR die lokalen UVs weiter (wie oben)

            meshData.UV.AddRange(uvs);
            
            var vector = (direction is Direction.Forward or Direction.Backwards) ? new Vector2(quadHeight, quadWidth) :  new Vector2(quadWidth, quadHeight);
            uv2s[0] = vector;
            uv2s[1] = vector;
            uv2s[2] = vector;
            uv2s[3] = vector;
            
            meshData.UV2.AddRange(uv2s);
            
            return meshData;
        }
        
        #endregion
    }
}