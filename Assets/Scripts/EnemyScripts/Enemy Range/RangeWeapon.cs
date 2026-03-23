using UnityEngine;

public class RangeWeapon : MonoBehaviour
{
    [Header("Настройки снаряда")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 8f;
    public int damage = 20;

    public void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            Collider ownerCollider = GetComponentInParent<Collider>();
            eb.Init(damage, bulletSpeed, ownerCollider);
        }
    }
}