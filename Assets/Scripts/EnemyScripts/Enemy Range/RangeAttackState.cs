using UnityEngine;
using UnityEngine.AI;

public class RangeAttackState : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;

    // Дальше этой дистанции — выходим из Attack обратно в Chase
    public float stopAttackingDistance = 14f;
    public float retreatDistance = 4f;
    
    // Задержка перед ПЕРВЫМ выстрелом (время поднятия оружия)
    public float initialDelay = 0.8f;
    private bool _initialDelayDone;
    private float _initialTimer;

    // Интервал между выстрелами (секунды)
    public float fireRate = 1.5f;
    private float _fireTimer;

    // Компонент, который реально стреляет — ищем на том же GameObject
    private RangeWeapon _weapon;

    // Проверка прямой видимости перед каждым выстрелом
    public LayerMask obstacleMask;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player  = GameObject.FindGameObjectWithTag("Player").transform;
        agent   = animator.GetComponent<NavMeshAgent>();
        _weapon = animator.GetComponentInChildren<RangeWeapon>();

        agent.isStopped = true;

        _initialDelayDone = false;
        _initialTimer  = 0f;
        _fireTimer  = fireRate;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!agent.isOnNavMesh) return;
        
        LookAtPlayer(animator.transform);

        float dist = Vector3.Distance(player.position, animator.transform.position);
        
        if (dist < retreatDistance)
        {
            agent.isStopped = false;
            Vector3 retreatDir = (animator.transform.position - player.position).normalized;
            agent.SetDestination(animator.transform.position + retreatDir * retreatDistance);
        }
        else
        {
            agent.isStopped = true;
        }

        // Игрок вышел из зоны — возвращаемся в Chase
        if (dist > stopAttackingDistance)
        {
            agent.isStopped = false;
            animator.SetBool("isAttacking", false);
            return;
        }

        // Выстрел по таймеру, только если есть прямая видимость
        if (!_initialDelayDone)
        {
            _initialTimer += Time.deltaTime;
            if (_initialTimer >= initialDelay)
                _initialDelayDone = true;
        }
        else
        {
            _fireTimer += Time.deltaTime;
        }

        if (_initialDelayDone && _fireTimer >= fireRate && HasLineOfSight(animator.transform))
        {
            _fireTimer = 0f;
            _weapon?.Fire();
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            _fireTimer = fireRate;
        }
    }

    private void LookAtPlayer(Transform self)
    {
        Vector3 targetPos = player.position + Vector3.up * 1.2f; // Целимся в корпус
        Vector3 dir = targetPos - self.position;
        dir.y = 0;
        // Если нужно, чтобы враг НЕ падал (стоял ровно), 
        // но голова/оружие смотрели вверх:
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            self.rotation = Quaternion.Slerp(self.rotation, targetRot, Time.deltaTime * 10f);
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