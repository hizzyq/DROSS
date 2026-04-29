using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("— Sound —")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Текст значений (необязательно)")]
    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI musicValueText;
    public TextMeshProUGUI sfxValueText;
    public TextMeshProUGUI sensXValueText;
    public TextMeshProUGUI sensYValueText;
    public TextMeshProUGUI pixelationValueText;
    public TextMeshProUGUI brightnessValueText;
    public TextMeshProUGUI fovValueText;

    [Header("— Controls —")]
    public Slider sensXSlider;
    public Slider sensYSlider;
    public Toggle invertYToggle;

    [Header("— Graphics —")]
    public Slider pixelationSlider;
    public Slider brightnessSlider;
    public Slider fovSlider;

    [Header("— Кнопки —")]
    public Button saveButton;
    public Button resetButton;

    private GameSettings _s;

    private void OnEnable()
    {
        // Обновляем UI каждый раз, когда открываем меню настроек
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (SettingsManager.Instance == null) return;
        _s = SettingsManager.Instance.Get();

        // Сначала ОТПИСЫВАЕМСЯ — чтобы не было дублей при повторном открытии
        RemoveAllListeners();

        // Устанавливаем значения БЕЗ триггера onValueChanged
        SetSliderSilent(masterSlider,     _s.masterVolume);
        SetSliderSilent(musicSlider,      _s.musicVolume);
        SetSliderSilent(sfxSlider,        _s.sfxVolume);
        SetSliderSilent(sensXSlider,      _s.sensitivityX);
        SetSliderSilent(sensYSlider,      _s.sensitivityY);
        SetSliderSilent(pixelationSlider, _s.pixelation);
        SetSliderSilent(fovSlider, _s.fov);
        SetSliderSilent(brightnessSlider, _s.brightness);

        if (invertYToggle) invertYToggle.SetIsOnWithoutNotify(_s.invertY);

        // Обновляем текст
        UpdateAllTexts();

        // Теперь подписываемся
        masterSlider?.onValueChanged.AddListener(v => {
            _s.masterVolume = v;
            SettingsManager.Instance.ApplyAudio();
            UpdateText(masterValueText, v, "0%");
        });
        musicSlider?.onValueChanged.AddListener(v => {
            _s.musicVolume = v;
            SettingsManager.Instance.ApplyAudio();
            UpdateText(musicValueText, v, "0%");
        });
        sfxSlider?.onValueChanged.AddListener(v => {
            _s.sfxVolume = v;
            SettingsManager.Instance.ApplyAudio();
            UpdateText(sfxValueText, v, "0%");
        });
        sensXSlider?.onValueChanged.AddListener(v => {
            _s.sensitivityX = v;
            UpdateText(sensXValueText, v, "0");
        });
        sensYSlider?.onValueChanged.AddListener(v => {
            _s.sensitivityY = v;
            UpdateText(sensYValueText, v, "0");
        });
        pixelationSlider?.onValueChanged.AddListener(v => {
            _s.pixelation = Mathf.RoundToInt(v);
            SettingsManager.Instance.ApplyGraphics();
            if (pixelationValueText)
                pixelationValueText.text = Mathf.RoundToInt(v).ToString();
        });
        fovSlider?.onValueChanged.AddListener(v => {
            _s.fov = Mathf.RoundToInt(v);
            SettingsManager.Instance.ApplyGraphics();
            if (fovValueText)
                fovValueText.text = Mathf.RoundToInt(v).ToString();
        });
        brightnessSlider?.onValueChanged.AddListener(v => {
            _s.brightness = v;
            SettingsManager.Instance.ApplyGraphics();
            UpdateText(brightnessValueText, v, "0%");
        });

        invertYToggle?.onValueChanged.AddListener(v => _s.invertY = v);

        saveButton?.onClick.AddListener(OnSave);
        resetButton?.onClick.AddListener(OnReset);
    }

    private void OnDisable()
    {
        RemoveAllListeners();
    }

    // Ключевой метод — устанавливает значение БЕЗ вызова onValueChanged
    private void SetSliderSilent(Slider slider, float value)
    {
        if (slider == null) return;
        slider.SetValueWithoutNotify(value);
    }

    private void RemoveAllListeners()
    {
        masterSlider?.onValueChanged.RemoveAllListeners();
        musicSlider?.onValueChanged.RemoveAllListeners();
        sfxSlider?.onValueChanged.RemoveAllListeners();
        sensXSlider?.onValueChanged.RemoveAllListeners();
        sensYSlider?.onValueChanged.RemoveAllListeners();
        pixelationSlider?.onValueChanged.RemoveAllListeners();
        invertYToggle?.onValueChanged.RemoveAllListeners();
        saveButton?.onClick.RemoveAllListeners();
        resetButton?.onClick.RemoveAllListeners();
        fovSlider?.onValueChanged.RemoveAllListeners();
        brightnessSlider?.onValueChanged.RemoveAllListeners();
    }

    private void UpdateAllTexts()
    {
        UpdateText(masterValueText,     _s.masterVolume,  "0%");
        UpdateText(musicValueText,      _s.musicVolume,   "0%");
        UpdateText(sfxValueText,        _s.sfxVolume,     "0%");
        UpdateText(sensXValueText,      _s.sensitivityX,  "0");
        UpdateText(sensYValueText,      _s.sensitivityY,  "0");
        if (pixelationValueText)
            pixelationValueText.text = _s.pixelation.ToString();
        if (fovValueText)
            fovValueText.text = _s.fov.ToString();
        UpdateText(brightnessValueText, _s.brightness, "0%");
    }

    private void UpdateText(TextMeshProUGUI label, float value, string format)
    {
        if (label == null) return;
        // "0%" → показывает 0..100%, "0" → просто число
        if (format == "0%")
            label.text = Mathf.RoundToInt(value * 100f) + "%";
        else
            label.text = Mathf.RoundToInt(value).ToString();
    }

    private void Start()
    {
        // Гарантируем, что UI обновится при старте сцены
        RefreshUI();
    }

    private void OnSave()
    {
        SettingsManager.Instance.Save();
    }

    private void OnReset()
    {
        _s.ResetToDefault();
        SettingsManager.Instance.Apply();
        OnEnable();
    }
}