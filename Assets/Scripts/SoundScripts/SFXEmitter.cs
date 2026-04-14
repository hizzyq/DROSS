using UnityEngine;

// Компонент для воспроизведения SFX с защитой от слишком частых повторов
public class SFXEmitter : MonoBehaviour
{
    [Tooltip("SFX-событие, которое будет проигрываться.")]
    public SFXEvent sfxEvent;

    [Tooltip("Если true — звук привязан к объекту и двигается вместе с ним.")]
    public bool attached = true;

    [Tooltip("Минимальный интервал между проигрываниями в секундах.")]
    [SerializeField] private float playInterval = 5f;

    // Время, когда звук был проигран в последний раз
    private float lastPlayTime = -999f;

    /// <summary>
    /// Проиграть звук с учетом интервала
    /// </summary>
    public void Play()
    {
        if (sfxEvent == null)
            return;

        // Проверяем, прошло ли достаточно времени с прошлого проигрывания
        if (Time.time - lastPlayTime < playInterval)
            return;

        // Обновляем время последнего проигрывания
        lastPlayTime = Time.time;

        if (attached)
            AudioManager.PlayAttached(sfxEvent, transform);
        else
            AudioManager.PlayAt(sfxEvent, transform.position);
    }

    /// <summary>
    /// Всегда проигрывает звук как отдельный источник в позиции объекта
    /// </summary>
    public void PlayDetached()
    {
        if (sfxEvent == null)
            return;

        // Та же защита по интервалу
        if (Time.time - lastPlayTime < playInterval)
            return;

        lastPlayTime = Time.time;
        AudioManager.PlayAt(sfxEvent, transform.position);
    }
}