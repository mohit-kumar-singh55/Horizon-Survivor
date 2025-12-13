using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵の移動を制御し、各ウェイポイントで一定時間待機しながら次の地点へ移動させるクラス
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] float waitTimeAtWaypoint = 2f;

    private Transform[] _waypoints;
    private int _currentWaypointIndex = 0;
    private NavMeshAgent _agent;
    private float _waitTimer = 0f;
    private bool _waiting = false;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // _agentがwaypointに到達していない場合、またはウ_waypointsが割り当てられていない場合は、何もしない。
        if (_agent.pathPending || _waypoints?.Length == 0) return;

        // waypointに待機
        if (!_waiting && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _waiting = true;
            _waitTimer = waitTimeAtWaypoint;
        }

        // 待機中ならタイマーを減らして、タイマーが終わったら次のwaypointに行く
        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;
                GoToNextWaypoint();
            }
        }
    }

    // 次のwaypointに行く
    void GoToNextWaypoint()
    {
        if (_waypoints.Length == 0) return;

        _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
    }

    // この関数はEnemySpawnerスクリプト内で呼び出されています
    public void SetWaypoints(Transform[] _waypoints)
    {
        this._waypoints = _waypoints;
        GoToNextWaypoint();
    }

    // 視覚的デバッグ目的のみ
    // void OnDrawGizmosSelected()
    // {
    //     if (_waypoints == null) return;

    //     Gizmos.color = Color.green;
    //     for (int i = 0; i < _waypoints.Length; i++)
    //     {
    //         Vector3 wp = _waypoints[i].position;
    //         Gizmos.DrawSphere(wp, 0.2f);

    //         if (i < _waypoints.Length - 1)
    //             Gizmos.DrawLine(wp, _waypoints[i + 1].position);
    //     }
    // }
}
