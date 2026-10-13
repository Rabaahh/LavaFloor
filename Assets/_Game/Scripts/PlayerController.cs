using UnityEngine;
using UnityEngine.InputSystem;

namespace DontTouchTheFloor
{
    /// <summary>
    /// Beginner-friendly First-Person Player Controller using Unity's Input System package.
    /// Supports WASD and Arrow keys for movement, Mouse for looking, and Space for jumping.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Horizontal movement speed in units/sec")]
        public float moveSpeed = 6.0f;

        [Tooltip("Jump height in units")]
        public float jumpHeight = 1.4f;

        [Tooltip("Downward gravity acceleration")]
        public float gravity = -20.0f;

        [Header("Look Settings")]
        [Tooltip("Mouse sensitivity multiplier")]
        public float mouseSensitivity = 0.15f;

        [Tooltip("Reference to the camera transform to pitch up/down")]
        public Transform cameraTransform;

        [Header("Spawn Reference")]
        public Vector3 initialPosition;

        private CharacterController characterController;
        private Vector3 verticalVelocity = Vector3.zero;
        private float pitch = 0.0f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            initialPosition = transform.position;
        }

        private void Start()
        {
            // Lock cursor for mouse look during gameplay
            LockCursor();
        }

        private void Update()
        {
            HandleCursorToggle();
            HandleLook();
            HandleMovement();
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleCursorToggle()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
            }

            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                // Only re-lock if the level is not completed
                if (GameManager.Instance == null || !GameManager.Instance.isLevelComplete)
                {
                    LockCursor();
                }
            }
        }

        private void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mouseDelta = mouse.delta.ReadValue();
            float yaw = mouseDelta.x * mouseSensitivity;
            float pitchDelta = mouseDelta.y * mouseSensitivity;

            // Horizontal rotation on the player body
            transform.Rotate(Vector3.up * yaw);

            // Vertical pitch clamped on the camera
            pitch -= pitchDelta;
            pitch = Mathf.Clamp(pitch, -80.0f, 80.0f);

            if (cameraTransform != null)
            {
                cameraTransform.localEulerAngles = new Vector3(pitch, 0.0f, 0.0f);
            }
        }

        private void HandleMovement()
        {
            var keyboard = Keyboard.current;
            float moveX = 0.0f;
            float moveZ = 0.0f;
            bool jumpPressed = false;

            if (keyboard != null)
            {
                // WASD + Arrow Keys support
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ += 1.0f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ -= 1.0f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1.0f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1.0f;

                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            }

            Vector3 moveInput = new Vector3(moveX, 0.0f, moveZ);
            if (moveInput.sqrMagnitude > 1.0f)
            {
                moveInput.Normalize();
            }

            // Direction relative to player orientation
            Vector3 worldDirection = transform.TransformDirection(moveInput);
            Vector3 horizontalMove = worldDirection * moveSpeed;

            // Ground check & gravity
            if (characterController.isGrounded)
            {
                if (verticalVelocity.y < 0.0f)
                {
                    // Small grounded stick force
                    verticalVelocity.y = -2.0f;
                }

                if (jumpPressed && (GameManager.Instance == null || !GameManager.Instance.isLevelComplete))
                {
                    // v = sqrt(2 * g * h)
                    verticalVelocity.y = Mathf.Sqrt(2.0f * -gravity * jumpHeight);
                }
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
            }

            Vector3 totalMovement = (horizontalMove + verticalVelocity) * Time.deltaTime;
            characterController.Move(totalMovement);
        }

        /// <summary>
        /// Respawns the player safely at the specified coordinates.
        /// CharacterController must be disabled during teleportation to prevent position overriding.
        /// </summary>
        public void Respawn(Vector3 targetPosition)
        {
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.position = targetPosition;
            verticalVelocity = Vector3.zero;

            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        public Vector3 GetInitialPosition()
        {
            return initialPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckExitTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CheckExitTrigger(other);
        }

        private void CheckExitTrigger(Collider other)
        {
            var exit = other.GetComponentInParent<ExitDoor>();
            if (exit != null)
            {
                exit.TriggerComplete();
            }
        }
    }
}
