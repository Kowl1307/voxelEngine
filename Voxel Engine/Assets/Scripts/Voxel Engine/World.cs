using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Voxel_Engine;
using Voxel_Engine.WorldGen;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Voxel_Engine
{
    public class World : MonoBehaviour
    {
        public int chunkSize = 16, chunkHeight = 100;

        public TerrainGenerator terrainGenerator;
        public Vector2Int MapSeedOffset;

        public UnityEvent OnWorldCreated, OnNewChunksGenerated;

        public WorldData WorldData { get; private set; }
        public WorldRenderer WorldRenderer;
        public int ChunkDrawingRange = 8;
        
        public bool IsWorldCreated { get; set; }

        private CancellationTokenSource taskTokenSource = new CancellationTokenSource();

        private void Awake()
        {
            WorldData = new WorldData
            {
                ChunkHeight = this.chunkHeight,
                ChunkSize = chunkSize,
                ChunkDataDictionary = new ConcurrentDictionary<Vector3Int, ChunkData>(),
                ChunkDictionary = new ConcurrentDictionary<Vector3Int, ChunkRenderer>()
            };

            MapSeedOffset = new Vector2Int(new Random().Next(10000), new Random().Next(10000));
        }

        public void OnDisable()
        {
            if(taskTokenSource.Token.CanBeCanceled)
                taskTokenSource.Cancel();
        }

        public async void GenerateWorld()
        {
            await GenerateWorld(Vector3Int.zero);
        }
        
        private async Task GenerateWorld(Vector3Int position)
        {
            terrainGenerator.GenerateBiomePoints(position, ChunkDrawingRange, chunkSize, MapSeedOffset);
            var worldGenerationData = await Task.Run(() => GetPositionThatPlayerSees(position), taskTokenSource.Token);

            //This cant be async because data is on main thread
            //Remove unneeded chunks
            foreach (var pos in worldGenerationData.ChunkPositionsToRemove)
            {
                WorldDataHelper.RemoveChunk(this, pos);
            }
            
            foreach (var pos in worldGenerationData.ChunkDataToRemove)
            {
                WorldDataHelper.RemoveChunkData(this, pos);
            }

            ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;
            try
            {
                dataDictionary = await CalculateWorldChunkData(worldGenerationData.ChunkDataPositionsToCreate);
            }
            catch (Exception e)
            {
                Debug.Log("Task Cancelled " + e);
                return;
            }
            
            //Create data for new chunks
            foreach (var calculatedData in dataDictionary)
            {
                WorldData.ChunkDataDictionary.TryAdd(calculatedData.Key, calculatedData.Value);
            }

            //Add structure blocks after initializing the chunks
            foreach (var chunkData in WorldData.ChunkDataDictionary.Values)
            {
                AddStructureVoxels(chunkData);
            }
            //Load renderers for chunks with generated data in range
            
            var dataToRender =
                WorldData.ChunkDataDictionary.Where(kvp =>
                    worldGenerationData.ChunkPositionsToCreate.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();

            
            ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary;
            try
            {
                meshDataDictionary = await CreateMeshDataAsync(dataToRender);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));
        }

        private void AddStructureVoxels(ChunkData chunkData)
        {
            Parallel.ForEach(chunkData.Structures, (structureData) =>
            {
                foreach(var (pos, voxelType) in structureData.StructureVoxels)
                {
                    Chunk.SetVoxel(chunkData, pos, voxelType);
                }
            });
            /*
            foreach (var structureData in chunkData.Structures)
            {
                foreach(var (pos, voxelType) in structureData.StructureVoxels)
                {
                    Chunk.SetVoxel(chunkData, pos, voxelType);
                }
            }
            */
        }

        private async Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
        {
            var dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

            return await Task.Run(() =>
            {
                foreach (var data in dataToRender)
                {
                    if(taskTokenSource.Token.IsCancellationRequested)
                        taskTokenSource.Token.ThrowIfCancellationRequested();

                    var meshData = Chunk.GetChunkMeshData(data);
                    dictionary.TryAdd(data.WorldPosition, meshData);
                }

                return dictionary;
            }, taskTokenSource.Token);
        }

        private async Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
        {
            var dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

            return await Task.Run(() =>
            {
                foreach (var pos in chunkDataPositionsToCreate)
                {
                    if(taskTokenSource.Token.IsCancellationRequested)
                        taskTokenSource.Token.ThrowIfCancellationRequested();
                    
                    var data = new ChunkData(chunkSize, chunkHeight, this, pos);
                    var newData = terrainGenerator.GenerateChunkData(data, MapSeedOffset);
                    dictionary.TryAdd(pos, newData);
                }
                return dictionary;
            }, taskTokenSource.Token);
            
        }

        private IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshData)
        {
            foreach (var item in meshData)
            {
                //If the data is already gone or this function was called several times and already added the chunk, dont add the chunk
                if (!WorldData.ChunkDataDictionary.ContainsKey(item.Key) || WorldData.ChunkDictionary.ContainsKey(item.Key))
                    continue;
                CreateChunk(WorldData, item.Key, item.Value);
                yield return new WaitForEndOfFrame();
            }

            if (IsWorldCreated) yield break;
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }

        private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData)
        {
            var chunkRenderer = WorldRenderer.RenderChunk(worldData, position, meshData);
            WorldData.ChunkDictionary.TryAdd(position, chunkRenderer);
        }


        private WorldGenerationData GetPositionThatPlayerSees(Vector3Int playerPosition)
        {
            //What needs to exist
            var allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, playerPosition);
            var allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundPlayer(this, playerPosition);

            //Things needed to create (do not exist yet)
            var chunkPositionsToCreate = WorldDataHelper.SelectPositionsToCreate(WorldData, allChunkPositionsNeeded, playerPosition);
            var chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositionsToCreate(WorldData, allChunkDataPositionsNeeded, playerPosition);

            var chunkPositionsToRemove = WorldDataHelper.GetUnneededChunks(WorldData, allChunkPositionsNeeded);
            var chunkDataToRemove = WorldDataHelper.GetUnneededData(WorldData, allChunkDataPositionsNeeded);

            var data = new WorldGenerationData
            {
                ChunkPositionsToCreate = chunkPositionsToCreate,
                ChunkDataPositionsToCreate = chunkDataPositionsToCreate,
                ChunkPositionsToRemove = chunkPositionsToRemove,
                ChunkDataToRemove = chunkDataToRemove
            };
            return data;
        }

        public VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            var pos = Chunk.ChunkPositionFromVoxelCoords(this, worldPositionX, worldPositionY, worldPositionZ);
            ChunkData containerChunk = null;

            WorldData.ChunkDataDictionary.TryGetValue(pos, out containerChunk);

            if (containerChunk == null)
                return VoxelType.Nothing;
            var voxelChunkCoordinates = Chunk.GetVoxelInChunkCoordinates(containerChunk,
                new Vector3Int(worldPositionX, worldPositionY, worldPositionZ));
            return Chunk.GetVoxelFromChunkCoordinates(containerChunk, voxelChunkCoordinates);
        }

        public async void LoadAdditionalChucksRequest(GameObject player)
        {
            await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
            OnNewChunksGenerated?.Invoke();
        }

        public bool SetBlock(RaycastHit hit, VoxelType voxelType)
        {
            var chunk = hit.collider.GetComponent<ChunkRenderer>();
            if (chunk == null) return false;

            var pos = GetVoxelPos(hit);

            WorldDataHelper.SetVoxel(chunk.ChunkData.WorldReference, pos, voxelType);
            chunk.ModifiedByPlayer = true;
            
            //If block is on edge, update neighbour chunk
            if (Chunk.IsOnEdge(chunk.ChunkData, pos))
            {
                var neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
                foreach (var neighbourData in neighbourDataList)
                {
                    var chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.WorldReference, neighbourData.WorldPosition);
                    if (chunkToUpdate != null)
                        chunkToUpdate.UpdateChunk();
                }
            }
            
            chunk.UpdateChunk();
            return true;
        }

        private Vector3Int GetVoxelPos(RaycastHit hit)
        {
            var pos = new Vector3(GetVoxelPosIn(hit.point.x, hit.normal.x), GetVoxelPosIn(hit.point.y, hit.normal.y),
                GetVoxelPosIn(hit.point.z, hit.normal.z));
            return Vector3Int.RoundToInt(pos);
        }

        private float GetVoxelPosIn(float pos, float normal)
        {
            if (Math.Abs(Mathf.Abs(pos % 1) - .5f) < .001f)
            {
                pos -= normal / 2f;
            }

            return pos;
        }
    }

    public struct WorldGenerationData
    {
        public List<Vector3Int> ChunkPositionsToCreate;
        public List<Vector3Int> ChunkDataPositionsToCreate;
        public List<Vector3Int> ChunkPositionsToRemove;
        public List<Vector3Int> ChunkDataToRemove;
    }
}

public struct WorldData
{
    public ConcurrentDictionary<Vector3Int, ChunkData> ChunkDataDictionary;
    public ConcurrentDictionary<Vector3Int, ChunkRenderer> ChunkDictionary;
    public int ChunkSize;
    public int ChunkHeight;
}