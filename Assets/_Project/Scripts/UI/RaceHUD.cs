using UnityEngine;
using TMPro;
using RCRush.Racing;

namespace RCRush.UI
{
    /// <summary>
    /// Displays real-time Lap count and race status HUD on screen.
    /// </summary>
    public class RaceHUD : MonoBehaviour
    {
        [Header("Target Player Tracker")]
        [SerializeField] private CarCheckpointTracker playerTracker;

        [Header("Race Reference")]
        [SerializeField] private CheckpointManager checkpointManager;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI lapText;

        private int TotalLaps => checkpointManager != null ? checkpointManager.TotalLaps : 1;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted -= OnLapCompleted;
                playerTracker.OnLapCompleted += OnLapCompleted;
            }

            UpdateLapDisplay(playerTracker != null ? Mathf.Max(0, playerTracker.CurrentLap - 1) : 0);
        }

        private void OnDisable()
        {
            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted -= OnLapCompleted;
            }
        }

        private void ResolveReferences()
        {
            if (playerTracker == null)
            {
                playerTracker = FindObjectOfType<CarCheckpointTracker>();
            }

            if (checkpointManager == null)
            {
                checkpointManager = FindObjectOfType<CheckpointManager>();
            }

            if (playerTracker == null)
            {
                Debug.LogWarning("[RaceHUD] No CarCheckpointTracker found in the scene.");
            }

            if (checkpointManager == null)
            {
                Debug.LogWarning("[RaceHUD] No CheckpointManager found in the scene.");
            }
        }

        private void OnLapCompleted(int newLap)
        {
            UpdateLapDisplay(newLap);
        }

        public void UpdateLapDisplay(int currentLap)
        {
            if (lapText == null) return;

            int displayLap = Mathf.Clamp(currentLap, 0, TotalLaps);

            if (displayLap < TotalLaps)
            {
                lapText.text = $"LAP {displayLap} / {TotalLaps}";
            }
            else if (displayLap == TotalLaps)
            {
                lapText.text = $"LAP {displayLap} / {TotalLaps}";
            }
            else
            {
                lapText.text = "FINISH!";
            }
        }
    }
}