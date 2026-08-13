using UnityEngine;
using UnityEngine.InputSystem;
using RCRush.UI;

namespace RCRush.Player
{
    /// <summary>
    /// Detects when player car is flipped upside down, handles 'R' key reset on PC,
    /// and dynamically pops up/hides the Mobile Reset UI button.
    /// </summary>
    public class CarResetHandler : MonoBehaviour
    {
        [Header("Mobile UI Button Reference")]
        [SerializeField] private GameObject mobileResetButton;

        [Header("Detection Settings")]
        [Tooltip("Max angle from upright before considered flipped (degrees)")]
        [SerializeField] private float flippedAngleThreshold = 75f;
        [SerializeField] private float checkDelay = 0.8f;

        private Rigidbody rb;
        private float flippedTimer = 0f;
        private bool isFlipped = false;
        private TouchButtonHandler mobileResetTouchHandler;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (mobileResetButton != null)
            {
                // Ensure there is a TouchButtonHandler to reliably capture touches on mobile
                mobileResetTouchHandler = mobileResetButton.GetComponent<TouchButtonHandler>();
                if (mobileResetTouchHandler == null)
                {
                    mobileResetTouchHandler = mobileResetButton.AddComponent<TouchButtonHandler>();
                }
                mobileResetButton.SetActive(false); // Hidden by default
            }
        }

        private void Update()
        {
            // 1. PC Key Input (R key)
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetCarUpright();
                return;
            }

            // 1.5 Mobile Touch Reset Input (via TouchButtonHandler)
            if (mobileResetTouchHandler != null && mobileResetTouchHandler.IsPressed)
            {
                ResetCarUpright();
                return;
            }

            // 2. Flip Detection Logic
            float angleFromUpright = Vector3.Angle(transform.up, Vector3.up);
            if (angleFromUpright > flippedAngleThreshold)
            {
                flippedTimer += Time.deltaTime;
                if (flippedTimer >= checkDelay)
                {
                    isFlipped = true;
                    if (mobileResetButton != null && !mobileResetButton.activeSelf)
                    {
                        mobileResetButton.SetActive(true); // Pop up UI button on mobile!
                    }
                }
            }
            else
            {
                flippedTimer = 0f;
                if (isFlipped)
                {
                    isFlipped = false;
                    if (mobileResetButton != null && mobileResetButton.activeSelf)
                    {
                        mobileResetButton.SetActive(false); // Hide when right-side up
                    }
                }
            }
        }

        /// <summary>
        public void ResetCarUpright()
        {
            var vehicleController = GetComponent<RCVehicleController>();
            if (vehicleController != null)
            {
                vehicleController.RecoverVehicle();
            }
            else
            {
                // Fallback: update Rigidbody directly and sync transforms
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                    rb.position += Vector3.up * 0.8f;
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                    transform.position += Vector3.up * 0.8f;
                }
                Physics.SyncTransforms();
            }

            isFlipped = false;
            flippedTimer = 0f;

            if (mobileResetButton != null)
            {
                mobileResetButton.SetActive(false); // Hide button after reset
            }

            Debug.Log("[CarReset] Player car reset upright!");
        }
    }
}