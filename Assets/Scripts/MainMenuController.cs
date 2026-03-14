using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject settingsPanel;

    [Header("Scene Transition Settings")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public AudioSource uiAudio;
    public AudioClip clickSound;

    private void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeInHierarchy)
            {
                if (settingsPanel != null) settingsPanel.SetActive(false);
                if (menuPanel != null) menuPanel.SetActive(true);
            }
        }
    }
    //начало игры
    public void StartGame(string sceneName)
    {
        PlayClickSound();
        StartCoroutine(FadeAndLoad(sceneName));
    }

    //настройки
    public void OpenSettings()
    {
        PlayClickSound();
        if (menuPanel != null) menuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        PlayClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }
    //выход из игры
    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Выход из игры...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    //зыук для кнопки
    private void PlayClickSound()
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
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