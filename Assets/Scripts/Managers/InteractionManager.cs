using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; set; }

    public Weapon hoveredWeapon = null;
    private Weapon lastHoveredWeapon = null;
    private AmmoBox hoveredAmmoBox = null;
    private GrenadePickup hoveredGrenadePickup = null;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHitByRaycast = hit.transform.gameObject;

            if (objectHitByRaycast.GetComponent<Weapon>() && objectHitByRaycast.GetComponent<Weapon>().isActiveWeapon == false)
            {
                hoveredWeapon = objectHitByRaycast.GetComponent<Weapon>();
                if (hoveredWeapon != lastHoveredWeapon)
                {
                    if (lastHoveredWeapon != null)
                    {
                        lastHoveredWeapon.GetComponent<Outline>().enabled = false;
                    }
                    hoveredWeapon.GetComponent<Outline>().enabled = true;
                    lastHoveredWeapon = hoveredWeapon;
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    WeaponManager.Instance.PickUpWeapon(objectHitByRaycast.gameObject);
                }
            }
            else
            {
                if (hoveredWeapon)
                {
                    hoveredWeapon.GetComponent<Outline>().enabled = false;
                    lastHoveredWeapon = null;
                }
            }
            if (objectHitByRaycast.GetComponent<AmmoBox>())
            {
                hoveredAmmoBox = objectHitByRaycast.gameObject.GetComponent<AmmoBox>();
                hoveredAmmoBox.GetComponent<Outline>().enabled = true;
            }
            else
            {
                if (hoveredAmmoBox)
                {
                    hoveredAmmoBox.GetComponent<Outline>().enabled = false;
                    hoveredAmmoBox = null;
                }
            }
            if (objectHitByRaycast.CompareTag("GrenadePickup"))
            {
                hoveredGrenadePickup = objectHitByRaycast.GetComponent<GrenadePickup>();
                hoveredGrenadePickup.GetComponent<Outline>().enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GetComponent<GrenadeThrow>().grenadeCount += hoveredGrenadePickup.amount;
                    HUDManager.Instance.UpdateGrenadeCount(
                        GetComponent<GrenadeThrow>().grenadeCount);
                    Destroy(hoveredGrenadePickup.gameObject);
                    hoveredGrenadePickup = null;
                }
            }
            else
            {
                if (hoveredGrenadePickup != null)
                {
                    hoveredGrenadePickup.GetComponent<Outline>().enabled = false;
                    hoveredGrenadePickup = null;
                }
            }
        }
        else
        {
            if (hoveredWeapon)
            {
                hoveredWeapon.GetComponent<Outline>().enabled = false;
                hoveredWeapon = null;
                lastHoveredWeapon = null;
            }

        }
    }
}