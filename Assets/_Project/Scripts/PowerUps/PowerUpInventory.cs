using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using RCRush.Player;
using RCRush.AI;

namespace RCRush.PowerUps
{
    /// <summary>
    /// Attached to vehicles to hold and activate collected power-ups.
    /// </summary>
    public class PowerUpInventory : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private PowerUpType currentPowerUp = PowerUpType.None;
        public PowerUpType CurrentPowerUp => currentPowerUp;

        [Header("Speed Boost Settings")]
        [SerializeField] private float boostForce = 3500f;
        [SerializeField] private float boostDuration = 3f;

        [Header("EMP Settings")]
        [SerializeField] private float empRadius = 35f;
        [SerializeField] private float empDuration = 3f;
        [Range(0f, 1f)]
        [SerializeField] private float empSlowdownFactor = 0.3f;

        private Rigidbody rb;
        private RCVehicleController playerVehicle;
        private PlayerInputController playerInput;
        private AIRaceController aiVehicle;

        private Coroutine empDebuffCoroutine;
        private bool isEMPAffected = false;
        private float maxSlowedSpeed = 3f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            playerVehicle = GetComponent<RCVehicleController>();
            playerInput = GetComponent<PlayerInputController>();
            aiVehicle = GetComponent<AIRaceController>();
        }

        private void FixedUpdate()
        {
            if (isEMPAffected && rb != null)
            {
                if (rb.velocity.magnitude > maxSlowedSpeed)
                {
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSlowedSpeed);
                }
            }
        }

        private void Update()
        {
            // Activate Power-Up for Player via PlayerInputController or InputSystem Keyboard
            if (currentPowerUp == PowerUpType.None) return;

            if (playerInput != null && playerInput.IsPowerUpPressed)
            {
                UsePowerUp();
            }
            else if (playerVehicle != null)
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.leftShiftKey.wasPressedThisFrame))
                {
                    UsePowerUp();
                }
            }
        }

        public void CollectPowerUp(PowerUpType type)
        {
            currentPowerUp = type;
            Debug.Log($"[PowerUp] {gameObject.name} collected: {type}");

            // AI automatically uses power-up after 1.5 seconds
            if (aiVehicle != null)
            {
                Invoke(nameof(UsePowerUp), 1.5f);
            }
        }

        public void UsePowerUp()
        {
            if (currentPowerUp == PowerUpType.None) return;

            switch (currentPowerUp)
            {
                case PowerUpType.SpeedBoost:
                    StartCoroutine(ApplySpeedBoostRoutine());
                    break;
                case PowerUpType.EMP:
                    ApplyEMPRoutine();
                    break;
            }

            currentPowerUp = PowerUpType.None;
        }

        private IEnumerator ApplySpeedBoostRoutine()
        {
            Debug.Log($"[PowerUp] {gameObject.name} activated SPEED BOOST!");
            if (rb != null)
            {
                rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(boostDuration);
        }

        private void ApplyEMPRoutine()
        {
            Debug.Log($"[PowerUp] {gameObject.name} activated EMP!");
            // Slow down opponent vehicles nearby for empDuration seconds
            Collider[] hits = Physics.OverlapSphere(transform.position, empRadius);
            foreach (var hit in hits)
            {
                if (hit.transform.root != transform.root)
                {
                    PowerUpInventory targetInventory = hit.GetComponentInParent<PowerUpInventory>();
                    if (targetInventory != null)
                    {
                        targetInventory.ReceiveEMPHit(empDuration, empSlowdownFactor);
                    }
                    else
                    {
                        Rigidbody targetRb = hit.GetComponentInParent<Rigidbody>();
                        if (targetRb != null)
                        {
                            targetRb.velocity *= empSlowdownFactor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when this vehicle is hit by an opponent's EMP blast.
        /// Slows down and limits speed for the specified duration.
        /// </summary>
        public void ReceiveEMPHit(float duration, float factor)
        {
            if (empDebuffCoroutine != null)
            {
                StopCoroutine(empDebuffCoroutine);
            }
            empDebuffCoroutine = StartCoroutine(EMPSlowdownRoutine(duration, factor));
        }

        private IEnumerator EMPSlowdownRoutine(float duration, float factor)
        {
            if (rb != null)
            {
                rb.velocity *= factor;
                maxSlowedSpeed = Mathf.Max(2.5f, rb.velocity.magnitude);
            }
            else
            {
                maxSlowedSpeed = 3f;
            }

            isEMPAffected = true;
            Debug.Log($"[PowerUp] {gameObject.name} is slowed down by EMP for {duration} seconds!");

            yield return new WaitForSeconds(duration);

            isEMPAffected = false;
            empDebuffCoroutine = null;
            Debug.Log($"[PowerUp] {gameObject.name} recovered from EMP effect.");
        }
    }
}