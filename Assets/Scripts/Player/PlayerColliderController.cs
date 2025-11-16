using UnityEngine;

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
        PlayerController.Instance.PlayHitVFX();     // play hit vfx whenever player hits any collider (プレイヤーが何かにヒットしたら、ヒットVFXを再生します。)

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // chasing player if player hits the enemy
            // プレイヤーが敵にヒットした場合、追跡する
            collision.gameObject.GetComponent<EnemyController>().ChasePlayerAfterHit();
        }

        // playing ball bounce sfx whenever player hits any collider
        // プレイヤーが何かにヒットしたら、ボールの反射音を再生します。
        AudioManager.Instance.PlayBallBounceSFX();
    }
}
