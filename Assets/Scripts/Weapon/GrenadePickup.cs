using UnityEngine;

public class GrenadePickup : MonoBehaviour
{
    public int amount = 1; // ������� ������ ��� �������

    private GrenadeThrow playerGrenadeThrow;
    private bool canPickup;
    [SerializeField] public SFXEvent pickupSFX;
    private void Update()
    {
        // ���� ����� ����� � ����� E � ���������
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
            AudioManager.Play(pickupSFX);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���������, ��� � ������� ����� �����
        if (!other.CompareTag("Player"))
            return;

        // ���� GrenadeThrow �� ������, � ����� � � ��������
        playerGrenadeThrow = other.GetComponent<GrenadeThrow>();

        if (playerGrenadeThrow == null)
            playerGrenadeThrow = other.GetComponentInChildren<GrenadeThrow>();

        if (playerGrenadeThrow == null)
            playerGrenadeThrow = other.GetComponentInParent<GrenadeThrow>();

        if (playerGrenadeThrow != null)
        {
            canPickup = true;
            Debug.Log("����� ����� � ��������");
        }
        else
        {
            Debug.LogError("GrenadeThrow �� ������ � Player");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ����� ����� ����� �� ���� ������� � ��������� ������
        if (!other.CompareTag("Player"))
            return;

        canPickup = false;
        playerGrenadeThrow = null;
    }

    private void PickUp()
    {
        // ���� GrenadeThrow �� ������ � ������ �� ������
        if (playerGrenadeThrow == null)
        {
            Debug.LogError("playerGrenadeThrow == null");
            return;
        }

        Debug.Log("������ ������� ����� E");
        // ��������� ������� ������
        playerGrenadeThrow.AddGrenades(amount);
        
        // ������� �������
        Destroy(gameObject);
    }
}