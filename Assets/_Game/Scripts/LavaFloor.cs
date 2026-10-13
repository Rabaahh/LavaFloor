using UnityEngine;

namespace DontTouchTheFloor
{
    /// <summary>
    /// Detects player collision or entry with the lava floor and triggers safe respawn.
    /// Also animates a subtle heat pulse effect on the material if available.
    /// </summary>
    public class LavaFloor : MonoBehaviour
    {
        [Header("Visual Pulse")]
        public bool animateEmission = true;
        public float pulseFrequency = 2.0f;

        private Material lavaMaterial;
        private Color baseEmissionColor = new Color(1.0f, 0.25f, 0.05f);

        private void Start()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                lavaMaterial = renderer.material;
            }
        }

        private void Update()
        {
            if (animateEmission && lavaMaterial != null)
            {
                float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * pulseFrequency);
                Color currentEmission = baseEmissionColor * pulse;
                if (lavaMaterial.HasProperty("_EmissionColor"))
                {
                    lavaMaterial.SetColor("_EmissionColor", currentEmission);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleHit(collision.gameObject);
        }

        private void HandleHit(GameObject hitObj)
        {
            var player = hitObj.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnPlayerTouchedLava(player);
                }
                else
                {
                    player.Respawn(player.GetInitialPosition());
                }
            }
        }
    }
}
