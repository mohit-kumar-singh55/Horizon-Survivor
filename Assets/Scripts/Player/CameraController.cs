using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// カメラの切り替えと衝動の制御するクラス
/// </summary>
[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private CinemachineCamera freelookCam;
    [SerializeField] private CinemachineCamera cinematicCam;

    private CinemachineImpulseSource _impulseSource;

    void Awake()
    {
        // ** singleton **
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void ShowCinematicCam(bool show = true)
    {
        cinematicCam.gameObject.SetActive(show);
        freelookCam.gameObject.SetActive(!show);
    }

    // 蹴られたときの衝動
    public void ScreenShake() => _impulseSource.GenerateImpulse(20f);
}