using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class RoomCamera : MonoBehaviour
{
    private void Start()
    {
        GetComponent<CinemachineCamera>().Target.TrackingTarget = PlayerController.Instance.CameraTarget;
    }
}