using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int HP = 100;
    [SerializeField] private float destroyDelay = 3f; // Время до удаления объекта
    
    private Animator animator;
    private NavMeshAgent navAgent;
    private Collider enemyCollider; // Ссылка на коллайдер
    public bool isDead;

    [Header("SFX")]
    [SerializeField] private SFXEvent deathSFX;
    [SerializeField] private SFXEvent hurtSFX;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent  = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // Чтобы не срабатывало повторно после смерти

        HP -= damageAmount;

        if (HP <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("DAMAGE");
            AudioManager.PlayAttached(hurtSFX, transform);
        }
    }

    private void Die()
    {
        isDead = true;

        // Рандомная анимация смерти
        int randomValue = Random.Range(0, 2);
        if (randomValue == 0) animator.SetTrigger("DIE1");
        else                  animator.SetTrigger("DIE2");

        AudioManager.PlayAttached(deathSFX, transform);

        // --- ЛОГИКА ОЧИСТКИ ---
        
        // Отключаем навигацию, чтобы враг не скользил после смерти и не мешал другим
        if (navAgent != null)
        {
            navAgent.enabled = false; 
        }

        // Отключаем коллайдер, чтобы пули игрока не попадали в "труп" 
        // и игрок мог проходить сквозь него
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Удаляем объект через заданное время (например, 5 секунд)
        Destroy(gameObject, destroyDelay);
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