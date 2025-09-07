using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Voxel_Engine;
using Voxel_Engine.Saving;
using Voxel_Engine.WorldGen;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Voxel_Engine
{
    public class World : MonoBehaviour
    {
        [SerializeField] private int chunkSizeInVoxel = 16;
        [SerializeField] private int chunkHeightInVoxel = 100;
        [SerializeField] private Vector2Int worldSeed;
        [SerializeField] private Vector3 voxelScaling = Vector3.one;
        
        public readonly ParallelOptions WorldParallelOptions = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount-1
        };

        public bool saveOnDisable = true;

        public TerrainGenerator terrainGenerator;

        public UnityEvent OnWorldCreated, OnNewChunksGenerated;

        public WorldData WorldData;
        public WorldRenderer WorldRenderer;
        public int ChunkDrawingRange = 8;
        
        //TODO: This should be somewhere else
        public readonly ConcurrentDictionary<Vector3Int, ChunkSaveData> ChunkSaveCache = new();
        
        public bool IsWorldCreated { get; set; }

        private readonly CancellationTokenSource _taskTokenSource = new CancellationTokenSource();
        
        private struct ChunkToCreate
        {
            public Vector3 WorldPosition;
            public MeshData MeshData;
        }
        private readonly ConcurrentQueue<ChunkToCreate> _chunksToCreate = new();
        private bool _isProcessingChunkMeshData = false;
        
        private void Awake()
        {
            WorldData = new WorldData
            {
                ChunkHeightInVoxel = chunkHeightInVoxel,
                ChunkSizeInVoxel = chunkSizeInVoxel,
                ChunkDataDictionary = new ConcurrentDictionary<Vector3Int, ChunkData>(),
                ChunkDictionary = new ConcurrentDictionary<Vector3Int, ChunkRenderer>(),
                WorldSeed = worldSeed,
                VoxelScaling = voxelScaling
            };

            //MapSeedOffset = new Vector2Int(new Random().Next(10000), new Random().Next(10000));
        }

        private void Update()
        {
            if (_chunksToCreate.IsEmpty) return;

            while (!_chunksToCreate.IsEmpty)
            {
                _chunksToCreate.TryDequeue(out var chunkToCreate);
                CreateChunk(WorldData, chunkToCreate.WorldPosition, chunkToCreate.MeshData);
            }

            if (!_isProcessingChunkMeshData)
            {
                OnWorldCreated?.Invoke();
            }
        }

        public void OnDisable()
        {
            if(_taskTokenSource.Token.CanBeCanceled)
                _taskTokenSource.Cancel();
            
            if(saveOnDisable)
            {
                SaveWorld();
            }
        }

        private void OnEnable()
        {
            if (saveOnDisable)
            {
                LoadWorld();
            }
        }

        public void SaveWorld()
        {
            WorldSaveHelper.SaveWorld(this);
        }

        public void LoadWorld()
        {
            WorldSaveHelper.LoadWorld(this);
        }

        public async void GenerateWorld()
        {
            // Fill the Chunk Pool of the WorldRenderer
            WorldRenderer.FillChunkPool(ChunkDrawingRange);

            await GenerateWorld(Vector3Int.zero);
        }
        
        //TODO: make world position vector3 as it should be..
        private async Task GenerateWorld(Vector3 worldPosition)
        {
            var voxelPosition = WorldDataHelper.GetVoxelPositionFromWorldPosition(this, worldPosition);
            
            print("Starting generating World call");
            var worldGenerationData = await Task.Run(() => GetPositionThatPlayerSees(voxelPosition), _taskTokenSource.Token);
            terrainGenerator.InitBiomeSelector(this, WorldDataHelper.GetWorldPositionFromVoxelPosition(this, voxelPosition));
            
            print("Deleting old Chunks..");
            //This cant be async because data is on main thread
            //Remove unneeded chunks
            foreach (var voxelPos in worldGenerationData.ChunkPositionsToRemove)
            {
                WorldDataHelper.RemoveChunk(this, voxelPos);
            }
            
            foreach (var voxelPos in worldGenerationData.ChunkDataToRemove)
            {
                WorldDataHelper.RemoveChunkData(this, voxelPos);
            }
            
            print("Populating new world data..");
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
            
            // Insert new data into dictionary
            foreach (var calculatedData in dataDictionary)
            {
                WorldData.ChunkDataDictionary.TryAdd(calculatedData.Key, calculatedData.Value);
            }
            
            //Load renderers for chunks with generated data in range
            var dataToRender =
                WorldData.ChunkDataDictionary.Where(kvp =>
                    worldGenerationData.ChunkPositionsToCreate.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();

            print("Creating mesh data..");
            await CreateMeshDataAsyncAddToQueue(dataToRender);
            print("Finished World Generation.");
        }

        private async Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
        {
            var dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
            
            await Task.Run(() => Parallel.ForEach(dataToRender, WorldParallelOptions, data =>
            {
                var meshData = Chunk.GetChunkMeshData(data);
                if (_taskTokenSource.Token.IsCancellationRequested) _taskTokenSource.Token.ThrowIfCancellationRequested();;
                dictionary.TryAdd(data.ChunkPositionInVoxel, meshData);
            }), _taskTokenSource.Token);
            
            return dictionary;
        }

        private async Task CreateMeshDataAsyncAddToQueue(List<ChunkData> dataToRender)
        {
            _isProcessingChunkMeshData = true;
            await Task.Run(
                () => {
                    Parallel.ForEach(dataToRender, WorldParallelOptions, data =>
                    {
                        var meshData = Chunk.GetChunkMeshData(data);
                        _chunksToCreate.Enqueue(new ChunkToCreate()
                            { WorldPosition = data.ChunkPositionInWorld, MeshData = meshData });
                    });
                    _isProcessingChunkMeshData = false;
                }
                , _taskTokenSource.Token);
        }

        private async Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
        {
            var dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

            return await Task.Run(() =>
            {
                Parallel.ForEach(chunkDataPositionsToCreate, WorldParallelOptions, voxelPosition =>
                {
                    if (_taskTokenSource.Token.IsCancellationRequested)
                        _taskTokenSource.Token.ThrowIfCancellationRequested();

                    var newData = CreateAndLoadChunkData(voxelPosition);
                    dictionary.TryAdd(voxelPosition, newData);
                });
                
                return dictionary;
            }, _taskTokenSource.Token);
            
        }

        private ChunkData CreateAndLoadChunkData(Vector3Int voxelPosition)
        {
            var data = new ChunkData(WorldData.ChunkSizeInVoxel, WorldData.ChunkHeightInVoxel, this, WorldDataHelper.GetWorldPositionFromVoxelPosition(this, voxelPosition),
                voxelPosition);
            var newData = terrainGenerator.GenerateChunkData(data, worldSeed);

            if (!ChunkSaveCache.TryGetValue(voxelPosition, out var chunkSaveData)) return newData;
            
            print("Modifying voxels due to saved data");
            newData.SetVoxelsMarkDirty(chunkSaveData.modifiedVoxels);

            return newData;
        }

        private IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshData)
        {
            foreach (var item in meshData)
            {
                //If the data is already gone or this function was called several times and already added the chunk, dont add the chunk
                if (!WorldData.ChunkDataDictionary.ContainsKey(item.Key) || WorldData.ChunkDictionary.ContainsKey(item.Key))
                    continue;
                CreateChunk(WorldData, item.Key, item.Value);
                yield return null;
            }

            if (IsWorldCreated) yield break;
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }

        private void CreateChunk(WorldData worldData, Vector3 worldPosition, MeshData meshData)
        {
            var chunkRenderer = WorldRenderer.RenderChunk(this, worldData, worldPosition, meshData);
            var voxelPosition = WorldDataHelper.GetVoxelPositionFromWorldPosition(this, worldPosition);
            WorldData.ChunkDictionary.TryAdd(voxelPosition, chunkRenderer);
        }


        private WorldGenerationData GetPositionThatPlayerSees(Vector3Int voxelPosition)
        {
            //What needs to exist
            var allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, voxelPosition);
            // var allChunkDataPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, playerPosition);
            var allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundPlayer(this, voxelPosition);

            //Things needed to create (do not exist yet)
            var chunkPositionsToCreate = WorldDataHelper.SelectPositionsToCreate(WorldData, allChunkPositionsNeeded, voxelPosition);
            var chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositionsToCreate(WorldData, allChunkDataPositionsNeeded, voxelPosition);

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

        public VoxelType GetVoxelFromChunkCoordinates(ChunkData chunkData, int chunkPositionX, int chunkPositionY, int chunkPositionZ)
        {
            var voxelCoords = Chunk.GetVoxelCoordsFromChunkCoords(chunkData, chunkPositionX, chunkPositionY, chunkPositionZ);
            var pos = WorldDataHelper.GetChunkPositionFromVoxelCoords(this, voxelCoords);

            WorldData.ChunkDataDictionary.TryGetValue(pos, out var containerChunk);

            if (containerChunk == null)
                return VoxelType.Nothing;
            var voxelChunkCoordinates = Chunk.GetChunkCoordinateOfVoxelPosition(containerChunk,
                voxelCoords);
            return Chunk.GetVoxelFromChunkCoordinates(containerChunk, voxelChunkCoordinates);
        }

        public async void LoadAdditionalChucksRequest(GameObject player)
        {
            await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
            OnNewChunksGenerated?.Invoke();
        }

        public void SetVoxel(RaycastHit hit, VoxelType voxelType)
        {
            var chunk = hit.collider.GetComponent<ChunkRenderer>();
            if (chunk == null) return;

            var pos = GetVoxelPosOfRaycastHit(hit);

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

        public Vector3Int GetVoxelPosOfRaycastHit(RaycastHit hit)
        {
            var hitPos = hit.point;
            hitPos -= Vector3.Scale(hit.normal, voxelScaling / 2);
            return WorldDataHelper.GetVoxelPositionFromWorldPosition(this, hitPos);
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
    public ConcurrentDictionary<Vector3Int, ChunkData> ChunkDataDictionary; // Position in Voxel Space
    public ConcurrentDictionary<Vector3Int, ChunkRenderer> ChunkDictionary; // Position in Voxel Space
    public int ChunkSizeInVoxel;
    public int ChunkHeightInVoxel;
    public Vector2Int WorldSeed;
    public Vector3 VoxelScaling;
    
    public float ChunkSizeInWorld => Mathf.FloorToInt(ChunkSizeInVoxel * VoxelScaling.x);
    public float ChunkHeightInWorld => Mathf.FloorToInt(ChunkHeightInVoxel * VoxelScaling.y);
}