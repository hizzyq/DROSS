using UnityEngine;

public class RotatingCube : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        // Вращение куба с постоянной скоростью
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}