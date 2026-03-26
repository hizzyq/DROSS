using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("Player managment")]
    public GameObject playerPrefab;
    public Transform currentCheckpoint;
    public GameObject currentPlayer;
    public PlayerCam playerCam;
    public CameraPosition camPos;
    public Camera mainCamera;
    public TextMeshProUGUI playerHealthUI;
    public GameObject gameOverUI;
    public Player player;
    public ScreenBlackout screenBlackout;

    private bool revivable = false;
    private bool isDead = false;

    
    public void KillPlayer()
    {
        if (!isDead)
        {
            //currentPlayer.GetComponent<PlayerMovementAdvanced>().enabled = false;
            //playerCam.enabled = false;
            StartCoroutine(ReviveCooldown());
            isDead = true;
        }
    }

    public void RevivePlayer()
    {
        if (isDead)
        {
            // Destroy(currentPlayer);
            // currentPlayer = Instantiate(playerPrefab, currentCheckpoint.position, Quaternion.identity);
            // camPos.cameraPosition = currentPlayer.transform.Find("CameraPos");
            // playerCam.orientation = currentPlayer.transform.Find("Orientation");
            // currentPlayer.GetComponent<InteractRaycast>().playerCamera = playerCam.GetComponent<Camera>();
            revivable = false;
            isDead = false;
            // playerCam.enabled = true;
            StopAllCoroutines();
            transform.position = currentCheckpoint.transform.position;
            GetComponent<Dashing>().enabled = true;
            GetComponent<PlayerMovementAdvanced>().enabled = true;
            GetComponent<Sliding>().enabled = true;
            GetComponent<WallRunning>().enabled = true;

            mainCamera.GetComponent<PlayerCam>().enabled = true;
            //mainCamera.GetComponent<Animator>().enabled = false;
            screenBlackout.enabled = false;
            screenBlackout.ReverseFade();
            player.HP = 100;
            playerHealthUI.gameObject.SetActive(true);
            gameOverUI.gameObject.SetActive(false);
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
