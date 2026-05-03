using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGame : MonoBehaviour
{
    public string _sceneName;
    public TextMeshProUGUI playerHealthUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BodyPlayer"))
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            if (playerHealthUI != null) playerHealthUI.gameObject.SetActive(false);

            // Просто затемняем экран и переходим на сцену
            FadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene(_sceneName);
            });
        }
    }
}