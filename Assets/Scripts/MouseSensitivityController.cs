using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;

    private void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSens", 50f);
        sensitivitySlider.value = savedSensitivity;
        UpdateSensitivityText(savedSensitivity);
    }
    // слайдер
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSens", sensitivity);
        PlayerPrefs.Save();
        UpdateSensitivityText(sensitivity);
    }
    private void UpdateSensitivityText(float value)
    {
        sensitivityValueText.text = value.ToString("0");
    }
}