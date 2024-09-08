using System;
using UnityEngine;

namespace Player
{
    public class PlayerCameraFirstPerson : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 300f;
        [SerializeField] private Transform playerBody;
        [SerializeField] private PlayerInput _playerInput;

        private float verticalRotation = 0f;

        private void Awake()
        {
            _playerInput = GetComponentInParent<PlayerInput>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            var mouseX = _playerInput.MousePosition.x * sensitivity * Time.deltaTime;
            var mouseY = _playerInput.MousePosition.y * sensitivity * Time.deltaTime;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

            transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            playerBody.Rotate(Vector3.up * mouseX);

        }
    }
}
