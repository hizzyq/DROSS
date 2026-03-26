using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("Player managment")]
    public Player player;

    [Header("UI and Effects")]
    public GameObject gameOverUI;
    public ScreenBlackout screenBlackout;

    private bool revivable = false;
    private bool isDead = false;

    
    public void KillPlayer()
    {
        if (!isDead)
        {
            isDead = true;

            if (gameOverUI != null) gameOverUI.SetActive(true);
            if (screenBlackout != null) 
            {
                screenBlackout.enabled = true;
                // Если у экрана затемнения есть метод для обычного фейда, можете вызвать его здесь
                // screenBlackout.Fade(); 
            }

            StartCoroutine(ReviveCooldown());
        }
    }

    public void RevivePlayer()
    {
        if (isDead)
        {
            revivable = false;
            isDead = false;
            
            CheckpointSaveSystem saveSystem = player.GetComponent<CheckpointSaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.LoadCheckpoint(player);
            }
            else
            {
                Debug.LogWarning("CheckpointSaveSystem not found on Player!");
            }
        }
    }

    private void Update()
    {
        if (isDead && revivable)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RevivePlayer();
                Debug.Log("Revived using CheckpointSaveSystem");
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
