using UnityEngine;

public class CamHold : MonoBehaviour
{
    public Transform cameraPosition;
    
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
