using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;
    public int weaponDamage;

    [Header("Shooting")]
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    [Header("Burst")]
    public int bulletsPerBurst = 1;
    public int burstBulletsLeft;

    [Header("Spread")]
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;
    private float spreadIntensity;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifetime = 3f;

    public GameObject muzzleEffect;

    internal Animator animator;

    [Header("Loading")]
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    [Header("Spawn")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    public bool isADS;

    public enum WeaponModel { Pistol1911, AK74 }
    public WeaponModel thisWeaponModel;

    public enum ShootingMode { Single, Burst, Auto }
    public ShootingMode currentShootingMode;

    [Header("SFX")]
    [SerializeField] private SFXEvent shotSFX;
    [SerializeField] private SFXEvent reloadSFX;
    [SerializeField] private SFXEvent emptySFX; // ← ДОБАВЛЕНО: было объявлено, но не существовало

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();
        bulletsLeft = magazineSize;
        spreadIntensity = hipSpreadIntensity;
    }

    void Update()
    {
        if (isActiveWeapon)
        {
            GetComponent<Outline>().enabled = false;
            foreach (Transform child in transform)
                child.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
            transform.gameObject.layer = LayerMask.NameToLayer("WeaponRender");

            if (Input.GetMouseButtonDown(1)) EnterADS();
            if (Input.GetMouseButtonUp(1))   ExitADS();

            if (currentShootingMode == ShootingMode.Auto)
                isShooting = Input.GetKey(KeyCode.Mouse0);
            else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);

            if (readyToShoot && isShooting && bulletsLeft > 0 && !isReloading)
            {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false &&
                WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
                Reload();

            if (readyToShoot == false && isReloading == false && bulletsLeft <= 0 &&
                WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
                Reload();

            if (bulletsLeft == 0 && isShooting)
                AudioManager.Play(emptySFX);
        }
        else
        {
            foreach (Transform child in transform)
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            transform.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    private void EnterADS()
    {
        animator.SetTrigger("enterADS");
        isADS = true;
        HUDManager.Instance.middleDot.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        animator.SetTrigger("exitADS");
        isADS = false;
        HUDManager.Instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletsLeft--;
        muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (!isADS) animator.SetTrigger("RECOIL");

        AudioManager.PlayAt(shotSFX, transform.position);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().bulletDamage = weaponDamage;
        bullet.transform.forward = shootingDirection;
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifetime));

        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        AudioManager.PlayAt(reloadSFX, transform.position);
        animator.SetTrigger("RELOAD");
        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        if (WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > magazineSize)
        {
            WeaponManager.Instance.DecreaseTotalAmmo(magazineSize - bulletsLeft, thisWeaponModel);
            bulletsLeft = magazineSize;
        }
        else
        {
            bulletsLeft = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }
        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(100);
        Vector3 direction = targetPoint - bulletSpawn.position;
        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        return direction + new Vector3(0f, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
