using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Пул AudioSource для воспроизведения звуковых эффектов без аллоцирования.
/// Поддерживает 2D, 3D (в точке) и 3D (привязанный к трансформу).
/// </summary>
public class SFXPool : MonoBehaviour
{
    [Header("Размер пула")]
    [Tooltip("Сколько AudioSource создать заранее.")]
    [SerializeField] private int initialSize = 20;

    [Tooltip("Разрешить рост пула сверх initialSize при нехватке.")]
    [SerializeField] private bool allowGrowth = true;

    // ─── Внутреннее состояние ────────────────────────────────────────────

    private readonly Queue<AudioSource> _free = new Queue<AudioSource>();
    private readonly List<AudioSource>  _busy = new List<AudioSource>();

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
            if (src == null) { _busy.RemoveAt(i); continue; }

            if (!src.isPlaying)
            {
                src.transform.SetParent(transform);
                src.transform.localPosition = Vector3.zero;
                src.gameObject.SetActive(false);
                _busy.RemoveAt(i);
                _free.Enqueue(src);
            }
        }
    }

    // ─── Публичный API ───────────────────────────────────────────────────

    /// <summary>2D-звук (UI, шаги вида от первого лица и т.п.).</summary>
    public void Play(SFXEvent sfx)
    {
        if (sfx == null) return;
        var src = Rent();
        if (src == null) return;

        Configure(src, sfx);
        src.spatialBlend = 0f; // 2D
        src.Play();
    }

    /// <summary>3D-звук в фиксированной точке мира (взрыв, пуля в стену).</summary>
    public void PlayAt(SFXEvent sfx, Vector3 worldPos)
    {
        if (sfx == null) return;
        var src = Rent();
        if (src == null) return;

        Configure(src, sfx);
        src.transform.position = worldPos;
        src.Play();
    }

    /// <summary>3D-звук, движущийся вместе с объектом (выстрел из пушки на турели).</summary>
    public void PlayAttached(SFXEvent sfx, Transform parent)
    {
        if (sfx == null) return;
        var src = Rent();
        if (src == null) return;

        Configure(src, sfx);
        src.transform.SetParent(parent);
        src.transform.localPosition = Vector3.zero;
        src.Play();
    }

    /// <summary>Остановить и вернуть в пул все активные источники.</summary>
    public void StopAll()
    {
        foreach (var src in _busy)
        {
            if (src == null) continue;
            src.Stop();
            src.transform.SetParent(transform);
            src.gameObject.SetActive(false);
            _free.Enqueue(src);
        }
        _busy.Clear();
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
        src.clip             = sfx.GetRandomClip();
        src.volume           = sfx.GetVolume();
        src.pitch            = sfx.GetPitch();
        src.spatialBlend     = sfx.spatialBlend;
        src.minDistance      = sfx.minDistance;
        src.maxDistance      = sfx.maxDistance;
        src.priority         = sfx.priority;
        src.outputAudioMixerGroup = sfx.mixerGroup;
        src.loop             = false;
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("SFX_PooledSource");
        go.transform.SetParent(transform);
        go.SetActive(false);
        return go.AddComponent<AudioSource>();
    }
}
