using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour
{
    [Header("Sliding Settings")]
    public float slideDistance = 2f;
    public float slideDuration = 1f;
    public Vector3 slideDirection = Vector3.right;
    public Transform pairedDoor;

    public bool calledByPair = false;
    public bool locked = false;

    [HideInInspector]
    public Vector3 closedPosition;
    [HideInInspector]
    public Vector3 openPosition;
    [HideInInspector]
    public bool isOpen = false;
    private bool isMoving = false;

    void Start()
    {
        closedPosition = transform.position;
        slideDirection = slideDirection.normalized;
        openPosition = closedPosition + slideDirection * slideDistance;
    }

    public void ToggleDoor()
    {
        if (!isMoving)
        {
            if (!isOpen)
            {
                if (!locked)
                    OpenDoor();
            }
            else
                CloseDoor();
        }

        if (pairedDoor != null)
        {
            if (!calledByPair)
            {
                SlidingDoor pairedDoorScript = pairedDoor.GetComponent<SlidingDoor>();
                pairedDoorScript.calledByPair = true;
                pairedDoorScript.ToggleDoor();
                if (locked)
                    pairedDoorScript.locked = true;
            }
        }
    }

void OpenDoor()
    {
        StartCoroutine(SlideDoor(transform.position + slideDirection * slideDistance));
        isOpen = true;
    }

void CloseDoor()
    {
        StartCoroutine(SlideDoor(transform.position - slideDirection * slideDistance));
        isOpen = false;
    }

    IEnumerator SlideDoor(Vector3 targetPosition)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        calledByPair = false;
        transform.position = targetPosition;
        isMoving = false;
        
        
    }
}