using System;
using UnityEngine;

/// <summary>
/// 太陽の強度を制御する
/// </summary>
public class SunController : MonoBehaviour
{
    [SerializeField] Light sunLight;
    [SerializeField] float dayDuration = 300f;      // 300 = 5min
    [SerializeField] AnimationCurve sunIntensityCurve;

    public static event Action OnSunSet = delegate { };

    private float currentTime = 0f;
    private bool hasTriggerdWin = false;

    void Start()
    {
        // ** 難易度に応じて日の長さを上書き **
        DifficultySettings settings = DifficultyManager.Instance.CurrentSettings;
        dayDuration = settings.dayDuration;
    }

    void Update()
    {
        if (hasTriggerdWin) return;

        currentTime += Time.deltaTime;

        // プログレスを演算する
        float progress = Mathf.Clamp01(currentTime / dayDuration);

        // 太陽を回転させる（0から180）
        float sunAngle = Mathf.Lerp(20f, -50f, progress);        // need to adjust values
        transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

        // 進行度に応じて強度をフェードさせる
        if (sunIntensityCurve != null) sunLight.intensity = sunIntensityCurve.Evaluate(progress);

        // UIを更新
        UIManager.Instance.UpdateTimerUI(dayDuration - currentTime);

        // trigger win
        if (!hasTriggerdWin && progress >= 1f)
        {
            hasTriggerdWin = true;
            OnSunSet?.Invoke();     // 勝利時にトリガーされるイベント
            // Debug.Log("WIN!");
        }
    }
}