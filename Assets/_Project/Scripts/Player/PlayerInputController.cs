using UnityEngine;
using UnityEngine.InputSystem;

namespace RCRush.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [Header("Input Values (Read Only)")]
        [SerializeField] private float accelerateInput;
        [SerializeField] private float brakeReverseInput;
        [SerializeField] private float steerInput;
        [SerializeField] private bool isBoostPressed;
        [SerializeField] private bool isPowerUpPressed;
        [SerializeField] private bool resetVehiclePressed;

        private bool mobileAccelerate;
        private bool mobileBrake;
        private bool mobileSteerLeft;
        private bool mobileSteerRight;
        private bool mobileResetVehicle;

        public float AccelerateInput => accelerateInput;
        public float BrakeReverseInput => brakeReverseInput;
        public float SteerInput => steerInput;
        public bool IsBoostPressed => isBoostPressed;
        public bool IsPowerUpPressed => isPowerUpPressed;
        public bool ResetVehiclePressed => resetVehiclePressed;

        private void Update()
        {
            ReadKeyboardInputs();
            CombineInputs();
        }

        private void ReadKeyboardInputs()
        {
            accelerateInput = 0f;
            brakeReverseInput = 0f;
            steerInput = 0f;
            isBoostPressed = false;
            isPowerUpPressed = false;
            resetVehiclePressed = false;

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return;

            // Acceleration
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                accelerateInput = 1f;

            // Brake / Reverse
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                brakeReverseInput = 1f;

            // Steering
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                steerInput -= 1f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                steerInput += 1f;

            steerInput = Mathf.Clamp(steerInput, -1f, 1f);

            // Boost
            isBoostPressed =
                keyboard.spaceKey.isPressed ||
                keyboard.leftShiftKey.isPressed;

            // Power-up
            isPowerUpPressed =
                keyboard.eKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame;

            // Reset / Recover vehicle
            resetVehiclePressed = keyboard.rKey.wasPressedThisFrame;
        }

        private void CombineInputs()
        {
            if (mobileAccelerate)
                accelerateInput = 1f;

            if (mobileBrake)
                brakeReverseInput = 1f;

            float mobileSteer = 0f;

            if (mobileSteerLeft)
                mobileSteer -= 1f;

            if (mobileSteerRight)
                mobileSteer += 1f;

            if (mobileSteer != 0f)
                steerInput = Mathf.Clamp(mobileSteer, -1f, 1f);

            if (mobileResetVehicle)
            {
                resetVehiclePressed = true;
                mobileResetVehicle = false;
            }
        }

        public void SetMobileAccelerate(bool isPressed)
        {
            mobileAccelerate = isPressed;
        }

        public void SetMobileBrake(bool isPressed)
        {
            mobileBrake = isPressed;
        }

        public void SetMobileSteerLeft(bool isPressed)
        {
            mobileSteerLeft = isPressed;
        }

        public void SetMobileSteerRight(bool isPressed)
        {
            mobileSteerRight = isPressed;
        }

        public void SetMobileBoost(bool isPressed)
        {
            isBoostPressed = isPressed;
        }

        public void TriggerMobilePowerUp()
        {
            isPowerUpPressed = true;
        }

        public void SetMobileResetVehicle(bool isPressed)
        {
            mobileResetVehicle = isPressed;
        }

        public void TriggerMobileResetVehicle()
        {
            mobileResetVehicle = true;
        }
    }
}