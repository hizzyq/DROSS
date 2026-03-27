using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Health")]
    public int HP = 60;

    [Header("Explosion")]
    public float blastRadius = 5f;
    public int   maxDamage   = 120;

    [Header("FX & SFX")]
    public GameObject damagedVFX;          // дым/искры при повреждении
    [SerializeField] private SFXEvent explosionSFX;
    [SerializeField] private SFXEvent hitSFX;

    private bool _exploded;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // Bullet.cs уже вызывает TakeDamage на Enemy — добавим аналогичный паттерн для бочки
    public void TakeDamage(int amount)
    {
        if (_exploded) return;

        HP -= amount;
        AudioManager.PlayAt(hitSFX, transform.position);

        // Визуальный сигнал повреждения (включить smoke VFX)
        if (HP <= 30 && damagedVFX != null)
            damagedVFX.SetActive(true);

        if (HP <= 0)
            TriggerExplosion();
    }

    public void TriggerExplosion()
    {
        if (_exploded) return;
        _exploded = true;

        Explosion.Explode(transform.position, blastRadius, maxDamage,
            GlobalReferences.Instance.explosionVFXPrefab, explosionSFX);

        // Скрываем меш до уничтожения
        if (_meshRenderer) _meshRenderer.enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, blastRadius);
    }
}