using UnityEngine;

public class Zombie : MonoBehaviour
{
    public ZombieHand zombieHand;

    public int zombieDamage;

    public void Start()
    {
        zombieHand.damage = zombieDamage;
    }
}
