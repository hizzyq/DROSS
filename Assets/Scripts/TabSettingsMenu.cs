using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class TabSettingsMenu : MonoBehaviour
{
    public Button soundTabButton;
    public Button controlsTabButton;
    public GameObject soundTabContent;
    public GameObject controlsTabContent;
    public AudioSource uiAudio;
    public AudioClip clickSound;
    void Start()
    {
        PlayClickSound();
        soundTabButton.onClick.AddListener(SwitchToSoundTab);
        controlsTabButton.onClick.AddListener(SwitchToControlsTab);
        SwitchToControlsTab();
    }

    public void SwitchToSoundTab()
    {
      
        soundTabContent.SetActive(true);
        controlsTabContent.SetActive(false);
    }

    public void SwitchToControlsTab()
    {
        soundTabContent.SetActive(false);
        controlsTabContent.SetActive(true);
    }
    private void PlayClickSound()
    {
        if (uiAudio != null && clickSound != null)
            uiAudio.PlayOneShot(clickSound);
    }
}