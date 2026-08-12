using UnityEngine;

namespace RCRush.Racing
{
    /// <summary>
    /// Tracks individual vehicle progress through checkpoints and laps.
    /// </summary>
    public class CarCheckpointTracker : MonoBehaviour
    {
        [Header("Vehicle Identity")]
        [SerializeField] private bool isPlayer = false;
        public bool IsPlayer
        {
            get => isPlayer;
            set => isPlayer = value;
        }

        [Header("Read Only Progress")]
        public int CurrentCheckpointIndex = 0;
        public int CurrentLap = 1;
        public int TotalCheckpointsPassed = 0;

        [Header("Finish State")]
        [SerializeField] private bool hasFinished = false;
        [SerializeField] private int finishPosition = -1;
        [SerializeField] private float finishTime = 0f;

        public bool HasFinished => hasFinished;
        public int FinishPosition => finishPosition;
        public float FinishTime => finishTime;

        public float LastCheckpointTime = Mathf.NegativeInfinity;

        public System.Action<int> OnLapCompleted;
        public System.Action<int, int> OnCheckpointPassed;
        public System.Action<int> OnCarFinished;

        // Used later by the UI to show the wrong-direction X.
        public System.Action OnWrongDirection;

        private void Awake()
        {
            AutoDetectPlayer();
            ResetProgress();
        }

        private void AutoDetectPlayer()
        {
            // Auto-detect player if not manually flagged in Inspector
            if (!isPlayer)
            {
                if (GetComponent<RCRush.Player.RCVehicleController>() != null ||
                    GetComponent<RCRush.Player.PlayerInputController>() != null ||
                    CompareTag("Player"))
                {
                    isPlayer = true;
                }
            }
        }

        public void SetFinished(int position, float time = 0f)
        {
            if (hasFinished && finishPosition > 0)
                return; // Prevent duplicate finish calls or overwriting already locked position

            hasFinished = true;
            finishPosition = position;
            finishTime = time;
            OnCarFinished?.Invoke(position);
        }

        public void ResetProgress()
        {
            // Player starts after CP0.
            // Therefore CP1 is the first checkpoint required.
            CurrentCheckpointIndex = 0;
            CurrentLap = 1;
            TotalCheckpointsPassed = 0;
            LastCheckpointTime = Mathf.NegativeInfinity;
            hasFinished = false;
            finishPosition = -1;
            finishTime = 0f;
        }
    }
}