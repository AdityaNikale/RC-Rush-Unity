using System.Collections.Generic;
using UnityEngine;

namespace RCRush.Racing
{
    /// <summary>
    /// Manages track checkpoints, validates the required sequence,
    /// tracks laps, detects wrong direction, and finishes the race.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        [Header("Checkpoints Sequence (In Order)")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        [Header("Race Settings")]
        [SerializeField] private int totalLaps = 3;

        public int TotalCheckpoints => checkpoints.Count;
        public int TotalLaps => totalLaps;

        private bool raceFinished = false;

        private void Awake()
        {
            InitializeCheckpoints();
        }

        private void InitializeCheckpoints()
        {
            if (checkpoints.Count == 0)
            {
                checkpoints.AddRange(
                    GetComponentsInChildren<Checkpoint>()
                );
            }

            for (int i = 0; i < checkpoints.Count; i++)
            {
                checkpoints[i].Initialize(i, this);
            }
        }

        public void OnCarEnteredCheckpoint(
            CarCheckpointTracker carTracker,
            int checkpointIndex)
        {
            // Do nothing after the race has finished.
            if (raceFinished)
                return;

            if (checkpoints.Count == 0)
                return;

            // ---------------------------------------------------------
            // DETERMINE EXPECTED CHECKPOINT
            // ---------------------------------------------------------

            int nextExpectedIndex =
                (carTracker.CurrentCheckpointIndex + 1)
                % checkpoints.Count;

            // ---------------------------------------------------------
            // CHECK SEQUENCE
            // ---------------------------------------------------------

            if (checkpointIndex == carTracker.CurrentCheckpointIndex &&
                Time.time - carTracker.LastCheckpointTime < 0.25f)
            {
                // Ignore duplicate trigger events from the same checkpoint
                // while the car is still inside or just passing through it.
                return;
            }

            if (checkpointIndex != nextExpectedIndex)
            {
                // The player entered a checkpoint that is not the
                // checkpoint expected next.
                Debug.Log(
                    $"[CheckpointManager] {carTracker.name} " +
                    $"is going in the WRONG DIRECTION! " +
                    $"Expected CP{nextExpectedIndex}, " +
                    $"but entered CP{checkpointIndex}."
                );

                // Notify the UI system later.
                carTracker.OnWrongDirection?.Invoke();

                // IMPORTANT:
                // Do not change checkpoint progress.
                // Do not change lap.
                return;
            }

            // ---------------------------------------------------------
            // VALID CHECKPOINT
            // ---------------------------------------------------------

            carTracker.CurrentCheckpointIndex = checkpointIndex;
            carTracker.LastCheckpointTime = Time.time;

            carTracker.TotalCheckpointsPassed++;

            // Notify other systems.
            carTracker.OnCheckpointPassed?.Invoke(
                checkpointIndex,
                carTracker.TotalCheckpointsPassed
            );

            // Immediately log the valid checkpoint.
            Debug.Log(
                $"[CheckpointManager] {carTracker.name} " +
                $"passed Checkpoint {checkpointIndex} / {checkpoints.Count - 1}"
            );

            // ---------------------------------------------------------
            // LAP SEQUENCE
            // ---------------------------------------------------------
            //
            // Required sequence:
            //
            // CP1 → CP2 → CP3 → CP0
            //
            // CP0 is only reached after CP3 because the sequential
            // validation above requires it.
            // ---------------------------------------------------------

            if (checkpointIndex == 0)
            {
                CompleteLap(carTracker);
            }
        }

        private void CompleteLap(CarCheckpointTracker carTracker)
        {
            Debug.Log(
                $"[CheckpointManager] {carTracker.name} " +
                $"completed Lap {carTracker.CurrentLap}!"
            );

            // Notify other systems such as the lap UI.
            carTracker.OnLapCompleted?.Invoke(
                carTracker.CurrentLap
            );

            // ---------------------------------------------------------
            // THREE LAPS = RACE FINISHED
            // ---------------------------------------------------------

            if (carTracker.CurrentLap >= totalLaps)
            {
                raceFinished = true;

                Debug.Log(
                    $"[CheckpointManager] {carTracker.name} WINS!"
                );

                return;
            }

            // Move to the next lap.
            carTracker.CurrentLap++;
        }
    }
}