using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class EndGame : MonoBehaviour
{
    public ScreenBlackout screenBlackout;
    public GameObject gameOverUI;
    public TextMeshProUGUI playerHealthUI;
    public Image fadePanel;
    public float fadeDuration = 1f;
    public string _sceneName;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BodyPlayer"))
        {
            // GetComponent<Dashing>().enabled = false;
            // GetComponent<PlayerMovementAdvanced>().enabled = false;
            // GetComponent<Sliding>().enabled = false;
            // GetComponent<WallRunning>().enabled = false;

            playerHealthUI.gameObject.SetActive(false);
            
            screenBlackout.enabled = true;
            screenBlackout.StartFade();
            StartCoroutine(ShowGameOverUI());
            StartGame(_sceneName);
        }
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         
    //     }
    // }
    
    public void StartGame(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }
    
    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(1f);
        gameOverUI.gameObject.SetActive(true);
    }
    
    private System.Collections.IEnumerator FadeAndLoad(string sceneName)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }
}