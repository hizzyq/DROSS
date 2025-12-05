using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    [Header("Настройки")]
    [SerializeField] private string mixerParameter = "MasterVolume";

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("VolumePreference", 0.3f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }
    public void SetVolume(float sliderValue)
    {
        int percentage = Mathf.RoundToInt(sliderValue * 100);
        if (volumeText != null)
            volumeText.text = percentage.ToString();
        float dB;
        if (sliderValue <= 0.0001f)
        {
            dB = -80f;
        }
        else
        {
            dB = Mathf.Log10(sliderValue) * 20;
        }
        audioMixer.SetFloat(mixerParameter, dB);

        PlayerPrefs.SetFloat("VolumePreference", sliderValue);
        PlayerPrefs.Save();
    }
}