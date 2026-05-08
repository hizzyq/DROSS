using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform throwOrigin;     // точка, откуда вылетает граната
    public float throwForce = 18f;
    public float throwUpAngle = 10f;  // небольшой угол вверх

    [Header("Inventory")]
    public int grenadeCount = 3;      // текущее количество гранат
    public int maxGrenades = 10;      // максимум гранат

    private void Start()
    {
        // Обновляем HUD только если менеджер существует
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateGrenadeCount(grenadeCount);
    }

    private void Update()
    {
        // Бросок гранаты на G, если есть хотя бы одна граната
        if (Input.GetKeyDown(KeyCode.G) && grenadeCount > 0)
            ThrowGrenade();
    }

    private void ThrowGrenade()
    {
        // Проверяем, что камера существует
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main не найден");
            return;
        }

        // Проверяем, что throwOrigin назначен
        if (throwOrigin == null)
        {
            Debug.LogError("throwOrigin не назначен");
            return;
        }

        // Проверяем, что prefab гранаты назначен
        if (grenadePrefab == null)
        {
            Debug.LogError("grenadePrefab не назначен");
            return;
        }

        // Уменьшаем количество гранат перед броском
        grenadeCount--;

        // Направление броска: вперёд от камеры + немного вверх
        Vector3 dir = Camera.main.transform.forward +
                      Camera.main.transform.up * (throwUpAngle * Mathf.Deg2Rad);

        // Создаём гранату
        GameObject g = Instantiate(
            grenadePrefab,
            throwOrigin.position,
            Camera.main.transform.rotation
        );

        // Добавляем импульс, если у гранаты есть Rigidbody
        Rigidbody rb = g.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(dir.normalized * throwForce, ForceMode.Impulse);

        // Обновляем HUD после броска
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateGrenadeCount(grenadeCount);
    }

    public void AddGrenades(int amount)
    {
        // Добавляем гранаты
        grenadeCount += amount;

        // Ограничиваем максимум
        if (grenadeCount > maxGrenades)
            grenadeCount = maxGrenades;

        // Обновляем HUD после подбора
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateGrenadeCount(grenadeCount);
    }
}