using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody rb;

    [Header("Player Settings")]
    [SerializeField] float speed;
    [SerializeField] float jumpingPower;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;

    [Header("Responsiveness")]
    [SerializeField] float cayoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;

    [SerializeField] Transform groundCheck;
    [SerializeField] Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);

    private float horizontal;
    private bool grounded = false;
    private bool jumpCut = false; 
    private bool canDoubleJump = true;
    private bool doubleJumped = false;

    private float cayoteTimeCounter;
    private float jumpBufferCounter;

    public static PlayerController Instance;
    
    public static float CameraMoveValue;
    private void Awake()
    {
        Physics.gravity = new Vector3(0, -19.6f, 0);

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void FixedUpdate()
    {
        grounded = Physics.CheckBox(groundCheck.position, groundCheckSize / 2f, Quaternion.identity, groundLayer);
        
        if (grounded) {
            cayoteTimeCounter = cayoteTime;
            doubleJumped = false;
        }
        else
            cayoteTimeCounter -= Time.fixedDeltaTime;

        jumpBufferCounter -= Time.fixedDeltaTime;

        float verticalVelocity = rb.linearVelocity.y;

        bool wantsToJump = jumpBufferCounter > 0f;
        bool canCayoteJump = cayoteTimeCounter > 0f;
        bool canAirJump = !grounded && !canCayoteJump && canDoubleJump && !doubleJumped;

        if (wantsToJump && canCayoteJump)
        {
            verticalVelocity = jumpingPower;
            jumpBufferCounter = 0f;
            cayoteTimeCounter = 0f;
            jumpCut = false;
            doubleJumped = false;
        }
        else if (wantsToJump && canAirJump)
        {
            verticalVelocity = jumpingPower;
            jumpBufferCounter = 0f;
            doubleJumped = true;
            jumpCut = false;
        }

        if (jumpCut && verticalVelocity > 0f && !doubleJumped)
        {
            verticalVelocity /= 3f;
            jumpCut = false;
        }

        if (Mathf.Approximately(CameraMoveValue, 0f))
            rb.linearVelocity = new Vector3(horizontal * speed, verticalVelocity, 0);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (context.canceled)
        {
            jumpCut = true;
        }
    }

    public void CameraMove(InputAction.CallbackContext context)
    {
        if (rb.linearVelocity != new Vector3(0, 0, 0))
            CameraMoveValue = 0;
        else if (context.performed)
            CameraMoveValue = context.ReadValue<float>();
        else if (context.canceled)
            CameraMoveValue = 0;
    }
}