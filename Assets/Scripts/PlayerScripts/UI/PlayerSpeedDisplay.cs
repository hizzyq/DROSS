using UnityEngine;
using TMPro;

public class PlayerSpeedDisplay : MonoBehaviour
{
    public Rigidbody rb;
    public TextMeshProUGUI speedText;

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;
        speedText.text = $"Velocity: {speed:F2} f";
    }
}