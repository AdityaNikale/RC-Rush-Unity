using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using RCRush.Core;
using RCRush.Racing;

namespace RCRush.UI
{
    /// <summary>
    /// Displays race completion results, handles position ranking, final time, 
    /// and persists new personal records via PlayerPrefs.
    /// </summary>
    public class ResultsScreenController : MonoBehaviour
    {
        [Header("UI Panel Reference")]
        [SerializeField] private GameObject resultsOverlayPanel;

        [Header("UI Text Displays")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI positionResultText;
        [SerializeField] private TextMeshProUGUI finalTimeText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI newRecordNoticeText;

        private const string BEST_TIME_KEY = "RCRush_BestTime";
        private CarCheckpointTracker playerTracker;
        private CanvasGroup panelCanvasGroup;

        private void Start()
        {
            if (resultsOverlayPanel != null)
            {
                // Retrieve or add CanvasGroup to hide panel visually while keeping script/GameObject active
                panelCanvasGroup = resultsOverlayPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = resultsOverlayPanel.AddComponent<CanvasGroup>();
                }
                SetPanelActive(false);
            }

            if (newRecordNoticeText != null)
            {
                newRecordNoticeText.gameObject.SetActive(false);
            }

            // Robust player tracker lookup
            playerTracker = System.Array.Find(FindObjectsOfType<CarCheckpointTracker>(), t => t.IsPlayer);

            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnStateChanged += OnRaceStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnStateChanged -= OnRaceStateChanged;
            }
        }

        private void OnRaceStateChanged(RaceState state)
        {
            if (state == RaceState.Finished)
            {
                StartCoroutine(ShowResultsRoutine());
            }
        }

        private void SetPanelActive(bool active)
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = active ? 1f : 0f;
                panelCanvasGroup.blocksRaycasts = active;
                panelCanvasGroup.interactable = active;
            }
            else if (resultsOverlayPanel != null)
            {
                resultsOverlayPanel.SetActive(active);
            }
        }

        private IEnumerator ShowResultsRoutine()
        {
            yield return new WaitForSeconds(1.2f); // Brief delay after crossing finish line

            SetPanelActive(true);

            // 1. Get Final Position
            int pos = 1;
            int totalCars = 4;
            if (RacePositionManager.Instance != null && playerTracker != null)
            {
                pos = RacePositionManager.Instance.GetCarPosition(playerTracker);
                totalCars = RacePositionManager.Instance.TotalCars;
            }

            if (positionResultText != null)
            {
                positionResultText.text = $"POSITION: {pos} / {totalCars}";
            }

            // 2. Get Final Race Time
            float raceTime = RaceManager.Instance != null ? RaceManager.Instance.CurrentRaceTime : 0f;
            if (finalTimeText != null)
            {
                finalTimeText.text = $"TIME: {FormatTime(raceTime)}";
            }

            // 3. Handle Best Time Saving in PlayerPrefs
            float previousBest = PlayerPrefs.GetFloat(BEST_TIME_KEY, float.MaxValue);
            bool isNewRecord = raceTime < previousBest;

            if (isNewRecord)
            {
                PlayerPrefs.SetFloat(BEST_TIME_KEY, raceTime);
                PlayerPrefs.Save();
                previousBest = raceTime;

                if (newRecordNoticeText != null)
                {
                    newRecordNoticeText.gameObject.SetActive(true);
                    newRecordNoticeText.text = "NEW PERSONAL RECORD!";
                }
            }

            if (bestTimeText != null)
            {
                bestTimeText.text = $"BEST TIME: {FormatTime(previousBest)}";
            }
        }

        private string FormatTime(float timeSeconds)
        {
            if (timeSeconds >= float.MaxValue || timeSeconds <= 0f) return "--:--.--";

            int minutes = Mathf.FloorToInt(timeSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeSeconds % 60f);
            int milliseconds = Mathf.FloorToInt((timeSeconds * 100f) % 100f);

            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }

        public void RetryRace()
        {
            SceneManager.LoadScene("Race");
        }

        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}