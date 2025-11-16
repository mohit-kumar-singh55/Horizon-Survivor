using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton Class
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private UIManager uiManager;
    private AudioManager audioManager;
    private bool gameEnded = false;
    private bool menuActive = false;

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
        uiManager = UIManager.Instance;
        audioManager = AudioManager.Instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameEnded) SetShowMenu();
    }

    public void ResumeGame() => SetShowMenu();

    public void SetShowMenu()
    {
        menuActive = !menuActive;
        uiManager.ShowMenuUI(menuActive);
        Time.timeScale = menuActive ? 0 : 1;
        ShowCursor(menuActive);
    }

    public void TriggerLose()
    {
        if (gameEnded) return;

        gameEnded = true;

        GameOverSequence();
        uiManager.ShowLoseUI(true);

        // lose sfx
        audioManager.PlayLoseSFX();
    }

    public void TriggerWin()
    {
        if (gameEnded) return;

        gameEnded = true;

        GameOverSequence();
        uiManager.ShowWinUI(true);

        // win sfx
        audioManager.PlayWinSFX();
    }

    public void GameOverSequence()
    {
        ShowCursor(true);
        audioManager.StopBGM();
        uiManager.ShowGameOverPanelUI(true);
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
        // reload fresh level (remove all singleton script as well if needed)
        // 新しいレベルをリロードします（必要に応じてすべてのシングルトンスクリプトも削除します）
        Instance = null;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    // TODO: create go to next level function

    public void QuitGame() => Application.Quit();
}
