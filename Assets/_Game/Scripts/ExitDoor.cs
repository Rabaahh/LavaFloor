using UnityEngine;

namespace DontTouchTheFloor
{
    /// <summary>
    /// Finish trigger for reaching the exit door on the far side of the bedroom.
    /// </summary>
    public class ExitDoor : MonoBehaviour
    {
        [Header("Door Visuals")]
        public Transform doorLightTransform;
        public float bobSpeed = 2.0f;
        public float bobHeight = 0.1f;

        private bool isTriggered = false;
        private Vector3 lightInitialPos;

        private void Start()
        {
            if (doorLightTransform != null)
            {
                lightInitialPos = doorLightTransform.localPosition;
            }
        }

        private void Update()
        {
            if (doorLightTransform != null)
            {
                float newY = lightInitialPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                doorLightTransform.localPosition = new Vector3(lightInitialPos.x, newY, lightInitialPos.z);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleTrigger(other.gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            HandleTrigger(other.gameObject);
        }

        public void TriggerComplete()
        {
            if (isTriggered) return;
            isTriggered = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelComplete();
            }
        }

        private void HandleTrigger(GameObject hitObj)
        {
            if (isTriggered) return;

            var player = hitObj.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                TriggerComplete();
            }
        }

        public void ResetTrigger()
        {
            isTriggered = false;
        }
    }
}
