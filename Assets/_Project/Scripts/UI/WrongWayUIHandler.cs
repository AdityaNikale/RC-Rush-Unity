using UnityEngine;
using TMPro;
using RCRush.Racing;

namespace RCRush.UI
{
    /// <summary>
    /// Displays and blinks a Red 'WRONG WAY' warning when the vehicle drives backward.
    /// </summary>
    public class WrongWayUIHandler : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private TextMeshProUGUI wrongWayText;

        [Header("Blink Settings")]
        [SerializeField] private float blinkSpeed = 5f;

        private bool isWrongWayActive = false;

        private CarCheckpointTracker playerTracker;
        private CheckpointManager checkpointManager;
        private float correctDirectionTimer = 0f;

        private void Start()
        {
            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (isWrongWayActive && wrongWayText != null)
            {
                // Smooth red blinking effect using alpha ping-pong
                float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                wrongWayText.color = new Color(1f, 0.1f, 0.1f, alpha);

                // Auto-resolve references dynamically
                ResolveReferences();

                if (playerTracker != null && checkpointManager != null)
                {
                    int currentIdx = playerTracker.CurrentCheckpointIndex;
                    int nextIdx = (currentIdx + 1) % checkpointManager.TotalCheckpoints;

                    Checkpoint currentCp = checkpointManager.GetCheckpoint(currentIdx);
                    Checkpoint nextCp = checkpointManager.GetCheckpoint(nextIdx);

                    if (currentCp != null && nextCp != null)
                    {
                        Vector3 trackDir = (nextCp.transform.position - currentCp.transform.position).normalized;
                        Vector3 playerDir = playerTracker.transform.forward;

                        // Check if player is facing the correct direction
                        float dot = Vector3.Dot(playerDir, trackDir);
                        if (dot > 0.3f)
                        {
                            correctDirectionTimer += Time.deltaTime;
                            if (correctDirectionTimer >= 1.0f)
                            {
                                SetWrongWayState(false);
                            }
                        }
                        else
                        {
                            correctDirectionTimer = 0f;
                        }
                    }
                }
            }
            else
            {
                correctDirectionTimer = 0f;
            }
        }

        private void ResolveReferences()
        {
            if (playerTracker == null)
            {
                var playerVehicle = FindObjectOfType<RCRush.Player.RCVehicleController>();
                if (playerVehicle != null)
                {
                    playerTracker = playerVehicle.GetComponent<CarCheckpointTracker>();
                }
            }

            if (playerTracker == null)
            {
                var allTrackers = FindObjectsOfType<CarCheckpointTracker>();
                foreach (var tracker in allTrackers)
                {
                    if (tracker.IsPlayer)
                    {
                        playerTracker = tracker;
                        break;
                    }
                }
            }

            if (checkpointManager == null)
            {
                checkpointManager = FindObjectOfType<CheckpointManager>();
            }
        }

        /// <summary>
        /// Call this method from your existing wrong way script!
        /// Pass 'true' when driving wrong way, 'false' when driving correct direction.
        /// </summary>
        public void SetWrongWayState(bool isWrongWay)
        {
            if (isWrongWayActive == isWrongWay) return;

            isWrongWayActive = isWrongWay;

            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(isWrongWayActive);
            }

            if (!isWrongWayActive)
            {
                correctDirectionTimer = 0f;
            }
        }
    }
}
