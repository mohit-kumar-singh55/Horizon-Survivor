using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] float waitTimeAtWaypoint = 2f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;
    private float waitTimer = 0f;
    private bool waiting = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // if agent hasn't reached the waypoint or no waypoints are assigned, do nothing
        // agentがwaypointに到達していない場合、またはウwaypointsが割り当てられていない場合は、何もしない。
        if (agent.pathPending || waypoints?.Length == 0) return;

        // wait at waypoint
        // waypointに待機
        if (!waiting && agent.remainingDistance <= agent.stoppingDistance)
        {
            waiting = true;
            waitTimer = waitTimeAtWaypoint;
        }

        // if waiting, decrement timer and if timer runs out, go to next waypoint
        // 待機中ならタイマーを減らして、タイマーが終わったら次のwaypointに行く
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                GoToNextWaypoint();
            }
        }
    }

    // go to next waypoint
    // 次のwaypointに行く
    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    // this function is being called in the EnemySpawner script
    // この関数はEnemySpawnerスクリプト内で呼び出されています
    public void SetWaypoints(Transform[] waypoints)
    {
        this.waypoints = waypoints;
        GoToNextWaypoint();
    }

    // for visual debugging purpose only
    // void OnDrawGizmosSelected()
    // {
    //     if (waypoints == null) return;

    //     Gizmos.color = Color.green;
    //     for (int i = 0; i < waypoints.Length; i++)
    //     {
    //         Vector3 wp = waypoints[i].position;
    //         Gizmos.DrawSphere(wp, 0.2f);

    //         if (i < waypoints.Length - 1)
    //             Gizmos.DrawLine(wp, waypoints[i + 1].position);
    //     }
    // }
}
