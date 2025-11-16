using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    [SerializeField] AudioClip[] bgm;
    [SerializeField] AudioSource bgmAS;

    [Header("Player SFX")]
    [SerializeField] AudioSource ballBounceSFX;

    [Header("During Kick SFX")]
    [SerializeField] AudioClip playerSpottedMaleSFX;
    [SerializeField] AudioClip playerSpottedFemaleSFX;
    [SerializeField] AudioClip chasingSFX;
    [SerializeField] AudioClip slowMotionSFX;
    [SerializeField] AudioClip kickExplosionSFX;

    [Header("Game Over SFX")]
    [SerializeField] AudioClip winSFX;
    [SerializeField] AudioClip loseSFX;

    [Header("Common Audio Sorce")]
    [SerializeField] AudioSource commonAS;

    private bool forceStopBGM = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // ** BGM **
        if (bgmAS == null || bgm.Length == 0)
        {
            Debug.LogWarning("No Audio Source or BGM Provided!");
            enabled = false;
            return;
        }

        PlayBGM();
        // DontDestroyOnLoad(gameObject);
    }

    void LateUpdate()
    {
        if (!forceStopBGM && !bgmAS.isPlaying) PlayBGM();
    }

    // ** BGM **
    public void PlayBGM()
    {
        if (bgmAS.isPlaying) return;

        forceStopBGM = false;
        bgmAS.PlayOneShot(bgm[Random.Range(0, bgm.Length)]);
    }

    public void StopBGM()
    {
        if (!bgmAS.isPlaying) return;

        forceStopBGM = true;
        bgmAS.Stop();
    }

    // ** Player SFX **
    public void PlayBallBounceSFX() => ballBounceSFX.Play();

    // ** During Kick SFX **
    public void PlayPlayerSpottedSFX(EnemyGender enemyGender)
    {
        if (commonAS.isPlaying) return;
        commonAS.PlayOneShot(enemyGender == EnemyGender.Male ? playerSpottedMaleSFX : playerSpottedFemaleSFX);
    }

    public void PlaySlowMotionSFX()
    {
        commonAS.Stop();
        commonAS.PlayOneShot(slowMotionSFX);
    }

    public void PlayKickExplosionSFX() => commonAS.PlayOneShot(kickExplosionSFX);

    // ** Game Over SFX **
    public void PlayWinSFX() => commonAS.PlayOneShot(winSFX);
    public void PlayLoseSFX() => commonAS.PlayOneShot(loseSFX);
}