using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int HP = 100;
    public int maxHP = 100;
    public HUDManager hudManager;
    public GameObject bloodyScreen;
    public PlayerDeathManager deathManager;
    public TextMeshProUGUI playerHealthUI;
    public GameObject gameOverUI;
    public Camera mainCamera;
    public ScreenBlackout screenBlackout;

    public bool isDead;

    // ← ДОБАВЛЕНО: два отдельных поля вместо несуществующего sfx
    [Header("SFX")]
    [SerializeField] private SFXEvent hurtSFX;
    [SerializeField] private SFXEvent deathSFX;

    private void Start()
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHealthBar(HP, maxHP);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHealthBar(HP, maxHP);
        }
        if (HP <= 0)
        {
            print("Player dead");
            PlayerDead();
            isDead = true;
        }
        else
        {
            print("Player hit");
            StartCoroutine(BloodyScreenEffect());
            playerHealthUI.text = $"Health: {HP}";
            AudioManager.Play(hurtSFX); // ← БЫЛО: sfx (не объявлен)
        }
    }

    public void PlayerDead()
    {
        AudioManager.Play(deathSFX); // ← БЫЛО: sfx (не объявлен)

        GetComponent<Dashing>().enabled = false;
        GetComponent<PlayerMovementAdvanced>().enabled = false;
        GetComponent<Sliding>().enabled = false;
        GetComponent<WallRunning>().enabled = false;

        mainCamera.GetComponent<PlayerCam>().enabled = false;

        playerHealthUI.gameObject.SetActive(false);

        screenBlackout.enabled = true;
        screenBlackout.StartFade();
        StartCoroutine(ShowGameOverUI());
        deathManager.KillPlayer();
    }

    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(1f);
        gameOverUI.gameObject.SetActive(true);
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreen.activeInHierarchy == false)
            bloodyScreen.SetActive(true);

        var image = bloodyScreen.GetComponentInChildren<Image>();

        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (bloodyScreen.activeInHierarchy)
            bloodyScreen.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<AmmoBox>())
        {
            var ammoBox = other.gameObject.GetComponent<AmmoBox>();
            WeaponManager.Instance.PickupAmmo(ammoBox);
            Destroy(ammoBox.gameObject);
            ammoBox = null;
        }
        if (other.TryGetComponent<GrenadePickup>(out var grenadePickup))
        {
            GetComponent<GrenadeThrow>().grenadeCount += grenadePickup.amount;
            HUDManager.Instance.UpdateGrenadeCount(GetComponent<GrenadeThrow>().grenadeCount);
            Destroy(other.gameObject);
        }
    }

    public void RespawnAtCheckpoint()
    {
        CheckpointSaveSystem saveSystem = GetComponent<CheckpointSaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.LoadCheckpoint(this);
        }
    }


    void Update()
    {
        // For testing - Press K to save, L to load
        if (Input.GetKeyDown(KeyCode.K))
        {
            GetComponent<CheckpointSaveSystem>().SaveCheckpoint(this, "manual_save");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<CheckpointSaveSystem>().LoadCheckpoint(this);
        }
    }
}
