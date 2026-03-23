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

    // ── ИЗМЕНЕНИЕ: вместо SoundManager.zombieChannel ─────────────────────
    // Назначь ассет прямо в окне Animator, кликнув на State "ZombieAttacking"
    public SFXEvent attackSFX;

    // Таймер повтора звука — вместо проверки isPlaying на общем канале.
    // Значение подбери под длину клипа (секунды).
    public float soundRepeatInterval = 2.0f;
    private float _soundTimer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackTimer = 0;
        _soundTimer = soundRepeatInterval; // сыграть сразу при входе в стейт

        zombieHandCollider = GameObject.Find("ZombieHand").GetComponent<SphereCollider>();
        zombieHandCollider.enabled = true;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent  = animator.GetComponent<NavMeshAgent>();
        playerCollider = player.gameObject.GetComponentInChildren<Collider>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ── БЫЛО: if (!zombieChannel.isPlaying) zombieChannel.PlayOneShot(...)
        // Теперь каждый зомби получает свой слот из пула — никакого конфликта
        _soundTimer += Time.deltaTime;
        if (_soundTimer >= soundRepeatInterval)
        {
            _soundTimer = 0f;
            AudioManager.PlayAttached(attackSFX, animator.transform);
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
                player.GetComponent<Player>().TakeDamage(
                    zombieHandCollider.gameObject.GetComponent<ZombieHand>().damage);
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
        // ── БЫЛО: SoundManager.Instance.zombieChannel.Stop()
        // Не нужно — звуки из пула доигрывают сами и возвращаются обратно.
        // Просто сбросим таймер чтобы при следующем входе звук сыграл сразу.
        _soundTimer = soundRepeatInterval;
    }
}
