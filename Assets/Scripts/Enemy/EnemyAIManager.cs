using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyController), typeof(EnemyPatrol))]
public class EnemyAIManager : MonoBehaviour
{
    private EnemyController enemy;
    private EnemyPatrol patrol;
    private Animator animator;
    private NavMeshAgent agent;

    const string ANIM_WALKING_SPEED = "speed";

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
        patrol = GetComponent<EnemyPatrol>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        animator.SetFloat(ANIM_WALKING_SPEED, agent.velocity.magnitude);

        switch (enemy.CurrentState)
        {
            case EnemyState.Idle:
                patrol.enabled = false;
                break;
            case EnemyState.Patrol:
                patrol.enabled = true;
                break;
            case EnemyState.Chasing:
                patrol.enabled = false;
                break;
        }
    }
}