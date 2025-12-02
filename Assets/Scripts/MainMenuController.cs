using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public AudioSource uiAudio;
    public AudioClip clickSound;

    //начало игры
    public void StartGame(string sceneName)
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
        StartCoroutine(FadeAndLoad(sceneName));
    }

    //настройки
    public void OpenSettings()
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
        Debug.Log("Открываем настройки...");
    }

    //выход из игры
    public void ExitGame()
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
        Debug.Log("Выход из игры...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
 
    //затухание экрана при переключении сцен
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