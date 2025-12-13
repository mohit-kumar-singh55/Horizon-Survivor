
using TMPro;
using UnityEngine;

public enum GameDifficulty { Easy, Normal, Hard };

/// <summary>
/// ゲームの難易度を管理するクラス
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    #region Serialized Fields
    // デフォルト設定をノーマルにする
    [SerializeField] GameDifficulty currentDifficulty = GameDifficulty.Normal;

    [Header("Scriptable Objects")]
    [SerializeField] DifficultySettings easySettings;
    [SerializeField] DifficultySettings normalSettings;
    [SerializeField] DifficultySettings hardSettings;

    [Header("References")]
    [SerializeField] TMP_Dropdown difficultyDropdown;   // ui dropdown
    #endregion

    public DifficultySettings CurrentSettings { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // player prefsから読み込むか、デフォルトをノーマルにする
        int saved = PlayerPrefs.GetInt(nameof(GameDifficulty), (int)currentDifficulty);  // 0=Easy, 1=Normal, 2=Hard
        currentDifficulty = (GameDifficulty)saved;

        difficultyDropdown.value = saved;
        difficultyDropdown.onValueChanged.AddListener(SetDifficulty);

        // 初回ゲームロード時に保存された設定がない場合、デフォルト設定を適用する
        SetDifficulty(saved);
    }

    public void SetDifficulty(int value)
    {
        currentDifficulty = (GameDifficulty)value;
        PlayerPrefs.SetInt(nameof(GameDifficulty), value);
        PlayerPrefs.Save();

        ApplySettings();

        // Debug.Log("Difficulty set to " + currentDifficulty);
    }

    /// <summary>
    /// 現在の難易度に基づいて適切な難易度設定を適用します。
    /// CurrentSettingsフィールドを選択されたGameDifficultyに合わせて更新します。
    /// </summary>
    void ApplySettings()
    {
        switch (currentDifficulty)
        {
            case GameDifficulty.Easy: CurrentSettings = easySettings; break;
            case GameDifficulty.Normal: CurrentSettings = normalSettings; break;
            case GameDifficulty.Hard: CurrentSettings = hardSettings; break;
        }
    }
}