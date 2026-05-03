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
        public bool isEmpty;
        public Weapon.WeaponModel weaponType;
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
        public int grenadeCount;
    }

    void Awake()
    {
        savePath = Application.persistentDataPath + "/checkpoint.json";
        Debug.Log("Saving to: " + savePath);
    }

    void Start()
    {
        // Проверяем, есть ли данные чекпоинта для загрузки после перезапуска сцены
        if (PlayerPrefs.HasKey("TempCheckpointData"))
        {
            Debug.Log("Found temp checkpoint data. Applying after scene load...");
            string json = PlayerPrefs.GetString("TempCheckpointData");

            // Обязательно удаляем ключ, чтобы не грузить чекпоинт при обычном рестарте уровня
            PlayerPrefs.DeleteKey("TempCheckpointData");

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data != null)
            {
                Player player = GetComponent<Player>();
                if (player != null)
                {
                    // Применяем данные
                    StartCoroutine(ApplyLoadedDataSmooth(player, data));
                }
            }
        }
    }

    public void SaveCheckpoint(Player player, string checkpointID)
    {
        if (player.isDead) 
        {
            Debug.LogWarning("Cannot save checkpoint while dead.");
            return;
        }

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

        if (HUDManager.Instance != null && HUDManager.Instance.gameObject != null)
        {
            HUDManager.Instance.gameObject.SetActive(true);
            HUDManager.Instance.ToggleHUD(true);
            HUDManager.Instance.UpdateHealthBar(player.HP, player.maxHP);
        }

        player.isDead = false;

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeIn();
        }

        // Возвращаем курсор в игровой режим после загрузки
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        PauseMenuController.GameIsPaused = false;

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

        var nades = player.GetComponent<GrenadeThrow>();
        if (nades != null) nades.enabled = !disable;

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.enabled = !disable;
        }

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
        if (weaponManager != null) 
        {
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
                            isEmpty = false,
                            weaponType = weapon.thisWeaponModel,
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
                        isEmpty = true,
                        bulletsInMagazine = 0,
                        slotIndex = i
                    });
                }
            }
        }

        GrenadeThrow grenadeThrow = FindObjectOfType<GrenadeThrow>();
        if (grenadeThrow != null)
        {
            weaponData.grenadeCount = grenadeThrow.grenadeCount;
        }

        return weaponData;
    }

    private void RestoreWeaponState(PlayerWeaponSaveData weaponData)
    {
        WeaponManager wm = WeaponManager.Instance;
        if (wm == null)
        {
            Debug.LogError("WeaponManager.Instance is null! Невозможно восстановить оружие.");
            return;
        }

        // Назначаем активный слот
        if (weaponData.activeWeaponIndex >= 0 && weaponData.activeWeaponIndex < wm.weaponSlots.Count)
        {
            wm.activeWeaponSlot = wm.weaponSlots[weaponData.activeWeaponIndex];
        }

        wm.totalPistolAmmo = weaponData.totalPistolAmmo;
        wm.totalRifleAmmo = weaponData.totalRifleAmmo;

        // 1. ПРАВИЛЬНАЯ ОЧИСТКА: моментально убираем старое оружие из иерархии слота
        foreach (var slot in wm.weaponSlots)
        {
            // Идем с конца списка дочерних объектов для безопасного удаления
            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = slot.transform.GetChild(i);
                child.gameObject.SetActive(false); // Отключаем сразу
                child.SetParent(null);             // Отвязываем от слота
                Destroy(child.gameObject);         // Уничтожаем
            }
        }

        // Спавним оружие в нужные слоты
        foreach (var wData in weaponData.ownedWeapons)
        {
            if (wData.isEmpty) continue;

            GameObject prefab = GetWeaponPrefab(wData.weaponType);
            if (prefab != null)
            {
                GameObject newWp = Instantiate(prefab, wm.weaponSlots[wData.slotIndex].transform);
                Weapon w = newWp.GetComponent<Weapon>();

                // 2. ЗАЩИТА ПАТРОНОВ: Устанавливаем патроны с задержкой
                StartCoroutine(ApplyWeaponDataAfterStart(w, wData));

                newWp.transform.localPosition = w.spawnPosition;
                newWp.transform.localRotation = Quaternion.Euler(w.spawnRotation);
                w.isActiveWeapon = (wData.slotIndex == weaponData.activeWeaponIndex);
            }
            else
            {
                Debug.LogError($"[SaveSystem] Префаб для {wData.weaponType} не найден! Назначь его в инспекторе на новой сцене или помести в папку Resources.");
            }
        }

        // Финальное обновление состояния
        wm.SwitchActiveSlot(weaponData.activeWeaponIndex);

        // Восстанавливаем гранаты
        GrenadeThrow gt = GetComponent<GrenadeThrow>();
        if (gt != null)
        {
            gt.grenadeCount = weaponData.grenadeCount;
            if (HUDManager.Instance != null) HUDManager.Instance.UpdateGrenadeCount(gt.grenadeCount);
        }
    }

    // Корутина: ждем конца кадра, чтобы Start() внутри скрипта Weapon отработал и не стер наши данные
    private IEnumerator ApplyWeaponDataAfterStart(Weapon w, WeaponSaveData wData)
    {
        yield return new WaitForEndOfFrame();

        w.bulletsLeft = wData.bulletsInMagazine;

        // Если у тебя пушка обновляет интерфейс патронов в момент доставания,
        // имеет смысл принудительно обновить HUD здесь для активного оружия.
    }

    private GameObject GetWeaponPrefab(Weapon.WeaponModel weaponType)
    {
        if (weaponType == Weapon.WeaponModel.Pistol1911)
        {
            if (pistolPrefab != null) return pistolPrefab;
            return Resources.Load<GameObject>("Pistol1911_Weapon");
        }
        else if (weaponType == Weapon.WeaponModel.AK74)
        {
            if (riflePrefab != null) return riflePrefab;
            return Resources.Load<GameObject>("AK74_Weapon");
        }
        return null;
    }

    public void TransitionToLevel(Player player, string targetScene, Vector3 spawnPos, float spawnRotY)
    {
        // Генерируем данные, но подменяем сцену и координаты на входные для нового уровня
        SaveData data = new SaveData
        {
            health = player.HP,
            posX = spawnPos.x,
            posY = spawnPos.y,
            posZ = spawnPos.z,
            rotY = spawnRotY,
            lastCheckpointID = "level_start",
            currentScene = targetScene,
            weaponData = CaptureWeaponState() // Используем твой существующий метод сбора оружия
        };

        string json = JsonUtility.ToJson(data, true);

        // Сохраняем в PlayerPrefs, чтобы Start() в новой сцене это подхватил
        PlayerPrefs.SetString("TempCheckpointData", json);
        PlayerPrefs.Save();

        SceneManager.LoadScene(targetScene);
    }
}

