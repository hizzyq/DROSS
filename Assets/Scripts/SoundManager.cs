using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    public AudioSource ShootingChannel;

    public AudioSource reloading1911;
    public AudioSource emptyMagazine1911;
    public AudioSource reloadingAK74;

    public AudioClip shotAK74;
    public AudioClip shot1911;

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

    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                ShootingChannel.PlayOneShot(shot1911);
                break;
            case WeaponModel.AK74:
                ShootingChannel.PlayOneShot(shotAK74);
                break;
        }
    }
    public void PlayReloadingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                reloading1911.Play();
                break;
            case WeaponModel.AK74:
                reloadingAK74.Play();
                break;
        }
    }
}
