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
            StartCoroutine(DelayedLoad());
        }
    }

    IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(0.1f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CheckpointSaveSystem saveSystem = player.GetComponent<CheckpointSaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.LoadCheckpoint(player.GetComponent<Player>());
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}