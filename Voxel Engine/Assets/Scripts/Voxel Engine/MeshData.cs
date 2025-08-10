using System.Collections.Generic;
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
        private bool isMainMesh = true;

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
}
