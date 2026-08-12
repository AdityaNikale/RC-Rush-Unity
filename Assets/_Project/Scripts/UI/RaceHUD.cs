using UnityEngine;
using TMPro;
using RCRush.Core;
using RCRush.Racing;
using RCRush.PowerUps;

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
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI powerUpText;
        
        private PowerUpInventory playerPowerUpInventory;

        private int TotalLaps => checkpointManager != null ? checkpointManager.TotalLaps : 1;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (playerTracker != null)
            {
                playerPowerUpInventory = playerTracker.GetComponent<PowerUpInventory>();
                playerTracker.OnLapCompleted -= OnLapCompleted;
                playerTracker.OnLapCompleted += OnLapCompleted;
            }

            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnCountdownUpdated -= UpdateCountdownDisplay;
                RaceManager.Instance.OnCountdownUpdated += UpdateCountdownDisplay;
                RaceManager.Instance.OnTimerUpdated -= UpdateTimerDisplay;
                RaceManager.Instance.OnTimerUpdated += UpdateTimerDisplay;
            }

            UpdateLapDisplay(playerTracker != null ? Mathf.Max(0, playerTracker.CurrentLap - 1) : 0);
        }

        private void OnDisable()
        {
            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted -= OnLapCompleted;
            }

            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnCountdownUpdated -= UpdateCountdownDisplay;
                RaceManager.Instance.OnTimerUpdated -= UpdateTimerDisplay;
            }
        }

        private void Update()
        {
            if (powerUpText != null && playerPowerUpInventory != null)
            {
                PowerUpType current = playerPowerUpInventory.CurrentPowerUp;
                powerUpText.text = current == PowerUpType.None ? "POWER-UP: NONE" : $"POWER-UP: {current.ToString().ToUpper()} [E]";
            }
            UpdatePositionDisplay();
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
                if (playerTracker == null && allTrackers.Length > 0)
                {
                    playerTracker = allTrackers[0];
                }
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

        private void UpdatePositionDisplay()
        {
            if (positionText == null || playerTracker == null || RacePositionManager.Instance == null) return;

            int pos = RacePositionManager.Instance.GetCarPosition(playerTracker);
            int total = RacePositionManager.Instance.TotalCars;

            positionText.text = $"POS {pos} / {total}";
        }

        public void UpdateLapDisplay(int currentLap)
        {
            if (lapText == null) return;

            int displayLap = Mathf.Clamp(currentLap, 0, TotalLaps);

            if (displayLap >= TotalLaps)
            {
                lapText.text = "FINISH!";
            }
            else
            {
                lapText.text = $"LAP {displayLap} / {TotalLaps}";
            }
        }

        private void UpdateCountdownDisplay(string text)
        {
            if (countdownText != null)
            {
                countdownText.text = text;
            }
        }

        private void UpdateTimerDisplay(float timeSeconds)
        {
            if (timerText == null)
            {
                return;
            }

            int minutes = Mathf.FloorToInt(timeSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeSeconds % 60f);
            int milliseconds = Mathf.FloorToInt((timeSeconds * 100f) % 100f);

            timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
    }
}