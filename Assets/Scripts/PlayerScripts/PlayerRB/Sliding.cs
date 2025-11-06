using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    public Transform cameraObj;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    [Header("Sliding")]
    public float slideForce = 200f;

    [Header("Position")]
    public float slideYScalePlayer = 0.5f;
    public float slideYPosCamera = 0.5f;
    private float startYScalePlayer;
    private float startYPosCamera;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float verticalInput;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();

        startYScalePlayer = playerObj.localScale.y;
        startYPosCamera = cameraObj.localPosition.y;
    }

    private void Update()
    {
        verticalInput = Mathf.Clamp(Input.GetAxisRaw("Vertical"), 0, 1);

        if (Input.GetKeyDown(slideKey) && verticalInput != 0)
            StartSlide();

        if (pm.sliding && verticalInput == 0)
        {
            SlideIntoCrouch();
        }

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScalePlayer, playerObj.localScale.z);
        cameraObj.localPosition = new Vector3(playerObj.localPosition.x, slideYPosCamera, playerObj.localPosition.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void SlidingMovement()
    {

        Vector3 inputDirection = orientation.forward * verticalInput;
        // sliding normal
        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }
    }

    private void SlideIntoCrouch()
    {
        pm.sliding = false;
        pm.crouching = true;
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScalePlayer, playerObj.localScale.z);
        cameraObj.localPosition = new Vector3(playerObj.localPosition.x, startYPosCamera, playerObj.localPosition.z);
    }
}
