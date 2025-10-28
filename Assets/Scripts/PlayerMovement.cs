using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;


    private InputAction moveAction;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Debug.DrawRay(transform.position, Vector3.down * 5f);

        //if (grounded)
        //{
            rb.linearDamping = groundDrag;
        //    Debug.Log("Grounded - Drag applied: " + groundDrag);
        //}
        //else
        //{
        //    rb.linearDamping = 0;
        //    Debug.Log("Not grounded");
        //}
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}
