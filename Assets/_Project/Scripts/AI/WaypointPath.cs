using System.Collections.Generic;
using UnityEngine;

namespace RCRush.AI
{
    /// <summary>
    /// Holds the list of track waypoints and draws cyan visual path lines in Scene View.
    /// </summary>
    public class WaypointPath : MonoBehaviour
    {
        [Header("Path Setup")]
        [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();

        public List<Waypoint> Waypoints => waypoints;
        public int Count => waypoints.Count;

        private void Awake()
        {
            if (waypoints.Count == 0)
            {
                waypoints.AddRange(GetComponentsInChildren<Waypoint>());
            }
        }

        public Waypoint GetWaypoint(int index)
        {
            if (waypoints.Count == 0) return null;
            return waypoints[index % waypoints.Count];
        }

        private void OnDrawGizmos()
        {
            Waypoint[] nodes = GetComponentsInChildren<Waypoint>();
            if (nodes.Length < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < nodes.Length; i++)
            {
                Vector3 current = nodes[i].transform.position;
                Vector3 next = nodes[(i + 1) % nodes.Length].transform.position;
                Gizmos.DrawLine(current, next);
            }
        }
    }
}