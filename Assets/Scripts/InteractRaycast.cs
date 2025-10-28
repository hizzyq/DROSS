using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractRaycast : MonoBehaviour
{
    public Camera playerCamera;
    public float rayDistance = 100f;
    public LayerMask interactableLayers;

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = playerCamera.ScreenPointToRay(mousePosition);
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
