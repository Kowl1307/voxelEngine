using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Voxel_Engine
{
    public class MeshData
    {
        public List<Vector3> Vertices = new List<Vector3>();
        public List<int> Triangles = new List<int>();
        public List<Vector2> UV = new List<Vector2>();
        public List<Vector2> UV2 = new List<Vector2>();
        public List<Vector2> UV3 = new List<Vector2>();

        public List<Vector3> ColliderVertices = new List<Vector3>();
        public List<int> ColliderTriangles = new List<int>();

        public Material Material;

        public MeshData WaterMesh;

        public MeshData(bool isMainMesh)
        {
            if (isMainMesh)
            {
                WaterMesh = new MeshData(false);
            }
        }

        public void AddVertex(Vector3 vertex, bool vertexGeneratesCollider)
        {
            Vertices.Add(vertex);
            if(vertexGeneratesCollider)
                ColliderVertices.Add(vertex);
        }

        public void AddQuadTriangles(bool quadGeneratesCollider)
        {
            //A quad has vertices 0 to 3, so 4-4 = 0, 4-3 = 1, 4-2 = 2, the upper left triangle
            Triangles.Add(Vertices.Count - 4);
            Triangles.Add(Vertices.Count - 3);
            Triangles.Add(Vertices.Count - 2);
            
            Triangles.Add(Vertices.Count - 4);
            Triangles.Add(Vertices.Count - 2);
            Triangles.Add(Vertices.Count - 1);

            if (!quadGeneratesCollider) return;
            
            ColliderTriangles.Add(Vertices.Count - 4);
            ColliderTriangles.Add(Vertices.Count - 3);
            ColliderTriangles.Add(Vertices.Count - 2);
                
            ColliderTriangles.Add(Vertices.Count - 4);
            ColliderTriangles.Add(Vertices.Count - 2);
            ColliderTriangles.Add(Vertices.Count - 1);
        }
    }

    public struct MeshDataStruct
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<int> Triangles;
        public NativeArray<Vector2> UV;
        public NativeArray<Vector2> UV2;
        public NativeArray<Vector2> UV3;
        public NativeArray<Vector3> ColliderVertices;
        public NativeArray<int> ColliderTriangles;

        public MeshDataStruct(MeshData meshData)
        {
            Vertices = meshData.Vertices.ToNativeArray(Allocator.TempJob);
            Triangles = meshData.Triangles.ToNativeArray(Allocator.TempJob);
            UV = meshData.UV.ToNativeArray(Allocator.TempJob);
            UV2 = meshData.UV2.ToNativeArray(Allocator.TempJob);
            UV3 = meshData.UV3.ToNativeArray(Allocator.TempJob);
            ColliderVertices = meshData.ColliderVertices.ToNativeArray(Allocator.TempJob);
            ColliderTriangles = meshData.ColliderTriangles.ToNativeArray(Allocator.TempJob);
        }
    }
}
