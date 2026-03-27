using UnityEngine;

public class RangeIdleState : StateMachineBehaviour
{
    float timer;
    public float idleTime = 0f;
    Transform player;
    public float detectionAreaRadius = 18f;
    
    private Enemy _enemy; // Ссылка на скрипт врага

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        _enemy = animator.GetComponent<Enemy>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Если враг мертв, выходим из логики
        if (_enemy != null && _enemy.isDead) return;

        timer += Time.deltaTime;
        if (timer > idleTime)
        {
            animator.SetBool("isPatroling", true);
        }

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        if (distanceFromPlayer < detectionAreaRadius)
        {
            animator.SetBool("isChasing", true);
        }
    }
}