using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; private set; }

    [Header("VFX")]
    public GameObject bulletImpactPrefabEffect;
    public GameObject bloodSprayEffect;
    public GameObject explosionVFXPrefab;
    public GameObject grenadeExplosionVFXPrefab;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}