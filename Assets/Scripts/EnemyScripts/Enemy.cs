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

    [Header("Push Settings")]
    [SerializeField] private float pushForce = 25f;
    [SerializeField] private float pushRadius = 2.5f;
    [SerializeField] private float pushCooldown = 0.05f;

    private float lastPushTime;
    private Transform playerTransform;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent  = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
    }


    private void Update()
    {
        if (isDead) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= pushRadius && Time.time - lastPushTime >= pushCooldown)
            {
                // Толкаем игрока
                Vector3 pushDirection = (playerTransform.position - transform.position).normalized;
                pushDirection.y = 0;

                Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 currentVelocity = playerRb.linearVelocity;
                    playerRb.linearVelocity = new Vector3(pushDirection.x * pushForce, currentVelocity.y, pushDirection.z * pushForce);
                }

                lastPushTime = Time.time;
            }
        }
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
        Gizmos.DrawWireSphere(transform.position, pushRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 18f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 21f);
    }
}