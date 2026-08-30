using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // [SerializeField] private float followSpeed = 1f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float nudgeAmount = 10f;
    [SerializeField] private float nudgeSmoothTime = 0.3f;

    [SerializeField] private Transform lookAhead;
    private float horizontalNudge = 1.0f;
    private float currentHorizontalNudge = 1.0f;

    private float currentVerticalNudge;
    private float nudgeVelocity;
    private float nudgeHVelocity;

    // Update is called once per frame
    void LateUpdate()
    {
        float verticalInput = PlayerController.CameraMoveValue;
        float targetVerticalNudge = (Mathf.Abs(verticalInput) > 0.5f) ? verticalInput * nudgeAmount : 0f;

        currentVerticalNudge = Mathf.SmoothDamp(
            currentVerticalNudge, targetVerticalNudge, ref nudgeVelocity, nudgeSmoothTime);

        var facing = PlayerController.Instance.Facing;
        if (facing == PlayerController.FacingDirection.Right)
            horizontalNudge = 1.0f;
        else
            horizontalNudge = -1.0f;
        
        currentHorizontalNudge = Mathf.SmoothDamp(
            currentHorizontalNudge, horizontalNudge, ref nudgeHVelocity, nudgeSmoothTime);

        Vector3 playerPos = PlayerController.Instance.transform.position;

        transform.position = new Vector3(
            lookAhead.position.x + offset.x + currentHorizontalNudge,
            lookAhead.position.y + offset.y + currentVerticalNudge,
            lookAhead.position.z + offset.z 
        );
    }
}
