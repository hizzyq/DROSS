using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [Header("Explosion")]
    public float fuseTime      = 3f;   // секунд до взрыва
    public float blastRadius   = 6f;
    public int   maxDamage     = 100;

    [Header("Bounce")]
    public PhysicsMaterial bounceMaterial;  // назначь в инспекторе (bounciness ~0.4)

    [Header("FX & SFX")]
    [SerializeField] private SFXEvent explosionSFX;
    [SerializeField] private SFXEvent pinSFX;          // звук выдёргивания чеки

    private bool _exploded;

    private void Start()
    {
        AudioManager.PlayAt(pinSFX, transform.position);
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    // Граната взрывается при сильном ударе (опционально)
    private void OnCollisionEnter(Collision col)
    {
        if (col.relativeVelocity.magnitude > 15f)
            Explode();
    }

    public void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        Explosion.Explode(transform.position, blastRadius, maxDamage,
                          GlobalReferences.Instance.grenadeExplosionVFXPrefab, explosionSFX);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, blastRadius);
    }
}