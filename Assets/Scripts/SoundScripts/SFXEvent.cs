using UnityEngine;
using UnityEngine.Audio;

// ═══════════════════════════════════════════════════════════════════════════
//  SFXEvent — ScriptableObject для настройки одного звукового события.
//  Создание: ПКМ → Create → Audio → SFX Event
// ═══════════════════════════════════════════════════════════════════════════

[CreateAssetMenu(menuName = "Audio/SFX Event", fileName = "SFXEvent")]
public class SFXEvent : ScriptableObject
{
    [Header("Клипы")]
    [Tooltip("Один или несколько клипов. Будет выбран случайный.")]
    public AudioClip[] clips;

    [Header("Громкость и питч")]
    [Range(0f, 1f)] public float volume = 1f;

    [Tooltip("Разброс громкости ±")]
    [Range(0f, 0.5f)] public float volumeVariance = 0.05f;

    [Range(0.1f, 3f)] public float pitch = 1f;

    [Tooltip("Разброс питча ±")]
    [Range(0f, 0.5f)] public float pitchVariance = 0.05f;

    [Header("Пространство")]
    [Tooltip("0 = 2D (UI, музыка), 1 = полностью 3D.")]
    [Range(0f, 1f)] public float spatialBlend = 1f;

    public float minDistance = 1f;
    public float maxDistance = 30f;

    [Header("Микшер")]
    public AudioMixerGroup mixerGroup;

    [Header("Приоритет")]
    [Range(0, 256)] public int priority = 128;

    [Header("Anti-overlap")]
    [Tooltip("Если включено — для этого звука будет ограничение от наслоения.")]
    public bool limitOverlap = false;

    [Tooltip("Минимальный интервал между одинаковыми проигрываниями этого события.")]
    [Min(0f)] public float minInterval = 0f;

    [Tooltip("Максимум одновременно играющих копий этого события.")]
    [Min(1)] public int maxVoices = 8;

    // ── Вспомогательные методы ──────────────────────────────────────────

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetVolume() => Mathf.Clamp01(volume + Random.Range(-volumeVariance, volumeVariance));
    public float GetPitch() => pitch + Random.Range(-pitchVariance, pitchVariance);
}