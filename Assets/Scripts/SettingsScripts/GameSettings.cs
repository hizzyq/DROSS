using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Графика")]
    [Range(64, 540)] public int pixelation = 180;

    [Header("Звук")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume  = 0.8f;
    [Range(0f, 1f)] public float sfxVolume    = 1f;

    [Header("Управление")]
    [Range(1f, 100f)] public float sensitivityX = 50f; // ← пользователю показываем 1-100
    [Range(1f, 100f)] public float sensitivityY = 50f;
    public bool invertY = false;

    // Конвертер: 1-100 → реальное значение для PlayerCam
    public float RealSensX => sensitivityX * 6f;  // 50 × 6 = 300
    public float RealSensY => sensitivityY * 6f;

    // Сохранить всё в PlayerPrefs
    public void Save()
    {
        PlayerPrefs.SetInt  ("s_pixelation",   pixelation);
        PlayerPrefs.SetFloat("s_masterVolume", masterVolume);
        PlayerPrefs.SetFloat("s_musicVolume",  musicVolume);
        PlayerPrefs.SetFloat("s_sfxVolume",    sfxVolume);
        PlayerPrefs.SetFloat("s_sensX",        sensitivityX);
        PlayerPrefs.SetFloat("s_sensY",        sensitivityY);
        PlayerPrefs.SetInt  ("s_invertY",      invertY ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Загрузить из PlayerPrefs
    public void Load()
    {
        pixelation   = PlayerPrefs.GetInt  ("s_pixelation",   pixelation);
        masterVolume = PlayerPrefs.GetFloat("s_masterVolume", masterVolume);
        musicVolume  = PlayerPrefs.GetFloat("s_musicVolume",  musicVolume);
        sfxVolume    = PlayerPrefs.GetFloat("s_sfxVolume",    sfxVolume);
        sensitivityX = PlayerPrefs.GetFloat("s_sensX",        sensitivityX);
        sensitivityY = PlayerPrefs.GetFloat("s_sensY",        sensitivityY);
        invertY      = PlayerPrefs.GetInt  ("s_invertY",      0) == 1;
    }

    // Сброс до дефолта
    public void ResetToDefault()
    {
        pixelation   = 180;
        masterVolume = 1f;
        musicVolume  = 0.8f;
        sfxVolume    = 1f;
        sensitivityX = 300f;
        sensitivityY = 300f;
        invertY      = false;
    }
}