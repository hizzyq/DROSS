using UnityEngine;

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
