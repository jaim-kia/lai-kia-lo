using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;
    // [SerializeField] private float followSpeed = 1f;
    [SerializeField] private float nudgeAmount = 5f;
    [SerializeField] private float nudgeSmoothTime = 0.3f;

    private float horizontalNudge = 1.0f;
    private float currentHorizontalNudge = 1.0f;

    private float currentVerticalNudge;
    private float nudgeVelocity;
    private float nudgeHVelocity;

    public bool isPanning;

    private void Awake()
    {
        instance = this;    
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float verticalInput = PlayerController.CameraMoveValue;

        // add constraints for when its pan camera
        float targetVerticalNudge = (Mathf.Abs(verticalInput) > 0.5f) ? verticalInput * nudgeAmount : 0f;

        currentVerticalNudge = (!isPanning) ?
            Mathf.SmoothDamp(currentVerticalNudge, targetVerticalNudge, ref nudgeVelocity, nudgeSmoothTime): 
            currentVerticalNudge;

        var facing = PlayerController.Instance.Facing;
        if (facing == PlayerController.FacingDirection.Right)
            horizontalNudge = 1.0f;
        else
            horizontalNudge = -1.0f;
        
        currentHorizontalNudge = Mathf.SmoothDamp(
            currentHorizontalNudge, horizontalNudge, ref nudgeHVelocity, nudgeSmoothTime);


        transform.localPosition = new Vector3(
            currentHorizontalNudge,
            currentVerticalNudge,
            transform.localPosition.z
        );
    }
}
