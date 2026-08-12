using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RCRush.Racing;

namespace RCRush.Core
{
    public enum RaceState
    {
        Waiting,
        Countdown,
        Racing,
        Paused,
        Finished
    }

    /// <summary>
    /// Central manager for race states, countdown sequence, timer, and finish detection.
    /// Handles the 3-second delay after player finish before triggering the Results UI event.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private RaceState currentState = RaceState.Waiting;
        public RaceState CurrentState => currentState;

        [Header("Race Settings")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private int totalLaps = 3;
        [SerializeField] private float resultsUIDelay = 3f;

        [Header("Timer (Read Only)")]
        [SerializeField] private float currentRaceTime = 0f;
        public float CurrentRaceTime => currentRaceTime;

        private bool isPlayerFinished = false;
        public bool IsPlayerFinished => isPlayerFinished;

        private Coroutine resultsDelayCoroutine = null;

        // Events for UI and controllers
        public System.Action<RaceState> OnStateChanged;
        public System.Action<string> OnCountdownUpdated; // "3", "2", "1", "GO!", ""
        public System.Action<float> OnTimerUpdated;

        // Event for Future Results UI (invoked after 3-second delay)
        public System.Action<List<RaceResultData>> OnPlayerRaceFinished;
        public System.Action<List<RaceResultData>> OnRaceResultsReady;

        [Header("Tracked Player Reference")]
        [SerializeField] private CarCheckpointTracker playerTracker;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            ResolvePlayerTracker();

            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted -= CheckPlayerLap;
                playerTracker.OnLapCompleted += CheckPlayerLap;
            }

            if (RacePositionManager.Instance != null)
            {
                RacePositionManager.Instance.OnPlayerFinished -= HandlePlayerFinished;
                RacePositionManager.Instance.OnPlayerFinished += HandlePlayerFinished;
                RacePositionManager.Instance.OnAllCarsFinished -= OnAllCarsCompleted;
                RacePositionManager.Instance.OnAllCarsFinished += OnAllCarsCompleted;
            }

            StartCoroutine(StartRaceSequence());
        }

        private void ResolvePlayerTracker()
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
        }

        private void OnDestroy()
        {
            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted -= CheckPlayerLap;
            }

            if (RacePositionManager.Instance != null)
            {
                RacePositionManager.Instance.OnPlayerFinished -= HandlePlayerFinished;
                RacePositionManager.Instance.OnAllCarsFinished -= OnAllCarsCompleted;
            }
        }

        private void Update()
        {
            // Only tick race timer while actively racing and before the player finishes
            if (currentState == RaceState.Racing && !isPlayerFinished)
            {
                currentRaceTime += Time.deltaTime;
                OnTimerUpdated?.Invoke(currentRaceTime);
            }
        }

        private IEnumerator StartRaceSequence()
        {
            SetState(RaceState.Countdown);

            int seconds = Mathf.CeilToInt(countdownDuration);
            for (int i = seconds; i > 0; i--)
            {
                OnCountdownUpdated?.Invoke(i.ToString());
                yield return new WaitForSeconds(1f);
            }

            OnCountdownUpdated?.Invoke("GO!");
            SetState(RaceState.Racing);

            yield return new WaitForSeconds(1f);
            OnCountdownUpdated?.Invoke(""); // Clear countdown text
        }

        public void SetState(RaceState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"[RaceManager] State changed to: {currentState}");
        }

        private void CheckPlayerLap(int currentLap)
        {
            if (currentLap >= totalLaps && !isPlayerFinished)
            {
                HandlePlayerFinished();
            }
        }

        /// <summary>
        /// Initiates the finish sequence when the player completes the race.
        /// Standings are immediately locked, the timer stops, and a 3-second delay begins before triggering Results UI.
        /// </summary>
        public void HandlePlayerFinished()
        {
            if (isPlayerFinished) return;

            isPlayerFinished = true;
            Debug.Log($"[RaceManager] Player Finished! Lap completed at {currentRaceTime:F2}s. Freezing standings and starting {resultsUIDelay}s delay...");

            // Ensure standings are locked immediately at this exact moment
            if (RacePositionManager.Instance != null && !RacePositionManager.Instance.IsStandingsLocked)
            {
                RacePositionManager.Instance.CaptureAndLockFinalStandings();
            }

            if (resultsDelayCoroutine != null)
            {
                StopCoroutine(resultsDelayCoroutine);
            }
            resultsDelayCoroutine = StartCoroutine(PlayerFinishDelaySequenceRoutine());
        }

        private IEnumerator PlayerFinishDelaySequenceRoutine()
        {
            yield return new WaitForSeconds(resultsUIDelay);

            SetState(RaceState.Finished);

            var finalResults = RacePositionManager.Instance != null
                ? RacePositionManager.Instance.GetFinalResults()
                : new List<RaceResultData>();

            Debug.Log($"[RaceManager] {resultsUIDelay}s post-finish delay complete. Triggering Results UI events with {finalResults.Count} racers.");
            OnPlayerRaceFinished?.Invoke(finalResults);
            OnRaceResultsReady?.Invoke(finalResults);

            resultsDelayCoroutine = null;
        }

        private void OnAllCarsCompleted()
        {
            if (!isPlayerFinished)
            {
                HandlePlayerFinished();
            }
        }

        /// <summary>
        /// Restarts the race cleanly, resetting all car positions, lap counters, and countdown sequence.
        /// </summary>
        public void RestartRace()
        {
            if (resultsDelayCoroutine != null)
            {
                StopCoroutine(resultsDelayCoroutine);
                resultsDelayCoroutine = null;
            }

            isPlayerFinished = false;
            currentRaceTime = 0f;

            if (RacePositionManager.Instance != null)
            {
                RacePositionManager.Instance.ResetPositions();
            }

            StopAllCoroutines();
            StartCoroutine(StartRaceSequence());
        }
    }
}