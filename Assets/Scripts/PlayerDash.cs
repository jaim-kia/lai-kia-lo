using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float sprintHoldThreshold = 0.2f;

    private bool isHeld;
    private bool isSprinting;
    private float holdTimer;

    void Update()
    {
        if (isHeld && !isSprinting)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= sprintHoldThreshold)
            {
                isSprinting = true;
                StartSprint();
            }
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                isHeld = true;
                holdTimer = 0f;
                isSprinting = false;
                DoDash();
                break;

            case InputActionPhase.Canceled:
                PlayerController.Instance.ResetDash();
                isHeld = false;
                if (isSprinting)
                {
                    isSprinting = false;
                    StopSprint();
                }
                break;
        }
    }

    private void DoDash()
    {
        PlayerController.Instance.Dash();
    }

    private void StartSprint()
    {
        PlayerController.Instance.SprintEnable();
    }

    private void StopSprint()
    {
        PlayerController.Instance.SprintDisable();
    }
}