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
    private Vector3 _previousVelocity;
    private Vector3 _acceleration;
    
    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashSpeedChangeFactor = 40f;
    public float maxYSpeed = 8f;

    [Header("Jumping")]
    public float jumpForce = 14f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.5f;
    private RaycastHit wallFront;
    [SerializeField] bool readyToJump;

    [Header("Wall Bouncing")]
    [SerializeField] float wallBounceUpForce = 10f;
    [SerializeField] float wallBounceSideForce = 10f;
    [SerializeField] private float wallBounceWallDetection = 1f;
    
    [Header("Crouching")]
    public float crouchSpeed = 5f;

    [Header("Position")]
    public float slideYScalePlayer = 0.7f;
    public float slideYPosCamera = 0.7f;
    private float startYScalePlayer;
    private float startYPosCamera;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")] 
    private float playerHeightStart = 2f;
    public float playerHeight;
    public LayerMask whatIsGround;
    [SerializeField] bool grounded;
    
    [Header("Coyote Time")]
    float coyoteTime;
    public float coyoteTimer;
    [SerializeField] float coyoteTimeCounter;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 46f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    // ──────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private SFXEvent[] footstepSFX;   // несколько звуков шагов — назначь в инспекторе
    [SerializeField] private SFXEvent jumpSFX;          // звук в момент прыжка
    [SerializeField] private SFXEvent landSFX;          // звук приземления
    [Tooltip("Минимальная скорость (XZ) для воспроизведения шагов")]
    [SerializeField] private float footstepMinSpeed = 1.5f;
    [Tooltip("Интервал между шагами в секундах")]
    [SerializeField] private float footstepInterval = 0.35f;
    [Tooltip("Множитель интервала во время wallrun (< 1 = чаще)")]
    [SerializeField] private float wallrunFootstepMultiplier = 0.7f;

    private float _footstepTimer;
    private int   _lastFootstepIndex = -1;   // не повторять один и тот же звук дважды подряд
    private bool  _wasGrounded;              // для определения момента приземления
    // ──────────────────────────────────────────────

    //input & direction
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    //states
    public MovementState state;
    private MovementState lastState;
    public bool dashing;
    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    private bool keepMomentum;
    
    public enum MovementState
    {
        walking,
        dashing,
        wallrunning,
        crouching,
        sliding,
        air
    }
    
    private void Start()
    {
        playerHeight = playerHeightStart;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScalePlayer = playerObj.localScale.y;
        startYPosCamera = cameraObj.localPosition.y;
        
        cameraSpring.Initialize();
        cameraLean.Initialize();

        _wasGrounded = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        if (grounded)
            coyoteTimeCounter = coyoteTimer;
        else
            coyoteTimeCounter -= Time.deltaTime;
        
        MyInput();
        StateHandler();
        HandleFootstepAudio();
        HandleLandingAudio();

        rb.linearDamping = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        isSloped = OnSlope();
        if(!wallrunning) rb.useGravity = !isSloped;
        SpeedControl();
    }

    private void LateUpdate()
    {
        _acceleration = (rb.linearVelocity - _previousVelocity) / Time.deltaTime;
        _previousVelocity = rb.linearVelocity;

        cameraSpring.UpdateSpring(Time.deltaTime, cameraObj.up);
        cameraLean.UpdateLean(Time.deltaTime, sliding, _acceleration, cameraObj.up);
    }

    // ──────────────────────────────────────────────
    // AUDIO HELPERS
    // ──────────────────────────────────────────────

    /// <summary>
    /// Тикает таймер шагов и воспроизводит случайный (не повторяющийся) звук
    /// когда игрок движется по земле / стене.
    /// </summary>
    private void HandleFootstepAudio()
    {
        if (footstepSFX == null || footstepSFX.Length == 0) return;

        bool canStep = (grounded || wallrunning) &&
                       !dashing &&
                       !sliding &&
                       GetHorizontalSpeed() > footstepMinSpeed;

        if (canStep)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                PlayRandomFootstep();
                _footstepTimer = wallrunning ? footstepInterval * wallrunFootstepMultiplier : footstepInterval;
            }
        }
        else
        {
            // сбрасываем таймер, чтобы первый шаг после остановки не запаздывал
            _footstepTimer = 0f;
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepSFX.Length == 1)
        {
            AudioManager.PlayAt(footstepSFX[0], transform.position);
            return;
        }

        // выбираем индекс, отличный от предыдущего
        int index;
        do { index = Random.Range(0, footstepSFX.Length); }
        while (index == _lastFootstepIndex);

        _lastFootstepIndex = index;
        AudioManager.PlayAt(footstepSFX[index], transform.position);
    }

    /// <summary>
    /// Фиксирует момент приземления (переход !grounded → grounded) и играет звук.
    /// </summary>
    private void HandleLandingAudio()
    {
        if (!_wasGrounded && grounded)
        {
            if (landSFX != null)
                AudioManager.PlayAt(landSFX, transform.position);
        }
        _wasGrounded = grounded;
    }

    private float GetHorizontalSpeed()
    {
        return new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
    }

    // ──────────────────────────────────────────────

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && coyoteTimeCounter > 0f && !dashing)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, startYScalePlayer * slideYScalePlayer, playerObj.localScale.z);
            cameraObj.localPosition = new Vector3(playerObj.localPosition.x, startYPosCamera * slideYPosCamera, playerObj.localPosition.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            playerHeight = playerHeight * 0.5f;
            crouching = true;
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey) && crouching)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, startYScalePlayer, playerObj.localScale.z);
            cameraObj.localPosition = new Vector3(playerObj.localPosition.x, startYPosCamera, playerObj.localPosition.z);

            playerHeight = playerHeightStart;
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
            if (Physics.Raycast(transform.position, orientation.forward, out wallFront, wallBounceWallDetection, whatIsGround))
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
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        if (state == MovementState.dashing) return;
        
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (sliding) {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.linearVelocity.normalized);
            ApplyHorizontalDragIfNeeded();
            return;
        }
        
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

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
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        
        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
    }

    private void Jump()
    {
        coyoteTimeCounter = 0f;
        exitingSlope = true;

        // звук прыжка
        if (jumpSFX != null)
            AudioManager.PlayAt(jumpSFX, transform.position);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void WallBounce()
    {
        Vector3 wallNormal = wallFront.normal;
        Vector3 forceToApply = transform.up * wallBounceUpForce + wallNormal * wallBounceSideForce;

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