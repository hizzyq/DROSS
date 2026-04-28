using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndGame : MonoBehaviour
{
    public ScreenBlackout screenBlackout;
    public GameObject gameOverUI;
    public TextMeshProUGUI playerHealthUI;
    public string _sceneName;

    // Убрали дублирующиеся переменные fadePanel и fadeDuration,
    // так как теперь полагаемся на ScreenBlackout

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BodyPlayer"))
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            if (playerHealthUI != null)
                playerHealthUI.gameObject.SetActive(false);

            if (screenBlackout != null)
            {
                screenBlackout.enabled = true;
                screenBlackout.StartFade();
            }

            StartCoroutine(ShowGameOverUIAndLoad());
        }
    }

    private IEnumerator ShowGameOverUIAndLoad()
    {
        // Ждем немного перед показом UI смерти
        yield return new WaitForSeconds(1f);
        if (gameOverUI != null)
            gameOverUI.gameObject.SetActive(true);

        // Ждем завершения фейда (берем время из ScreenBlackout)
        float waitTime = screenBlackout != null ? screenBlackout.fadeDuration : 2f;

        // Вычитаем 1 секунду, так как мы уже прождали ее перед показом UI
        yield return new WaitForSeconds(Mathf.Max(0, waitTime - 1f));

        SceneManager.LoadScene(_sceneName);
    }
}