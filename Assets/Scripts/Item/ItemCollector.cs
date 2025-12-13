using UnityEngine;
using NaughtyAttributes;

public enum ItemType { OXY_CAN, TOOLBOX };

/// <summary>
/// アイテムを収集し、アイテムの種類に応じた処理を行うクラス
/// </summary>
public class ItemCollector : MonoBehaviour
{
    public ItemType type = ItemType.OXY_CAN;

    [ShowIf("type", ItemType.OXY_CAN)]
    [SerializeField, Range(0, 100)] private float lifeToIncrease = 40;       // パーセント

    public void Init()
    {
        if (type == ItemType.OXY_CAN)
        {
            bool can_destroy = PlayerSystem.Instance.IncreaseHealth(lifeToIncrease);
            if (can_destroy) Destroy(gameObject);

            // Debug.Log("Life Increased by " + lifeToIncrease + "%");
        }
        else
        {
            bool can_destroy = PlayerSystem.Instance.AddBoost();
            if (can_destroy) Destroy(gameObject);

            // Debug.Log("Boost Added");
        }
    }
}
