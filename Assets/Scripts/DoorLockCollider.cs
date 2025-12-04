using System.Collections;
using UnityEngine;

public class DoorLockCollider : MonoBehaviour
{

    public SlidingDoor slidingDoor;

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.CompareTag("Player"))
        {
            if (slidingDoor != null)
            {
                slidingDoor.locked = true;
                StartCoroutine(TryUntilLocked());
            }
        }
    }

    IEnumerator TryUntilLocked()
    {
        while (slidingDoor.isOpen)
        {
            slidingDoor.ToggleDoor();
            yield return new WaitForSeconds(0.25f);
        }
        Destroy(gameObject);
    }
}
