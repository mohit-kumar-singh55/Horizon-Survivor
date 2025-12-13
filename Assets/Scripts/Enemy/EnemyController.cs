using UnityEngine;
using UnityEngine.AI;

public enum EnemyGender { Male, Female };
public enum EnemyType { StandingDuty, Patrollable };
public enum EnemyState { Idle, Patrol, Chasing };

/*
* 1. 立っている敵NPCのステートマシン
* アイドル -> 立っているNPC専用 -> 追跡 -> 初期位置に戻る -> アイドル
* ----------------------------------------------------------
* 2. 操作可能な敵NPCのステートマシン
* パトロール -> プレイヤー検出 ->
* 追跡 -> プレイヤーの視線を失う（数秒後） ->
* 検査中数秒間 -> パトロールに戻る
*/

/// <summary>
/// 敵の行動を制御するクラス
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(EnemyAttack))]
public class EnemyController : MonoBehaviour
{
    #region Serialized Fields
    [Header("General Settings")]
    [SerializeField] EnemyGender enemyGender = EnemyGender.Male;
    [SerializeField] EnemyType enemyType = EnemyType.Patrollable;
    [SerializeField] EnemyState currentState = EnemyState.Patrol;

    [Header("Vision Settings")]
    [SerializeField] Transform eyePosition; // 敵の目の位置
    [SerializeField] float viewRadius = 10f;
    [Range(0f, 360f)][SerializeField] float viewAngle = 90f;
    [SerializeField] LayerMask obstacleMask;      // 壁や岩など

    [Header("Detection Settings")]
    [SerializeField] float detectionTime = 2f;
    [SerializeField] float losePlayerTime = 3f;     // プレイヤーが視界にない場合は、プレイヤーを失う時間です
    [SerializeField] float inspectionTime = 3f;     // 検査タイマー -> プレイヤーを探すためのタイマー

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 3f;       // 敵の移動速度 - これはナビメッシュエージェントのデフォルト速度を上書きします
    [SerializeField] float chaseSpeed = 5f;       // プレイヤーを追いかける速度
    [SerializeField] float attackDistance = 6.5f;   // キックプレイヤーまでの距離
    #endregion

    #region Private Fields
    private float _currentDetectTimer = 0f;
    private float _losePlayerTimer = 0f;
    private float _inspectionTimer = 0f;

    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;
    private EnemyAttack _enemyAttack;

    // プレイヤーを追いかけた後、元の位置に戻るための待機任務NPCのために
    private Vector3 _startingPosition;
    private AudioManager _audioManager;

    // アニメーター変数
    private const string ANIM_RUNNING = "isRunning";
    private const string ANIM_KICKING = "isKicking";
    private const string ANIM_INSPECTING = "isInspecting";
    #endregion

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
        // initialize
        _player = FindAnyObjectByType<PlayerController>().gameObject.transform;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _enemyAttack = GetComponent<EnemyAttack>();
        _audioManager = AudioManager.Instance;

        _startingPosition = transform.position;

        // 難易度設定を適用する
        // インスペクターで設定されている内容に関係なく、難易度に応じていくつかの変数を上書きします
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
        // 開始位置に戻る
        if (transform.position != _startingPosition) _agent.SetDestination(_startingPosition);
        PatrolBehaviour();
    }

    // パトロール可能な敵のためのパトロール行動。この機能は、敵が巡回し、プレイヤーを見ると追跡状態に設定します。
    void PatrolBehaviour()
    {
        if (_player == null) return;

        // 敵を動かす
        _agent.speed = walkSpeed;

        // ** プレイヤーが視界に入っていない場合 **
        if (!IsPlayerInSight())
        {
            _currentDetectTimer = Mathf.Max(0f, _currentDetectTimer - Time.deltaTime);
            return;
        }

        // ** プレイヤーが視界にいる場合 **
        _currentDetectTimer += Time.deltaTime;

        if (_currentDetectTimer >= detectionTime)
        {
            // sfx
            _audioManager.PlayPlayerSpottedSFX(enemyGender);

            // 敵の状態を追跡に変更
            currentState = EnemyState.Chasing;
            _agent.SetDestination(_player.position);
            _audioManager.StopBGM();    // BGMを止める
            // Debug.Log("❗ PLAYER DETECTED! CHASING...");
        }
    }

    void ChasingBehaviour()
    {
        if (_enemyAttack.IsKicking) return;

        // プレイヤーを追いかける 
        _agent.speed = chaseSpeed;
        _agent.SetDestination(_player.position);
        _animator.SetBool(ANIM_RUNNING, true);

        // ** プレイヤーが視界にいる場合 **
        if (IsPlayerInSight())
        {
            CheckDistanceAndAttack();
            return;
        }

        // ** プレイヤーが視界から外れた場合 **
        SearchPlayer();
    }

    // 追跡中、プレイヤーが視界にいる場合
    private void CheckDistanceAndAttack()
    {
        // 敵からプレイヤーまでの距離
        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        // ********** Attack **********
        // プレイヤーが十分に近い場合に攻撃する
        if (distToPlayer <= attackDistance && _player.position.y < 1.5f && !PlayerController.Instance.WasKickBefore)
        {
            // スローモーション SFX
            _audioManager.PlaySlowMotionSFX();

            // 敵を止めて、攻撃する
            _agent.isStopped = true;
            _enemyAttack.Attack(ANIM_KICKING);
            PlayerController.Instance.SetWasKickedBefore();
            // Debug.Log("🗡️ Attacking _player");
        }
        else _agent.isStopped = false;

        // タイマーをリセット
        _losePlayerTimer = losePlayerTime;
        _inspectionTimer = inspectionTime;
    }

    // 追跡中、プレイヤーが視界から外れた場合
    private void SearchPlayer()
    {
        // プレイヤーを失った場合、タイマーの更新
        _losePlayerTimer -= Time.deltaTime;

        // プレイヤーが失ったら、検査する
        if (_losePlayerTimer < 0)
        {
            // 敵を止めて、検査中アニメーションを再生する
            _agent.isStopped = true;
            _animator.SetBool(ANIM_RUNNING, false);
            _animator.SetBool(ANIM_INSPECTING, true);
            // Debug.Log("🔍 Inspecting the place");

            // 検査時間を減らす
            _inspectionTimer -= Time.deltaTime;

            // 検査が終了し、プレイヤーが失われた場合、巡回状態に戻る
            if (_inspectionTimer <= 0)
            {
                // BGMを再生
                _audioManager.PlayBGM();

                _agent.isStopped = false;
                currentState = enemyType == EnemyType.StandingDuty ? EnemyState.Idle : EnemyState.Patrol;
                _animator.SetBool(ANIM_INSPECTING, false);
                // Debug.Log("👁️ Lost _player. Returning to patrol.");
            }
        }
    }

    // プレイヤーが敵の視界にいるかどうかを確認します。
    bool IsPlayerInSight()
    {
        Vector3 enemyPosition = eyePosition ? eyePosition.position : transform.position + Vector3.up * 1.5f;
        Vector3 dirToPlayer = (_player.position - enemyPosition).normalized;
        float distToPlayer = Vector3.Distance(enemyPosition, _player.position);

        if (distToPlayer > viewRadius) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        // this obstacle mask is so that if _player is hiding behind any obstacle this raycast should be blocked by the obstacle
        // この障害物マスクは、プレイヤーが障害物の後ろに隠れている場合、このレイキャストが障害物によってブロックされるようにするためのものです。
        if (Physics.Raycast(enemyPosition, dirToPlayer, distToPlayer, obstacleMask)) return false;

        return true;
    }

    // 敵のlose条件を発生した後にこのスクリプトを無効化します。
    void TriggerLose()
    {
        // stop all audios
        // _audioManager.StopBGM();

        enabled = false;
    }

    // 難易度設定を適用し
    void ApplyDifficultySettings()
    {
        DifficultySettings settings = DifficultyManager.Instance?.CurrentSettings;
        if (settings == null) return;        // テスト中のみ

        viewRadius = settings.viewRadius;
        detectionTime = settings.detectionTime;
        losePlayerTime = settings.losePlayerTime;
        chaseSpeed = settings.enemyChaseSpeed;
    }

    // player collider controllerスクリプトで呼び出されます
    public void ChasePlayerAfterHit()
    {
        currentState = EnemyState.Chasing;
        _agent.SetDestination(_player.position);
        transform.LookAt(_player.position);
        _agent.updateRotation = true;
    }

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

    // 視覚的デバッグ目的のみ
    public Vector3 DirFromAngle(float angle, bool global)
    {
        if (!global) angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}