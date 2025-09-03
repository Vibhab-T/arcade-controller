using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
       if (other.CompareTag("Player"))
        {
            RaceManager.Instance.CheckpointEntered(checkpointIndex);
        }
    }
}
