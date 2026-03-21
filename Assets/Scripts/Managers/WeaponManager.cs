using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    public List<GameObject> weaponSlots;
    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo  = 0;
    public int totalPistolAmmo = 0;

    private void Awake()
    {
        // НЕ DontDestroyOnLoad — привязан к Player в сцене
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        // Чистим Instance при выходе из сцены
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        activeWeaponSlot = weaponSlots[0];
    }

    private void Update()
    {
        foreach (GameObject slot in weaponSlots)
            slot.SetActive(slot == activeWeaponSlot);

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchActiveSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchActiveSlot(1);
    }

    public void PickUpWeapon(GameObject pickedUpWeapon)
        => AddWeaponIntoActiveSlot(pickedUpWeapon);

    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon)
    {
        DropCurrentWeapon(pickedUpWeapon);

        pickedUpWeapon.transform.SetParent(activeWeaponSlot.transform, false);
        Weapon weapon = pickedUpWeapon.GetComponent<Weapon>();

        pickedUpWeapon.transform.localPosition = weapon.spawnPosition;
        pickedUpWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation);

        weapon.isActiveWeapon = true;
        weapon.GetComponent<MeshCollider>().enabled = false;
        weapon.animator.enabled = true;
    }

    private void DropCurrentWeapon(GameObject pickedUpWeapon)
    {
        if (activeWeaponSlot.transform.childCount == 0) return;

        var toDrop = activeWeaponSlot.transform.GetChild(0).gameObject;
        var w = toDrop.GetComponent<Weapon>();

        w.isActiveWeapon = false;
        w.animator.enabled = false;
        toDrop.GetComponent<MeshCollider>().enabled = true;

        toDrop.transform.SetParent(pickedUpWeapon.transform.parent);
        toDrop.transform.localPosition = pickedUpWeapon.transform.localPosition;
        toDrop.transform.localRotation = pickedUpWeapon.transform.localRotation;
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (activeWeaponSlot.transform.childCount > 0)
            activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>().isActiveWeapon = false;

        activeWeaponSlot = weaponSlots[slotNumber];

        if (activeWeaponSlot.transform.childCount > 0)
            activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>().isActiveWeapon = true;
    }

    public void PickupAmmo(AmmoBox ammoBox)
    {
        switch (ammoBox.ammoType)
        {
            case AmmoBox.AmmoType.RifleAmmo:  totalRifleAmmo  += ammoBox.ammoAmount; break;
            case AmmoBox.AmmoType.PistolAmmo: totalPistolAmmo += ammoBox.ammoAmount; break;
        }
    }

    public int CheckAmmoLeftFor(WeaponModel model)
    {
        switch (model)
        {
            case WeaponModel.Pistol1911: return totalPistolAmmo;
            case WeaponModel.AK74:       return totalRifleAmmo;
            default:                     return 0;
        }
    }

    public void DecreaseTotalAmmo(int amount, WeaponModel model)
    {
        switch (model)
        {
            case WeaponModel.Pistol1911: totalPistolAmmo -= amount; break;
            case WeaponModel.AK74:       totalRifleAmmo  -= amount; break;
        }
    }
}