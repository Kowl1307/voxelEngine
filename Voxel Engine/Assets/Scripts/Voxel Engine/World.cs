using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Voxel_Engine;
using Voxel_Engine.ChunkSelectors;
using Voxel_Engine.Saving;
using Voxel_Engine.WorldGen;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

namespace Voxel_Engine
{
    [RequireComponent(typeof(IChunkSelector))]
    public class World : MonoBehaviour
    {
        [SerializeField] private int chunkSizeInVoxel = 16;
        [SerializeField] private int chunkHeightInVoxel = 100;
        [SerializeField] private Vector2Int worldSeed;
        [SerializeField] private Vector3 voxelScaling = Vector3.one;
        [SerializeField] private int chunkDrawingRange = 8;
        
        public readonly ParallelOptions WorldParallelOptions = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount-1
        };

        public bool saveOnDisable = true;

        public TerrainGenerator terrainGenerator;

        public UnityEvent OnWorldCreated, OnNewChunksGenerated;

        public WorldData WorldData;
        public WorldRenderer WorldRenderer;
        public IChunkSelector ChunkSelector;
        
        //TODO: This should be somewhere else
        public readonly ConcurrentDictionary<Vector3Int, ChunkSaveData> ChunkSaveCache = new();

        private readonly CancellationTokenSource _taskTokenSource = new();
        
        private struct ChunkToCreate
        {
            public Vector3 WorldPosition;
            public MeshData MeshData;
        }
        
        private readonly ConcurrentQueue<ChunkToCreate> _chunksToCreate = new();
        private bool _isProcessingChunkMeshData;
        
        private void Awake()
        {
            WorldData = new WorldData
            {
                ChunkHeightInVoxel = chunkHeightInVoxel,
                ChunkSizeInVoxel = chunkSizeInVoxel,
                ChunkDataDictionary = new ConcurrentDictionary<Vector3Int, ChunkData>(),
                ChunkDictionary = new ConcurrentDictionary<Vector3Int, ChunkRenderer>(),
                WorldSeed = worldSeed,
                VoxelScaling = voxelScaling,
                ChunkDrawingRange = chunkDrawingRange
            };
                
            ChunkSelector = GetComponent<IChunkSelector>();

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
            WorldRenderer.FillChunkPool(chunkDrawingRange);

            await GenerateWorld(Vector3Int.zero);
        }
        
        private async Task GenerateWorld(Vector3 worldPosition)
        {
            var voxelPosition = WorldDataHelper.GetVoxelPositionFromWorldPosition(this, worldPosition);
            
            print("Starting generating World call");
            var worldGenerationData = ChunkSelector.GetWorldGenerationData(WorldData, voxelPosition);
            //var worldGenerationData = await Task.Run(() => GetPositionThatPlayerSees(voxelPosition), _taskTokenSource.Token);
            terrainGenerator.InitBiomeSelector(WorldData, WorldDataHelper.GetWorldPositionFromVoxelPosition(this, voxelPosition));
            
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
            ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary;
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

            var existsSaveData = ChunkSaveCache.TryGetValue(voxelPosition, out var chunkSaveData);
            
            if (!existsSaveData)
            {
                return newData;
            }
            
            newData.SetVoxelsMarkDirty(chunkSaveData.modifiedVoxels);

            return newData;
        }

        private void CreateChunk(WorldData worldData, Vector3 worldPosition, MeshData meshData)
        {
            var chunkRenderer = WorldRenderer.RenderChunk(this, worldData, worldPosition, meshData);
            var voxelPosition = WorldDataHelper.GetVoxelPositionFromWorldPosition(this, worldPosition);
            WorldData.ChunkDictionary.TryAdd(voxelPosition, chunkRenderer);
        }

        public async void LoadAdditionalChucksRequest(GameObject player)
        {
            await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
            OnNewChunksGenerated?.Invoke();
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
        /// <summary>
        /// List of chunks that have to be created (data & render)
        /// </summary>
        public List<Vector3Int> ChunkPositionsToCreate;
        /// <summary>
        /// List of chunk data that needs to be created (no render needed yet)
        /// </summary>
        public List<Vector3Int> ChunkDataPositionsToCreate;
        
        /// <summary>
        /// List of chunks to remove (render only)
        /// </summary>
        public List<Vector3Int> ChunkPositionsToRemove;
        
        /// <summary>
        /// List of chunk data to remove
        /// </summary>
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
    public int ChunkDrawingRange;
    
    public float ChunkSizeInWorld => ChunkSizeInVoxel * VoxelScaling.x;
    public float ChunkHeightInWorld => ChunkHeightInVoxel * VoxelScaling.y;
}