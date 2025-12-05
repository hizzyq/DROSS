using System.Collections;
using UnityEngine;

public class LightsOut : MonoBehaviour
{
    [Header("Lights")]
    public GameObject lightContainer;

    [Header("Audio")]
    public AudioClip lightFlickerOff;
    public AudioClip lightFlickerOn;

    private LightFlickerController[] lightControllers;

    private bool hasTriggered = false;
    public bool freezePlayer = false;

    [Header("Player camera")]
    public PlayerCam playerCam;

    private PlayerMovementAdvanced playerMove;

    private void Start()
    {
        lightControllers = lightContainer.GetComponentsInChildren<LightFlickerController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasTriggered)
            {
                playerMove = other.GetComponentInParent<PlayerMovementAdvanced>();
                TriggerLights();
                hasTriggered = true;
                Debug.Log($"{playerMove}");
            }
        }
    }

    public void TriggerLights()
    {
        if (lightControllers != null && lightControllers.Length > 0)
        {
            StartCoroutine(ManageLightsState());
        }
    }

    private IEnumerator ManageLightsState()
    {
        if (lightFlickerOff != null) { SoundFXManager.instance.PlaySoundFXClip(lightFlickerOff, transform, 1f); }
        foreach (var lightController in lightControllers)
        {
            if (lightController != null)
            {
                lightController.TriggerFlickerSequence(false);
            }
        }
        if (freezePlayer)
        {
            FreezePlayer();
        }

        yield return new WaitForSeconds(5f);

        if (lightFlickerOn != null) { SoundFXManager.instance.PlaySoundFXClip(lightFlickerOn, transform, 1f); }
        foreach (var lightController in lightControllers)
        {
            if (lightController != null)
            {
                lightController.TriggerFlickerSequence(true);
            }
        }
        if (freezePlayer)
        {
            UnfreezePlayer();
        }
    }

    public void FreezePlayer()
    {
        playerMove.enabled = false;
        playerCam.enabled = false;
    }

    public void UnfreezePlayer()
    {
        playerMove.enabled = true;
        playerCam.enabled = true;
    }
}
