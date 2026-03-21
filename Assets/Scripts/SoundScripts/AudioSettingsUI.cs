using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Добавь на панель настроек звука в Canvas.
/// Связывает слайдеры UI с AudioSettings.
/// Слайдеры необязательны — незаполненные просто игнорируются.
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Слайдеры (необязательно — оставь пустым если не нужен)")]
    public Slider sliderMaster;
    public Slider sliderMusic;
    public Slider sliderSFX;
    public Slider sliderUI;

    [Header("Тестовый звук при движении слайдера SFX")]
    public SFXEvent previewSFX;

    private AudioSettings _settings;
    private bool _initialized;

    void OnEnable()
    {
        if (AudioManager.Instance == null) return;
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _settings = AudioManager.Instance.Settings;

        // Установить значения слайдеров из сохранённых настроек
        if (sliderMaster) sliderMaster.value = _settings.MasterVolume;
        if (sliderMusic)  sliderMusic.value  = _settings.MusicVolume;
        if (sliderSFX)    sliderSFX.value    = _settings.SFXVolume;
        if (sliderUI)     sliderUI.value     = _settings.UIVolume;

        // Подписаться на изменения
        sliderMaster?.onValueChanged.AddListener(_settings.SetMasterVolume);
        sliderMusic?.onValueChanged.AddListener(_settings.SetMusicVolume);
        sliderSFX?.onValueChanged.AddListener(OnSFXSliderChanged);
        sliderUI?.onValueChanged.AddListener(_settings.SetUIVolume);
    }

    private void OnSFXSliderChanged(float value)
    {
        _settings.SetSFXVolume(value);
        if (previewSFX != null)
            AudioManager.Play(previewSFX);
    }

    /// <summary>Назначь на кнопку «По умолчанию» через OnClick.</summary>
    public void OnResetClicked()
    {
        if (_settings == null) return;
        _settings.ResetToDefaults();

        if (sliderMaster) sliderMaster.value = _settings.MasterVolume;
        if (sliderMusic)  sliderMusic.value  = _settings.MusicVolume;
        if (sliderSFX)    sliderSFX.value    = _settings.SFXVolume;
        if (sliderUI)     sliderUI.value     = _settings.UIVolume;
    }

    void OnDisable()
    {
        sliderMaster?.onValueChanged.RemoveAllListeners();
        sliderMusic?.onValueChanged.RemoveAllListeners();
        sliderSFX?.onValueChanged.RemoveAllListeners();
        sliderUI?.onValueChanged.RemoveAllListeners();
        _initialized = false;
    }
}
