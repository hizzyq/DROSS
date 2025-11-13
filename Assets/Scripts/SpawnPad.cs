using UnityEditor.UI;
using UnityEngine;

public class SpawnPad : MonoBehaviour
{
    public PlayerDeathManager deathManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            deathManager.currentCheckpoint = transform;
        }
    }
}
