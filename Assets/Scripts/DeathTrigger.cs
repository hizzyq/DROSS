using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    //ublic PlayerDeathManager deathManager;
    public Player player;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BodyPlayer"))
        {
            player.PlayerDead();
        }
    }
}
