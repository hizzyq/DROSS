using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;
    private bool _wasActiveWeapon; // Для отслеживания смены состояния и экономии ресурсов

    public int weaponDamage;

    [Header("Shooting")]
    public bool isShooting, readyToShoot;
    private bool allowReset = true;
    public float shootingDelay = 0.5f;

    [Header("Burst")]
    public int bulletsPerBurst = 1;
    public int burstBulletsLeft;

    [Header("Spread")]
    public float hipSpreadIntensity;
    private float spreadIntensity;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 500f;
    public float bulletPrefabLifetime = 3f;

    [Header("Muzzle Flash")]
    public GameObject muzzleEffect; // Префаб эффекта выстрела
    public Vector3 muzzleEffectRotationOffset = new Vector3(0f, 90f, 0f); // Поправка поворота
    public Vector3 muzzleEffectScale = new Vector3(0.05f, 0.05f, 0.05f); // Поправка масштаба

    internal Animator animator;

    [Header("Loading")]
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    [Header("Spawn")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    public enum WeaponModel { Pistol1911, AK74, Shotgun }
    public WeaponModel thisWeaponModel;

    public enum ShootingMode { Single, Burst, Auto }
    public ShootingMode currentShootingMode;

    [Header("SFX")]
    [SerializeField] private SFXEvent shotSFX;
    [SerializeField] private SFXEvent reloadSFX;
    [SerializeField] private SFXEvent emptySFX;

    // Кэшируем ссылки, чтобы не искать их каждый кадр
    private Player _player;
    private Outline _outline;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();
        bulletsLeft = magazineSize;
        spreadIntensity = hipSpreadIntensity;
    }

    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _outline = GetComponent<Outline>();

        // Принудительно обновляем визуал при старте
        _wasActiveWeapon = !isActiveWeapon;
    }

    private void Update()
    {
        if (_player != null && _player.isDead) return;

        // Обновляем слои и обводку только в момент смены оружия
        if (isActiveWeapon != _wasActiveWeapon)
        {
            _wasActiveWeapon = isActiveWeapon;
            UpdateWeaponVisuals(isActiveWeapon);
        }

        if (!isActiveWeapon) return;

        if (currentShootingMode == ShootingMode.Auto)
            isShooting = Input.GetKey(KeyCode.Mouse0);
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (readyToShoot && isShooting && bulletsLeft > 0 && !isReloading)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }

        if (Input.GetKeyDown(KeyCode.R) &&
            bulletsLeft < magazineSize &&
            !isReloading &&
            WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
        {
            Reload();
        }

        if (!readyToShoot &&
            !isReloading &&
            bulletsLeft <= 0 &&
            WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
        {
            Reload();
        }

        // Звук пустого магазина только при клике, чтобы не спамить в режиме Auto
        if (bulletsLeft == 0 && Input.GetKeyDown(KeyCode.Mouse0) && !isReloading)
        {
            AudioManager.Play(emptySFX);
        }
    }

    private void UpdateWeaponVisuals(bool active)
    {
        if (_outline != null)
            _outline.enabled = false;

        string targetLayer = active ? "WeaponRender" : "Default";
        int layerIndex = LayerMask.NameToLayer(targetLayer);

        gameObject.layer = layerIndex;

        foreach (Transform child in transform)
        {
            child.gameObject.layer = layerIndex;
        }
    }

    private void FireWeapon()
    {
        bulletsLeft--;

        // Создаем эффект выстрела как дочерний объект точки выстрела,
        // чтобы он был привязан к оружию
        if (muzzleEffect != null && bulletSpawn != null)
        {
            GameObject fx = Instantiate(muzzleEffect, bulletSpawn);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            fx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Destroy(fx, 0.15f);
        }

        if (animator != null)
            animator.SetTrigger("RECOIL");

        AudioManager.PlayAt(shotSFX, transform.position);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        if (bulletPrefab != null && bulletSpawn != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

            if (bullet.TryGetComponent<Bullet>(out var bulletComp))
            {
                bulletComp.bulletDamage = weaponDamage;
            }

            if (bullet.TryGetComponent<Rigidbody>(out var rb))
            {
                bullet.transform.forward = shootingDirection;
                rb.AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
            }

            StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifetime));
        }

        if (allowReset)
        {
            Invoke(nameof(ResetShot), shootingDelay);
            allowReset = false;
        }

        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke(nameof(FireWeapon), shootingDelay);
        }
    }

    private void Reload()
    {
        AudioManager.PlayAt(reloadSFX, transform.position);

        if (animator != null)
            animator.SetTrigger("RELOAD");

        isReloading = true;
        Invoke(nameof(ReloadCompleted), reloadTime);
    }

    private void ReloadCompleted()
    {
        // Правильная математика дозарядки магазина
        int bulletsNeeded = magazineSize - bulletsLeft;
        int ammoAvailable = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);

        // Берем ровно столько, сколько нужно, но не больше, чем есть в запасе
        int ammoToTake = Mathf.Min(bulletsNeeded, ammoAvailable);

        bulletsLeft += ammoToTake;
        WeaponManager.Instance.DecreaseTotalAmmo(ammoToTake, thisWeaponModel);

        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        if (bulletSpawn == null)
            return transform.forward;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(100f);
        Vector3 direction = targetPoint - bulletSpawn.position;

        float z = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(0f, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bullet != null)
            Destroy(bullet);
    }
}