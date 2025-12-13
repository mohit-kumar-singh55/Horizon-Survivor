using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵の AI を制御するクラス
/// </summary>
[RequireComponent(typeof(EnemyController), typeof(EnemyPatrol))]
public class EnemyAIManager : MonoBehaviour
{
    private EnemyController _enemy;
    private EnemyPatrol _patrol;
    private Animator _animator;
    private NavMeshAgent _agent;

    const string ANIM_WALKING_SPEED = "speed";

    private void Awake()
    {
        _enemy = GetComponent<EnemyController>();
        _patrol = GetComponent<EnemyPatrol>();
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        _animator.SetFloat(ANIM_WALKING_SPEED, _agent.velocity.magnitude);

        switch (_enemy.CurrentState)
        {
            case EnemyState.Idle:
                _patrol.enabled = false;
                break;
            case EnemyState.Patrol:
                _patrol.enabled = true;
                break;
            case EnemyState.Chasing:
                _patrol.enabled = false;
                break;
        }
    }
}