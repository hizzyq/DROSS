using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Система адаптивной музыки в стиле DOOM.
/// 
/// Особенности:
/// — Треки имеют структуру intro → loop → outro.
/// — Несколько параллельных стемов (слоёв), плавно появляющихся с ростом интенсивности.
/// — Crossfade при переходе между состояниями.
/// — Каждое MusicState → свой MusicTrack.
/// </summary>
public class MusicSystem : MonoBehaviour
{
    [Header("Маппинг состояний → треки")]
    [Tooltip("Сопоставление каждого состояния с нужным треком.")]
    public StateMusicPair[] stateTracks;

    [Header("Микшер")]
    [Tooltip("Exposed параметр в AudioMixer для группы Music. Используется при crossfade.")]
    public AudioMixerGroup musicMixerGroup;

    // ─── Внутренние поля ────────────────────────────────────────────────

    // Два «слота» AudioSource для плавного crossfade
    private AudioSource[] _baseSlots;       // основной трек (intro + loop)
    private List<AudioSource> _stemSources; // стемы текущего трека

    private int _activeSlot = 0;

    private MusicState _currentState = MusicState.None;
    private MusicTrack _currentTrack;
    private Coroutine _transitionCoroutine;
    private Coroutine _intensityCoroutine;

    private float _intensity = 0f;

    // ─── Инициализация ──────────────────────────────────────────────────

    void Awake()
    {
        _baseSlots  = new AudioSource[2];
        _stemSources = new List<AudioSource>();

        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject($"MusicSlot_{i}");
            go.transform.SetParent(transform);
            _baseSlots[i] = go.AddComponent<AudioSource>();
            _baseSlots[i].playOnAwake = false;
            _baseSlots[i].volume = 0f;
            _baseSlots[i].outputAudioMixerGroup = musicMixerGroup;
        }
    }

    // ─── Публичный API ──────────────────────────────────────────────────

    /// <summary>Плавно переключить музыкальное состояние.</summary>
    public void TransitionTo(MusicState state)
    {
        if (state == _currentState) return;

        var track = FindTrack(state);
        if (track == null)
        {
            Debug.LogWarning($"[MusicSystem] Трек для состояния {state} не задан.");
            return;
        }

        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        _transitionCoroutine = StartCoroutine(CrossfadeTo(state, track));
    }

    /// <summary>Задать интенсивность боя [0..1]. Стемы реагируют плавно.</summary>
    public void SetIntensity(float value)
    {
        _intensity = Mathf.Clamp01(value);
        ApplyStemVolumes(_intensity);
    }

    /// <summary>Плавно изменить интенсивность за заданное время.</summary>
    public void SmoothSetIntensity(float target, float duration = 1.5f)
    {
        if (_intensityCoroutine != null)
            StopCoroutine(_intensityCoroutine);
        _intensityCoroutine = StartCoroutine(SmoothIntensityCoroutine(target, duration));
    }

    public MusicState CurrentState => _currentState;
    public float CurrentIntensity  => _intensity;

    // ─── Внутренняя логика ──────────────────────────────────────────────

    private IEnumerator CrossfadeTo(MusicState newState, MusicTrack newTrack)
    {
        int incomingSlot = 1 - _activeSlot;
        AudioSource outgoing = _baseSlots[_activeSlot];
        AudioSource incoming = _baseSlots[incomingSlot];

        float fadeDuration = newTrack.crossfadeDuration;
        float elapsed = 0f;
        float startVolume = outgoing.volume;

        // Подготовить входящий слот
        incoming.clip   = newTrack.intro != null ? newTrack.intro : newTrack.loop;
        incoming.loop   = newTrack.intro == null; // если нет intro — сразу зацикливаем
        incoming.volume = 0f;
        incoming.Play();

        // Crossfade
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            outgoing.volume = Mathf.Lerp(startVolume, 0f, t);
            incoming.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        outgoing.Stop();
        outgoing.volume = 0f;
        incoming.volume = 1f;

        _activeSlot  = incomingSlot;
        _currentState = newState;
        _currentTrack = newTrack;

        // Если был intro — дождаться его окончания и переключить на loop
        if (newTrack.intro != null)
            StartCoroutine(WaitForIntroThenLoop(incoming, newTrack));

        // Запустить стемы
        StartCoroutine(SpawnStems(newTrack));
    }

    /// <summary>Дождаться окончания intro, затем запустить зацикленный loop.</summary>
    private IEnumerator WaitForIntroThenLoop(AudioSource src, MusicTrack track)
    {
        // Ждём, пока intro почти закончится
        float remaining = track.intro.length - src.time;
        if (remaining > 0.05f)
            yield return new WaitForSeconds(remaining - 0.05f);

        if (_currentTrack != track) yield break; // трек сменился, выходим

        src.clip = track.loop;
        src.loop = true;
        src.Play();
    }

    /// <summary>Создать AudioSource-объекты для каждого стема нового трека.</summary>
    private IEnumerator SpawnStems(MusicTrack track)
    {
        // Убрать старые стемы
        foreach (var s in _stemSources)
        {
            if (s != null) Destroy(s.gameObject);
        }
        _stemSources.Clear();

        if (track.stems == null || track.stems.Length == 0) yield break;

        // Дождаться начала loop (мы уже в нём или в intro)
        // Стемы запускаем синхронно с основным источником
        AudioSource master = _baseSlots[_activeSlot];

        foreach (var def in track.stems)
        {
            if (def.clip == null) continue;

            var go = new GameObject($"Stem_{def.label}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = musicMixerGroup;
            src.clip       = def.clip;
            src.loop       = true;
            src.playOnAwake = false;
            src.volume     = 0f;

            // Синхронизируем позицию с мастером
            src.timeSamples = master.timeSamples % def.clip.samples;
            src.Play();

            _stemSources.Add(src);
        }

        // Применить текущую интенсивность
        ApplyStemVolumes(_intensity);
    }

    private void ApplyStemVolumes(float intensity)
    {
        if (_currentTrack == null || _currentTrack.stems == null) return;

        for (int i = 0; i < _stemSources.Count && i < _currentTrack.stems.Length; i++)
        {
            var def = _currentTrack.stems[i];
            var src = _stemSources[i];
            if (src == null) continue;

            float target = 0f;
            if (intensity >= def.thresholdMin)
            {
                float range = Mathf.Max(0.001f, def.thresholdMax - def.thresholdMin);
                float t = Mathf.Clamp01((intensity - def.thresholdMin) / range);
                target = t * def.maxVolume;
            }

            // Плавный переход громкости стема
            StopCoroutine(nameof(FadeStemVolume));
            StartCoroutine(FadeStemVolume(src, target, 0.4f));
        }
    }

    private IEnumerator FadeStemVolume(AudioSource src, float target, float duration)
    {
        float start   = src.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (src == null) yield break;
            elapsed += Time.deltaTime;
            src.volume = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        if (src != null) src.volume = target;
    }

    private IEnumerator SmoothIntensityCoroutine(float target, float duration)
    {
        float start   = _intensity;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed   += Time.deltaTime;
            _intensity = Mathf.Lerp(start, target, elapsed / duration);
            ApplyStemVolumes(_intensity);
            yield return null;
        }
        _intensity = target;
        ApplyStemVolumes(_intensity);
    }

    private MusicTrack FindTrack(MusicState state)
    {
        foreach (var pair in stateTracks)
            if (pair.state == state) return pair.track;
        return null;
    }
}

[System.Serializable]
public class StateMusicPair
{
    public MusicState state;
    public MusicTrack track;
}
