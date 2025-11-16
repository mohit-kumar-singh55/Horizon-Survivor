using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject fader;
    public Image faderImage;
    public AudioSource menuBGM;
    public GameObject mainPanel;
    public GameObject optionsPanel;

    void Start()
    {
        faderImage = fader.GetComponent<Image>();
    }

    public void LoadNewGame()
    {
        FadeOutScreen();
    }

    public void Quit() => Application.Quit();

    public void ShowOptions(bool show = true)
    {
        mainPanel.SetActive(!show);
        optionsPanel.SetActive(show);
    }

    void FadeOutScreen()
    {
        if (!fader || !faderImage) return;

        fader.SetActive(true);
        StartCoroutine(SetColorAlphaValueAndVolume());
    }

    /// <summary>
    /// Gradually increases the alpha value of the fader image to fade out the screen
    /// while simultaneously decreasing the volume of the menu background music.
    /// Once the screen is fully faded out and the music has stopped, loads the next scene.
    /// フェーダーイメージのアルファ値を徐々に増加させて画面をフェードアウトします。
    /// 同時にメニューの背景音楽の音量を徐々に減少させ、画面が完全にフェードアウトしたら次のシーンをロードします。
    /// </summary>
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

        menuBGM.Stop(); // Stop audio completely after fading out (フェードアウト後に音声を完全に停止させる)
        SceneLoader.LoadScene(2);
    }
}
