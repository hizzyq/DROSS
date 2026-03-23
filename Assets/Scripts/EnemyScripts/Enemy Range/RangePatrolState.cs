using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangePatrolState : StateMachineBehaviour
{
    float timer;
    public float patrolingTime = 10f;

    Transform player;
    NavMeshAgent agent;

    public float detectionArea = 18f;
    public float patrolSpeed = 2f;

    List<Transform> waypointsList = new List<Transform>();

    public SFXEvent walkSFX;

    public float soundRepeatInterval = 3.0f;
    private float _soundTimer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent  = animator.GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        timer = 0;

        _soundTimer = soundRepeatInterval - 1f;

        GameObject waypointCluster = GameObject.FindGameObjectWithTag("Waypoints");
        foreach (Transform t in waypointCluster.transform)
            waypointsList.Add(t);

        agent.SetDestination(waypointsList[Random.Range(0, waypointsList.Count)].position);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _soundTimer += Time.deltaTime;
        if (_soundTimer >= soundRepeatInterval)
        {
            _soundTimer = 0f;
            AudioManager.PlayAttached(walkSFX, animator.transform);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            agent.SetDestination(waypointsList[Random.Range(0, waypointsList.Count)].position);

        timer += Time.deltaTime;
        if (timer > patrolingTime)
            animator.SetBool("isPatroling", false);

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        if (distanceFromPlayer < detectionArea)
            animator.SetBool("isChasing", true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(agent.transform.position);

        _soundTimer = soundRepeatInterval;
    }
}
