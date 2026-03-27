using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class WeaponCameraSetup : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        var weaponCam = GetComponent<Camera>();
        var additionalData = weaponCam.GetUniversalAdditionalCameraData();

        // Overlay — рендерится поверх основной камеры
        additionalData.renderType = CameraRenderType.Overlay;

        // Добавляем в стек основной камеры
        var mainData = mainCamera.GetUniversalAdditionalCameraData();
        mainData.cameraStack.Add(weaponCam);

        weaponCam.cullingMask  = LayerMask.GetMask("WeaponRender");
        weaponCam.clearFlags   = CameraClearFlags.Depth;
        weaponCam.nearClipPlane = 0.01f;
        weaponCam.depth        = 1;
    }
}
