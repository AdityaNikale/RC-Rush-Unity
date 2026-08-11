using UnityEngine;

/// <summary>
/// Re-Volt style chase camera: low, close, smooth follow with slight rotational lag.
/// Attach to the Main Camera and assign the car's transform as "target".
/// </summary>
public class ReVoltChaseCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;          // Car body / root transform

    [Header("Positioning (Re-Volt-like defaults)")]
    [SerializeField] private float distance = 6.26f;      // How far behind the car
    [SerializeField] private float height = 1.97f;        // How high above the car's pivot
    [SerializeField] private float lookAheadHeight = 0.75f; // Slight upward look offset so horizon isn't dead center

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.08f; // Lower = snappier, higher = floatier
    [SerializeField] private float rotationSmoothSpeed = 8f;   // Degrees/sec-ish lag on turning

    [Header("Speed-based FOV kick (optional, Re-Volt-ish boost feel)")]
    [SerializeField] private bool useSpeedFov = true;
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float maxFov = 72f;
    [SerializeField] private float maxSpeedForFov = 25f; // m/s at which FOV maxes out

    [Header("Collision Avoidance")]
    [SerializeField] private bool avoidWalls = true;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.3f;

    private Vector3 _velocity = Vector3.zero;
    private Camera _cam;
    private Rigidbody _targetRb;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (target != null)
            _targetRb = target.GetComponentInParent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // --- Desired position: behind and above the car, in the car's own facing direction ---
        Vector3 desiredPosition = target.position
                                   - target.forward * distance
                                   + Vector3.up * height;

        // --- Wall/obstacle avoidance so camera doesn't clip through geometry ---
        if (avoidWalls)
        {
            Vector3 castOrigin = target.position + Vector3.up * height;
            Vector3 dir = desiredPosition - castOrigin;
            float dist = dir.magnitude;

            if (dist > 0.001f && Physics.SphereCast(castOrigin, collisionRadius, dir.normalized,
                    out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                desiredPosition = castOrigin + dir.normalized * Mathf.Max(hit.distance - 0.1f, 0.5f);
            }
        }

        // --- Smoothly move camera to desired position (SmoothDamp = no overshoot, stable at speed) ---
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            positionSmoothTime
        );

        // --- Look-at point: slightly above the car so we get that Re-Volt "looking down the track" tilt ---
        Vector3 lookTarget = target.position + Vector3.up * lookAheadHeight;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        // Rotational lag/smoothing (car snaps its heading faster than camera turns, like Re-Volt's follow-cam drift)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime)
        );

        // --- Optional: FOV widens with speed for a sense of velocity, matches arcade racer feel ---
        if (useSpeedFov && _cam != null)
        {
            float speed = _targetRb != null ? _targetRb.velocity.magnitude : 0f;
            float t = Mathf.Clamp01(speed / maxSpeedForFov);
            _cam.fieldOfView = Mathf.Lerp(baseFov, maxFov, t);
        }
    }

    // Call this if you swap cars (e.g. respawn/vehicle select)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        _targetRb = target != null ? target.GetComponentInParent<Rigidbody>() : null;
    }
}