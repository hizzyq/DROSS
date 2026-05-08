using UnityEngine;

public class HealBox : MonoBehaviour
{
    [SerializeField] int healAmount;
    [SerializeField] public SFXEvent pickupSFX;
    public int HealAmount() {return healAmount;}
    
    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;
        AudioManager.Play(pickupSFX);
    }
}
