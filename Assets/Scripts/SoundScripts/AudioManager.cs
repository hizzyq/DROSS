using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Главный синглтон аудиосистемы. Не уничтожается при смене сцен.
/// Точка входа для всех звуковых запросов из игрового кода.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer masterMixer;

    [Header("Sub-systems")]
    public MusicSystem Music { get; private set; }
    public SFXPool SFX { get; private set; }
    public AudioSettings Settings { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Music    = GetComponentInChildren<MusicSystem>();
        SFX      = GetComponentInChildren<SFXPool>();
        Settings = GetComponentInChildren<AudioSettings>();

        Settings.Load();
        Settings.Apply(masterMixer);
    }

    // ── Удобные шорткаты ─────────────────────────────────────────────────

    /// <summary>Переключить музыкальное состояние (например, Combat → Explore).</summary>
    public static void SetMusicState(MusicState state) =>
        Instance?.Music.TransitionTo(state);

    /// <summary>Задать интенсивность боя [0..1]. Влияет на активные стемы.</summary>
    public static void SetCombatIntensity(float t) =>
        Instance?.Music.SetIntensity(t);

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>Сыграть 2D-звук по SFXEvent.</summary>
    public static void Play(SFXEvent sfx) =>
        Instance?.SFX.Play(sfx);

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>Сыграть 3D-звук в точке мира.</summary>
    public static void PlayAt(SFXEvent sfx, Vector3 worldPos) =>
        Instance?.SFX.PlayAt(sfx, worldPos);

    /// <summary>Сыграть 3D-звук, привязанный к трансформу (движущийся источник).</summary>
    public static void PlayAttached(SFXEvent sfx, Transform parent) =>
        Instance?.SFX.PlayAttached(sfx, parent);
}
