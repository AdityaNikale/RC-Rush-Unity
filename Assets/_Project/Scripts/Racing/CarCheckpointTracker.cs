using UnityEngine;

namespace RCRush.Racing
{
    /// <summary>
    /// Tracks individual vehicle progress through checkpoints and laps.
    /// </summary>
    public class CarCheckpointTracker : MonoBehaviour
    {
        [Header("Read Only Progress")]
        public int CurrentCheckpointIndex = 0;
        public int CurrentLap = 1;
        public int TotalCheckpointsPassed = 0;

        public float LastCheckpointTime = Mathf.NegativeInfinity;

        public System.Action<int> OnLapCompleted;
        public System.Action<int, int> OnCheckpointPassed;

        // Used later by the UI to show the wrong-direction X.
        public System.Action OnWrongDirection;

        private void Awake()
        {
            ResetProgress();
            LastCheckpointTime = Mathf.NegativeInfinity;
        }

        public void ResetProgress()
        {
            // Player starts after CP0.
            // Therefore CP1 is the first checkpoint required.
            CurrentCheckpointIndex = 0;

            CurrentLap = 1;
            TotalCheckpointsPassed = 0;
            LastCheckpointTime = Mathf.NegativeInfinity;
        }
    }
}