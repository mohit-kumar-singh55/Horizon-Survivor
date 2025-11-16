using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float speed = 6f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float boostSpeed = 20f;    // for boostTime seconds
    [SerializeField] float boostTime = 3f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float groundCheckDistance = .3f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] TrailRenderer sprintTrail;
    [SerializeField] TrailRenderer boostTrail;
    [SerializeField] ParticleSystem ballHitShockwave;        // when player hits any collider (プレイヤーが何かにヒットしたら、ヒットVFXを再生します。)
    [SerializeField] TrailRenderer ballHitTrail;        // when player gets kicked (蹴られたときのトレイルを再生します。)

    private Rigidbody rb;
    public CinemachineCamera cm_cam;
    private Vector2 moveInput;
    private bool isSprinting = false;
    private bool isBoosting = false;
    private bool playerFreezed = false;      // to freeze player while getting kicked (蹴られたときにプレイヤーを止めます。)
    private bool wasKickedBefore = false;       // to prevent player from getting kicked multiple times while multiple enemies chasing (蹴られたときに一度しか蹴られないようにします。)
    private float wasKickedCooldown = 3f;       // time to wait before player can get kicked again (蹴られた後、再び蹴られるまでの待機時間)

    public bool WasKickBefore => wasKickedBefore;

    void Awake()
    {
        // ** singleton **
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);

        // ** getting components **
        rb = GetComponent<Rigidbody>();

        // ** hiding cursor (カーソルを隠す) **
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

    // ** Input System - Callbacks (入力システム - コールバック) **
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        HandleJump();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.Get<float>() == 1f;
    }

    public void OnBoost(InputValue value)
    {
        HandleBoost();
    }

    // ** freeze player movements (プレイヤーの動きを止める) **
    public void FreezePlayer(bool freeze = true) => playerFreezed = freeze;

    // ** handling player movements (プレイヤーの動きを制御する) **
    void HandleMove()
    {
        if (playerFreezed) return;

        // Flatten the camera's forward and right (カメラの前方と右方向を平面化)
        // rotating player as per camera's y rotation (カメラのy軸回転に合わせてプレイヤーを回転)
        Vector3 camForward = cm_cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cm_cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // ** moving player (プレイヤーを移動させる) **
        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;
        Vector3 targetVelocity = move * (isBoosting ? boostSpeed : (isSprinting ? sprintSpeed : speed));        // priority boost > sprint > walk
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;

        rb.AddForce(velocityChange * Time.deltaTime, ForceMode.VelocityChange);
    }

    void HandleJump()
    {
        if (IsGrounded() && !playerFreezed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleBoost()
    {
        if (PlayerSystem.Instance.AvailableBoosts > 0)
        {
            isBoosting = true;
            Invoke(nameof(RemoveBoost), boostTime);
        }
    }

    void RemoveBoost()
    {
        isBoosting = false;
        PlayerSystem.Instance.RemoveBoost();
    }

    public void SetWasKickedBefore() => wasKickedBefore = true;

    void StartWasKickedCooldown()
    {
        if (wasKickedBefore)
        {
            wasKickedCooldown -= Time.deltaTime;

            if (wasKickedCooldown <= 0f)
            {
                wasKickedBefore = false;
                wasKickedCooldown = 3f;
            }
        }
    }

    // ** showing trails (トレイルを表示する) **
    void ShowTrails()
    {
        if (isSprinting || isBoosting) sprintTrail.emitting = true;
        else sprintTrail.emitting = false;
        if (isBoosting) boostTrail.emitting = true;
        else boostTrail.emitting = false;
    }

    // ** VFX **
    // whenever player hits any collider (プレイヤーが何かにヒットしたら)
    public void PlayHitVFX() => ballHitShockwave.Play();

    // when player gets kicked (プレイヤーが蹴られたとき)
    public void PlayKickHitVFX()
    {
        ballHitTrail.emitting = true;
        // stop emitting after 3 seconds (3秒後に発射を止める)
        Invoke(nameof(StopBallHitTrail), 3f);
    }

    void StopBallHitTrail() => ballHitTrail.emitting = false;

    bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
}
