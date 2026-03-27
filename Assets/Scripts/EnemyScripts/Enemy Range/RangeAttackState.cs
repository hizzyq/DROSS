using UnityEngine;
using UnityEngine.AI;

public class RangeAttackState : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;

    // Дальше этой дистанции — выходим из Attack обратно в Chase
    public float stopAttackingDistance = 14f;
    public float retreatDistance = 4f;
    
    // Интервал между выстрелами (секунды)
    public float fireRate = 1.5f;
    private float _fireTimer;

    // Компонент, который реально стреляет — ищем на том же GameObject
    private RangeWeapon _weapon;

    // Звук атаки
    public SFXEvent attackSFX;
    public float soundRepeatInterval = 1.5f;
    private float _soundTimer;

    // Проверка прямой видимости перед каждым выстрелом
    public LayerMask obstacleMask;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player  = GameObject.FindGameObjectWithTag("Player").transform;
        agent   = animator.GetComponent<NavMeshAgent>();
        _weapon = animator.GetComponentInChildren<RangeWeapon>();

        agent.isStopped = true;

        _fireTimer  = fireRate;   // выстрелить сразу при входе
        _soundTimer = soundRepeatInterval;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
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

        // Звук атаки
        _soundTimer += Time.deltaTime;
        if (_soundTimer >= soundRepeatInterval)
        {
            _soundTimer = 0f;
            AudioManager.PlayAttached(attackSFX, animator.transform);
        }

        // Выстрел по таймеру, только если есть прямая видимость
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireRate && HasLineOfSight(animator.transform))
        {
            _fireTimer = 0f;
            _weapon?.Fire();
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.isStopped = false;
        _fireTimer  = fireRate;
        _soundTimer = soundRepeatInterval;
    }

    private void LookAtPlayer(Transform self)
    {
        Vector3 dir = player.position - self.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            self.rotation = Quaternion.Slerp(self.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 10f);
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