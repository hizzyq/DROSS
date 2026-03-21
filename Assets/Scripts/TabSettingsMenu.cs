using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class TabSettingsMenu : MonoBehaviour
{
    [Header("Кнопки вкладок")]
    public Button soundTabButton;
    public Button controlsTabButton;
    public Button graphicsTabButton;      // ← новая кнопка

    [Header("Контент вкладок")]
    public GameObject soundTabContent;
    public GameObject controlsTabContent;
    public GameObject graphicsTabContent; // ← новый контент

    [Header("Звук")]
    public AudioSource uiAudio;
    public AudioClip clickSound;

    void Start()
    {
        soundTabButton.onClick.AddListener(SwitchToSoundTab);
        controlsTabButton.onClick.AddListener(SwitchToControlsTab);
        graphicsTabButton.onClick.AddListener(SwitchToGraphicsTab); // ← новый листенер

        SwitchToSoundTab(); // открываем Sound по умолчанию
    }

    public void SwitchToSoundTab()
    {
        soundTabContent.SetActive(true);
        controlsTabContent.SetActive(false);
        graphicsTabContent.SetActive(false);
        PlayClickSound();
    }

    public void SwitchToControlsTab()
    {
        soundTabContent.SetActive(false);
        controlsTabContent.SetActive(true);
        graphicsTabContent.SetActive(false);
        PlayClickSound();
    }

    public void SwitchToGraphicsTab()    // ← новый метод
    {
        soundTabContent.SetActive(false);
        controlsTabContent.SetActive(false);
        graphicsTabContent.SetActive(true);
        PlayClickSound();
    }

    private void PlayClickSound()
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
    }
}