using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using Visual;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameSettings settings;
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private AudioMixer audioMixer;

    private PixelationFeature _pixelationFeature;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Находим PixelationFeature
        foreach (var f in rendererData.rendererFeatures)
            if (f is PixelationFeature pf) { _pixelationFeature = pf; break; }

        settings.Load();
    }

    // Применить все настройки сразу
    public void Apply()
    {
        ApplyGraphics();
        ApplyAudio();
        // Чувствительность читается напрямую через settings в PlayerCam
    }

    private void Start()
    {
        // В Start() AudioMixer уже полностью готов принимать значения громкости.
        Apply();
    }

    public void ApplyGraphics()
    {
        if (_pixelationFeature == null) return;
        _pixelationFeature.settings.verticalPixels = settings.pixelation;
        rendererData.SetDirty();
    }

    public void ApplyAudio()
    {
        // AudioMixer принимает децибелы — конвертируем из 0..1
        audioMixer.SetFloat("MasterVolume", VolumeToDb(settings.masterVolume));
        audioMixer.SetFloat("MusicVolume",  VolumeToDb(settings.musicVolume));
        audioMixer.SetFloat("SFXVolume",    VolumeToDb(settings.sfxVolume));
    }

    public void Save() => settings.Save();

    // Утилита: 0..1 → децибелы (-80 .. 0)
    private float VolumeToDb(float value)
        => value > 0.001f ? Mathf.Log10(value) * 20f : -80f;

    // Геттер для PlayerCam и других скриптов
    public GameSettings Get() => settings;
}