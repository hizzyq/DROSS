using UnityEngine;

public class HealBox : MonoBehaviour
{
    [SerializeField] int healAmount;
    
    public int HealAmount() {return healAmount;}
}
