using UnityEngine;

public class InvisibleWallToggle : MonoBehaviour
{
    private BoxCollider wallCollider;
    private void Start()
    {
        wallCollider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.CompareTag("Player"))
        {
            wallCollider.enabled = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.CompareTag("Player"))
        {
            wallCollider.enabled = true;
        }
    }
}
