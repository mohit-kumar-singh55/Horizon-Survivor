using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private UIManager _uiManager;
    private AudioManager _audioManager;
    private bool _gameEnded = false;
    private bool _menuActive = false;

    void OnEnable()
    {
        SunController.OnSunSet += TriggerWin;
    }

    void OnDisable()
    {
        SunController.OnSunSet -= TriggerWin;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _uiManager = UIManager.Instance;
        _audioManager = AudioManager.Instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_gameEnded) SetShowMenu();
    }

    public void ResumeGame() => SetShowMenu();

    public void SetShowMenu()
    {
        _menuActive = !_menuActive;
        _uiManager.ShowMenuUI(_menuActive);
        Time.timeScale = _menuActive ? 0 : 1;
        ShowCursor(_menuActive);
    }

    public void TriggerLose()
    {
        if (_gameEnded) return;

        _gameEnded = true;

        GameOverSequence();
        _uiManager.ShowLoseUI(true);

        // lose sfx
        _audioManager.PlayLoseSFX();
    }

    public void TriggerWin()
    {
        if (_gameEnded) return;

        _gameEnded = true;

        GameOverSequence();
        _uiManager.ShowWinUI(true);

        // win sfx
        _audioManager.PlayWinSFX();
    }

    public void GameOverSequence()
    {
        ShowCursor(true);
        _audioManager.StopBGM();
        _uiManager.ShowGameOverPanelUI(true);
        PlayerController.Instance.enabled = false;
        Time.timeScale = 0.05f;
    }

    void ShowCursor(bool show = true)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    public void ReloadGame()
    {
        // 新しいレベルをリロードします（必要に応じてすべてのシングルトンスクリプトも削除します）
        Instance = null;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}