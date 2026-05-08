using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public int ammoAmount = 200;
    public AmmoType ammoType;

    [SerializeField] public SFXEvent pickupSFX;
    public enum AmmoType
    {
        RifleAmmo,
        PistolAmmo,
        ShotgunAmmo
    }

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;
        AudioManager.Play(pickupSFX);
    }
}
