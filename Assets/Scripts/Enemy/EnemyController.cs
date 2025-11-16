using UnityEngine;
using UnityEngine.AI;

public enum EnemyGender { Male, Female };
public enum EnemyType { StandingDuty, Patrollable };
public enum EnemyState { Idle, Patrol, Chasing };

/*
* 1. Standing Duty Enemy NPC State Machine
* Idle -> for a standing NPC only -> Chasing -> Back to starting position -> Idle
* ----------------------------------------------------------
* 2. Petrollable Enemy NPC State Machine
* Patrol -> Player Detected -> 
* Chasing -> Player sight lost (after a few seconds) -> 
* Suspicious (inspecting) for a few seconds -> Return to patrol
*/
/*
* 1. 立っている敵NPCのステートマシン
* アイドル -> 立っているNPC専用 -> 追跡 -> 初期位置に戻る -> アイドル
* ----------------------------------------------------------
* 2. 操作可能な敵NPCのステートマシン
* パトロール -> プレイヤー検出 ->
* 追跡 -> プレイヤーの視線を失う（数秒後） ->
* 疑わしい（調査中）数秒間 -> パトロールに戻る
*/
[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(EnemyAttack))]
public class EnemyController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] EnemyGender enemyGender = EnemyGender.Male;
    [SerializeField] EnemyType enemyType = EnemyType.Patrollable;
    [SerializeField] EnemyState currentState = EnemyState.Patrol;

    [Header("Vision Settings")]
    [SerializeField] Transform eyePosition; // eye position of the enemy (敵の目の位置)
    [SerializeField] float viewRadius = 10f;
    [Range(0f, 360f)][SerializeField] float viewAngle = 90f;
    [SerializeField] LayerMask obstacleMask;      // like wall, rocks, etc (壁や岩など)

    [Header("Detection Settings")]
    [SerializeField] float detectionTime = 2f;
    private float currentDetectTimer = 0f;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 3f;       // enemy walk speed  -  this will override navmash agent's default speed (敵の移動速度 - これはナビメッシュエージェントのデフォルト速度を上書きします)
    [SerializeField] float chaseSpeed = 5f;       // speed to chase player (プレイヤーを追いかける速度)
    [SerializeField] float attackDistance = 6.5f;   // distance to kick player (キックプレイヤーまでの距離)

    // time to lose player if player is not in sight (プレイヤーが視界にない場合は、プレイヤーを失う時間です。)
    [SerializeField] float losePlayerTime = 3f;
    private float losePlayerTimer = 0f;

    // suspicious timer -> timer to search for player (疑わしいタイマー -> プレイヤーを探すためのタイマー)
    [SerializeField] float inspectionTime = 3f;
    private float inspectionTimer = 0f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyAttack enemyAttack;

    // for standing duty npc, to go back to its original position after chasing the player (プレイヤーを追いかけた後、元の位置に戻るための待機任務NPCのために)
    private Vector3 startingPosition;
    private AudioManager audioManager;

    // animator variables (アニメーター変数)
    const string ANIM_RUNNING = "isRunning";
    const string ANIM_KICKING = "isKicking";
    const string ANIM_INSPECTING = "isInspecting";

    public EnemyState CurrentState => currentState;
    public EnemyType CurrentEnemyType => enemyType;

    void OnEnable()
    {
        PlayerSystem.OnPlayerDeathSequence += TriggerLose;
    }

    void OnDisable()
    {
        PlayerSystem.OnPlayerDeathSequence -= TriggerLose;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>().gameObject.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyAttack = GetComponent<EnemyAttack>();

        audioManager = AudioManager.Instance;

        startingPosition = transform.position;

        // apply difficulty settings (難易度設定を適用する)
        // this will override some variables as per difficulty, irrespective of what is set in inspector (これは、インスペクターで設定されている内容に関係なく、難易度に応じていくつかの変数を上書きします。)
        ApplyDifficultySettings();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleBehaviour();
                break;
            case EnemyState.Patrol:
                PatrolBehaviour();
                break;
            case EnemyState.Chasing:
                ChasingBehaviour();
                break;
        }
    }

    void IdleBehaviour()
    {
        // go back to starting position (開始位置に戻る)
        if (transform.position != startingPosition) agent.SetDestination(startingPosition);
        PatrolBehaviour();
    }

    /// <summary>
    /// Patrol behaviour for the patrollable enemy.
    /// This function makes the enemy patrol and set to chasing state if it sees the player.
    /// パトロール可能な敵のためのパトロール行動。
    /// この機能は、敵が巡回し、プレイヤーを見ると追跡状態に設定します。
    /// </summary>
    void PatrolBehaviour()
    {
        if (player == null) return;

        agent.speed = walkSpeed;

        if (IsPlayerInSight())
        {
            currentDetectTimer += Time.deltaTime;

            if (currentDetectTimer >= detectionTime)
            {
                // sfx
                audioManager.PlayPlayerSpottedSFX(enemyGender);

                // chasing player
                currentState = EnemyState.Chasing;
                agent.SetDestination(player.position);

                Debug.Log("❗ PLAYER DETECTED! CHASING...");
            }
        }
        else
        {
            currentDetectTimer -= Time.deltaTime;
            currentDetectTimer = Mathf.Clamp(currentDetectTimer, 0f, detectionTime);
        }
    }

    /// <summary>
    /// Chasing behaviour for the enemy.
    /// This function makes the enemy chase the player and play the running animation.
    /// If the player is in sight, the enemy will attack (kick) the player if it is close enough.
    /// If the player is not in sight, the enemy will enter a suspicious (inspecting) state after a cooldown timer.
    /// 敵を追いかける行動。
    /// この機能は、敵がプレイヤーを追いかけ、走るアニメーションを再生します。
    /// プレイヤーが視界内にいる場合、敵が近くにいる場合は、プレイヤーを攻撃します。
    /// プレイヤーが視界内にいない場合、敵が数秒後に疑わしい(調査中)状態に入ります。
    /// </summary>
    void ChasingBehaviour()
    {
        if (enemyAttack.IsKicking) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);      // follow player
        animator.SetBool(ANIM_RUNNING, true);        // set running animation

        // stopping bgm audios
        audioManager.StopBGM();

        if (IsPlayerInSight())
        {
            // slash the player if close enough
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            // ********** Attack **********
            // attack if player is close and player is not above the enemy's head
            if (distToPlayer <= attackDistance && player.position.y < 1.5f && !PlayerController.Instance.WasKickBefore)
            {
                // slow motion sfx
                audioManager.PlaySlowMotionSFX();

                // stopping agent and attack
                agent.isStopped = true;
                enemyAttack.Attack(ANIM_KICKING);
                PlayerController.Instance.SetWasKickedBefore();

                Debug.Log("🗡️ Attacking player");
            }
            else agent.isStopped = false;

            // reset timer
            losePlayerTimer = losePlayerTime;
            inspectionTimer = inspectionTime;
        }
        else
        {
            // chasing cooldown timer
            losePlayerTimer -= Time.deltaTime;

            if (losePlayerTimer < 0)
            {
                // stopping and playing suspicious (inspecting) animation
                agent.isStopped = true;
                animator.SetBool(ANIM_RUNNING, false);        // set running animation
                animator.SetBool(ANIM_INSPECTING, true);        // set inspecting animation
                Debug.Log("🔍 Inspecting the place");

                // suspicious (inspecting) cooldown timer
                inspectionTimer -= Time.deltaTime;

                // inspection finished and player lost, return to patrol...
                if (inspectionTimer <= 0)
                {
                    // playing bgm audios
                    audioManager.PlayBGM();

                    agent.isStopped = false;
                    currentState = enemyType == EnemyType.StandingDuty ? EnemyState.Idle : EnemyState.Patrol;
                    animator.SetBool(ANIM_INSPECTING, false);        // set inspecting animation

                    Debug.Log("👁️ Lost player. Returning to patrol.");
                }
            }
        }
    }

    /// <summary>
    /// Checks whether the player is in the enemy's sight.
    /// This does the following checks:
    /// 1. Is the player in the view radius?
    /// 2. Is the player in the view angle?
    /// 3. Is there an obstacle in the way (raycast check)?
    /// If any of these conditions are false, the player is not in sight
    /// プレイヤーが敵の視界にいるかどうかを確認します。
    /// これは次のチェックを行います:
    /// 1. プレイヤーは視野半径内にいるかどうか？
    /// 2. プレイヤーは視野角度内にいるかどうか？
    /// 3. 視野の間に障害物があるかどうか（レイキャストチェック）？
    /// いずれかの条件が偽の場合、プレイヤーは視界にいない
    /// </summary>
    bool IsPlayerInSight()
    {
        Vector3 enemyPosition = eyePosition ? eyePosition.position : transform.position + Vector3.up * 1.5f;
        Vector3 dirToPlayer = (player.position - enemyPosition).normalized;
        float distToPlayer = Vector3.Distance(enemyPosition, player.position);

        if (distToPlayer > viewRadius) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        // this obstacle mask is so that if player is hiding behind any obstacle this raycast should be blocked by the obstacle
        // この障害物マスクは、プレイヤーが障害物の後ろに隠れている場合、このレイキャストが障害物によってブロックされるようにするためのものです。
        if (Physics.Raycast(enemyPosition, dirToPlayer, distToPlayer, obstacleMask)) return false;

        return true;
    }

    // Disables this script after triggering the lose condition.
    // 敵のlose条件を発生した後にこのスクリプトを無効化します。
    void TriggerLose()
    {
        // stop all audios
        // audioManager.StopBGM();

        enabled = false;
    }

    /// <summary>
    /// Applies difficulty settings from the current <see cref="DifficultySettings"/>,
    /// overriding the fields in this class with the values from the difficulty settings.
    /// <see cref="DifficultySettings"/>から現在の難易度設定を適用し、このクラスのフィールドを難易度設定からの値に上書きします。
    /// </summary>
    void ApplyDifficultySettings()
    {
        DifficultySettings settings = DifficultyManager.Instance?.CurrentSettings;
        if (settings == null) return;        // happens only when testing in editor while directing entering to level without going through main menu

        viewRadius = settings.viewRadius;
        detectionTime = settings.detectionTime;
        losePlayerTime = settings.losePlayerTime;
        chaseSpeed = settings.enemyChaseSpeed;
    }

    // chasing player if player hits the enemy (called in player collider controller script)
    // プレイヤーが敵にヒットした場合、追跡する（player collider controllerスクリプトで呼び出されます。)
    public void ChasePlayerAfterHit()
    {
        currentState = EnemyState.Chasing;
        agent.SetDestination(player.position);
        transform.LookAt(player.position);
        agent.updateRotation = true;
    }

    // for visual debugging purpose only
    // 視覚的デバッグ目的のみ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }

    // for visual debugging purpose only
    // 視覚的デバッグ目的のみ
    public Vector3 DirFromAngle(float angle, bool global)
    {
        if (!global) angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}