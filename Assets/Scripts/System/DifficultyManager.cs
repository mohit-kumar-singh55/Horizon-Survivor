
using TMPro;
using UnityEngine;

public enum GameDifficulty { Easy, Normal, Hard };

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    // default setting to Normal (デフォルト設定をノーマルにする)
    [SerializeField] GameDifficulty currentDifficulty = GameDifficulty.Normal;

    [Header("Scriptable Objects")]
    [SerializeField] DifficultySettings easySettings;
    [SerializeField] DifficultySettings normalSettings;
    [SerializeField] DifficultySettings hardSettings;

    [Header("References")]
    [SerializeField] TMP_Dropdown difficultyDropdown;   // ui dropdown

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

    /// <summary>
    /// Loads the current difficulty from player prefs, defaulting to the value of currentDifficulty if not found.
    /// Also sets up a listener for the dropdown menu to set the difficulty when changed.
    /// player prefsから現在の難易度を読み込み、見つからない場合はcurrentDifficultyの値を使用し、dropdownメニューが変更されたときに難易度を設定します。
    /// </summary>
    void Start()
    {
        // load from player prefs or default to normal
        // player prefsから読み込むか、デフォルトをノーマルにする
        int saved = PlayerPrefs.GetInt(nameof(GameDifficulty), (int)currentDifficulty);  // 0=Easy, 1=Normal, 2=Hard
        currentDifficulty = (GameDifficulty)saved;

        difficultyDropdown.value = saved;
        difficultyDropdown.onValueChanged.AddListener(SetDifficulty);

        // apply default setting for the first time the game loads if there is no saved setting
        // 初回ゲームロード時に保存された設定がない場合、デフォルト設定を適用する
        SetDifficulty(saved);
    }

    public void SetDifficulty(int value)
    {
        currentDifficulty = (GameDifficulty)value;
        PlayerPrefs.SetInt(nameof(GameDifficulty), value);
        PlayerPrefs.Save();

        ApplySettings();

        Debug.Log("Difficulty set to " + currentDifficulty);
    }

    /// <summary>
    /// Applies the appropriate difficulty settings based on the current difficulty level.
    /// Updates the CurrentSettings field to match the selected GameDifficulty.
    /// 現在の難易度に基づいて適切な難易度設定を適用します。CurrentSettingsフィールドを選択されたGameDifficultyに合わせて更新します。
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
