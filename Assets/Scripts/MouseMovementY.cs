using UnityEngine;

public class MouseMovementY : MonoBehaviour
{
    public float mouseSensitivity = 1500f;

    float yRotation = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(transform.localRotation.x, yRotation, 0f);
    }
}