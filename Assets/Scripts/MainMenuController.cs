using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject settingsPanel;

    [Header("UI Elements")]
    public Button continueButton;

    [Header("Scene Transition Settings")]
    public AudioSource uiAudio;
    public AudioClip clickSound;

    private string saveFilePath;

    private void Start()
    {
        FadeManager.Instance.FadeIn();
        saveFilePath = Application.persistentDataPath + "/checkpoint.json";

        if (menuPanel != null) menuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.interactable = File.Exists(saveFilePath);
        }
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
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        FadeManager.Instance.FadeOut(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }
    public void ContinueGame()
    {
        if (File.Exists(saveFilePath))
        {
            PlayClickSound();

            string json = File.ReadAllText(saveFilePath);

            CheckpointSaveSystem.SaveData data = JsonUtility.FromJson<CheckpointSaveSystem.SaveData>(json);

            PlayerPrefs.SetString("TempCheckpointData", json);
            PlayerPrefs.Save();

            FadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene(data.currentScene);
            });
        }
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
    //звук для кнопки
    private void PlayClickSound()
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
    }
}