using UnityEngine;

public class GrenadePickup : MonoBehaviour
{
    public int amount = 1; // сколько гранат даёт предмет

    private GrenadeThrow playerGrenadeThrow;
    private bool canPickup;

    private void Update()
    {
        // Если игрок рядом и нажал E — подбираем
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер вошёл игрок
        if (!other.CompareTag("Player"))
            return;

        // Ищем GrenadeThrow на игроке, у детей и у родителя
        playerGrenadeThrow = other.GetComponent<GrenadeThrow>();

        if (playerGrenadeThrow == null)
            playerGrenadeThrow = other.GetComponentInChildren<GrenadeThrow>();

        if (playerGrenadeThrow == null)
            playerGrenadeThrow = other.GetComponentInParent<GrenadeThrow>();

        if (playerGrenadeThrow != null)
        {
            canPickup = true;
            Debug.Log("Игрок рядом с гранатой");
        }
        else
        {
            Debug.LogError("GrenadeThrow не найден у Player");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Когда игрок вышел из зоны подбора — запрещаем подбор
        if (!other.CompareTag("Player"))
            return;

        canPickup = false;
        playerGrenadeThrow = null;
    }

    private void PickUp()
    {
        // Если GrenadeThrow не найден — ничего не делаем
        if (playerGrenadeThrow == null)
        {
            Debug.LogError("playerGrenadeThrow == null");
            return;
        }

        Debug.Log("Подбор гранаты через E");

        // Добавляем гранаты игроку
        playerGrenadeThrow.AddGrenades(amount);

        // Удаляем предмет
        Destroy(gameObject);
    }
}