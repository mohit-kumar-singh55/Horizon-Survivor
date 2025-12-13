using UnityEngine;

/// <summary>
/// プレイヤーの衝突判定を管理するクラス
/// </summary>
public class PlayerColliderController : MonoBehaviour
{
    [SerializeField] private string itemTag = "Item";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(itemTag))
        {
            other.gameObject.GetComponent<ItemCollector>().Init();
            // Debug.Log("Item picked up");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerController.Instance.PlayHitVFX();     // プレイヤーが何かにヒットしたら、ヒットVFXを再生します

        if (collision.gameObject.CompareTag(TAGS.ENEMY))
        {
            // プレイヤーが敵にヒットした場合、追跡する
            if (!collision.gameObject.TryGetComponent(out EnemyController ec)) return;
            ec.ChasePlayerAfterHit();
        }

        // プレイヤーが何かにヒットしたら、ボールの反射音を再生します。
        AudioManager.Instance.PlayBallBounceSFX();
    }
}