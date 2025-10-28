using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wall running script (CharacterController version)
/// Wallrun активируется однократным нажатием A (левая стена) или D (правая стена)
/// </summary>
public class WallRunningCC : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce = 10f;
    public float wallJumpUpForce = 8f;
    public float wallJumpSideForce = 5f;
    public float wallClimbSpeed = 5f;
    // public float maxWallRunTime = 1.5f; // Убрано
    // private float wallRunTimer; // Убрано

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance = 0.6f;
    public float minJumpHeight = 1.5f;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime = 0.2f;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity = true;
    public float gravityCounterForce = 5f;

    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    private PlayerMovementCC pm;
    private CharacterController controller;

    // Movement state
    private Vector3 velocity;
    private float gravity = -9.81f * 2f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        pm = GetComponent<PlayerMovementCC>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
        HandleWallrunInput();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    /// <summary>
    /// Wallrun активируется только по нажатию A (левая стена) или D (правая стена)
    /// </summary>
    private void HandleWallrunInput()
    {
        // A - левая стена
        if (wallLeft && horizontalInput < 0 && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRun();
            // wallRunTimer = maxWallRunTime; // Убрано
        }
        // D - правая стена
        if (wallRight && horizontalInput > 0 && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRun();
            // wallRunTimer = maxWallRunTime; // Убрано
        }
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // Wallrun уже может быть активирован через HandleWallrunInput (по нажатию)
        // Здесь только поддерживаем состояние до истечения exitWallTime

        // State 1 - Wallrunning (только если wallrun уже активирован)
        if (pm.wallrunning && (wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            // wallRunTimer логика убрана

            // wall jump
            if (Input.GetKeyDown(jumpKey)) WallJump();
        }
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        // State 3 - None
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;

        // wallRunTimer = maxWallRunTime; // Убрано

        velocity.y = 0f; // reset vertical velocity

        // apply camera effects
        if (cam != null)
        {
            cam.DoFov(90f);
            if (wallLeft) cam.DoTilt(-5f);
            if (wallRight) cam.DoTilt(5f);
        }
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void WallRunningMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        Vector3 move = wallForward * wallRunForce;

        // Up/down on wall
        if (upwardsRunning)
            velocity.y = wallClimbSpeed;
        else if (downwardsRunning)
            velocity.y = -wallClimbSpeed;
        else
            velocity.y = 0f;

        // Move forward along wall
        controller.Move(move * Time.fixedDeltaTime);

        // Stick to wall (simulate force)
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            controller.Move(-wallNormal * 5f * Time.fixedDeltaTime);
        }

        // Weaken gravity
        if (useGravity)
            controller.Move(Vector3.up * gravityCounterForce * Time.fixedDeltaTime);
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;

        // reset camera effects
        if (cam != null)
        {
            cam.DoFov(80f);
            cam.DoTilt(0f);
        }
    }

    private void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        // Добавьте горизонтальную составляющую от стены
        Vector3 jumpDirection = (transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce).normalized;

        // Установите вертикальную и горизонтальную скорость для прыжка
        velocity = transform.up * Mathf.Sqrt(wallJumpUpForce * -2f * gravity) + wallNormal * wallJumpSideForce;
    }
}