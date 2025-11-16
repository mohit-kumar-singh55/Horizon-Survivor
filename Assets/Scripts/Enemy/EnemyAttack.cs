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
        StartCoroutine(PlayKickSequence(ANIM_KICKING));     // kicking sequence (キックシーケンス)

        // Decreasing Health (健康の低下)
        PlayerSystem.Instance.TakeDamage(true);
    }

    IEnumerator PlayKickSequence(string ANIM_KICKING)
    {
        // ** 🔁 Step 1: Disable player control (プレイヤーの操作を無効にする) **
        playerController.enabled = false;
        playerRb.linearVelocity = Vector3.zero; // freeze player velocity (プレイヤーの速度を固定する)
        playerController.FreezePlayer(true);

        // ** 🔁 Step 2: Switch to cinematic camera (シネマティックカメラに切り替える) **
        cameraController.ShowCinematicCam(true);

        // ** 🔁 Step 3: Slow down time (時間を遅くする) **
        Time.timeScale = 0.15f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;       // for physics (物理学のために)

        // ** 🔁 Step 4: Play kick animation (キックアニメーションを再生する) **
        animator.SetTrigger(ANIM_KICKING);

        // ** 🔁 Step 5: Wait until foot reaches player (足がプレイヤーに届くまで待ってください) **
        yield return new WaitForSecondsRealtime(2.2f); // ➀  ↓

        // playing kicked sfx (キックの効果音を再生中)
        AudioManager.Instance.PlayKickExplosionSFX();

        yield return new WaitForSecondsRealtime(.6f);       // ➀ + this timing matches the foot contact with ball (このタイミングはボールとの接触と一致しています。)

        // Screen Shake
        cameraController.ScreenShake();

        // ** 🔁 Step 6: Apply physical kick (物理的なキックを適用する) **
        Vector3 dir = (player.transform.position - transform.position).normalized + Vector3.up * 0.5f;
        playerRb.AddForce(dir * 60f, ForceMode.Impulse);
        playerRb.AddTorque(transform.right * .05f);

        // ** 🔁 Step 7: Play hit animation of ball (ボールのヒットアニメーションを再生する) **
        playerController.PlayKickHitVFX();

        // ** 🔁 Step 8: Return to normal (元に戻る) **
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        cameraController.ShowCinematicCam(false);

        // playing bgm audios (BGMオーディオを再生する)
        AudioManager.Instance.PlayBGM();

        // ** 🔁 Step 9: Enable player control again (再びプレイヤーの操作を可能にする) **
        playerController.enabled = true;
        playerController.FreezePlayer(false);

        // ** waiting for player to go outoff enemy view range (敵の視界範囲から出るのを待っているプレイヤー) **
        yield return new WaitForSeconds(.3f);

        isKicking = false;
    }
}
