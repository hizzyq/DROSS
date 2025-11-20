using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAttackingState : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;
    Collider playerCollider;
    Collider zombieHandCollider;

    public float stopAttackingDistance = 2.5f;
    public float attackReset = 2.6f;
    private float attackTimer;
    private bool tookDamage;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackTimer = 0;
        zombieHandCollider = GameObject.Find("ZombieHand").GetComponent<SphereCollider>();
        zombieHandCollider.enabled = true;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();
        playerCollider = player.gameObject.GetComponentInChildren<Collider>();
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance.zombieChannel.isPlaying == false)
        {
            SoundManager.Instance.zombieChannel.PlayOneShot(SoundManager.Instance.zombieAttack);
        }

        LookAtPlayer();

        if (tookDamage)
        {
            attackTimer += Time.deltaTime;
        }

        if (attackTimer >= attackReset)
        {
            attackTimer = 0;
            tookDamage = false;
        }

        if (playerCollider.bounds.Intersects(zombieHandCollider.bounds) && attackTimer == 0)
        {
            if (!player.GetComponent<Player>().isDead)
            {
                player.GetComponent<Player>().TakeDamage(zombieHandCollider.gameObject.GetComponent<ZombieHand>().damage);
                tookDamage = true;
            }
        }

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer > stopAttackingDistance)
        {
            zombieHandCollider.enabled = false;

            animator.SetBool("isAttacking", false);
        }
    }
    
    private void LookAtPlayer()
    {
        Vector3 direction = player.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.Instance.zombieChannel.Stop();
    }
}
