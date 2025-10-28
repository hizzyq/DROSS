using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementCC : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float slideSpeed = 12f;
    public float wallRunSpeed = 14f;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier = 10f;
    public float slopeIncreaseMultiplier = 2f;

    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float airMultiplier = 0.5f;

    [Header("Crouching")]
    public float crouchSpeed = 4f;
    public float crouchYScale = 0.5f;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 velocity;
    float gravity = -9.81f * 2f;

    CharacterController controller;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        wallrunning,
        sliding,
        air
    }

    public bool sliding;
    public bool wallrunning;
    public bool crouching;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = controller.isGrounded;

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded && velocity.y < 0)
            velocity.y = -2f; // keep player grounded

        // Jump
        if (Input.GetKeyDown(jumpKey) && grounded)
        {
            Jump();
        }

        // Crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        // Move
        MovePlayer();

        //Apply gravity (with wallrun modification)
        if (wallrunning)
            velocity.y = 0; // lighter gravity for wallrun
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void StateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;
            desiredMoveSpeed = slideSpeed;
        }
        // Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }
    
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();

        Vector3 move = moveDirection * moveSpeed;

        // Slope handling
        if (OnSlope() && !exitingSlope)
        {
            move = GetSlopeMoveDirection(moveDirection) * moveSpeed;
            if (velocity.y > 0)
                velocity.y = 0f;
        }

        // Air control
        if (!grounded && !wallrunning)
            move *= airMultiplier;

        controller.Move(move * Time.deltaTime);
    }

    private void SpeedControl()
    {
        // CharacterController handles speed automatically, but you may clamp it if needed
    }

    private void Jump()
    {
        exitingSlope = true;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}