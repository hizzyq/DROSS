using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ElevatorCabin : MonoBehaviour
{
    public float moveDuration;
    public SlidingDoor elevatorDoors;
    public Transform point1;
    public Transform point2;

    private Rigidbody rb;
    private float doorCloseDuration;
    private Vector3 point1Position;
    private Vector3 point2Position;
    private Vector3 nextPosition;
    private bool isMoving = false;

    private void Start()
    {
        elevatorDoors.ToggleDoor();
        point1Position = point1.position;
        point2Position = point2.position;
        nextPosition = point2.position;
        doorCloseDuration = elevatorDoors.slideDuration;

        rb = GetComponent<Rigidbody>();
    }

    public void ToggleElevator()
    {
        if (!isMoving)
        {
            StartCoroutine(ChangeFloor(nextPosition));
            if (nextPosition == point2Position)
            {
                nextPosition = point1Position;
            }
            else
            {
                nextPosition = point2Position;
            }
        }
    }

    IEnumerator ChangeFloor(Vector3 targetPosition)
    {
        isMoving = true;
        elevatorDoors.ToggleDoor();
        yield return new WaitForSeconds(doorCloseDuration);
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            t = t * t * (3f - 2f * t);
            rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, t));
            yield return null;
        }
        elevatorDoors.ToggleDoor();
        yield return new WaitForSeconds(doorCloseDuration);
        isMoving = false;
    }
}
