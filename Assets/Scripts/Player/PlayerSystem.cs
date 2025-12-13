using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーの力と死亡条件を制御するスクリプト
/// </summary>
public class PlayerSystem : MonoBehaviour
{
    public static PlayerSystem Instance { get; private set; }

    #region Serialize Fields
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
    #endregion

    #region Private Fields
    private GameManager _gameManager;
    private UIManager _uiManager;

    private int _currentKicks;
    private float _currentHealth;
    private int _availableBoosts;
    private bool _gameover = false;
    #endregion

    public static event Action OnPlayerDeathSequence = delegate { };

    public int AvailableBoosts => _availableBoosts;

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
        // initialize
        _gameManager = GameManager.Instance;
        _uiManager = UIManager.Instance;

        _currentHealth = maxHealth;
        _currentKicks = 0;

        _availableBoosts = 0;
        _uiManager.UpdateBoostUI(_availableBoosts);

        // ** 難易度に応じてキックの数を上書き **
        DifficultySettings settings = DifficultyManager.Instance?.CurrentSettings;
        kicksToDie = settings.kicksToDie;

        // ** 固定時間後にダメージを受ける **
        StartCoroutine(TakeDamageWithTime());
    }

    void Update()
    {
        if (!_gameover) PlayerDeathCondition();
    }

    void PlayerDeathCondition()
    {
        // キックによる死
        if (_currentKicks >= kicksToDie)
        {
            StartCoroutine(OnDeathByKick());
            _gameover = true;
        }
        // 時間に基づく健康の低下による死
        else if (_currentHealth <= 0)
        {
            _gameManager.TriggerLose(); // Trigger lose
            OnPlayerDeathSequence?.Invoke();

            _gameover = true;
        }
    }

    // アイテムを受け取れば健康を回復
    public bool IncreaseHealth(float healthToIncrease)
    {
        if (_currentHealth >= maxHealth) return false;

        _currentHealth += healthToIncrease;
        if (_currentHealth > maxHealth) _currentHealth = maxHealth;

        _uiManager.UpdateHealthUI(_currentHealth);

        return true;
    }

    // ダメージを受ける
    public void TakeDamage(bool isKick)
    {
        if (_gameover) return;

        if (isKick)
        {
            _currentKicks++;
            _currentHealth -= damageToTakePerKick;
            _uiManager.UpdateKickBarUI((float)(kicksToDie - _currentKicks) / kicksToDie);
        }
        else _currentHealth -= damageToTakeWithTime;

        if (_currentHealth < 0) _currentHealth = 0;

        _uiManager.UpdateHealthUI(_currentHealth);

        // Debug.Log(_currentHealth + " | " + _currentKicks);
    }

    public bool AddBoost()
    {
        if (_availableBoosts >= MaxNumberOfBoosts) return false;

        _availableBoosts++;
        _uiManager.UpdateBoostUI(_availableBoosts);

        return true;
    }

    public void RemoveBoost()
    {
        if (_availableBoosts <= 0) return;

        _availableBoosts--;
        _uiManager.UpdateBoostUI(_availableBoosts);
    }

    IEnumerator OnDeathByKick()
    {
        // プレイヤーが蹴られるまで待機
        yield return new WaitForSeconds(3f);

        _gameManager.TriggerLose(); // Trigger lose
        OnPlayerDeathSequence();
    }

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