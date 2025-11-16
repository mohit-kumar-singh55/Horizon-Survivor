using MagicPigGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("In Game UI")]
    [SerializeField] private HorizontalProgressBar healthBar;
    [Tooltip("Reduces with no. of kicks")]
    [SerializeField] private HorizontalProgressBar kickBar;
    [SerializeField] private Image boostImage;
    [SerializeField] private TMP_Text boostText;
    [SerializeField] private TMP_Text timerText;            // time left to win

    [Header("Menu UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject menuUI;

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

    public void UpdateHealthUI(float newHealth) => healthBar.SetProgress(newHealth / 100f);

    public void UpdateKickBarUI(float newVal) => kickBar.SetProgress(newVal);

    public void UpdateBoostUI(int curBoosts)
    {
        // changing color of image (画像の色を変更する)
        Color color = boostImage.color;
        if (curBoosts <= 0) color.a = .3f;
        else color.a = 1f;
        boostImage.color = color;

        // setting text (テキストを設定する)
        boostText.text = $"x{curBoosts}";
    }

    public void UpdateTimerUI(float timeInSeconds)
    {
        if (timeInSeconds <= 0)
        {
            timerText.text = "00:00";
            return;
        }

        float min = Mathf.Floor(timeInSeconds / 60);
        float sec = Mathf.Floor(timeInSeconds % 60);
        timerText.text = $"{min}:{sec}";
    }

    public void ShowMenuUI(bool show) => menuUI.SetActive(show);

    public void ShowGameOverPanelUI(bool show) => gameOverPanel.SetActive(show);

    public void ShowLoseUI(bool show) => loseUI.SetActive(show);

    public void ShowWinUI(bool show) => winUI.SetActive(show);
}
