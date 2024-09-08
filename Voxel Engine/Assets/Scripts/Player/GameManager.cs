using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using Voxel_Engine;

namespace Player
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        private GameObject player;
        private Vector3Int currentPlayerChunkPosition;
        private Vector3Int currentChunkCenter = Vector3Int.zero;

        public World world;

        [SerializeField] private float detectionTime = .1f;

        [SerializeField] private CinemachineVirtualCamera cameraVm;

        public void SpawnPlayer()
        {
            if (player != null)
                return;

            var raycastStartPosition = new Vector3Int(world.chunkSize / 2, world.chunkHeight + 1, world.chunkSize / 2);
            RaycastHit hit;
            if (!Physics.Raycast(raycastStartPosition, Vector3.down, out hit, world.chunkHeight + 5)) return;
            player = Instantiate(playerPrefab, hit.point + Vector3.up * .5f, Quaternion.identity);
            cameraVm.Follow = player.transform.GetChild(0);
            StartCheckingMap();
        }

        private void Start()
        {
            FindObjectOfType<World>().GenerateWorld();
        }

        public void StartCheckingMap()
        {
            SetCurrentChunkCoordinates();
            StopAllCoroutines();
            StartCoroutine(CheckIfShouldLoadNextPosition());
        }

        private IEnumerator CheckIfShouldLoadNextPosition()
        {
            yield return new WaitForSeconds(detectionTime);
            if (Math.Abs(currentChunkCenter.x - player.transform.position.x) > world.chunkSize ||
                Math.Abs(currentChunkCenter.z - player.transform.position.z) > world.chunkSize ||
                Math.Abs(currentPlayerChunkPosition.y - player.transform.position.y) > world.chunkHeight)
            {
                world.LoadAdditionalChucksRequest(player);
            }
            else
            {
                StartCoroutine(CheckIfShouldLoadNextPosition());
            }
        }

        private void SetCurrentChunkCoordinates()
        {
            currentPlayerChunkPosition =
                WorldDataHelper.ChunkPositionFromVoxelCoords(world, Vector3Int.RoundToInt(player.transform.position));
            currentChunkCenter.x = currentPlayerChunkPosition.x + world.chunkSize / 2;
            currentChunkCenter.z = currentPlayerChunkPosition.z + world.chunkSize / 2;
        }
    }
}
