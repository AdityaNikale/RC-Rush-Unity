using UnityEngine;

namespace RCRush.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInputController))]
    public class RCVehicleController : MonoBehaviour
    {
        [System.Serializable]
        public struct WheelData
        {
            public WheelCollider collider;
            public Transform visualMesh;
            public bool isSteerable;
            public bool isMotor;
        }

        [Header("Wheel References")]
        [SerializeField] private WheelData frontLeft;
        [SerializeField] private WheelData frontRight;
        [SerializeField] private WheelData rearLeft;
        [SerializeField] private WheelData rearRight;

        [Header("Engine & Handling")]
        [SerializeField] private float motorTorque = 250f;
        [SerializeField] private float maxBrakeTorque = 1500f;
        [SerializeField] private float maxSteerAngle = 30f;
        [SerializeField] private float topSpeed = 30f; // km/h
        [SerializeField] private float reverseTopSpeed = 12f; // km/h

        [Header("Arcade Stability")]
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, 0f, 0f);

        private const float recoveryHeightOffset = 0.5f;

        private Rigidbody rb;
        private PlayerInputController inputController;

        public float CurrentSpeedKmh => rb != null ? rb.velocity.magnitude * 3.6f : 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            inputController = GetComponent<PlayerInputController>();

            rb.centerOfMass = centerOfMassOffset;
            rb.constraints &= ~(RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ);
        }

        private void FixedUpdate()
        {
            float accel = inputController.AccelerateInput;
            float brake = inputController.BrakeReverseInput;
            float steer = inputController.SteerInput;

            HandleMotorAndBrakes(accel, brake);
            HandleSteering(steer);
            UpdateWheelVisuals();

            if (inputController.ResetVehiclePressed)
            {
                RecoverVehicle();
            }
        }

        private void HandleMotorAndBrakes(float accel, float brake)
        {
            float currentSpeed = CurrentSpeedKmh;
            float motor = 0f;
            float brakeTorque = 0f;

          
            if (accel > 0f)
            {
                // Smoothly reduce available torque as we approach topSpeed
                float speedRatio = Mathf.Clamp01(currentSpeed / topSpeed);
                float torqueFalloff = 1f - Mathf.Pow(speedRatio, 3f); // stays strong, tapers near the top
                motor = accel * motorTorque * torqueFalloff;
                brakeTorque = 0f;
            }
            else if (brake > 0f)
            {
                // If moving forward, apply brakes. If stopped, reverse.
                if (Vector3.Dot(rb.velocity, transform.forward) > 0.5f)
                {
                    brakeTorque = brake * maxBrakeTorque;
                    motor = 0f;
                }
                else
                {
                    if (currentSpeed < reverseTopSpeed)
                    {
                        motor = -brake * (motorTorque * 0.3f);
                    }
                    brakeTorque = 0f;
                }
            }
            else
            {
                // Engine braking / coasting friction
                brakeTorque = 100f;
            }

            ApplyTorqueToWheel(frontLeft, motor, brakeTorque);
            ApplyTorqueToWheel(frontRight, motor, brakeTorque);
            ApplyTorqueToWheel(rearLeft, motor, brakeTorque);
            ApplyTorqueToWheel(rearRight, motor, brakeTorque);
        }

        private void ApplyTorqueToWheel(WheelData wheel, float motor, float brake)
        {
            if (wheel.collider == null) return;
            if (wheel.isMotor) wheel.collider.motorTorque = motor;
            wheel.collider.brakeTorque = brake;
        }

        private void HandleSteering(float steerInput)
        {
            float targetSteerAngle = steerInput * maxSteerAngle;
            if (frontLeft.collider != null && frontLeft.isSteerable) frontLeft.collider.steerAngle = targetSteerAngle;
            if (frontRight.collider != null && frontRight.isSteerable) frontRight.collider.steerAngle = targetSteerAngle;
        }

        private void UpdateWheelVisuals()
        {
            UpdateSingleWheelVisual(frontLeft);
            UpdateSingleWheelVisual(frontRight);
            UpdateSingleWheelVisual(rearLeft);
            UpdateSingleWheelVisual(rearRight);
        }

        private void RecoverVehicle()
        {
            if (rb == null)
                return;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward;

            forward.Normalize();
            Vector3 targetPosition = transform.position + Vector3.up * recoveryHeightOffset;
            Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

        private void UpdateSingleWheelVisual(WheelData wheel)
        {
            if (wheel.collider == null || wheel.visualMesh == null) return;

            Vector3 pos;
            Quaternion rot;
            wheel.collider.GetWorldPose(out pos, out rot);

            wheel.visualMesh.position = pos;
            wheel.visualMesh.rotation = rot * Quaternion.Euler(0, 0, 90); // compensate for cylinder orientation
        }
    }
}
