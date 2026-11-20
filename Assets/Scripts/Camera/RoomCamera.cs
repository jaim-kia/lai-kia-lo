using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class RoomCamera : MonoBehaviour
{
    [SerializeField] private bool followPlayer = true;

    private CinemachineCamera cam;

    private void Start()
    {
        cam = GetComponent<CinemachineCamera>();

        if (followPlayer)
            cam.Target.TrackingTarget = PlayerController.Instance.CameraTarget;

        CameraManager.instance.Register(cam);
    }

    private void OnDestroy()
    {
        if (CameraManager.instance != null && cam != null)
            CameraManager.instance.Unregister(cam);
    }
}