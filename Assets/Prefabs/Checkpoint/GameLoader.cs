using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameLoader : MonoBehaviour
{
    public static GameLoader Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("GameLoader initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerPrefs.HasKey("TempCheckpointData"))
        {
            string jsonData = PlayerPrefs.GetString("TempCheckpointData");
            PlayerPrefs.DeleteKey("TempCheckpointData");
            StartCoroutine(ApplySaveAfterSceneLoad(jsonData));
        }
        else
        {
            StartCoroutine(SaveDefaultCheckpointDelay());
        }
    }

    IEnumerator SaveDefaultCheckpointDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) yield break;

        Player player = playerObject.GetComponent<Player>();
        CheckpointSaveSystem saveSystem = playerObject.GetComponent<CheckpointSaveSystem>();

        if (player != null && saveSystem != null && !player.isDead)
        {
            saveSystem.SaveCheckpoint(player, "Level_Start");
            Debug.Log("Default checkpoint saved for level start.");
        }
    }

    IEnumerator ApplySaveAfterSceneLoad(string jsonData)
    {
        yield return new WaitForSeconds(0.2f);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) yield break;

        Player player = playerObject.GetComponent<Player>();
        CheckpointSaveSystem saveSystem = playerObject.GetComponent<CheckpointSaveSystem>();

        if (player == null || saveSystem == null) yield break;

        CheckpointSaveSystem.SaveData data = JsonUtility.FromJson<CheckpointSaveSystem.SaveData>(jsonData);

        if (data != null)
        {
            saveSystem.ApplySaveData(player, data);
            Debug.Log($"GameLoader: Save applied - Health: {data.health}");
        }
        else
        {
            Debug.LogError("GameLoader: Failed to parse save data!");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}