using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private CinemachineCamera freelookCam;
    [SerializeField] private CinemachineCamera cinematicCam;

    private CinemachineImpulseSource impulseSource;

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
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void ShowCinematicCam(bool show = true)
    {
        cinematicCam.gameObject.SetActive(show);
        freelookCam.gameObject.SetActive(!show);
    }

    // Impulse when getting kicked
    // 蹴られたときの衝動
    public void ScreenShake() => impulseSource.GenerateImpulse(20f);
}
