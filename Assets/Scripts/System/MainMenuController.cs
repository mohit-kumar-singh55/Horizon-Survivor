using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject fader;
    [SerializeField] private Image faderImage;
    [SerializeField] private AudioSource menuBGM;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject instructionsPanel;
    #endregion

    void Start()
    {
        faderImage = fader.GetComponent<Image>();
    }

    public void LoadNewGame() => FadeOutScreen();

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowOptions(bool show = true)
    {
        mainPanel.SetActive(!show);
        optionsPanel.SetActive(show);
    }

    public void ShowInstructions(bool show = true)
    {
        mainPanel.SetActive(!show);
        instructionsPanel.SetActive(show);
    }

    private void FadeOutScreen()
    {
        if (!fader || !faderImage) return;

        fader.SetActive(true);
        StartCoroutine(SetColorAlphaValueAndVolume());
    }

    // 画面フェードアウト
    IEnumerator SetColorAlphaValueAndVolume()
    {
        while (faderImage.color.a < 1f)
        {
            Color newColor = faderImage.color;
            newColor.a += .1f;
            faderImage.color = newColor;

            if (menuBGM.isPlaying && menuBGM.volume > 0) menuBGM.volume -= .1f;

            yield return new WaitForSeconds(.04f);
        }

        menuBGM.Stop(); // フェードアウト後に音声を完全に停止させる
        SceneLoader.LoadScene(2);
    }
}