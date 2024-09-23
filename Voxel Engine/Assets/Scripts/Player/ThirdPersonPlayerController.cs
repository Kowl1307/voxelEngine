using System;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(CharacterController), typeof(Rigidbody))]
    public class ThirdPersonPlayerController : MonoBehaviour
    {
        private PlayerInput input;
        [SerializeField]
        private Transform cameraFollowTarget;
        [SerializeField] private GameObject playerModel;
        private Rigidbody rigidbody;

        [SerializeField]
        private float cameraSensitivity = 1f;

        private float yaw, pitch;

        private CharacterController characterController;
        
        [SerializeField]
        private float movementSpeed = 5f;

        [SerializeField] private float sprintMultiplier = 1.6f;

        private Vector3 movement;

        [SerializeField]
        private float jumpHeight = 3f;

        private bool doDebugFly = false;

        [SerializeField] private float flySpeed = 10f;
        [SerializeField] private float flyVerticalSpeed = 5f;
        

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            characterController = GetComponent<CharacterController>();
            rigidbody = GetComponent<Rigidbody>();

            input.OnFly += ToggleFly;
        }

        private void ToggleFly()
        {
            doDebugFly = !doDebugFly;

            rigidbody.useGravity = !doDebugFly;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            if (Camera.main != null) pitch = -Camera.main.transform.localRotation.x;
        }

        private void Update()
        {
            RotateCamera();
            if (doDebugFly)
            {
                DebugFly();
            }
            else
            {
                MovePlayer();
            }
            AnimatePlayer();
        }

        private void DebugFly()
        {
            movement = GetRotatedInputSpeed(flySpeed);
            characterController.Move(movement);
            
            var speedVert = input.RunningPressed ? flyVerticalSpeed * sprintMultiplier : flyVerticalSpeed;
            if(input.IsJumping)
                characterController.Move(Vector3.up * (speedVert * Time.deltaTime));
        }

        private void AnimatePlayer()
        {
            if(input.MovementInput.magnitude >= .1f)
                playerModel.transform.forward = movement;
        }

        private void RotateCamera()
        {
            //This is a very dumb way but i dont wanna think rn
            yaw += input.MousePosition.x * cameraSensitivity;
            yaw %= 360;
            pitch = Mathf.Clamp(pitch + input.MousePosition.y * cameraSensitivity, -90, 90);
            cameraFollowTarget.localRotation = Quaternion.Euler(pitch, yaw, 0);
        }
        
        private void MovePlayer()
        {
            movement = GetRotatedInputSpeed(movementSpeed, false);
            characterController.SimpleMove(movement);

            if(input.IsJumping)
                characterController.Move(Vector3.up * (jumpHeight * Time.deltaTime));
        }

        private Vector3 GetRotatedInput()
        {
            return (Quaternion.Euler(0, yaw, 0) * (input.MovementInput)).normalized;
        }

        private Vector3 GetRotatedInputSpeed(float baseSpeed, bool useDeltaTime = true)
        {
            var speed = input.RunningPressed ? baseSpeed * sprintMultiplier : baseSpeed;
            return GetRotatedInput() * (speed * (useDeltaTime ? Time.deltaTime : 1));
        }
    }
}
