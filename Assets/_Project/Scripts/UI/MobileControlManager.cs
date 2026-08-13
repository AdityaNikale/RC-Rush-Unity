using UnityEngine;
using UnityEngine.UI;
using RCRush.Player;
using RCRush.PowerUps;

namespace RCRush.UI
{
    /// <summary>
    /// Bridges mobile touch UI buttons to PlayerInputController and PowerUpInventory.
    /// </summary>
    public class MobileControlManager : MonoBehaviour
    {
        [Header("Touch Buttons")]
        [SerializeField] private TouchButtonHandler btnSteerLeft;
        [SerializeField] private TouchButtonHandler btnSteerRight;
        [SerializeField] private TouchButtonHandler btnAccelerate;
        [SerializeField] private TouchButtonHandler btnBrake;
        [SerializeField] private Button btnPowerUp;

        private PlayerInputController playerInput;
        private PowerUpInventory playerPowerUp;

        private void Start()
        {
            PlayerInputController foundInput = FindObjectOfType<PlayerInputController>();
            if (foundInput != null)
            {
                playerInput = foundInput;
                playerPowerUp = foundInput.GetComponent<PowerUpInventory>();
            }

            if (btnPowerUp != null)
            {
                btnPowerUp.onClick.AddListener(OnPowerUpClicked);
            }
        }

        private void Update()
        {
            if (playerInput == null) return;

            // Pass button press states to PlayerInputController
            playerInput.SetMobileSteerLeft(btnSteerLeft != null && btnSteerLeft.IsPressed);
            playerInput.SetMobileSteerRight(btnSteerRight != null && btnSteerRight.IsPressed);
            playerInput.SetMobileAccelerate(btnAccelerate != null && btnAccelerate.IsPressed);
            playerInput.SetMobileBrake(btnBrake != null && btnBrake.IsPressed);
        }

        private void OnPowerUpClicked()
        {
            if (playerPowerUp != null)
            {
                playerPowerUp.UsePowerUp();
            }
        }
    }
}