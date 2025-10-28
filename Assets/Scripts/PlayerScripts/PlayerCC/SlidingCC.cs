using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlidingCC : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private CharacterController controller;
    private PlayerMovementCC pm;

    [Header("Sliding")]
    // public float maxSlideTime = 1.5f; // Убрано
    public float slideForce = 10f;
    // private float slideTimer; // Убрано

    public float slideYScale = 0.5f;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private Vector3 slideDirection = Vector3.zero;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        pm = GetComponent<PlayerMovementCC>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();

        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        // Optionally, push player a bit down to stay grounded
        controller.Move(Vector3.down * 0.1f);

        

        // Set slide direction initially
        slideDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        Vector3 slideMove = inputDirection.normalized;

        // Use previous slide direction if no input is given
        if (slideMove == Vector3.zero)
            slideMove = slideDirection;

        // On slope
        if (pm.OnSlope())
        {
            slideMove = pm.GetSlopeMoveDirection(slideMove);
        }

        controller.Move(slideMove * slideForce * Time.deltaTime);

        // slideTimer -= Time.deltaTime; // Убрано

        // if (slideTimer <= 0)
        //     StopSlide(); // Убрано
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}