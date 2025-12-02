using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    Rigidbody rb;
    public float doorForce = 2f;

    [Header("References")]
    public Transform player;
    public BoxCollider boxCollider;

    private HingeJoint hingeJoint;
    private bool isOpen = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        hingeJoint = GetComponent<HingeJoint>();
        boxCollider = GetComponent<BoxCollider>();
    }

    private Vector3 GetForceDirection()
    {
        Vector3 toPlayer = player.position - transform.position;

        float dotProduct = Vector3.Dot(transform.forward, toPlayer.normalized);

        return dotProduct > 0 ? -transform.forward : transform.forward;
    }

    public void ChangeDoorState()
    {
            Vector3 forceDirection = GetForceDirection();
            rb.AddForce(forceDirection * doorForce, ForceMode.Impulse);

        if (isOpen)
        {
            hingeJoint.useSpring = true;
            StartCoroutine(DoorLock());
        }
        else
        {
            rb.constraints = RigidbodyConstraints.None;
            hingeJoint.useSpring = false;
            isOpen = true;
        }
    }

    IEnumerator DoorLock()
    {
        boxCollider.enabled = false;
        yield return new WaitForSeconds(0.2f);
        boxCollider.enabled = true;

        rb.constraints = RigidbodyConstraints.FreezeRotationY;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        isOpen = false;
    }
}