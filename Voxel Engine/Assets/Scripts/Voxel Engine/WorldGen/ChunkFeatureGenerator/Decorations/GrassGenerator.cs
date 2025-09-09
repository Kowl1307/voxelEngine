using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations
{
    public class GrassGenerator : DecorationGenerator
    {
        [SerializeField] private List<VoxelType> allowedGroundTypes = new();
        [Range(0,1)] [SerializeField] private float grassProbability = .1f;
        
        
        [SerializeField] private GameObject grassPrefab;
        
        private ObjectPool<GameObject> _grassPool;

        private void Awake()
        {
            if (_grassPool != null) return;
            
            grassPrefab.SetActive(false);
            _grassPool = new ObjectPool<GameObject>(grassPrefab);
        }

        public override async void Handle(ChunkData chunkData)
        {
            if (chunkData.ChunkPositionInVoxel.y+chunkData.ChunkHeightInVoxel < 0) return;
            
            if(_grassPool.CurrentAmount() < 10) 
                await UnityMainThreadDispatcher.Instance().EnqueueAsync(() => _grassPool.FillTo(chunkData.ChunkSizeInVoxel*chunkData.ChunkSizeInVoxel));
            
            var randomSeed = (uint)chunkData.ChunkPositionInVoxel.sqrMagnitude;
            if (randomSeed == 0) randomSeed = 40; // Avoid 0-seed
            var random = new Random(randomSeed);
            for (var x = 0; x < chunkData.ChunkSizeInVoxel; x++)
            {
                for (var z = 0; z < chunkData.ChunkSizeInVoxel; z++)
                {
                    var heightInChunk = Chunk.GetChunkCoordinateOfVoxelPosition(chunkData, new Vector3Int(0, chunkData.HeightMap[x,z],0)).y;
                    var positionInChunk = new Vector3Int(x, heightInChunk, z);
                    if (!allowedGroundTypes.Contains(Chunk.GetVoxelTypeAt(chunkData, positionInChunk)))
                        continue;
                    
                    var voxelPosition = Chunk.GetVoxelCoordsFromChunkCoords(chunkData, positionInChunk) + Vector3Int.up;
                    
                    if (!GeneratesInBiome(WorldDataHelper.GetBiomeAt(chunkData.WorldReference, voxelPosition)))
                        continue;
                    
                    if (!(random.NextFloat() < grassProbability)) continue;
                    
                    var position =
                        WorldDataHelper.GetWorldPositionFromVoxelPosition(chunkData.WorldReference, voxelPosition) + Vector3.down * chunkData.WorldReference.WorldData.VoxelScaling.y/2;
                    var rotation = Quaternion.Euler(Vector3.up * (random.NextInt(0,3) * 90));
                    var grassDecoration = await SetupGrassDecoration(position, rotation, chunkData.WorldReference.WorldData.VoxelScaling);
                    chunkData.ChunkDecorations.Add(grassDecoration);
                }

            }
        }

        private void DisposeDecoration(DecorationObject decorationObject)
        {
            StartCoroutine(DisposeCoroutine());
            return;

            IEnumerator DisposeCoroutine()
            {
                decorationObject.gameObject.SetActive(false);
                yield return null;
                _grassPool.ReturnObject(decorationObject.gameObject);    
            }
        }

        private async Task<DecorationObject> SetupGrassDecoration(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var grassDecorationObject = await _grassPool.GetObjectAsync();
            var grassDecoration = await SetupDecorationObject(grassDecorationObject, DisposeDecoration);
            
            await UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                grassDecorationObject.transform.position = position;
                grassDecorationObject.transform.rotation = rotation;
                grassDecorationObject.transform.localScale = scale;
                grassDecorationObject.SetActive(true);
            });

            return grassDecoration;
        }
    }
}