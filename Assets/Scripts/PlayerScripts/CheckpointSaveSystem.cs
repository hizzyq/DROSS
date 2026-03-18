using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointSaveSystem : MonoBehaviour
{
    private string savePath;
    private bool isLoading = false;

    [System.Serializable]
    public class SaveData
    {
        public int health;
        public float posX, posY, posZ;
        public float rotY;
        public string lastCheckpointID;
        public string currentScene;
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
            currentScene = SceneManager.GetActiveScene().name
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

        if (data.currentScene != SceneManager.GetActiveScene().name)
        {
            PlayerPrefs.SetString("TempCheckpointData", json);
            PlayerPrefs.Save();
            SceneManager.LoadScene(data.currentScene);
            return true;
        }

        StartCoroutine(ApplyLoadedDataSmooth(player, data));
        return true;
    }

    IEnumerator ApplyLoadedDataSmooth(Player player, SaveData data)
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

        yield return null;

        DisableAllPlayerComponents(player, false);

        Debug.Log($"Respawned at checkpoint: {data.lastCheckpointID}");

        isLoading = false;
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
}