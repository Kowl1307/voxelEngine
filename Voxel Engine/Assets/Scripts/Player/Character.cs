using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Voxel_Engine;

namespace Player
{
    public class Character : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;
        [SerializeField]
        private PlayerInput playerInput;
        [SerializeField]
        private PlayerMovement _playerMovement;

        public float interactionRayLength = 5;

        public LayerMask groundMask;

        public bool fly = false;

        public Animator animator;

        bool isWaiting = false;

        public World world;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            playerInput = GetComponent<PlayerInput>();
            _playerMovement = GetComponent<PlayerMovement>();
            world = FindFirstObjectByType<World>();
        }

        private void Start()
        {
            playerInput.OnMouseClick += HandleMouseClick;
            playerInput.OnFly += HandleFlyClick;
        }

        private void HandleFlyClick()
        {
            fly = !fly;
        }

        void Update()
        {
            if (fly)
            {
                //animator.SetFloat("speed", 0);
                //animator.SetBool("isGrounded", false);
                //animator.ResetTrigger("jump");
                _playerMovement.Fly(playerInput.MovementInput, playerInput.IsJumping, playerInput.RunningPressed);

            }
            else
            {
                //animator.SetBool("isGrounded", _playerMovement.IsGrounded);
                if (_playerMovement.IsGrounded && playerInput.IsJumping && isWaiting == false)
                {
                    //animator.SetTrigger("jump");
                    isWaiting = true;
                    StopAllCoroutines();
                    StartCoroutine(ResetWaiting());
                }
                //animator.SetFloat("speed", playerInput.MovementInput.magnitude);
                _playerMovement.HandleGravity(playerInput.IsJumping);
                _playerMovement.Walk(playerInput.MovementInput, playerInput.RunningPressed);


            }

        }
        IEnumerator ResetWaiting()
        {
            yield return new WaitForSeconds(0.1f);
            //animator.ResetTrigger("jump");
            isWaiting = false;
        }

        private void HandleMouseClick()
        {
            var playerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(playerRay, out hit, interactionRayLength, groundMask))
            {
                ModifyTerrain(hit);
            }
        }

        private void ModifyTerrain(RaycastHit hit)
        {
            world.SetBlock(hit, VoxelType.Air);
        }
    }
}
