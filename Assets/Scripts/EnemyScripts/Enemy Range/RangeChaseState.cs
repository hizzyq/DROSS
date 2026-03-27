using UnityEngine;
using UnityEngine.AI;

public class RangeChaseState : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;

    public float chaseSpeed = 5f;

    // Дальше этого — теряем игрока и выходим из Chase
    public float stopChasingDistance = 25f;

    // Ближе этого — переходим в Attack (дистанция атаки для дальнего бойца)
    public float attackingDistance = 12f;

    // Ближе этого — враг слишком близко, отступает назад
    public float retreatDistance = 4f;

    // Звук погони (назначь SFXEvent в окне Animator)
    public SFXEvent chaseSFX;
    public float soundRepeatInterval = 2.5f;
    private float _soundTimer;

    // Проверка прямой видимости перед переходом в атаку
    public LayerMask obstacleMask;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent  = animator.GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.isStopped = false;

        _soundTimer = soundRepeatInterval;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!agent.isOnNavMesh) return;
        
        // Звук погони
        _soundTimer += Time.deltaTime;
        if (_soundTimer >= soundRepeatInterval)
        {
            _soundTimer = 0f;
            AudioManager.PlayAttached(chaseSFX, animator.transform);
        }

        float dist = Vector3.Distance(player.position, animator.transform.position);

        // Слишком близко — отступаем, чтобы держать дистанцию
        if (dist < retreatDistance)
        {
            Vector3 retreatDir = (animator.transform.position - player.position).normalized;
            Vector3 retreatTarget = animator.transform.position + retreatDir * retreatDistance;
            agent.SetDestination(retreatTarget);
        }
        else
        {
            agent.SetDestination(player.position);
        }

        // Плавный поворот к игроку
        Vector3 lookDir = (player.position - animator.transform.position);
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            animator.transform.rotation = Quaternion.Slerp(
                animator.transform.rotation,
                targetRot,
                Time.deltaTime * 8f
            );
        }

        // Потерял игрока — выходим из Chase
        if (dist > stopChasingDistance)
        {
            animator.SetBool("isChasing", false);
            return;
        }

        // В зоне атаки + прямая видимость — переходим в Attack
        if (dist <= attackingDistance && HasLineOfSight(animator.transform))
        {
            animator.SetBool("isAttacking", true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(animator.transform.position);
            agent.isStopped = true;
        }
    }

    private bool HasLineOfSight(Transform self)
    {
        Vector3 origin    = self.position + Vector3.up * 1.5f;
        Vector3 targetPos = player.position + Vector3.up * 1.5f;
        Vector3 dir       = (targetPos - origin).normalized;
        float   dist      = Vector3.Distance(origin, targetPos);

        return !Physics.Raycast(origin, dir, dist, obstacleMask);
    }
}