using UnityEngine;
using UnityEngine.InputSystem;

namespace DontTouchTheFloor
{
    /// <summary>
    /// Coordinates game state: safe respawn, exit triggers, and level completion.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene References")]
        public PlayerController player;
        public Transform spawnPoint;
        public ExitDoor exitDoor;

        [Header("State Tracking")]
        public bool isLevelComplete = false;
        public int fallCount = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartLevel();
            }
        }

        public void OnPlayerTouchedLava(PlayerController targetPlayer)
        {
            if (isLevelComplete) return;

            fallCount++;
            Vector3 respawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0f, 1.2f, -8.5f);
            targetPlayer.Respawn(respawnPos);
        }

        public void OnLevelComplete()
        {
            if (isLevelComplete) return;

            isLevelComplete = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartLevel()
        {
            isLevelComplete = false;

            if (exitDoor != null)
            {
                exitDoor.ResetTrigger();
            }

            if (player != null)
            {
                Vector3 respawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0f, 1.2f, -8.5f);
                player.Respawn(respawnPos);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
