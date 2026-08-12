using UnityEngine;
using RCRush.Core;

namespace RCRush.AI
{
    /// <summary>
    /// Lightweight arcade AI controller with waypoint steering, speed control, 
    /// wrong-direction recovery, and anti-stuck collision handling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AIRaceController : MonoBehaviour
    {
        [Header("Path Reference")]
        [SerializeField] private WaypointPath waypointPath;

        [Header("Driving Attributes")]
        [SerializeField] private float topSpeed = 14f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float turnSpeed = 120f;

        [Header("AI Personality / Speed Variation")]
        [Tooltip("Speed multiplier (e.g. 1.0 = Fast, 0.85 = Medium, 0.75 = Slow)")]
        [SerializeField] private float speedPersonality = 1f;

        [Header("Stuck & Recovery System")]
        [SerializeField] private float stuckSpeedThreshold = 1f;
        [SerializeField] private float stuckTimeLimit = 1.5f;

        // Internal Navigation Variables
        private int currentWaypointIndex = 0;
        private Rigidbody rb;
        private float stuckTimer = 0f;
        private bool isReversing = false;
        private float reverseTimer = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (waypointPath == null)
            {
                waypointPath = FindObjectOfType<WaypointPath>();
            }
        }

        private void FixedUpdate()
        {
            // Block AI driving if race is not active
            if (RaceManager.Instance != null && RaceManager.Instance.CurrentState != RaceState.Racing)
            {
                return;
            }

            if (waypointPath == null || waypointPath.Count == 0) return;

            Waypoint targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
            if (targetWaypoint == null) return;

            // 1. Check distance to target waypoint
            Vector3 toTarget = targetWaypoint.transform.position - transform.position;
            toTarget.y = 0; // Ignore height difference

            if (toTarget.magnitude < targetWaypoint.reachRadius)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypointPath.Count;
                targetWaypoint = waypointPath.GetWaypoint(currentWaypointIndex);
                toTarget = targetWaypoint.transform.position - transform.position;
                toTarget.y = 0;
            }

            // 2. Handle Reverse Recovery Mode if stuck on obstacle
            if (isReversing)
            {
                HandleReverseRecovery(toTarget);
                return;
            }

            // 3. Normal Forward Driving & Steering Math
            Vector3 targetDir = toTarget.normalized;
            float angleToTarget = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);

            // Rotate towards target waypoint
            float maxTurn = turnSpeed * Time.fixedDeltaTime;
            float steer = Mathf.Clamp(angleToTarget, -maxTurn, maxTurn);
            transform.Rotate(0, steer, 0);

            // Adjust target speed based on turn sharpness and AI personality
            float turnFactor = Mathf.Clamp01(1f - (Mathf.Abs(angleToTarget) / 90f));
            float targetSpeed = topSpeed * speedPersonality * targetWaypoint.speedMultiplier * turnFactor;

            // Apply forward velocity
            Vector3 forwardVelocity = transform.forward * targetSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, forwardVelocity, acceleration * Time.fixedDeltaTime);

            // 4. Stuck Detection Check
            CheckStuckCondition();
        }

        private void CheckStuckCondition()
        {
            // If car is trying to drive but moving slower than threshold (e.g. wall/obstacle collision)
            if (rb.velocity.magnitude < stuckSpeedThreshold)
            {
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer >= stuckTimeLimit)
                {
                    // Trigger reverse maneuver
                    isReversing = true;
                    reverseTimer = 1.2f;
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }

        private void HandleReverseRecovery(Vector3 toTarget)
        {
            reverseTimer -= Time.fixedDeltaTime;

            // Reverse backwards and steer away from obstacle
            Vector3 reverseVelocity = -transform.forward * (topSpeed * 0.4f);
            rb.velocity = Vector3.Lerp(rb.velocity, reverseVelocity, acceleration * Time.fixedDeltaTime);

            // Slightly turn car while backing up
            transform.Rotate(0, -45f * Time.fixedDeltaTime, 0);

            if (reverseTimer <= 0f)
            {
                isReversing = false;
                // Re-align rotation directly facing target waypoint
                transform.rotation = Quaternion.LookRotation(toTarget.normalized);
            }
        }
    }
}

