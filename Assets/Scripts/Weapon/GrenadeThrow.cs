using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform  throwOrigin;     // например, позиция камеры / руки
    public float      throwForce = 18f;
    public float      throwUpAngle = 10f; // небольшой угол вверх

    [Header("Inventory")]
    public int grenadeCount = 3;

    private void Start()
    {
        HUDManager.Instance.UpdateGrenadeCount(grenadeCount);

        // Если есть спрайт гранаты — назначь иконку
        // HUDManager.Instance.lethalUI.sprite = grenadeSprite;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && grenadeCount > 0)
            ThrowGrenade();
    }

    private void ThrowGrenade()
    {
        grenadeCount--;

        // Направление — вперёд камеры, слегка вверх
        Vector3 dir = Camera.main.transform.forward
                      + Camera.main.transform.up * (throwUpAngle * Mathf.Deg2Rad);

        GameObject g = Instantiate(grenadePrefab,
            throwOrigin.position,
            Camera.main.transform.rotation);

        g.GetComponent<Rigidbody>().AddForce(dir.normalized * throwForce, ForceMode.Impulse);

        // Обновить HUD (аналогично как bulletsLeft в Weapon.cs)
        HUDManager.Instance.UpdateGrenadeCount(grenadeCount);
    }
}