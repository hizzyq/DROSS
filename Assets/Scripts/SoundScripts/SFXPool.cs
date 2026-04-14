using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Пул AudioSource для воспроизведения звуковых эффектов без лишних аллокаций.
/// Поддерживает 2D, 3D (в точке) и 3D (привязанный к трансформу).
/// 
/// Дополнительно:
/// - ограничивает слишком частый запуск одного и того же SFXEvent
/// - ограничивает количество одновременно играющих копий одного и того же SFXEvent
/// </summary>
public class SFXPool : MonoBehaviour
{
    [Header("Размер пула")]
    [Tooltip("Сколько AudioSource создать заранее.")]
    [SerializeField] private int initialSize = 20;

    [Tooltip("Разрешить рост пула сверх initialSize при нехватке.")]
    [SerializeField] private bool allowGrowth = true;

    [Header("Защита от наслоения одинаковых звуков")]
    [Tooltip("Минимальный глобальный интервал между одинаковыми SFXEvent.")]
    [SerializeField] private float sameEventInterval = 0.2f;

    [Tooltip("Максимум одновременно играющих копий одного и того же SFXEvent.")]
    [SerializeField] private int maxSameEventVoices = 1;

    // ─── Внутреннее состояние пула ───────────────────────────────────────

    private readonly Queue<AudioSource> _free = new Queue<AudioSource>();
    private readonly List<AudioSource> _busy = new List<AudioSource>();

    // Когда конкретный SFXEvent запускался в последний раз
    private readonly Dictionary<SFXEvent, float> _lastPlayTimeByEvent = new Dictionary<SFXEvent, float>();

    // Сколько копий конкретного SFXEvent сейчас играет
    private readonly Dictionary<SFXEvent, int> _activeVoicesByEvent = new Dictionary<SFXEvent, int>();

    // Какой SFXEvent привязан к конкретному AudioSource
    // Нужен, чтобы при возврате источника в пул уменьшить счётчик активных голосов
    private readonly Dictionary<AudioSource, SFXEvent> _eventBySource = new Dictionary<AudioSource, SFXEvent>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            _free.Enqueue(CreateSource());
    }

    void Update()
    {
        // Возвращаем в пул те, что доиграли
        for (int i = _busy.Count - 1; i >= 0; i--)
        {
            var src = _busy[i];
            if (src == null)
            {
                _busy.RemoveAt(i);
                continue;
            }

            if (!src.isPlaying)
            {
                ReleaseSource(src, i);
            }
        }
    }

    // ─── Публичный API ───────────────────────────────────────────────────

    /// <summary>2D-звук (UI, шаги, интерфейс и т.п.).</summary>
    public void Play(SFXEvent sfx)
    {
        if (sfx == null) return;

        // Если одинаковый звук недавно уже запускался
        // или уже достигнут лимит одновременных копий — не играем его повторно
        if (!CanPlay(sfx)) return;

        var src = Rent();
        if (src == null) return;

        Configure(src, sfx);
        src.spatialBlend = 0f; // Принудительно 2D

        RegisterPlay(src, sfx);
        src.Play();
    }

    /// <summary>3D-звук в фиксированной точке мира.</summary>
    public void PlayAt(SFXEvent sfx, Vector3 worldPos)
    {
        if (sfx == null) return;
        if (!CanPlay(sfx)) return;

        var src = Rent();
        if (src == null) return;

        Configure(src, sfx);
        src.transform.position = worldPos;

        RegisterPlay(src, sfx);
        src.Play();
    }

    /// <summary>3D-звук, движущийся вместе с объектом.</summary>
    public void PlayAttached(SFXEvent sfx, Transform parent)
    {
        if (sfx == null) return;
        if (!CanPlay(sfx)) return;

        var src = Rent();
        if (src == null) return;

        Configure(src, sfx);
        src.transform.SetParent(parent);
        src.transform.localPosition = Vector3.zero;

        RegisterPlay(src, sfx);
        src.Play();
    }

    /// <summary>Остановить и вернуть в пул все активные источники.</summary>
    public void StopAll()
    {
        for (int i = _busy.Count - 1; i >= 0; i--)
        {
            var src = _busy[i];
            if (src == null) continue;

            src.Stop();
            ReleaseSource(src, i);
        }
    }

    // ─── Логика защиты от наслоения ──────────────────────────────────────

    /// <summary>
    /// Проверяет, можно ли сейчас запускать этот SFXEvent.
    /// </summary>
    private bool CanPlay(SFXEvent sfx)
    {
        // 1. Проверяем глобальный интервал для одинакового SFXEvent
        if (_lastPlayTimeByEvent.TryGetValue(sfx, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < sameEventInterval)
                return false;
        }

        // 2. Проверяем лимит одновременно играющих копий
        if (_activeVoicesByEvent.TryGetValue(sfx, out int activeVoices))
        {
            if (activeVoices >= maxSameEventVoices)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Регистрирует запуск звука:
    /// - запоминает время запуска
    /// - увеличивает число активных копий
    /// - связывает AudioSource с SFXEvent
    /// </summary>
    private void RegisterPlay(AudioSource src, SFXEvent sfx)
    {
        _lastPlayTimeByEvent[sfx] = Time.time;

        if (_activeVoicesByEvent.ContainsKey(sfx))
            _activeVoicesByEvent[sfx]++;
        else
            _activeVoicesByEvent[sfx] = 1;

        _eventBySource[src] = sfx;
    }

    /// <summary>
    /// Возвращает AudioSource в пул и снимает его с учёта.
    /// </summary>
    private void ReleaseSource(AudioSource src, int busyIndex)
    {
        // Если знаем, какой SFXEvent играл на этом source —
        // уменьшаем счётчик активных копий
        if (_eventBySource.TryGetValue(src, out var sfx))
        {
            if (_activeVoicesByEvent.TryGetValue(sfx, out int activeVoices))
            {
                activeVoices--;

                if (activeVoices <= 0)
                    _activeVoicesByEvent.Remove(sfx);
                else
                    _activeVoicesByEvent[sfx] = activeVoices;
            }

            _eventBySource.Remove(src);
        }

        // Сброс состояния источника перед возвратом в пул
        src.Stop();
        src.clip = null;
        src.transform.SetParent(transform);
        src.transform.localPosition = Vector3.zero;
        src.gameObject.SetActive(false);

        _busy.RemoveAt(busyIndex);
        _free.Enqueue(src);
    }

    // ─── Вспомогательные методы ──────────────────────────────────────────

    private AudioSource Rent()
    {
        AudioSource src;

        if (_free.Count > 0)
        {
            src = _free.Dequeue();
        }
        else if (allowGrowth)
        {
            src = CreateSource();
            Debug.LogWarning("[SFXPool] Пул вырос. Рассмотри увеличение initialSize.");
        }
        else
        {
            Debug.LogWarning("[SFXPool] Пул исчерпан, звук пропущен.");
            return null;
        }

        src.gameObject.SetActive(true);
        _busy.Add(src);
        return src;
    }

    private static void Configure(AudioSource src, SFXEvent sfx)
    {
        src.clip = sfx.GetRandomClip();
        src.volume = sfx.GetVolume();
        src.pitch = sfx.GetPitch();
        src.spatialBlend = sfx.spatialBlend;
        src.minDistance = sfx.minDistance;
        src.maxDistance = sfx.maxDistance;
        src.priority = sfx.priority;
        src.outputAudioMixerGroup = sfx.mixerGroup;
        src.loop = false;
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("SFX_PooledSource");
        go.transform.SetParent(transform);
        go.SetActive(false);
        return go.AddComponent<AudioSource>();
    }
}