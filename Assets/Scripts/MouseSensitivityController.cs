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
    // �������
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSens", sensitivity);
        PlayerPrefs.Save();
        UpdateSensitivityText(sensitivity);

        // Если мы в игре и меню паузы — обновить камеру сразу, без перезагрузки
        PlayerCam cam = FindObjectOfType<PlayerCam>();
        if (cam != null)
        {
            float mapped = Mathf.Lerp(10f, 600f, sensitivity / 100f);
            cam.sensX = mapped;
            cam.sensY = mapped;
        }
    }
    private void UpdateSensitivityText(float value)
    {
        sensitivityValueText.text = value.ToString("0");
    }
}