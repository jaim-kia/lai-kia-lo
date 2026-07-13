using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 grav;

    [Header("Player Settings")]
    [SerializeField] float speed;
    [SerializeField] float jumpingPower;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;

    [Header("Responsiveness")]
    [SerializeField] float cayoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;

    [SerializeField] Transform groundCheck;
    [SerializeField] Vector3 groundCheckSize;

    private float horizontal;
    private bool grounded = false;
    private bool jumpCut = false; 
    private bool canDoubleJump = true;
    private bool doubleJumped = false;

    private float cayoteTimeCounter;
    private float jumpBufferCounter;

    public static PlayerController Instance;
    
    public static float CameraMoveValue;

    [Header("Wall Jump")]
    [SerializeField] Transform wallCheckRight;
    [SerializeField] Transform wallCheckLeft;
    [SerializeField] Vector3 wallCheckSize;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float wallSlideSpeed;
    [SerializeField] float wallJumpForceX;
    [SerializeField] float wallJumpForceY;
    [SerializeField] float wallJumpLockTime;

    private bool touchingWallRight;
    private bool touchingWallLeft;
    private bool isWallSliding;
    private float wallJumpLockCounter;
    private int wallJumpDir;

    public enum FacingDirection { Left, Right }
    private FacingDirection facingDirection = FacingDirection.Right;
    public FacingDirection Facing => facingDirection;

    private void Awake()
    {
        Physics.gravity = grav;

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void FixedUpdate()
    {
        touchingWallRight = Physics.CheckBox(wallCheckRight.position, wallCheckSize / 2f, Quaternion.identity, wallLayer);
        touchingWallLeft = Physics.CheckBox(wallCheckLeft.position, wallCheckSize / 2f, Quaternion.identity, wallLayer);

        bool touchingWall = (touchingWallRight || touchingWallLeft) && !grounded;

        isWallSliding = touchingWall
            && ((touchingWallRight && horizontal > 0.1f) || (touchingWallLeft && horizontal < -0.1f))
            && rb.linearVelocity.y < 0f;

        if (isWallSliding)
        {
            doubleJumped = false;
        }

        grounded = Physics.CheckBox(groundCheck.position, groundCheckSize / 2f, Quaternion.identity, groundLayer);
        
        if (grounded) {
            cayoteTimeCounter = cayoteTime;
            doubleJumped = false;
        }
        else
            cayoteTimeCounter -= Time.fixedDeltaTime;

        jumpBufferCounter -= Time.fixedDeltaTime;

        float verticalVelocity = rb.linearVelocity.y;

        if (wallJumpLockCounter > 0f)
            wallJumpLockCounter -= Time.fixedDeltaTime;

        if (isWallSliding && verticalVelocity < -wallSlideSpeed)
            verticalVelocity = -wallSlideSpeed;

        bool wantsToJump = jumpBufferCounter > 0f;
        bool canCayoteJump = cayoteTimeCounter > 0f;
        bool canWallJump = touchingWall && wantsToJump;
        bool canAirJump = !grounded && !canCayoteJump && canDoubleJump && !doubleJumped;

        if (wantsToJump && canCayoteJump)
        {
            verticalVelocity = jumpingPower;
            jumpBufferCounter = 0f;
            cayoteTimeCounter = 0f;
            jumpCut = false;
            doubleJumped = false;   
        }
        else if (canWallJump)
        {
            wallJumpDir = touchingWallRight ? -1 : 1;
            verticalVelocity = wallJumpForceY;
            wallJumpLockCounter = wallJumpLockTime;

            jumpBufferCounter = 0f;
            jumpCut = false;        // disable jumpcutting on walls
            doubleJumped = false;   
        }
        else if (wantsToJump && canAirJump)
        {
            verticalVelocity = jumpingPower;
            jumpBufferCounter = 0f;
            doubleJumped = true;
        }

        if (jumpCut && verticalVelocity > 0f)
        {
            verticalVelocity /= 3f;
            jumpCut = false;
        }

        // Setting Player Linear Velocity
        float finalHorizontal;
        if (wallJumpLockCounter > 0f)
            finalHorizontal = wallJumpDir * (wallJumpForceX / speed); 
        else
            finalHorizontal = horizontal;
        
        if (finalHorizontal > 0f)
            facingDirection = FacingDirection.Right;
        else if (finalHorizontal < 0)
            facingDirection = FacingDirection.Left;

        if (Mathf.Approximately(CameraMoveValue, 0f))
            rb.linearVelocity = new Vector3(finalHorizontal * speed, verticalVelocity, 0);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpBufferCounter = jumpBufferTime;
        else if (context.canceled)
            jumpCut = true;
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