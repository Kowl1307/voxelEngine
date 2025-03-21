using System;
using System.Collections;
using Unity.Cinemachine;
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

        [SerializeField] private Canvas debugUi;

        public World world;

        [SerializeField] private float detectionTime = .1f;

        [SerializeField] private CinemachineCamera cameraVm;

        public void SpawnPlayer()
        {
            if (player != null)
                return;

            var raycastStartPosition = new Vector3Int(world.chunkSizeInWorld / 2, world.chunkHeightInWorld + 1, world.chunkSizeInWorld / 2);
            RaycastHit hit;
            if (!Physics.Raycast(raycastStartPosition, Vector3.down, out hit, world.chunkHeightInVoxel * 2)) return;
            player = Instantiate(playerPrefab, hit.point + Vector3.up * .5f, Quaternion.identity);
            cameraVm = player.GetComponentInChildren<CinemachineCamera>();
            //cameraVm.Follow = player.transform.GetChild(0);
            StartCheckingMap();
        }

        private void Start()
        {
            Debug.Log("Initial World Gen call..");
            FindFirstObjectByType<World>().GenerateWorld();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                debugUi.gameObject.SetActive(!debugUi.gameObject.activeSelf);
            }
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
            if (Math.Abs(currentChunkCenter.x - player.transform.position.x) > world.chunkSizeInWorld ||
                Math.Abs(currentChunkCenter.z - player.transform.position.z) > world.chunkSizeInWorld ||
                Math.Abs(currentPlayerChunkPosition.y - player.transform.position.y) > world.chunkHeightInWorld)
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
            currentPlayerChunkPosition = WorldDataHelper.GetChunkWorldPositionFromWorldCoords(world, Vector3Int.RoundToInt(player.transform.position));
            //currentPlayerChunkPosition =
            //    WorldDataHelper.GetChunkPositionFromVoxelCoords(world, Vector3Int.RoundToInt(player.transform.position));
            currentChunkCenter.x = currentPlayerChunkPosition.x + world.chunkSizeInWorld / 2;
            currentChunkCenter.z = currentPlayerChunkPosition.z + world.chunkSizeInWorld / 2;
        }
    }
}
