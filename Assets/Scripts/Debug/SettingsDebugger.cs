using UnityEngine;

public class SettingsDebugger : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F1)) return;

        Debug.Log("=== Сохранённые настройки ===");
        Debug.Log($"MasterVolume: {PlayerPrefs.GetFloat("s_masterVolume", -1)}");
        Debug.Log($"MusicVolume:  {PlayerPrefs.GetFloat("s_musicVolume", -1)}");
        Debug.Log($"SFXVolume:    {PlayerPrefs.GetFloat("s_sfxVolume", -1)}");
        Debug.Log($"SensX:        {PlayerPrefs.GetFloat("s_sensX", -1)}");
        Debug.Log($"SensY:        {PlayerPrefs.GetFloat("s_sensY", -1)}");
        Debug.Log($"Pixelation:   {PlayerPrefs.GetInt("s_pixelation", -1)}");
    }
}