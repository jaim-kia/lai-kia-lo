using System.Collections.Generic;
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
    [SerializeField] private float swapBlendTime = 0.8f;
    public float FallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;

    private CinemachineCamera _currentCamera;
    private CinemachinePositionComposer _positionComposer;

    private float _normYPanAmount;

    private Vector2 _startingTrackedObjectOffset;
    private readonly List<CinemachineCamera> roomCameras = new List<CinemachineCamera>();

    // instant: used while a room is loading
    public void SetCutBlend()
    {
        _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
    }

    // smooth: used for camera swaps inside a room
    public void EnableSmoothSwaps()
    {
        _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, swapBlendTime);
    }

    public void Register(CinemachineCamera cam)
    {
        if (!roomCameras.Contains(cam)) roomCameras.Add(cam);
    }

    public void Unregister(CinemachineCamera cam)
    {
        roomCameras.Remove(cam);
    }


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

    public void ResetForRoomChange()
    {
        SetCutBlend();
        StopAllCoroutines();
        IsLerpingYDamping = false;
        LerpedFromPlayerFalling = false;
        _currentCamera = null;
        _positionComposer = null;
    }

    private void RefreshCurrentCamera()
    {
        var live = _brain.ActiveVirtualCamera as CinemachineCamera;
        if (live == null) return;

        _currentCamera = live;
        Debug.Log("Live camera is now: " + live.name);
        _positionComposer = live.GetComponent<CinemachinePositionComposer>();

        if (_positionComposer != null)
        {
            _normYPanAmount = _positionComposer.Damping.y;
            _startingTrackedObjectOffset = _positionComposer.TargetOffset;
        }
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

    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        if (_positionComposer == null) return;

        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos =  Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        if(!panToStartingPos)
        {
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.right;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.left;
                    break;
                default:
                    break;
            }

            endPos *= panDistance;
            startingPos = _startingTrackedObjectOffset;
            endPos += startingPos;
        }
        else
        {
            startingPos = _positionComposer.TargetOffset;
            endPos = _startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime / panTime));
            _positionComposer.TargetOffset = panLerp;

            yield return null;
        }
    }

    private static void SetPriority(CinemachineCamera cam, int value)
    {
        var p = cam.Priority;
        p.Enabled = true;   // without this the value is ignored (treated as 0)
        p.Value = value;
        cam.Priority = p;
    }

    public void ActivateCamera(CinemachineCamera target)
    {
        if (target == null) return;

        foreach (var cam in roomCameras)
        {
            if (cam == null) continue;
            SetPriority(cam, cam == target ? 20 : 10);
        }

        Debug.Log($"Activate '{target.name}': component enabled={target.enabled}, " +
                $"gameObject active={target.gameObject.activeInHierarchy}, " +
                $"priority enabled={target.Priority.Enabled}, value={target.Priority.Value}");
    }

    public void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        // exiting right -> the right camera, otherwise the left one
        ActivateCamera(triggerExitDirection.x > 0f ? cameraFromRight : cameraFromLeft);
    }
}