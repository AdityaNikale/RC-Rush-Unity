using UnityEngine;

namespace RCRush.AI
{
    /// <summary>
    /// Individual waypoint node along the AI racing line.
    /// </summary>
    public class Waypoint : MonoBehaviour
    {
        [Header("Waypoint Settings")]
        [Tooltip("Radius around waypoint to trigger reaching it")]
        public float reachRadius = 4f;

        [Tooltip("Recommended speed multiplier for this section (e.g. 1.0 for straight, 0.6 for sharp turn)")]
        [Range(0.2f, 1f)]
        public float speedMultiplier = 1f;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, reachRadius);
        }
    }
}