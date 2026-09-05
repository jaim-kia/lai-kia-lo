using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineBrain _brain;
    [SerializeField] private CinemachineCamera[] _allVirtualCameras;

    [Header("Controls for lerping the Y Damping during player jump/fall")]
    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float FallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;

    private CinemachineCamera _currentCamera;
    private CinemachinePositionComposer _positionComposer;

    private float _normYPanAmount;

    private void Awake()
    {
        instance = this;

        if (_brain == null)
            _brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        RefreshCurrentCamera();
    }

    private void Update()
    {
        // keep tracking whichever camera is actually live (handles blends/switches too)
        if (_brain.ActiveVirtualCamera as CinemachineCamera != _currentCamera)
        {
            RefreshCurrentCamera();
        }
    }

    private void RefreshCurrentCamera()
    {
        var live = _brain.ActiveVirtualCamera as CinemachineCamera;
        if (live == null) return;

        _currentCamera = live;
        _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();

        if (_positionComposer != null)
            _normYPanAmount = _positionComposer.Damping.y;
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (_positionComposer == null) return;

        if (_lerpYPanCoroutine != null)
            StopCoroutine(_lerpYPanCoroutine);

        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = _positionComposer.Damping.y;
        float endDampAmount = isPlayerFalling ? _fallPanAmount : _normYPanAmount;

        if (isPlayerFalling)
            LerpedFromPlayerFalling = true;

        float elapsedTime = 0f;
        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / _fallYPanTime);

            Vector3 damping = _positionComposer.Damping;
            damping.y = lerpedPanAmount;
            _positionComposer.Damping = damping;

            yield return null;
        }

        IsLerpingYDamping = false;
    }
}