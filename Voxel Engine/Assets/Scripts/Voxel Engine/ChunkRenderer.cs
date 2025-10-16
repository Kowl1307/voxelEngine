using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Voxel_Engine
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkRenderer : MonoBehaviour
    {
        public ChunkData ChunkData { get; private set; }

        public bool showGizmo;
        
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _mesh = _meshFilter.mesh;
        }

        public void InitializeChunk(ChunkData data)
        {
            ChunkData = data;
        }

        private void RenderMesh(MeshData meshData)
        {
            _mesh.Clear();

            //2, one for opaque material one for transparent (water)
            _mesh.subMeshCount = 2;
            _mesh.vertices = meshData.Vertices.Concat(meshData.WaterMesh.Vertices).ToArray();

            _mesh.SetTriangles(meshData.Triangles.ToArray(), 0);
            _mesh.SetTriangles(meshData.WaterMesh.Triangles.Select(val => val + meshData.Vertices.Count).ToArray(), 1);

            _mesh.uv = meshData.UV.Concat(meshData.WaterMesh.UV).ToArray();
            _mesh.SetUVs(1, meshData.UV2.Concat(meshData.WaterMesh.UV2).ToArray());
            _mesh.SetUVs(2, meshData.UV3.Concat(meshData.WaterMesh.UV3).ToArray());
            _mesh.RecalculateNormals();

            _meshCollider.sharedMesh = null;

            var collisionMesh = new Mesh
            {
                vertices = meshData.ColliderVertices.ToArray(),
                triangles = meshData.ColliderTriangles.ToArray()
            };

            collisionMesh.RecalculateNormals();

            _meshCollider.sharedMesh = collisionMesh;
            _meshRenderer.materials[0] = meshData.Material;
        }

        private IEnumerator RenderMeshCoroutine(MeshData meshData)
        {
            _meshRenderer.enabled = false;
            
            _mesh.Clear();
            yield return null;
            
            //2, one for opaque material one for transparent (water)
            _mesh.subMeshCount = 2;
            yield return null;
            
            _mesh.vertices = meshData.Vertices.Concat(meshData.WaterMesh.Vertices).ToArray();
            yield return null;
            
            _mesh.SetTriangles(meshData.Triangles.ToArray(), 0);
            yield return null;
            _mesh.SetTriangles(meshData.WaterMesh.Triangles.Select(val => val + meshData.Vertices.Count).ToArray(), 1);
            yield return null;

            _mesh.uv = meshData.UV.Concat(meshData.WaterMesh.UV).ToArray();
            _mesh.SetUVs(1, meshData.UV2.Concat(meshData.WaterMesh.UV2).ToArray());
            yield return null;
            _mesh.SetUVs(2, meshData.UV3.Concat(meshData.WaterMesh.UV3).ToArray());
            yield return null;

            _mesh.RecalculateNormals();
            yield return null;

            _meshCollider.sharedMesh = null;
            yield return null;

            var collisionMesh = new Mesh
            {
                vertices = meshData.ColliderVertices.ToArray(),
                triangles = meshData.ColliderTriangles.ToArray()
            };
            yield return null;

            collisionMesh.RecalculateNormals();
            yield return null;

            _meshCollider.sharedMesh = collisionMesh;
            yield return null;

            _meshRenderer.materials[0] = meshData.Material;
            
            _meshRenderer.enabled = true;
        }

        public async void GetMeshDataAndUpdate()
        {
            var meshData = await Task.Run(() => Chunk.GetChunkMeshData(ChunkData));
            RenderMesh(meshData);
            // StartCoroutine(RenderMeshCoroutine(meshData));
        }

        public void UpdateChunk(MeshData data)
        {
            // RenderMesh(data);
            StartCoroutine(RenderMeshCoroutine(data));
        }

        #if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (!showGizmo || !Application.isPlaying || !ChunkData.IsValid)
                return;
            
            Gizmos.color = Selection.activeGameObject == gameObject ? new Color (0,1,0,.4f) : new Color(1,0,1,.4f);
            //var size = new Vector3(ChunkData.ChunkSize, ChunkData.ChunkHeight, ChunkData.ChunkSize);
            var size = new Vector3(ChunkData.WorldReference.WorldData.ChunkSizeInWorld, ChunkData.WorldReference.WorldData.ChunkHeightInWorld, ChunkData.WorldReference.WorldData.ChunkSizeInWorld);
            Gizmos.DrawCube(transform.position + size/2, size);
        }
        #endif
    }
}
