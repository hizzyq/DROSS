using UnityEngine;

public static class Explosion
{
    /// <summary>
    /// Наносит урон всем Enemy и Player в радиусе, с falloff.
    /// </summary>
    public static void Explode(Vector3 center, float radius, int maxDamage,
                               GameObject vfxPrefab, SFXEvent sfx)
    {
        GameObject vfx = vfxPrefab != null
            ? vfxPrefab
            : GlobalReferences.Instance.explosionVFXPrefab;

        // Визуал и звук
        if (vfx != null)
            Object.Instantiate(vfxPrefab, center, Quaternion.identity);

        if (sfx != null)
            AudioManager.PlayAt(sfx, center);

        // Урон по радиусу
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var col in hits)
        {
            float distance = Vector3.Distance(center, col.transform.position);
            float falloff   = 1f - Mathf.Clamp01(distance / radius); // 1.0 в центре → 0 на краю
            int   damage    = Mathf.RoundToInt(maxDamage * falloff);

            if (col.CompareTag("Enemy"))
            {
                var enemy = col.GetComponent<Enemy>();
                if (enemy != null && !enemy.isDead)
                    enemy.TakeDamage(damage);
            }
            else if (col.CompareTag("Player"))
            {
                var player = col.GetComponent<Player>();
                if (player != null && !player.isDead)
                    player.TakeDamage(damage);
            }

            // Цепная реакция: взрываем соседние бочки
            var barrel = col.GetComponent<ExplosiveBarrel>();
            if (barrel != null)
                barrel.TriggerExplosion();
        }
    }
}