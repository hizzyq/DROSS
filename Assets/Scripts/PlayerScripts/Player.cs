using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int HP = 100;
    public int maxHP = 100;
    public HUDManager hudManager;
    public GameObject bloodyScreen;
    public PlayerDeathManager deathManager;
    public Camera mainCamera;

    private bool godMode = false;

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
        if (isDead) return; // Защита от получения урона и двойной смерти

        HP -= damageAmount;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateHealthBar(HP, maxHP);
        }
        if (HP <= 0)
        {
            print("Player dead");
            isDead = true;
            PlayerDead();
        }
        else
        {
            print("Player hit");
            StartCoroutine(BloodyScreenEffect());
            AudioManager.Play(hurtSFX); // ← БЫЛО: sfx (не объявлен)
        }
    }

    public void PlayerDead()
    {
        AudioManager.Play(deathSFX);

        if (TryGetComponent<Dashing>(out var dashing)) dashing.enabled = false;
        if (TryGetComponent<PlayerMovementAdvanced>(out var movement)) movement.enabled = false;
        if (TryGetComponent<Sliding>(out var sliding)) sliding.enabled = false;
        if (TryGetComponent<WallRunning>(out var wall)) wall.enabled = false;
        if (TryGetComponent<GrenadeThrow>(out var nades)) nades.enabled = false;

        if (mainCamera != null && mainCamera.TryGetComponent<PlayerCam>(out var cam)) cam.enabled = false;

        if (WeaponManager.Instance != null) WeaponManager.Instance.enabled = false;

        if (HUDManager.Instance != null && HUDManager.Instance.gameObject != null)
        {
            HUDManager.Instance.ToggleHUD(false);
            // Если выключить сам gameObject, менеджер перестанет работать, поэтому выключаем только нужные UI-панели
            // HUDManager.Instance.gameObject.SetActive(false);
        }
        
        if (deathManager != null)
            deathManager.KillPlayer();
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

        if (other.gameObject.GetComponent<HealBox>())
        {
            var healBox = other.gameObject.GetComponent<HealBox>();
            if (HP != maxHP)
            {
                HP += healBox.gameObject.GetComponent<HealBox>().HealAmount();
                if (HP > maxHP)
                    HP = maxHP;
                Destroy(healBox.gameObject);
                HUDManager.Instance.UpdateHealthBar(HP, maxHP);
                healBox = null;
            }
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
        /*if (Input.GetKeyDown(KeyCode.K) && !isDead)
        {
            GetComponent<CheckpointSaveSystem>().SaveCheckpoint(this, "manual_save");
        }*/

        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<CheckpointSaveSystem>().LoadCheckpoint(this);
        }

        // Много много хп для тестинга
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!godMode)
            {
                maxHP = 10000000;
                HP = 10000000;
                godMode = true;
            }
            else
            {
                maxHP = 100;
                HP = 100;
                godMode = false;
            }
            HUDManager.Instance.UpdateHealthBar(HP, maxHP);
        }
    }
}
