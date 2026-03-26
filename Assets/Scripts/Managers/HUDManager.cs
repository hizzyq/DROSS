using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Player Stats (Bars)")]
    public Image healthBarFill;
    public Image staminaBarFill;

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    [Header("Throwables")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;
    public Image tacticallUI;
    public TextMeshProUGUI tacticalAmountUI;

    [Header("Icons")]
    public Sprite pistolSprite;
    public Sprite rifleSprite;
    public Sprite pistolAmmoSprite;
    public Sprite rifleAmmoSprite;

    public Sprite emptySlot;
    public GameObject middleDot;

    private void Awake()
    {
        // НЕ DontDestroyOnLoad — Canvas привязан к сцене
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        // Защита — WeaponManager мог ещё не создаться
        if (WeaponManager.Instance == null) return;

        Weapon active   = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
        Weapon unActive = GetUnActiveWeaponSlot()?.GetComponentInChildren<Weapon>();

        if (active)
        {
            magazineAmmoUI.text = $"{active.bulletsLeft / active.bulletsPerBurst}";
            totalAmmoUI.text    = $"{WeaponManager.Instance.CheckAmmoLeftFor(active.thisWeaponModel)}";
            ammoTypeUI.sprite   = GetAmmoSprite(active.thisWeaponModel);
            activeWeaponUI.sprite = GetWeaponSprite(active.thisWeaponModel);
            unActiveWeaponUI.sprite = unActive
                ? GetWeaponSprite(unActive.thisWeaponModel)
                : emptySlot;
        }
        else
        {
            magazineAmmoUI.text     = "";
            totalAmmoUI.text        = "";
            ammoTypeUI.sprite       = emptySlot;
            activeWeaponUI.sprite   = emptySlot;
            unActiveWeaponUI.sprite = emptySlot;
        }
    }

    private Sprite GetWeaponSprite(Weapon.WeaponModel model) => model switch
    {
        Weapon.WeaponModel.Pistol1911 => pistolSprite,
        Weapon.WeaponModel.AK74       => rifleSprite,
        _                             => null
    };

    private Sprite GetAmmoSprite(Weapon.WeaponModel model) => model switch
    {
        Weapon.WeaponModel.Pistol1911 => pistolAmmoSprite,
        Weapon.WeaponModel.AK74       => rifleAmmoSprite,
        _                             => null
    };

    private GameObject GetUnActiveWeaponSlot()
    {
        foreach (var slot in WeaponManager.Instance.weaponSlots)
            if (slot != WeaponManager.Instance.activeWeaponSlot) return slot;
        return null;
    }

    public void UpdateGrenadeCount(int count)
        => lethalAmountUI.text = count.ToString();

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null && maxHealth > 0)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBarFill != null && maxStamina > 0)
        {
            staminaBarFill.fillAmount = currentStamina / maxStamina;
        }
    }

    public void ToggleHUD(bool show)
    {
        if (healthBarFill != null && healthBarFill.canvas != null)
        {
            Transform hudPanel = healthBarFill.canvas.transform.Find("PlayerHUDPanel");
            if (hudPanel != null) hudPanel.gameObject.SetActive(show);
        }

        if (activeWeaponUI != null && activeWeaponUI.canvas != null)
        {
            Transform wpnPanel = activeWeaponUI.canvas.transform.Find("WeaponPanel");
            if (wpnPanel != null) wpnPanel.gameObject.SetActive(show);
        }

        if (middleDot != null)
        {
            middleDot.SetActive(show);
        }
    }
}