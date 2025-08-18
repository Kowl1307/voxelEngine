using System;
using Player;
using TMPro;
using UnityEngine;
using static System.String;

namespace Voxel_Engine
{
    [RequireComponent(typeof(TMP_Text))]
    public class DebugText : MonoBehaviour
    {
        TMP_Text _textComponent;
        private World _world;
        private Transform _playerTransform;

        private const string _debugText = "Coordinates:\n" +
                                          "World: \tX:{0:f}\tY:{1:f}\tZ:{2:f}\n" +
                                          "Voxel: \tX:{3:f}\tY:{4:f}\tZ:{5:f}\n" +
                                          "Chunk ({6:n0},{7:n0},{8:n0}): \tX:{9:f}\tY:{10:f}\tZ:{11:f}\n";                                  
        //"Looking at: {12:n0},{13:n0},{14:n0}";

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            _world = FindFirstObjectByType<World>();
        }

        private void OnEnable()
        {
            _playerTransform = FindFirstObjectByType<Character>().transform;
        }

        private void LateUpdate()
        {
            var worldPos = _playerTransform.position;
            var voxelPos = WorldDataHelper.GetVoxelPositionFromWorldPosition(_world, _playerTransform.position);

            var chunkData = WorldDataHelper.GetChunkDataFromVoxelCoords(_world, voxelPos);
            var chunkPos = WorldDataHelper.GetChunkPositionFromVoxelCoords(_world, voxelPos) / _world.chunkSizeInWorld;
            var chunkCoords = Chunk.GetChunkCoordinateOfVoxelPosition(chunkData, voxelPos);
            
            _textComponent.text = Format(_debugText, worldPos.x, worldPos.y, worldPos.z,
                voxelPos.x, voxelPos.y, voxelPos.z,
                chunkPos.x, chunkPos.y, chunkPos.z,
                chunkCoords.x, chunkCoords.y, chunkCoords.z);
        }
    }
}
