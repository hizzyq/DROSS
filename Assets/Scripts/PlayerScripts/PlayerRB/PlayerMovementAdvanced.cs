using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    public Transform cameraObj;
    private Rigidbody rb;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;

    [SerializeField] private bool isSloped;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed = 9f;
    public float slideSpeed = 30f;
    public float wallrunSpeed = 8f;

    public float speedIncreaseMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;

    public float groundDrag = 7f;
    
    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashSpeedChangeFactor = 40f;

    public float maxYSpeed = 8f;

    [Header("Jumping")]
    public float jumpForce = 14f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.5f;
    bool readyToJump;
    private bool afterAir;

    [Header("Crouching")]
    public float crouchSpeed = 5f;

    [Header("Position")]
    public float slideYScalePlayer = 0.5f;
    public float slideYPosCamera = 0.5f;
    private float startYScalePlayer;
    private float startYPosCamera;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 46f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public MovementState state;
    public enum MovementState
    {
        walking,
        dashing,
        wallrunning,
        crouching,
        sliding,
        air
    }

    public bool dashing;
    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    
    private MovementState lastState;
    private bool keepMomentum;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        afterAir = false;

        startYScalePlayer = playerObj.localScale.y;
        startYPosCamera = cameraObj.localPosition.y;
        
        cameraSpring.Initialize();
        cameraLean.Initialize();
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        MyInput();

        rb.linearDamping = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        isSloped = OnSlope();
        if(!wallrunning) rb.useGravity = !isSloped;
        SpeedControl();
        StateHandler();
    }

    private void LateUpdate()
    {
        cameraSpring.UpdateSpring(Time.deltaTime, cameraObj.up);
        cameraLean.UpdateLean(Time.deltaTime, sliding, moveDirection, cameraObj.up);
    }
    
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScalePlayer, playerObj.localScale.z);
            cameraObj.localPosition = new Vector3(playerObj.localPosition.x, slideYPosCamera, playerObj.localPosition.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            crouching = true;
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey) && crouching)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, startYScalePlayer, playerObj.localScale.z);
            cameraObj.localPosition = new Vector3(playerObj.localPosition.x, startYPosCamera, playerObj.localPosition.z);
            crouching = false;
        }
    }

    private void StateHandler()
    {
        
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        
        // Mode - Dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = walkSpeed;
        }

        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
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
            if (Physics.Raycast(transform.position, Vector3.forward, 1.5f, whatIsGround))
            {
                if (Input.GetKeyDown(jumpKey))
                {
                    WallBounce();
                }
            }
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing || state == MovementState.sliding) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(SmoothlyLerpMoveSpeed());
                }
                else
                {
                    StopAllCoroutines();
                    moveSpeed = desiredMoveSpeed;
                }
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
        
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else time += Time.deltaTime * boostFactor;
            
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void MovePlayer()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        if (state == MovementState.dashing) return;
        
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (sliding) {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.linearVelocity.normalized);
            ApplyHorizontalDragIfNeeded();
            return;
        }
        
        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        
        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        //if(!wallrunning) rb.useGravity = !isSloped;

        ApplyHorizontalDragIfNeeded();
    }

    private void ApplyHorizontalDragIfNeeded()
    {
        if ((state == MovementState.walking || state == MovementState.crouching) && grounded)
        {
            Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float dragFactor = 1f - groundDrag * Time.fixedDeltaTime;
            if (dragFactor < 0f) dragFactor = 0f;
            rb.linearVelocity = new Vector3(flat.x * dragFactor, rb.linearVelocity.y, flat.z * dragFactor);
        }
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        
        // limit y vel
        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
    }

    private void Jump()
    {
        exitingSlope = true;
        
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        afterAir = true;
        
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        
        exitingSlope = false;
    }

    private void WallBounce()
    {
        float wallBounceUpForce = 7;
        float wallBounceSideForce = 12;
        Vector3 wallNormal = Vector3.back;
        Vector3 forceToApply = transform.up * wallBounceUpForce + wallNormal * wallBounceSideForce;

        // reset y velocity and add force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
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