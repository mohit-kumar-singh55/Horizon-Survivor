using System;
using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private Transform foot;

    private Animator animator;
    private GameObject player;
    private Rigidbody playerRb;
    private PlayerController playerController;
    private CameraController cameraController;

    private bool isKicking = false;

    public bool IsKicking => isKicking;

    void Start()
    {
        // initialize
        animator = GetComponent<Animator>();
        player = PlayerController.Instance.gameObject;
        cameraController = CameraController.Instance;

        playerRb = player.GetComponent<Rigidbody>();
        playerController = player.GetComponent<PlayerController>();
    }

    // NPCがプレイヤーを蹴る
    public void Attack(string ANIM_KICKING)
    {
        if (isKicking) return;

        isKicking = true;
        StartCoroutine(PlayKickSequence(ANIM_KICKING));     // 攻撃開始

        // 健康の低下
        PlayerSystem.Instance.TakeDamage(true);
    }

    IEnumerator PlayKickSequence(string ANIM_KICKING)
    {
        // ** 1: プレイヤーを完全に止める **
        playerController.enabled = false;
        playerRb.linearVelocity = Vector3.zero;
        playerController.FreezePlayer(true);

        // ** 2: cinematic カメラに切り替える **
        cameraController.ShowCinematicCam(true);

        // ** 3: 時間を遅くする **
        Time.timeScale = 0.15f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // ** 4: キックアニメション **
        animator.SetTrigger(ANIM_KICKING);

        // ** 5: 足がプレイヤーに届くまで待ってください **
        yield return new WaitForSecondsRealtime(2.2f); // ➀  ↓

        // キックの効果音を再生中
        AudioManager.Instance.PlayKickExplosionSFX();

        yield return new WaitForSecondsRealtime(.6f);       // ➀ + このタイミングはボールとの接触と一致しています

        // 画面を揺らす
        cameraController.ScreenShake();

        // ** 6: 物理的なキックを適用する **
        Vector3 dir = (player.transform.position - transform.position).normalized + Vector3.up * 0.5f;
        playerRb.AddForce(dir * 60f, ForceMode.Impulse);
        playerRb.AddTorque(transform.right * .05f);

        // ** 7: ボールのヒットアニメーションを再生する **
        playerController.PlayKickHitVFX();

        // ** 8: 全てを元に戻す **
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        cameraController.ShowCinematicCam(false);

        // BGMオーディオを再生する
        AudioManager.Instance.PlayBGM();

        // ** 9: 再びプレイヤーの操作を可能にする **
        playerController.enabled = true;
        playerController.FreezePlayer(false);

        // 敵の視界範囲から出るのを待っているプレイヤー
        yield return new WaitForSeconds(.3f);

        isKicking = false;
    }
}