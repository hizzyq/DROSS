using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject visualIdle;
    public GameObject visualActive;
    public string checkpointName = "Checkpoint";
    [SerializeField] public SFXEvent activateSFX;
    private bool isActive = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BodyPlayer") && !isActive)
        {
            Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            foreach (Checkpoint cp in allCheckpoints)
            {
                if (cp != this)
                    cp.SetActive(false);
            }

            SetActive(true);

            Player player = other.GetComponentInParent<Player>();
            if (player == null)
                player = other.transform.root.GetComponent<Player>();

            if (player != null)
            {
                CheckpointSaveSystem saveSystem = player.GetComponent<CheckpointSaveSystem>();
                if (saveSystem != null)
                {
                    saveSystem.SaveCheckpoint(player, checkpointName);
                    Debug.Log($"Checkpoint activated: {checkpointName}");
                }
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        AudioManager.Play(activateSFX);
        if (visualIdle != null)
            visualIdle.SetActive(!active);
        if (visualActive != null)
            visualActive.SetActive(active);
    }
}