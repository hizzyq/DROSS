using UnityEngine;

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