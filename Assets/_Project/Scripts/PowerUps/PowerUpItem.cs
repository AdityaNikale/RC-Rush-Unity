using System.Collections;
using UnityEngine;

namespace RCRush.PowerUps
{
    public enum PowerUpType
    {
        None,
        SpeedBoost,
        EMP
    }

    /// <summary>
    /// Rotating item pickup box placed on the track.
    /// Respawns automatically after being collected.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class PowerUpItem : MonoBehaviour
    {
        [Header("Respawn Settings")]
        [SerializeField] private float respawnTime = 5f;
        [SerializeField] private float rotationSpeed = 90f;

        private MeshRenderer meshRenderer;
        private BoxCollider boxCollider;
        private bool isAvailable = true;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void Update()
        {
            if (isAvailable)
            {
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isAvailable) return;

            PowerUpInventory inventory = other.GetComponentInParent<PowerUpInventory>();
            if (inventory != null && inventory.CurrentPowerUp == PowerUpType.None)
            {
                // Assign random power-up
                PowerUpType randomType = Random.value > 0.5f ? PowerUpType.SpeedBoost : PowerUpType.EMP;
                inventory.CollectPowerUp(randomType);

                StartCoroutine(CollectAndRespawnRoutine());
            }
        }

        private IEnumerator CollectAndRespawnRoutine()
        {
            isAvailable = false;
            if (meshRenderer) meshRenderer.enabled = false;
            if (boxCollider) boxCollider.enabled = false;

            yield return new WaitForSeconds(respawnTime);

            isAvailable = true;
            if (meshRenderer) meshRenderer.enabled = true;
            if (boxCollider) boxCollider.enabled = true;
        }
    }
}