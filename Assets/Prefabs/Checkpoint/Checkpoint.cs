using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject visualIdle;
    public GameObject visualActive;
    public string checkpointName = "Checkpoint";

    private bool isActive = false;

    void Start()
    {
        Debug.Log($"Checkpoint {checkpointName} started at position {transform.position}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered checkpoint: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("BodyPlayer") && !isActive)
        {
            Debug.Log($"Player entered checkpoint {checkpointName}!");

            Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
            Debug.Log($"Found {allCheckpoints.Length} checkpoints in scene");

            foreach (Checkpoint cp in allCheckpoints)
            {
                if (cp != this)
                {
                    cp.SetActive(false);
                }
            }

            SetActive(true);

            Player player = other.GetComponentInParent<Player>();
            if (player == null)
                player = other.transform.root.GetComponent<Player>();

            if (player != null)
            {
                Debug.Log($"Player component found, checking for save system...");

                CheckpointSaveSystem saveSystem = player.GetComponent<CheckpointSaveSystem>();
                if (saveSystem != null)
                {
                    saveSystem.SaveCheckpoint(player, checkpointName);
                    Debug.Log($"Checkpoint activated and saved: {checkpointName}");
                }
                else
                {
                    Debug.LogError("CheckpointSaveSystem not found on player!");
                }
            }
            else
            {
                Debug.LogError("Player component not found on object with BodyPlayer tag or its parent!");
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (visualIdle != null)
            visualIdle.SetActive(!active);
        if (visualActive != null)
            visualActive.SetActive(active);

        Debug.Log($"Checkpoint {checkpointName} active state set to: {active}");
    }
}