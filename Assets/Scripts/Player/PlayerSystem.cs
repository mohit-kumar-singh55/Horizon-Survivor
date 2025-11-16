using System.Collections;
using UnityEngine;

// ** Script for controlling player power and death condition **
public class PlayerSystem : MonoBehaviour
{
    public static PlayerSystem Instance { get; private set; }

    [Tooltip("Number of kicks to die (player will die no matter what's the health)")] // 死亡するまでのキックの数（プレイヤーは健康状態に関係なく死亡します）
    [SerializeField][Range(1, 5)] int kicksToDie = 3;
    [SerializeField][Range(0f, 100f)] float maxHealth = 100f;
    [Tooltip("Damage to take when kicked")] // 蹴られたときのダメージ
    [SerializeField][Range(0f, 10f)] float damageToTakePerKick = 10f;
    [Tooltip("Damage to take after a fixed time (not related to sunset)")]  // 固定時間後にダメージを受ける
    [SerializeField][Range(0f, 10f)] float damageToTakeWithTime = 5f;
    [Tooltip("Take damange after every x seconds")] // x秒毎にダメージを受ける
    [SerializeField] float timeToTakeDamage = 10f;
    [Tooltip("Maximum number of boosts player can equip at once")]  // プレイヤーが同時に装備可能なブーストの最大数
    [SerializeField][Range(0, 5)] int MaxNumberOfBoosts = 3;

    private GameManager gameManager;
    private UIManager uiManager;

    private int currentKicks;
    private float currentHealth;
    private int availableBoosts;
    private bool gameover = false;

    public delegate void PlayerDeathSequence();
    public static event PlayerDeathSequence OnPlayerDeathSequence;

    public int AvailableBoosts => availableBoosts;

    void Awake()
    {
        // ** singleton **
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;

        currentHealth = maxHealth;
        currentKicks = 0;

        availableBoosts = 0;
        uiManager.UpdateBoostUI(availableBoosts);

        // ** overriding kicksToDie as per difficulty (難易度に応じてキックの数を上書き) **
        DifficultySettings settings = DifficultyManager.Instance?.CurrentSettings;
        kicksToDie = settings.kicksToDie;

        // ** taking damage after time (固定時間後にダメージを受ける) **
        StartCoroutine(TakeDamageWithTime());
    }

    void Update()
    {
        if (!gameover) PlayerDeathCondition();
    }

    void PlayerDeathCondition()
    {
        // death by kick (キックによる死)
        if (currentKicks >= kicksToDie)
        {
            StartCoroutine(OnDeathByKick());
            gameover = true;
        }
        // death by health that is based on time (時間に基づく健康の低下による死)
        else if (currentHealth <= 0)
        {
            gameManager.TriggerLose(); // Trigger lose
            OnPlayerDeathSequence?.Invoke();

            gameover = true;
        }
    }

    public bool IncreaseHealth(float healthToIncrease)
    {
        if (currentHealth >= maxHealth) return false;

        currentHealth += healthToIncrease;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        uiManager.UpdateHealthUI(currentHealth);

        return true;
    }

    public void TakeDamage(bool isKick)
    {
        if (gameover) return;

        if (isKick)
        {
            currentKicks++;
            currentHealth -= damageToTakePerKick;
            uiManager.UpdateKickBarUI((float)(kicksToDie - currentKicks) / kicksToDie);
        }
        else currentHealth -= damageToTakeWithTime;

        if (currentHealth < 0) currentHealth = 0;

        uiManager.UpdateHealthUI(currentHealth);

        Debug.Log(currentHealth + " | " + currentKicks);
    }

    public bool AddBoost()
    {
        if (availableBoosts >= MaxNumberOfBoosts) return false;

        availableBoosts++;
        uiManager.UpdateBoostUI(availableBoosts);

        return true;
    }

    public void RemoveBoost()
    {
        if (availableBoosts <= 0) return;

        availableBoosts--;
        uiManager.UpdateBoostUI(availableBoosts);
    }

    IEnumerator OnDeathByKick()
    {
        // waiting for player to get kicked (プレイヤーが蹴られるまで待機)
        yield return new WaitForSeconds(3f);

        gameManager.TriggerLose(); // Trigger lose
        OnPlayerDeathSequence();
    }

    // take damage every x seconds (not related to sunset)
    // x秒毎にダメージを受ける
    IEnumerator TakeDamageWithTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToTakeDamage);
            TakeDamage(false);
        }
    }
}
