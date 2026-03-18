using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int HP = 100;
    private Animator animator;
    private NavMeshAgent navAgent;
    public bool isDead;

    // ← ДОБАВЛЕНО: два отдельных поля вместо несуществующего sfx
    [Header("SFX")]
    [SerializeField] private SFXEvent deathSFX;
    [SerializeField] private SFXEvent hurtSFX;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent  = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {
            int randomValue = Random.Range(0, 2);
            if (randomValue == 0) animator.SetTrigger("DIE1");
            else                  animator.SetTrigger("DIE2");
            isDead = true;

            AudioManager.PlayAttached(deathSFX, transform); // ← БЫЛО: sfx (не объявлен)
        }
        else
        {
            animator.SetTrigger("DAMAGE");
            AudioManager.PlayAttached(hurtSFX, transform);  // ← БЫЛО: sfx (не объявлен)
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 18f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 21f);
    }
}
