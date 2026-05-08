using System.Collections;
using UnityEngine;

public class PhysicalButton : MonoBehaviour
{
    public GameObject[] connectedObjects;
    [SerializeField] public SFXEvent activateSFX;
    public void ActivateConnectedObjects()
    {
        AudioManager.PlayAt(activateSFX, transform.position);
        foreach (GameObject obj in connectedObjects)
        {
            if (obj != null)
            {
                SlidingDoor slidingDoor = obj.GetComponent<SlidingDoor>();
                Door regularDoor = obj.GetComponent<Door>();
                ElevatorCabin elevator = obj.GetComponent<ElevatorCabin>();

                if (slidingDoor != null)
                {
                    slidingDoor.ToggleDoor();
                }
                else if (regularDoor != null)
                {
                    regularDoor.ChangeDoorState();
                }
                else if (elevator != null)
                {
                    elevator.ToggleElevator();
                }
            }
        }
    }
}
