using UnityEngine;

public class WeaponAimOutline : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float distance = 4f;

    [Tooltip("—лои, по которым должен идти луч: и Weapon, и Environment/Default/Ground/Wall")]
    [SerializeField] private LayerMask raycastMask = ~0;

    private Outline currentOutline;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, distance, raycastMask, QueryTriggerInteraction.Ignore))
        {
            Outline newOutline = hit.collider.GetComponentInParent<Outline>();

            if (newOutline != currentOutline)
            {
                DisableCurrent();

                if (newOutline != null)
                {
                    newOutline.OutlineMode = Outline.Mode.OutlineVisible;
                    newOutline.enabled = true;
                    currentOutline = newOutline;
                }
            }
        }
        else
        {
            DisableCurrent();
        }
    }

    private void DisableCurrent()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
