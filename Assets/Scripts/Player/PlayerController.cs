using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤー入力、移動、ジャンプ、スタミナ管理など、プレイヤー関連の各種処理を担当するクラス
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    #region Serialize Fields
    [SerializeField] float speed = 6f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float boostSpeed = 20f;    // for boostTime seconds
    [SerializeField] float boostTime = 3f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float groundCheckDistance = .3f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] TrailRenderer sprintTrail;
    [SerializeField] TrailRenderer boostTrail;
    [SerializeField] ParticleSystem ballHitShockwave;        // プレイヤーが何かにヒットしたら、ヒットVFXを再生します
    [SerializeField] TrailRenderer ballHitTrail;        // 蹴られたときのトレイルを再生します
    [SerializeField] CinemachineCamera cm_cam;
    #endregion

    #region Private Fields
    private Rigidbody _rb;
    private Vector2 _moveInput;
    private bool _isSprinting = false;
    private bool _isBoosting = false;
    private bool _playerFreezed = false;      // 蹴られたときにプレイヤーを止めます
    private bool _wasKickedBefore = false;       // 蹴られたときに一度しか蹴られないようにします
    private float _wasKickedCooldown = 3f;       // 蹴られた後、再び蹴られるまでの待機時間
    #endregion

    public bool WasKickBefore => _wasKickedBefore;

    #region Unity Callbacks
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // initialize
        _rb = GetComponent<Rigidbody>();

        // カーソルを隠す
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ShowTrails();
        StartWasKickedCooldown();
    }

    void FixedUpdate()
    {
        HandleMove();
    }
    #endregion

    #region Input Callbacks
    // ** Input System - Callbacks **
    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void OnJump(InputValue value)
    {
        HandleJump();
    }

    private void OnSprint(InputValue value)
    {
        _isSprinting = value.Get<float>() == 1f;
    }

    private void OnBoost(InputValue value)
    {
        HandleBoost();
    }
    #endregion

    #region Private Methods
    // プレイヤーの動きを制御する
    private void HandleMove()
    {
        if (_playerFreezed) return;

        // カメラの前方と右方向を平面化
        // カメラのy軸回転に合わせてプレイヤーを回転させる
        Vector3 camForward = cm_cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cm_cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // プレイヤーを移動させる
        Vector3 move = camRight * _moveInput.x + camForward * _moveInput.y;
        Vector3 targetVelocity = move * (_isBoosting ? boostSpeed : (_isSprinting ? sprintSpeed : speed));        // 優先順位 boost > sprint > walk
        Vector3 velocityChange = targetVelocity - _rb.linearVelocity;

        _rb.AddForce(velocityChange * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    private void HandleJump()
    {
        if (IsGrounded() && !_playerFreezed)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void HandleBoost()
    {
        if (PlayerSystem.Instance.AvailableBoosts > 0)
        {
            _isBoosting = true;
            Invoke(nameof(RemoveBoost), boostTime);
        }
    }

    private void RemoveBoost()
    {
        _isBoosting = false;
        PlayerSystem.Instance.RemoveBoost();
    }

    private void StartWasKickedCooldown()
    {
        if (_wasKickedBefore)
        {
            _wasKickedCooldown -= Time.deltaTime;

            if (_wasKickedCooldown <= 0f)
            {
                _wasKickedBefore = false;
                _wasKickedCooldown = 3f;
            }
        }
    }

    // トレイルを表示する
    private void ShowTrails()
    {
        if (_isSprinting || _isBoosting) sprintTrail.emitting = true;
        else sprintTrail.emitting = false;
        if (_isBoosting) boostTrail.emitting = true;
        else boostTrail.emitting = false;
    }

    private void StopBallHitTrail() => ballHitTrail.emitting = false;

    private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    #endregion

    #region Public Methods
    // プレイヤーの動きを止める
    public void FreezePlayer(bool freeze = true) => _playerFreezed = freeze;

    public void SetWasKickedBefore() => _wasKickedBefore = true;

    // ** VFX **
    // プレイヤーが何かにヒットしたら
    public void PlayHitVFX() => ballHitShockwave.Play();

    // プレイヤーが蹴られたとき
    public void PlayKickHitVFX()
    {
        ballHitTrail.emitting = true;
        // 3秒後に発射を止める
        Invoke(nameof(StopBallHitTrail), 3f);
    }
    #endregion
}