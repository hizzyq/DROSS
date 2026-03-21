using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Хранит и применяет настройки громкости.
/// Связывает слайдеры UI ↔ exposed-параметры AudioMixer.
/// Сохранение — через PlayerPrefs.
/// </summary>
public class AudioSettings : MonoBehaviour
{
    // ─── Exposed-параметры AudioMixer (задай их в Inspector) ─────────────
    [Header("AudioMixer параметры (Exposed)")]
    [Tooltip("Имя exposed параметра мастер-громкости.")]
    public string paramMaster = "MasterVolume";
    public string paramMusic  = "MusicVolume";
    public string paramSFX    = "SFXVolume";
    public string paramUI     = "UIVolume";

    // ─── Значения по умолчанию ───────────────────────────────────────────
    [Header("Значения по умолчанию [0..1]")]
    [Range(0f, 1f)] public float defaultMaster = 1.0f;
    [Range(0f, 1f)] public float defaultMusic  = 0.8f;
    [Range(0f, 1f)] public float defaultSFX    = 1.0f;
    [Range(0f, 1f)] public float defaultUI     = 0.9f;

    // ─── Публичные свойства ──────────────────────────────────────────────
    public float MasterVolume { get; private set; }
    public float MusicVolume  { get; private set; }
    public float SFXVolume    { get; private set; }
    public float UIVolume     { get; private set; }

    // ─── Ключи PlayerPrefs ───────────────────────────────────────────────
    private const string KeyMaster = "audio_master";
    private const string KeyMusic  = "audio_music";
    private const string KeySFX    = "audio_sfx";
    private const string KeyUI     = "audio_ui";

    private AudioMixer _mixer;

    // ─── Инициализация ───────────────────────────────────────────────────

    public void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(KeyMaster, defaultMaster);
        MusicVolume  = PlayerPrefs.GetFloat(KeyMusic,  defaultMusic);
        SFXVolume    = PlayerPrefs.GetFloat(KeySFX,    defaultSFX);
        UIVolume     = PlayerPrefs.GetFloat(KeyUI,     defaultUI);
    }

    public void Apply(AudioMixer mixer)
    {
        _mixer = mixer;
        SetMasterVolumeInternal(MasterVolume);
        SetMusicVolumeInternal(MusicVolume);
        SetSFXVolumeInternal(SFXVolume);
        SetUIVolumeInternal(UIVolume);
    }

    // ─── Публичные сеттеры (вызываются из UI) ───────────────────────────

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        SetMasterVolumeInternal(MasterVolume);
        Save();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        SetMusicVolumeInternal(MusicVolume);
        Save();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        SetSFXVolumeInternal(SFXVolume);
        Save();
    }

    public void SetUIVolume(float value)
    {
        UIVolume = Mathf.Clamp01(value);
        SetUIVolumeInternal(UIVolume);
        Save();
    }

    /// <summary>Сбросить все настройки к значениям по умолчанию.</summary>
    public void ResetToDefaults()
    {
        SetMasterVolume(defaultMaster);
        SetMusicVolume(defaultMusic);
        SetSFXVolume(defaultSFX);
        SetUIVolume(defaultUI);
    }

    // ─── Внутренние методы ───────────────────────────────────────────────

    /// <summary>AudioMixer принимает громкость в децибелах. Конвертируем [0..1] → dB.</summary>
    private float LinearToDb(float linear) =>
        linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;

    private void SetMasterVolumeInternal(float v) =>
        _mixer?.SetFloat(paramMaster, LinearToDb(v));

    private void SetMusicVolumeInternal(float v) =>
        _mixer?.SetFloat(paramMusic, LinearToDb(v));

    private void SetSFXVolumeInternal(float v) =>
        _mixer?.SetFloat(paramSFX, LinearToDb(v));

    private void SetUIVolumeInternal(float v) =>
        _mixer?.SetFloat(paramUI, LinearToDb(v));

    private void Save()
    {
        PlayerPrefs.SetFloat(KeyMaster, MasterVolume);
        PlayerPrefs.SetFloat(KeyMusic,  MusicVolume);
        PlayerPrefs.SetFloat(KeySFX,    SFXVolume);
        PlayerPrefs.SetFloat(KeyUI,     UIVolume);
        PlayerPrefs.Save();
    }
}
