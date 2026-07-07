using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // [SerializeField] private float followSpeed = 1f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float nudgeAmount = 10f;
    [SerializeField] private float nudgeSmoothTime = 0.3f;

    private float currentVerticalNudge;
    private float nudgeVelocity;

    // Update is called once per frame
    void LateUpdate()
    {
        float verticalInput = PlayerController.CameraMoveValue;
        float targetNudge = (Mathf.Abs(verticalInput) > 0.5f) ? verticalInput * nudgeAmount : 0f;

        currentVerticalNudge = Mathf.SmoothDamp(
            currentVerticalNudge, targetNudge, ref nudgeVelocity, nudgeSmoothTime);

        Vector3 playerPos = PlayerController.Instance.transform.position;

        transform.position = new Vector3(
            playerPos.x + offset.x,
            playerPos.y + offset.y + currentVerticalNudge,
            playerPos.z + offset.z 
        );
    }
}
