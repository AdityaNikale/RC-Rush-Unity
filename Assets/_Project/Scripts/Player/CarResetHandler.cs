using UnityEngine;
using UnityEngine.InputSystem;

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

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (mobileResetButton != null)
            {
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
        /// Flips the car right-side up, zeroes out physics momentum, and lifts slightly above ground.
        /// </summary>
        public void ResetCarUpright()
        {
            // Re-align upright while keeping current forward facing heading
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            transform.position += Vector3.up * 0.8f;

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
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