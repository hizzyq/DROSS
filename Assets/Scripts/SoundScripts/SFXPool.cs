using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Пул AudioSource для воспроизведения звуковых эффектов без аллокаций.
/// Поддерживает 2D, 3D (в точке) и 3D (привязанный к трансформу).
///
/// Ограничение от наслоения применяется только к зомбиным звукам:
/// - SFX_Zombie_Attack
/// - SFX_Zombie_Chase
/// - SFX_Zombie_Death
/// - SFX_Zombie_Hurt
/// - SFX_Zombie_Walk
///
/// Остальные звуки (оружие, игрок и т.д.) не ограничиваются.
/// </summary>
public class SFXPool : MonoBehaviour
{
    [Header("Размер пула")]
    [Tooltip("Сколько AudioSource создать заранее.")]
    [SerializeField] private int initialSize = 20;

    [Tooltip("Разрешить рост пула сверх initialSize при нехватке.")]
    [SerializeField] private bool allowGrowth = true;

    private readonly Queue<AudioSource> _free = new Queue<AudioSource>();
    private readonly List<AudioSource> _busy = new List<AudioSource>();

    // Время последнего проигрывания группы
    private readonly Dictionary<string, float> _lastPlayTimeByKey = new Dictionary<string, float>();

    // Сколько копий группы сейчас играет
    private readonly Dictionary<string, int> _activeVoicesByKey = new Dictionary<string, int>();

    // Какая группа была назначена конкретному source
    private readonly Dictionary<AudioSource, string> _keyBySource = new Dictionary<AudioSource, string>();

    // Лимиты только для зомбиных групп
    private readonly Dictionary<string, (float minInterval, int maxVoices)> _groupLimits =
        new Dictionary<string, (float minInterval, int maxVoices)>
        {
            { "zombie_attack", (0.08f, 3) },
            { "zombie_chase",  (2f, 2) },
            { "zombie_hurt",   (3f, 2) },
            { "zombie_walk",   (0.12f, 2) },
            { "zombie_death",  (0.05f, 4) },
            { "zombie_other",  (2f, 2) },
        };

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            _free.Enqueue(CreateSource());
    }

    private void Update()
    {
        for (int i = _busy.Count - 1; i >= 0; i--)
        {
            var src = _busy[i];

            if (src == null)
            {
                _busy.RemoveAt(i);
                continue;
            }

            if (!src.isPlaying)
                ReleaseSource(src, i);
        }
    }

    /// <summary>2D-звук.</summary>
    public void Play(SFXEvent sfx)
    {
        if (sfx == null) return;

        string overlapKey = GetZombieOverlapKey(sfx);
        if (!CanPlay(overlapKey)) return;

        var src = Rent();
        if (src == null) return;

        if (!Configure(src, sfx))
        {
            ReleaseUnconfiguredSource(src);
            return;
        }

        src.spatialBlend = 0f; // Принудительно 2D

        RegisterPlay(src, overlapKey);
        src.Play();
    }

    /// <summary>3D-звук в фиксированной точке мира.</summary>
    public void PlayAt(SFXEvent sfx, Vector3 worldPos)
    {
        if (sfx == null) return;

        string overlapKey = GetZombieOverlapKey(sfx);
        if (!CanPlay(overlapKey)) return;

        var src = Rent();
        if (src == null) return;

        if (!Configure(src, sfx))
        {
            ReleaseUnconfiguredSource(src);
            return;
        }

        src.transform.SetParent(transform, false);
        src.transform.position = worldPos;

        RegisterPlay(src, overlapKey);
        src.Play();
    }

    /// <summary>3D-звук, движущийся вместе с объектом.</summary>
    public void PlayAttached(SFXEvent sfx, Transform parent)
    {
        if (sfx == null) return;
        if (parent == null) return;

        string overlapKey = GetZombieOverlapKey(sfx);
        if (!CanPlay(overlapKey)) return;

        var src = Rent();
        if (src == null) return;

        if (!Configure(src, sfx))
        {
            ReleaseUnconfiguredSource(src);
            return;
        }

        src.transform.SetParent(parent, false);
        src.transform.localPosition = Vector3.zero;

        RegisterPlay(src, overlapKey);
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

    /// <summary>
    /// Возвращает ключ группы только для зомбиных звуков.
    /// Для остальных возвращает null, и ограничения не применяются.
    /// </summary>
    private string GetZombieOverlapKey(SFXEvent sfx)
    {
        if (sfx == null)
            return null;

        string sfxName = sfx.name.ToLowerInvariant();

        if (!sfxName.Contains("zombie"))
            return null;

        if (sfxName.Contains("attack"))
            return "zombie_attack";

        if (sfxName.Contains("chase"))
            return "zombie_chase";

        if (sfxName.Contains("hurt"))
            return "zombie_hurt";

        if (sfxName.Contains("walk"))
            return "zombie_walk";

        if (sfxName.Contains("death"))
            return "zombie_death";

        return "zombie_other";
    }

    /// <summary>
    /// Если overlapKey == null, ограничение не применяется.
    /// </summary>
    private bool CanPlay(string overlapKey)
    {
        if (string.IsNullOrEmpty(overlapKey))
            return true;

        if (!_groupLimits.TryGetValue(overlapKey, out var limits))
            return true;

        float minInterval = limits.minInterval;
        int maxVoices = Mathf.Max(1, limits.maxVoices);

        if (_lastPlayTimeByKey.TryGetValue(overlapKey, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < minInterval)
                return false;
        }

        if (_activeVoicesByKey.TryGetValue(overlapKey, out int activeVoices))
        {
            if (activeVoices >= maxVoices)
                return false;
        }

        return true;
    }

    private void RegisterPlay(AudioSource src, string overlapKey)
    {
        if (string.IsNullOrEmpty(overlapKey))
            return;

        _lastPlayTimeByKey[overlapKey] = Time.time;

        if (_activeVoicesByKey.ContainsKey(overlapKey))
            _activeVoicesByKey[overlapKey]++;
        else
            _activeVoicesByKey[overlapKey] = 1;

        _keyBySource[src] = overlapKey;
    }

    private void ReleaseSource(AudioSource src, int busyIndex)
    {
        if (_keyBySource.TryGetValue(src, out var overlapKey))
        {
            if (_activeVoicesByKey.TryGetValue(overlapKey, out int activeVoices))
            {
                activeVoices--;

                if (activeVoices <= 0)
                    _activeVoicesByKey.Remove(overlapKey);
                else
                    _activeVoicesByKey[overlapKey] = activeVoices;
            }

            _keyBySource.Remove(src);
        }

        ResetSource(src);

        _busy.RemoveAt(busyIndex);
        _free.Enqueue(src);
    }

    private void ReleaseUnconfiguredSource(AudioSource src)
    {
        ResetSource(src);
        _busy.Remove(src);
        _free.Enqueue(src);
    }

    private AudioSource Rent()
    {
        AudioSource src = null;

        // Цикл, пока не найдём живой объект или пока очередь не опустеет
        while (_free.Count > 0)
        {
            src = _free.Dequeue();
            if (src != null) break; 
        }

        // Если в очереди не осталось живых объектов, создаём новый (если разрешено)
        if (src == null)
        {
            if (allowGrowth)
            {
                src = CreateSource();
                Debug.LogWarning("[SFXPool] Пул вырос или восстановился после уничтожения объектов.");
            }
            else
            {
                Debug.LogWarning("[SFXPool] Пул исчерпан или объекты уничтожены, звук пропущен.");
                return null;
            }
        }

        src.gameObject.SetActive(true);
        _busy.Add(src);
        return src;
    }

    private static bool Configure(AudioSource src, SFXEvent sfx)
    {
        src.clip = sfx.GetRandomClip();
        if (src.clip == null)
            return false;

        src.volume = sfx.GetVolume();
        src.pitch = sfx.GetPitch();
        src.spatialBlend = sfx.spatialBlend;
        src.minDistance = sfx.minDistance;
        src.maxDistance = sfx.maxDistance;
        src.priority = sfx.priority;
        src.outputAudioMixerGroup = sfx.mixerGroup;
        src.loop = false;

        return true;
    }

    private void ResetSource(AudioSource src)
    {
        src.Stop();
        src.clip = null;
        src.transform.SetParent(transform, false);
        src.transform.localPosition = Vector3.zero;
        src.gameObject.SetActive(false);
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("SFX_PooledSource");
        go.transform.SetParent(transform, false);
        go.SetActive(false);
        return go.AddComponent<AudioSource>();
    }
}