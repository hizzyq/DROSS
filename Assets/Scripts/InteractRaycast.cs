using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class InteractRaycast : MonoBehaviour
{
    public Camera playerCamera;
    public float rayDistance = 100f;
    public LayerMask interactableLayers;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance, interactableLayers))
            {

                if (hit.collider.CompareTag("Door"))
                {
                    hit.collider.GetComponent<Door>().ChangeDoorState();
                }
                else if (hit.collider.CompareTag("SlidingDoor"))
                {
                    hit.collider.GetComponent<SlidingDoor>().ToggleDoor();
                }
                else if (hit.collider.CompareTag("PhysicalButton"))
                {
                    Animator physicalButtonAnimator = hit.collider.GetComponent<Animator>();
                    if (!physicalButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("PhysicalButtonPress"))
                    {
                        physicalButtonAnimator.Play("PhysicalButtonPress");
                        hit.collider.GetComponent<PhysicalButton>().ActivateConnectedObjects();
                    }
                }
            }
        }
    }
}
