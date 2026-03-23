using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSaveSystem : MonoBehaviour
{
    private string savePath;
    private bool isLoading = false;

    [Header("Save Settings")]
    public bool reloadSceneOnLoad = true;
    public float reloadDelay = 0.05f;

    [Header("Weapon Prefabs")]
    public GameObject pistolPrefab;
    public GameObject riflePrefab;

    [System.Serializable]
    public class SaveData
    {
        public int health;
        public float posX, posY, posZ;
        public float rotY;
        public string lastCheckpointID;
        public string currentScene;
        public PlayerWeaponSaveData weaponData;
    }

    [System.Serializable]
    public class WeaponSaveData
    {
        public string weaponType;
        public int bulletsInMagazine;
        public int slotIndex;
    }

    [System.Serializable]
    public class PlayerWeaponSaveData
    {
        public List<WeaponSaveData> ownedWeapons = new List<WeaponSaveData>();
        public int activeWeaponIndex;
        public int totalRifleAmmo;
        public int totalPistolAmmo;
    }

    void Awake()
    {
        savePath = Application.persistentDataPath + "/checkpoint.json";
        Debug.Log("Saving to: " + savePath);
    }

    public void SaveCheckpoint(Player player, string checkpointID)
    {
        SaveData data = new SaveData
        {
            health = player.HP,
            posX = player.transform.position.x,
            posY = player.transform.position.y,
            posZ = player.transform.position.z,
            rotY = player.transform.rotation.eulerAngles.y,
            lastCheckpointID = checkpointID,
            currentScene = SceneManager.GetActiveScene().name,
            weaponData = CaptureWeaponState()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Checkpoint saved: {checkpointID}");
    }

    public bool LoadCheckpoint(Player player)
    {
        if (isLoading) return false;

        if (!File.Exists(savePath))
        {
            Debug.Log("No checkpoint found");
            return false;
        }

        isLoading = true;
        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogError("Failed to parse save data!");
            isLoading = false;
            return false;
        }

        Debug.Log($"Save data loaded: Health={data.health}, Scene={data.currentScene}, Weapons={data.weaponData?.ownedWeapons?.Count ?? 0}");

        // Check if we need to load a different scene
        if (data.currentScene != SceneManager.GetActiveScene().name)
        {
            Debug.Log($"Loading different scene: {data.currentScene}");
            PlayerPrefs.SetString("TempCheckpointData", json);
            PlayerPrefs.Save();
            SceneManager.LoadScene(data.currentScene);
            return true;
        }

        // If reloadSceneOnLoad is true, reload the current scene first
        if (reloadSceneOnLoad)
        {
            Debug.Log("Reloading current scene before applying checkpoint...");
            PlayerPrefs.SetString("TempCheckpointData", json);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return true;
        }

        // Otherwise, apply directly without reloading
        StartCoroutine(ApplyLoadedDataSmooth(player, data));
        return true;
    }

    public IEnumerator ApplyLoadedDataSmooth(Player player, SaveData data)
    {
        DisableAllPlayerComponents(player, true);

        CharacterController controller = player.GetComponent<CharacterController>();

        yield return null;

        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            player.transform.rotation = Quaternion.Euler(0, data.rotY, 0);
            controller.enabled = true;
        }
        else
        {
            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            player.transform.rotation = Quaternion.Euler(0, data.rotY, 0);
        }

        player.HP = data.health;

        if (player.playerHealthUI != null)
        {
            player.playerHealthUI.text = $"Health: {data.health}";
            player.playerHealthUI.gameObject.SetActive(true);
        }

        player.isDead = false;

        if (player.gameOverUI != null)
            player.gameOverUI.SetActive(false);

        if (player.screenBlackout != null)
            player.screenBlackout.enabled = false;

        if (data.weaponData != null)
        {
            RestoreWeaponState(data.weaponData);
        }

        yield return null;

        DisableAllPlayerComponents(player, false);

        Debug.Log($"Respawned at checkpoint: {data.lastCheckpointID}");

        isLoading = false;
    }

    public void ApplySaveData(Player player, SaveData data)
    {
        StartCoroutine(ApplyLoadedDataSmooth(player, data));
    }

    void DisableAllPlayerComponents(Player player, bool disable)
    {
        var dashing = player.GetComponent<Dashing>();
        if (dashing != null) dashing.enabled = !disable;

        var movement = player.GetComponent<PlayerMovementAdvanced>();
        if (movement != null) movement.enabled = !disable;

        var sliding = player.GetComponent<Sliding>();
        if (sliding != null) sliding.enabled = !disable;

        var wallRunning = player.GetComponent<WallRunning>();
        if (wallRunning != null) wallRunning.enabled = !disable;

        if (player.mainCamera != null)
        {
            var playerCam = player.mainCamera.GetComponent<PlayerCam>();
            if (playerCam != null) playerCam.enabled = !disable;
        }

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (disable)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
    }

    public void RespawnPlayer(Player player)
    {
        LoadCheckpoint(player);
    }

    private PlayerWeaponSaveData CaptureWeaponState()
    {
        PlayerWeaponSaveData weaponData = new PlayerWeaponSaveData();

        WeaponManager weaponManager = WeaponManager.Instance;
        if (weaponManager == null) return weaponData;

        weaponData.totalRifleAmmo = weaponManager.totalRifleAmmo;
        weaponData.totalPistolAmmo = weaponManager.totalPistolAmmo;

        for (int i = 0; i < weaponManager.weaponSlots.Count; i++)
        {
            if (weaponManager.weaponSlots[i] == weaponManager.activeWeaponSlot)
            {
                weaponData.activeWeaponIndex = i;
                break;
            }
        }

        for (int i = 0; i < weaponManager.weaponSlots.Count; i++)
        {
            GameObject weaponSlot = weaponManager.weaponSlots[i];
            if (weaponSlot.transform.childCount > 0)
            {
                Weapon weapon = weaponSlot.transform.GetChild(0).GetComponent<Weapon>();
                if (weapon != null)
                {
                    WeaponSaveData weaponSave = new WeaponSaveData
                    {
                        weaponType = weapon.thisWeaponModel.ToString(),
                        bulletsInMagazine = weapon.bulletsLeft,
                        slotIndex = i
                    };
                    weaponData.ownedWeapons.Add(weaponSave);
                }
            }
            else
            {
                weaponData.ownedWeapons.Add(new WeaponSaveData
                {
                    weaponType = "Empty",
                    bulletsInMagazine = 0,
                    slotIndex = i
                });
            }
        }

        return weaponData;
    }

    private void RestoreWeaponState(PlayerWeaponSaveData weaponData)
    {
        WeaponManager weaponManager = WeaponManager.Instance;
        if (weaponManager == null) return;

        weaponManager.totalRifleAmmo = weaponData.totalRifleAmmo;
        weaponManager.totalPistolAmmo = weaponData.totalPistolAmmo;

        // Clear all existing weapons
        foreach (GameObject weaponSlot in weaponManager.weaponSlots)
        {
            if (weaponSlot.transform.childCount > 0)
            {
                DestroyImmediate(weaponSlot.transform.GetChild(0).gameObject);
            }
        }

        // Restore weapons
        foreach (WeaponSaveData savedWeapon in weaponData.ownedWeapons)
        {
            if (savedWeapon.weaponType != "Empty" && savedWeapon.slotIndex < weaponManager.weaponSlots.Count)
            {
                GameObject targetSlot = weaponManager.weaponSlots[savedWeapon.slotIndex];
                GameObject weaponPrefab = GetWeaponPrefab(savedWeapon.weaponType);

                if (weaponPrefab != null)
                {
                    GameObject newWeapon = Instantiate(weaponPrefab, targetSlot.transform);
                    Weapon weapon = newWeapon.GetComponent<Weapon>();

                    if (weapon != null)
                    {
                        weapon.bulletsLeft = savedWeapon.bulletsInMagazine;
                        weapon.isActiveWeapon = false;

                        MeshCollider meshCollider = weapon.GetComponent<MeshCollider>();
                        if (meshCollider != null) meshCollider.enabled = false;
                        if (weapon.animator != null) weapon.animator.enabled = true;

                        newWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
                        newWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);
                    }
                }
            }
        }

        // Restore active weapon
        if (weaponData.activeWeaponIndex < weaponManager.weaponSlots.Count)
        {
            weaponManager.SwitchActiveSlot(weaponData.activeWeaponIndex);
        }
    }

    private GameObject GetWeaponPrefab(string weaponType)
    {
        if (weaponType == "Pistol1911")
            return pistolPrefab;
        else if (weaponType == "AK74")
            return riflePrefab;
        return null;
    }
}