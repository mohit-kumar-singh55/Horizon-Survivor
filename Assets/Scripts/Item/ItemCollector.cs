using UnityEngine;
using NaughtyAttributes;

public enum ItemType { OXY_CAN, TOOLBOX };

public class ItemCollector : MonoBehaviour
{
    public ItemType type = ItemType.OXY_CAN;

    [ShowIf("type", ItemType.OXY_CAN)]
    [Range(0, 100)]
    [SerializeField]
    private float lifeToIncrease = 40;       // percent

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
