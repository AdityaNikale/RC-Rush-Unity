using System.Collections;
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

        [Header("Timer (Read Only)")]
        [SerializeField] private float currentRaceTime = 0f;
        public float CurrentRaceTime => currentRaceTime;

        // Events for UI and controllers
        public System.Action<RaceState> OnStateChanged;
        public System.Action<string> OnCountdownUpdated; // "3", "2", "1", "GO!", ""
        public System.Action<float> OnTimerUpdated;

        private CarCheckpointTracker playerTracker;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            playerTracker = FindObjectOfType<CarCheckpointTracker>();
            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted += CheckPlayerFinished;
            }

            StartCoroutine(StartRaceSequence());
        }

        private void OnDestroy()
        {
            if (playerTracker != null)
            {
                playerTracker.OnLapCompleted -= CheckPlayerFinished;
            }
        }

        private void Update()
        {
            if (currentState == RaceState.Racing)
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

        private void CheckPlayerFinished(int currentLap)
        {
            if (currentLap >= totalLaps && currentState == RaceState.Racing)
            {
                SetState(RaceState.Finished);
                Debug.Log($"[RaceManager] Race Finished! Total Time: {currentRaceTime:F2}s");
            }
        }
    }
}