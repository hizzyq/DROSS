using UnityEngine;

public class WeaponCam : MonoBehaviour
{
    public Camera weaponCamera;
    
    void Start()
    {
        weaponCamera = GetComponent<Camera>();
            
        weaponCamera.clearFlags = CameraClearFlags.Depth;
        weaponCamera.depth = 1;
    }
    
    void Update()
    {
        weaponCamera.clearFlags = CameraClearFlags.Depth;
    }
}
