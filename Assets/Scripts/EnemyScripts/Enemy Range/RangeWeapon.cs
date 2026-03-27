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

        // Находим игрока
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // Вычисляем направление к центру игрока (чуть выше его ног)
        Vector3 targetPoint = playerObj.transform.position + Vector3.up * 1.2f;
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        // Создаем пулю, направленную в сторону игрока
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(fireDirection));
    
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            Collider ownerCollider = GetComponentInParent<Collider>();
            eb.Init(damage, bulletSpeed, ownerCollider);
        }
    }
}