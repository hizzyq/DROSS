using UnityEngine;
using UnityEngine.UI;

// ═══════════════════════════════════════════════════════════════════════════
//  MusicTrigger — Box/Sphere-коллайдер для смены состояния музыки.
//  Добавляй на пустой объект в уровне вместе с Collider (isTrigger = true).
// ═══════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Collider))]
public class MusicTrigger : MonoBehaviour
{
    [Tooltip("Состояние, на которое переключится музыка при входе.")]
    public MusicState onEnterState = MusicState.CombatLow;

    [Tooltip("Вернуться к этому состоянию при выходе. None = не менять.")]
    public MusicState onExitState  = MusicState.Explore;

    [Tooltip("Тег объекта, который активирует триггер (обычно Player).")]
    public string playerTag = "Player";

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        AudioManager.SetMusicState(onEnterState);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (onExitState != MusicState.None)
            AudioManager.SetMusicState(onExitState);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.25f);
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position, sphere.radius);
        }
    }
#endif
}


// ═══════════════════════════════════════════════════════════════════════════
//  IntensityController — автоматически управляет интенсивностью музыки
//  на основе количества врагов в радиусе или HP игрока.
//
//  Пример: чем больше живых врагов рядом — тем выше интенсивность стемов.
// ═══════════════════════════════════════════════════════════════════════════

public class IntensityController : MonoBehaviour
{
    [Header("Радиус поиска врагов")]
    public float detectionRadius = 30f;
    public LayerMask enemyLayer;

    [Header("Диапазон врагов → интенсивность")]
    [Tooltip("При скольких врагах интенсивность = 1.")]
    public int maxEnemiesForMaxIntensity = 5;

    [Header("Обновление")]
    public float updateInterval = 0.5f;   // раз в 0.5 с
    public float smoothTime     = 1.5f;   // время плавного перехода

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position, detectionRadius,
            _colliderBuffer, enemyLayer);

        float target = Mathf.Clamp01((float)count / maxEnemiesForMaxIntensity);
        AudioManager.Instance?.Music.SmoothSetIntensity(target, smoothTime);
    }

    private readonly Collider[] _colliderBuffer = new Collider[32];

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.15f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }
#endif
}


// ═══════════════════════════════════════════════════════════════════════════
//  AudioSettingsUI — связывает слайдеры Unity UI с AudioSettings.
//  Назначай в Inspector объекты Slider для каждого канала.
// ═══════════════════════════════════════════════════════════════════════════

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Слайдеры (назначь в Inspector)")]
    public Slider sliderMaster;
    public Slider sliderMusic;
    public Slider sliderSFX;
    public Slider sliderUI;

    [Header("Тестовый SFX для превью")]
    [Tooltip("Этот звук сыграет при отпускании слайдера SFX.")]
    public SFXEvent previewSFX;

    private AudioSettings _settings;
    private bool _initialized;

    void OnEnable()
    {
        // Ждём пока AudioManager не будет готов
        if (AudioManager.Instance == null) return;
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _settings = AudioManager.Instance.Settings;

        // Установить начальные значения слайдеров
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
        // Превью: играем тестовый звук чтобы игрок слышал результат
        if (previewSFX != null)
            AudioManager.Play(previewSFX);
    }

    /// <summary>Вызывается кнопкой «По умолчанию» в UI.</summary>
    public void OnResetClicked()
    {
        _settings?.ResetToDefaults();

        // Обновить слайдеры
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


// ═══════════════════════════════════════════════════════════════════════════
//  SFXEmitter — компонент для удобного воспроизведения SFX
//  прямо с GameObject. Используй из других скриптов через ссылку.
// ═══════════════════════════════════════════════════════════════════════════

public class SFXEmitter : MonoBehaviour
{
    [Tooltip("SFX-событие, которое будет воспроизведено методом Play().")]
    public SFXEvent sfxEvent;

    [Tooltip("Если true — звук привязан к этому объекту (3D-движение).")]
    public bool attached = true;

    /// <summary>Сыграть звук (вызывается из аниматора, другого скрипта или UnityEvent).</summary>
    public void Play()
    {
        if (sfxEvent == null) return;

        if (attached)
            AudioManager.PlayAttached(sfxEvent, transform);
        else
            AudioManager.PlayAt(sfxEvent, transform.position);
    }

    public void PlayDetached()
    {
        if (sfxEvent != null)
            AudioManager.PlayAt(sfxEvent, transform.position);
    }
}
