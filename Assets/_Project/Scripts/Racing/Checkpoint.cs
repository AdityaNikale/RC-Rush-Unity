using UnityEngine;

namespace RCRush.Racing
{
    /// <summary>
    /// Attached to each trigger box on the track. Detects when a car passes through.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Checkpoint : MonoBehaviour
    {
        public int CheckpointIndex { get; private set; }
        private CheckpointManager manager;

        public void Initialize(int index, CheckpointManager managerRef)
        {
            CheckpointIndex = index;
            manager = managerRef;
        }

        private void OnTriggerEnter(Collider other)
        {
            CarCheckpointTracker tracker = other.GetComponentInParent<CarCheckpointTracker>();
            if (tracker != null && manager != null)
            {
                manager.OnCarEnteredCheckpoint(tracker, CheckpointIndex);
            }
        }
    }
}