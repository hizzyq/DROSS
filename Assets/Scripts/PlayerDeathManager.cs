using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("Player managment")]
    public GameObject playerPrefab;
    public Transform currentCheckpoint;
    public GameObject currentPlayer;
    public PlayerCam playerCam;
    public CameraPosition camPos;

    private bool revivable = false;
    private bool isDead = false;

    
    public void KillPlayer()
    {
        if (!isDead)
        {
            currentPlayer.GetComponent<PlayerMovement>().enabled = false;
            playerCam.enabled = false;
            StartCoroutine(ReviveCooldown());
            isDead = true;
        }
    }

    public void RevivePlayer()
    {
        if (isDead)
        {
            Destroy(currentPlayer);
            currentPlayer = Instantiate(playerPrefab, currentCheckpoint.position, Quaternion.identity);
            camPos.cameraPosition = currentPlayer.transform.Find("CameraPos");
            playerCam.orientation = currentPlayer.transform.Find("Orientation");
            currentPlayer.GetComponent<InteractRaycast>().playerCamera = playerCam.GetComponent<Camera>();
            revivable = false;
            isDead = false;
            playerCam.enabled = true;
        }
    }

    private void Update()
    {
        if (isDead & revivable)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RevivePlayer();
                Debug.Log($"Revived at {currentCheckpoint.position}");
            }
        }
    }

    private IEnumerator ReviveCooldown()
    {
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Revivable");
        revivable = true;
    }
}
