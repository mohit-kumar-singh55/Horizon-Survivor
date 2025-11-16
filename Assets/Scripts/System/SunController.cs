using UnityEngine;

public class SunController : MonoBehaviour
{
    [SerializeField] Light sunLight;
    [SerializeField] float dayDuration = 300f;      // 300 = 5min
    [SerializeField] AnimationCurve sunIntensityCurve;

    public delegate void SunSet();
    public static event SunSet OnSunSet;

    private float currentTime = 0f;
    private bool hasTriggerdWin = false;

    void Start()
    {
        // overriding day duration as per difficulty
        DifficultySettings settings = DifficultyManager.Instance.CurrentSettings;
        dayDuration = settings.dayDuration;
    }

    void Update()
    {
        if (hasTriggerdWin) return;

        currentTime += Time.deltaTime;

        // calculate progress
        float progress = Mathf.Clamp01(currentTime / dayDuration);

        // Rotate the sun (0 to 180 = rise to set)
        float sunAngle = Mathf.Lerp(20f, -50f, progress);        // need to adjust values
        transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

        // fade intensity based on progress
        if (sunIntensityCurve != null) sunLight.intensity = sunIntensityCurve.Evaluate(progress);

        // setting ui
        UIManager.Instance.UpdateTimerUI(dayDuration - currentTime);

        // trigger win
        if (!hasTriggerdWin && progress >= 1f)
        {
            hasTriggerdWin = true;
            OnSunSet?.Invoke();     // events to be triggered when win (勝利時にトリガーされるイベント)
            // Debug.Log("WIN!");
        }
    }
}
