using UnityEngine;
using UnityEngine.SceneManagement;
using RCRush.Core;
using UnityEngine.InputSystem;

namespace RCRush.UI
{
    /// <summary>
    /// Manages in-game pausing, freezing timeScale, and navigating pause menu options.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI Panel Reference")]
        [SerializeField] private GameObject pauseOverlayPanel;

        public bool IsPaused { get; private set; } = false;

        private void Start()
        {
            if (pauseOverlayPanel != null)
            {
                pauseOverlayPanel.SetActive(false);
            }
            Time.timeScale = 1f; // Ensure time is un-frozen on scene load
        }

        private void Update()
        {
            // Toggle pause on Escape key on PC or Android Back button
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;

            if (IsPaused)
            {
                Time.timeScale = 0f; // Freeze all physics, AI, and timers
                if (pauseOverlayPanel != null) pauseOverlayPanel.SetActive(true);
                if (RaceManager.Instance != null) RaceManager.Instance.SetState(RaceState.Paused);
                Debug.Log("[PauseMenu] Game Paused");
            }
            else
            {
                ResumeRace();
            }
        }

        public void ResumeRace()
        {
            IsPaused = false;
            Time.timeScale = 1f; // Unfreeze time
            if (pauseOverlayPanel != null) pauseOverlayPanel.SetActive(false);
            if (RaceManager.Instance != null) RaceManager.Instance.SetState(RaceState.Racing);
            Debug.Log("[PauseMenu] Game Resumed");
        }

        public void RestartRace()
        {
            Time.timeScale = 1f; // Always reset time scale before loading scene!
            Debug.Log("[PauseMenu] Restarting Race...");
            SceneManager.LoadScene("Race");
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f; // Always reset time scale before loading scene!
            Debug.Log("[PauseMenu] Returning to Main Menu...");
            SceneManager.LoadScene("MainMenu");
        }
    }
}