using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("Player management")]
    public Player player;

    [Header("UI")]
    public GameObject gameOverUI;

    private bool revivable = false;
    private bool isDead = false;

    public void KillPlayer()
    {
        if (!isDead)
        {
            isDead = true;

            // Используем глобальный фейд
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeOut(() =>
                {
                    // Показываем UI окончания игры только когда экран потемнел
                    if (gameOverUI != null) gameOverUI.SetActive(true);
                    StartCoroutine(ReviveCooldown());
                });
            }
            else
            {
                // Фолбэк, если FadeManager не найден
                if (gameOverUI != null) gameOverUI.SetActive(true);
                StartCoroutine(ReviveCooldown());
            }
        }
    }

    public void RevivePlayer()
    {
        if (isDead)
        {
            revivable = false;
            isDead = false;

            if (gameOverUI != null) gameOverUI.SetActive(false);

            // Осветляем экран обратно
            if (FadeManager.Instance != null) FadeManager.Instance.FadeIn();

            CheckpointSaveSystem saveSystem = player.GetComponent<CheckpointSaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.LoadCheckpoint(player);
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
