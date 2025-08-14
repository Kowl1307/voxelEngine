using System;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations
{
    public class GrassGenerator : DecorationGenerator
    {
        [SerializeField] private List<VoxelType> allowedGroundTypes = new();
        [Range(0,1)] [SerializeField] private float grassProbability = .1f;
        
        
        [SerializeField] private GameObject grassPrefab;
        
        private static ObjectPool<GameObject> _grassPool;

        private void Awake()
        {
            if (_grassPool == null)
                _grassPool = new ObjectPool<GameObject>(grassPrefab);
        }

        public override void Handle(ChunkData chunkData)
        {
            if (chunkData.ChunkPositionInVoxel.y+chunkData.ChunkHeightInVoxel < 0) return;
            
            UnityMainThreadDispatcher.Instance().Enqueue(() => _grassPool.FillTo(chunkData.ChunkSizeInVoxel*chunkData.ChunkSizeInVoxel));
            
            var randomSeed = (uint)chunkData.ChunkPositionInVoxel.sqrMagnitude;
            if (randomSeed == 0) randomSeed = 40; // Avoid 0-seed
            var random = new Random(randomSeed);
            for (var x = 0; x < chunkData.ChunkSizeInVoxel; x++)
            {
                for (var z = 0; z < chunkData.ChunkSizeInVoxel; z++)
                {
                    if (!(random.NextFloat() < grassProbability)) continue;
                    var positionInChunk = new Vector3Int(x, chunkData.HeightMap[x, z], z);

                    if (!allowedGroundTypes.Contains(Chunk.GetVoxelTypeAt(chunkData, positionInChunk)))
                        return;
                    
                    var voxelPosition = chunkData.ChunkPositionInVoxel + positionInChunk + Vector3Int.up;
                    
                    var position =
                        WorldDataHelper.GetWorldPositionFromVoxelPosition(chunkData.WorldReference, voxelPosition) + Vector3.down * chunkData.WorldReference.WorldData.VoxelScaling.y/2;
                    
                    var instantiatedObject = UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
                    {
                        var instantiatedObject = _grassPool.GetObject();
                        instantiatedObject.transform.position = position;
                        instantiatedObject.transform.rotation = Quaternion.identity;
                        instantiatedObject.transform.localScale = chunkData.WorldReference.WorldData.VoxelScaling;
                        instantiatedObject.isStatic = true;
                        return instantiatedObject;
                    }).Result;
                    // var instantiatedObject = InstantiateGameObjectOnMainThread(grassPrefab, position, new Quaternion());

                    chunkData.ChunkDecorations.Add(instantiatedObject);
                }
            }
        }
    }
}