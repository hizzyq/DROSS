using UnityEngine;

public class DeathCollide : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.CompareTag("Player"))
        {
            
        }
    }
}
