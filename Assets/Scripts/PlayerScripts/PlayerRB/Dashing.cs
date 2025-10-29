using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float dashUpwardForce = 0f;
    public float maxDashYSpeed = 15f;
    public float dashDuration = 0.25f;

    [Header("CameraEffects")]
    public PlayerCam cam;
    public float dashFov = 95f;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCd = 0.5f;
    public float rechargeTime = 1.5f;
    public int maxCharges = 2;
    private float dashCdTimer;
    public float dashChargeTimer;
    public int curCharges = 2;
    

    [Header("Input")]
    public KeyCode dashKey = KeyCode.LeftShift;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey) && curCharges > 0)
        {
            Dash();
            curCharges--;
        }

        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;

        if (curCharges < maxCharges)
        {
            dashChargeTimer -= Time.deltaTime;
            if (dashChargeTimer <= 0f)
            {
                curCharges++;
                dashChargeTimer += rechargeTime; // Сбрасываем таймер

                // Если все еще есть заряды для восстановления, продолжаем отсчет
                if (curCharges < maxCharges)
                {
                    dashChargeTimer = rechargeTime;
                }
                else
                {
                    dashChargeTimer = 0f; // Все заряды восстановлены
                }
            }
        }
    }

    private void Dash()
    {
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;
        dashChargeTimer += rechargeTime;

        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        cam.DoFov(dashFov);

        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam; // where you're looking
        else
            forwardT = orientation; // where you're facing (no up or down)

        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if (disableGravity)
            rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.linearVelocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;
        pm.maxYSpeed = 0;

        cam.DoFov(85f);

        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }
}
