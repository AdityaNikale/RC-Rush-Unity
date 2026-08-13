using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace RCRush.UI
{
    /// <summary>
    /// Manages Main Menu interactions, loading race scenes, and displaying saved best lap time.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI bestTimeText;

        private const string BEST_TIME_KEY = "RCRush_BestTime";

        private void Start()
        {
            LoadBestTimeDisplay();
        }

        private void LoadBestTimeDisplay()
        {
            if (bestTimeText == null) return;

            if (PlayerPrefs.HasKey(BEST_TIME_KEY))
            {
                float bestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY);
                int minutes = Mathf.FloorToInt(bestTime / 60f);
                int seconds = Mathf.FloorToInt(bestTime % 60f);
                int milliseconds = Mathf.FloorToInt((bestTime * 100f) % 100f);

                bestTimeText.text = string.Format("BEST TIME: {0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }
            else
            {
                bestTimeText.text = "BEST TIME: --:--.--";
            }
        }

        public void PlayRace()
        {
            Debug.Log("[MainMenu] Loading Race Scene...");
            SceneManager.LoadScene("Race");
        }

        public void QuitGame()
        {
            Debug.Log("[MainMenu] Quitting Game...");
            Application.Quit();
        }
    }
}