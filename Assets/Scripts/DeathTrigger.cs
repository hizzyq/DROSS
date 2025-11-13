using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    public PlayerDeathManager deathManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            deathManager.KillPlayer();
        }
    }
}
