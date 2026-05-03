using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ZombieChasingState : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;

    public float chaseSpeed = 6f;
    public float stopChasingDistance = 21f;
    public float attackingDistance = 2.5f;

    // ── ИЗМЕНЕНИЕ: вместо SoundManager.zombieChannel ─────────────────────
    // Назначь ассет в окне Animator, кликнув на State "ZombieChasing"
    public SFXEvent chaseSFX;

    // Интервал повтора звука погони (подбери под длину клипа)
    public float soundRepeatInterval = 2.5f;
    private float _soundTimer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent  = animator.GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;

        _soundTimer = soundRepeatInterval; // сыграть сразу при входе
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ── БЫЛО: if (!zombieChannel.isPlaying) zombieChannel.PlayOneShot(zombieChase)
        if (agent == null || !agent.isOnNavMesh) return;

        _soundTimer += Time.deltaTime;
        if (_soundTimer >= soundRepeatInterval)
        {
            _soundTimer = 0f;
            AudioManager.PlayAttached(chaseSFX, animator.transform);
        }

        agent.SetDestination(player.position);
        Vector3 direction = player.position - animator.transform.position;
        direction.y = 0f; // Игнорируем высоту!

        if (direction != Vector3.zero)
        {
            animator.transform.rotation = Quaternion.LookRotation(direction);
        }

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer > stopChasingDistance)
            animator.SetBool("isChasing", false);

        if (distanceFromPlayer < attackingDistance)
            animator.SetBool("isAttacking", true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.isOnNavMesh)
            agent.SetDestination(animator.transform.position);

        _soundTimer = soundRepeatInterval;
    }
}
