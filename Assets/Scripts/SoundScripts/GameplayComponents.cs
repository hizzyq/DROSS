using UnityEngine;

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

