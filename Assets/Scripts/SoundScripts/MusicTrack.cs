using UnityEngine;

/// <summary>
/// Состояния музыки в движение-шутере.
/// Добавляй свои варианты при необходимости.
/// </summary>
public enum MusicState
{
    None,
    Explore,      // Исследование / тихая зона
    CombatLow,    // Малое количество врагов
    CombatHigh,   // Жаркий бой
    Boss,         // Босс
    Victory,
    Death,
}

/// <summary>
/// ScriptableObject — один музыкальный трек со структурой intro → loop → outro,
/// плюс слои (стемы), которые нарастают с интенсивностью.
/// 
/// Создание: ПКМ → Create → Audio → Music Track
/// </summary>
[CreateAssetMenu(menuName = "Audio/Music Track", fileName = "MusicTrack")]
public class MusicTrack : ScriptableObject
{
    [Header("Структура трека")]
    [Tooltip("Вступление. Если null — начало сразу с Loop.")]
    public AudioClip intro;

    [Tooltip("Зацикленная часть (основа трека). Обязательный.")]
    public AudioClip loop;

    [Tooltip("Аутро. Если null — трек просто обрывается при переходе.")]
    public AudioClip outro;

    [Header("Стем-слои (Stems)")]
    [Tooltip("Слои нарастают по мере роста интенсивности боя [0..1].")]
    public StemDefinition[] stems;

    [Header("Переход")]
    [Tooltip("Время fade-out текущего трека и fade-in нового (секунды).")]
    [Range(0f, 4f)] public float crossfadeDuration = 1.0f;

    [Tooltip("Допустимо ли прерывать трек немедленно (true = да, false = дождаться конца intro).")]
    public bool allowImmediateInterrupt = true;
}

/// <summary>
/// Один стем-слой трека (например, перкуссия, бас, мелодия).
/// </summary>
[System.Serializable]
public class StemDefinition
{
    public string label;
    public AudioClip clip;

    [Tooltip("При каком минимальном значении интенсивности слой начинает звучать.")]
    [Range(0f, 1f)] public float thresholdMin = 0f;

    [Tooltip("При каком значении интенсивности слой достигает полной громкости.")]
    [Range(0f, 1f)] public float thresholdMax = 1f;

    [Tooltip("Максимальная громкость этого слоя.")]
    [Range(0f, 1f)] public float maxVolume = 1f;
}
