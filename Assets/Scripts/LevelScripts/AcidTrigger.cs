using UnityEngine;

public class AcidTriggerZone : MonoBehaviour
{
    public AcidController acidController;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.GetComponentInParent<Player>() != null)
        {
            acidController.StartRising();
        }
    }
}