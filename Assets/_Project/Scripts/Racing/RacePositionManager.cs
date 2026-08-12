using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RCRush.AI;

namespace RCRush.Racing
{
    [System.Serializable]
    public struct RaceResultData
    {
        public CarCheckpointTracker tracker;
        public string carName;
        public int position;
        public bool isPlayer;
        public bool hasFinished;
        public float finishTime;

        public RaceResultData(CarCheckpointTracker tracker, int position, bool isPlayer, bool hasFinished, float finishTime)
        {
            this.tracker = tracker;
            this.carName = tracker != null ? tracker.gameObject.name : "Unknown";
            this.position = position;
            this.isPlayer = isPlayer;
            this.hasFinished = hasFinished;
            this.finishTime = finishTime;
        }
    }

    /// <summary>
    /// Evaluates and ranks all vehicles on track based on lap, checkpoint, and distance progress.
    /// Locks finished cars permanently in their finish order while ranking remaining active cars.
    /// Freezes all standings permanently when the player finishes.
    /// </summary>
    public class RacePositionManager : MonoBehaviour
    {
        public static RacePositionManager Instance { get; private set; }

        [Header("Tracked Cars")]
        [SerializeField] private List<CarCheckpointTracker> cars = new List<CarCheckpointTracker>();

        private readonly List<CarCheckpointTracker> finishedCars = new List<CarCheckpointTracker>();
        private readonly List<RaceResultData> finalResults = new List<RaceResultData>();
        private CheckpointManager checkpointManager;

        private bool isStandingsLocked = false;
        public bool IsStandingsLocked => isStandingsLocked;

        public System.Action<CarCheckpointTracker, int> OnCarFinished;
        public System.Action OnAllCarsFinished;
        public System.Action OnPlayerFinished;

        public int TotalCars => cars.Count;
        public bool AreAllCarsFinished => cars.Count > 0 && cars.All(car => car != null && car.HasFinished);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            checkpointManager = FindObjectOfType<CheckpointManager>();
            RefreshCarList();
        }

        public void RefreshCarList()
        {
            if (cars == null)
            {
                cars = new List<CarCheckpointTracker>();
            }

            cars.RemoveAll(car => car == null);
            finishedCars.RemoveAll(car => car == null);

            var trackedCars = FindObjectsOfType<CarCheckpointTracker>();
            foreach (var car in trackedCars)
            {
                if (!cars.Contains(car))
                {
                    cars.Add(car);
                }
            }
        }

        private void Update()
        {
            // If standings are frozen (e.g. after player finishes), positions never change.
            if (isStandingsLocked)
                return;

            RefreshCarList();

            if (cars.Count == 0) return;

            // 1. Finished cars are permanently ranked first according to their FinishPosition.
            // 2. Cars that have not finished are dynamically ranked using race progress calculation.
            // 3. A racing car can never move ahead of an already finished car.
            var finishedList = cars
                .Where(car => car != null && car.HasFinished)
                .OrderBy(car => car.FinishPosition);

            var racingList = cars
                .Where(car => car != null && !car.HasFinished)
                .OrderByDescending(car => GetCarProgressScore(car));

            cars = finishedList.Concat(racingList).ToList();
        }

        /// <summary>
        /// Registers a car as finished and locks its permanent finish position.
        /// If the car is the player, immediately captures and freezes the standings of ALL cars.
        /// </summary>
        public void RegisterCarFinished(CarCheckpointTracker car)
        {
            if (car == null) return;

            // If standings are already locked, mark the car finished without changing the frozen rankings
            if (isStandingsLocked)
            {
                if (!car.HasFinished)
                {
                    // Look up its already assigned frozen rank
                    int frozenRank = GetCarPosition(car);
                    float curTime = Core.RaceManager.Instance != null ? Core.RaceManager.Instance.CurrentRaceTime : 0f;
                    car.SetFinished(frozenRank, curTime);
                }
                return;
            }

            if (car.HasFinished && car.FinishPosition > 0)
                return;

            if (!finishedCars.Contains(car))
            {
                finishedCars.Add(car);
                int position = finishedCars.Count;
                float raceTime = Core.RaceManager.Instance != null ? Core.RaceManager.Instance.CurrentRaceTime : 0f;
                car.SetFinished(position, raceTime);

                // Stop this individual AI using natural physics deceleration
                if (!car.IsPlayer)
                {
                    var ai = car.GetComponent<AIRaceController>();
                    if (ai != null)
                    {
                        ai.StopDriving();
                    }
                }

                Debug.Log($"[RacePositionManager] {car.name} finished in {GetOrdinal(position)} place (Position {position}) at {raceTime:F2}s!");
                OnCarFinished?.Invoke(car, position);
            }

            // If this finished car is the PLAYER, freeze standings for all cars immediately!
            if (car.IsPlayer)
            {
                CaptureAndLockFinalStandings();
                OnPlayerFinished?.Invoke();
            }
            else if (AreAllCarsFinished)
            {
                Debug.Log("[RacePositionManager] All cars have completed the race!");
                OnAllCarsFinished?.Invoke();
            }
        }

        /// <summary>
        /// Captures the current snapshot of standings at this exact moment and locks them permanently.
        /// AI cars can no longer overtake or alter final positions.
        /// Commands all racing AI cars to stop driving inputs and decelerate naturally via physics.
        /// </summary>
        public void CaptureAndLockFinalStandings()
        {
            if (isStandingsLocked) return;

            RefreshCarList();

            // Calculate exact ranking right now
            var finishedList = cars
                .Where(car => car != null && car.HasFinished)
                .OrderBy(car => car.FinishPosition)
                .ToList();

            var racingList = cars
                .Where(car => car != null && !car.HasFinished)
                .OrderByDescending(car => GetCarProgressScore(car))
                .ToList();

            var rankedCars = finishedList.Concat(racingList).ToList();
            cars = rankedCars;

            finalResults.Clear();
            float raceTime = Core.RaceManager.Instance != null ? Core.RaceManager.Instance.CurrentRaceTime : 0f;

            for (int i = 0; i < rankedCars.Count; i++)
            {
                var trackedCar = rankedCars[i];
                int finalPos = i + 1;

                if (!trackedCar.HasFinished)
                {
                    // Lock finish position on the tracker even if it didn't cross the finish line
                    trackedCar.SetFinished(finalPos, raceTime);
                }

                finalResults.Add(new RaceResultData(
                    trackedCar,
                    finalPos,
                    trackedCar.IsPlayer,
                    trackedCar.HasFinished,
                    trackedCar.FinishTime > 0f ? trackedCar.FinishTime : raceTime
                ));

                Debug.Log($"[RacePositionManager] FINAL RESULT -> Pos {finalPos}: {trackedCar.name} (Player: {trackedCar.IsPlayer}, Finished: {trackedCar.HasFinished})");
            }

            isStandingsLocked = true;

            // Stop driving input on all AI vehicles, allowing natural physics momentum and deceleration
            foreach (var car in cars)
            {
                if (car != null && !car.IsPlayer)
                {
                    var ai = car.GetComponent<AIRaceController>();
                    if (ai != null)
                    {
                        ai.StopDriving();
                    }
                }
            }

            Debug.Log("[RacePositionManager] Final standings locked & AI driving inputs stopped!");
        }

        /// <summary>
        /// Returns the permanently locked final results, or live results if the race is still active.
        /// </summary>
        public List<RaceResultData> GetFinalResults()
        {
            if (finalResults.Count > 0)
            {
                return new List<RaceResultData>(finalResults);
            }

            // Fallback to current live standings
            var liveResults = new List<RaceResultData>();
            float raceTime = Core.RaceManager.Instance != null ? Core.RaceManager.Instance.CurrentRaceTime : 0f;
            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i] != null)
                {
                    liveResults.Add(new RaceResultData(
                        cars[i],
                        i + 1,
                        cars[i].IsPlayer,
                        cars[i].HasFinished,
                        cars[i].FinishTime > 0f ? cars[i].FinishTime : raceTime
                    ));
                }
            }
            return liveResults;
        }

        /// <summary>
        /// Calculates a progress score for an active (unfinished) car. Higher score = further along in the race.
        /// </summary>
        private float GetCarProgressScore(CarCheckpointTracker car)
        {
            if (car == null) return 0f;

            float score = (car.CurrentLap * 100000f) + (car.TotalCheckpointsPassed * 1000f);

            // Subtract distance to next checkpoint so closer cars score higher
            if (checkpointManager != null && checkpointManager.TotalCheckpoints > 0)
            {
                int nextCPIndex = (car.CurrentCheckpointIndex + 1) % checkpointManager.TotalCheckpoints;
                Transform nextCPTransform = checkpointManager.transform.GetChild(nextCPIndex);
                if (nextCPTransform != null)
                {
                    float dist = Vector3.Distance(car.transform.position, nextCPTransform.position);
                    score -= dist;
                }
            }

            return score;
        }

        /// <summary>
        /// Returns 1-based position rank for a specific car (e.g. 1st, 2nd, 3rd, 4th).
        /// </summary>
        public int GetCarPosition(CarCheckpointTracker car)
        {
            if (car == null) return cars.Count;

            if (isStandingsLocked && finalResults.Count > 0)
            {
                var match = finalResults.Find(r => r.tracker == car);
                if (match.position > 0)
                    return match.position;
            }

            if (car.HasFinished && car.FinishPosition > 0)
            {
                return car.FinishPosition;
            }

            int index = cars.IndexOf(car);
            return index >= 0 ? index + 1 : cars.Count;
        }

        /// <summary>
        /// Resets all positions, finish order, and car progress cleanly for a new race or restart.
        /// </summary>
        public void ResetPositions()
        {
            isStandingsLocked = false;
            finishedCars.Clear();
            finalResults.Clear();
            RefreshCarList();
            foreach (var car in cars)
            {
                if (car != null)
                {
                    car.ResetProgress();
                    var ai = car.GetComponent<AIRaceController>();
                    if (ai != null)
                    {
                        ai.ResetAI();
                    }
                }
            }
        }

        private string GetOrdinal(int num)
        {
            if (num <= 0) return num.ToString();
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }
            switch (num % 10)
            {
                case 1: return num + "st";
                case 2: return num + "nd";
                case 3: return num + "rd";
                default: return num + "th";
            }
        }
    }
}