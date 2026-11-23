using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

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

    private float _fallSpeedYDampingChangeThreshold;

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
    public Transform GroundCheck => groundCheck;
    public bool IsGrounded => grounded;

    [Header("Dash")]
    [SerializeField] private float dashForce = 30f;
    [SerializeField] private float dashDuration = 0.15f;

    public bool dashReady = true;
    private bool isDashing;
    private Vector3 dashVelocity;

    [Header("Air Dash")]
    [SerializeField] private int baseMaxAirDashes = 1;
    [SerializeField] private CharmData extraAirDashCharm; // optional: equipping it adds one more air dash

    private int airDashesUsed;
    public int MaxAirDashes =>
        baseMaxAirDashes + (extraAirDashCharm != null && Inventory.Instance != null && Inventory.Instance.IsEquipped(extraAirDashCharm) ? 1 : 0);

    [Header("Dash Through Enemies")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector3 dashCheckSize = new Vector3(1f, 1f, 1f);
    [SerializeField] private int dashManaOnPassThrough = 1;

    [SerializeField] float sprintSpeedMultiplier = 1.5f;
    private float baseSpeed;

    private bool isSlowed;

    private FacingDirection previousDirection;

    [Header("Recoil")]
    private bool isRecoiling;
    private Vector3 recoilVelocity;

    [Header("Knockback (from enemy hits)")]
    private bool isKnockedBack;
    private Vector3 knockbackVelocity;

    [SerializeField] private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;
    private bool frozen;
    private float heldHorizontal;

    private MaterialPropertyBlock propBlock;
    private static readonly int CustomFlipID = Shader.PropertyToID("_CustomFlip");

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        horizontal = state == GameState.Overworld ? heldHorizontal : 0f;

        if (state != GameState.Overworld)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    public void SetFrozen(bool value)
    {
        frozen = value;

        if (value)
        {
            horizontal = 0f;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
    }

    public void Teleport(Vector3 position)
    {
        position.z = transform.position.z;
        transform.position = position;
        rb.position = position;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
        baseSpeed = speed;
        Physics.gravity = grav;

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void ApplyKnockback(Vector3 direction, float horizontalForce, float verticalForce, float duration)
    {
        StopCoroutine(nameof(KnockbackRoutine));
        StartCoroutine(KnockbackRoutine(direction, horizontalForce, verticalForce, duration));
    }

    public void ApplyRecoil(Vector3 direction, float force, float duration)
    {
        StopCoroutine(nameof(RecoilRoutine));
        StartCoroutine(RecoilRoutine(direction, force, duration));
    }

    private System.Collections.IEnumerator RecoilRoutine(Vector3 direction, float force, float duration)
    {
        isRecoiling = true;
        recoilVelocity = direction.normalized * force;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isRecoiling = false;
    }

    private System.Collections.IEnumerator KnockbackRoutine(Vector3 direction, float horizontalForce, float verticalForce, float duration)
    {
        isKnockedBack = true;
        knockbackVelocity = new Vector3(direction.normalized.x * horizontalForce, verticalForce, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isKnockedBack = false;
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (isSlowed) return;
        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private System.Collections.IEnumerator SlowRoutine(float slowMultiplier, float duration)
    {
        isSlowed = true;
        speed = baseSpeed * slowMultiplier;

        yield return new WaitForSeconds(duration);

        speed = baseSpeed;
        isSlowed = false;
    }

    private void FixedUpdate()
    {
        if (frozen) return; 

        if (rb.linearVelocity.y < CameraManager.instance.FallSpeedYDampingChangeThreshold
            && !CameraManager.instance.IsLerpingYDamping
            && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (rb.linearVelocity.y >= 0f
            && !CameraManager.instance.IsLerpingYDamping
            && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }

        touchingWallRight = Physics.CheckBox(wallCheckRight.position, wallCheckSize / 2f, Quaternion.identity, wallLayer);
        touchingWallLeft = Physics.CheckBox(wallCheckLeft.position, wallCheckSize / 2f, Quaternion.identity, wallLayer);

        bool touchingWall = (touchingWallRight || touchingWallLeft) && !grounded;

        isWallSliding = touchingWall
            && ((touchingWallRight && horizontal > 0.1f) || (touchingWallLeft && horizontal < -0.1f))
            && rb.linearVelocity.y < 0f;

        if (isWallSliding)
        {
            doubleJumped = false;
            airDashesUsed = 0;
        }

        grounded = Physics.CheckBox(groundCheck.position, groundCheckSize / 2f, Quaternion.identity, groundLayer);
        
        if (grounded) {
            cayoteTimeCounter = cayoteTime;
            doubleJumped = false;
            airDashesUsed = 0;
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
            jumpCut = false;
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

        if (isDashing)
        {
            rb.linearVelocity = dashVelocity;
            animator.SetBool("isDashing", true);
            return;
        }
        else
        {
            
        }

        if (isKnockedBack)
        {
            rb.linearVelocity = knockbackVelocity;
            return;
        }

        if (isRecoiling)
        {
            rb.linearVelocity = new Vector3(recoilVelocity.x, verticalVelocity, 0);
            return;
        }

        // Final settings:
        float finalHorizontal;
        if (wallJumpLockCounter > 0f)
            finalHorizontal = wallJumpDir * (wallJumpForceX / speed); 
        else
            finalHorizontal = horizontal;

        previousDirection = facingDirection;

        if (!grounded)
        {
            animator.SetBool("isGrounded", false);
            if (verticalVelocity < 0)
            {
                // animator.SetBool("isFalling", true);
                animator.SetBool("isJumping", false);
            }
                
            else if (verticalVelocity > 0)
            {
                animator.SetBool("isJumping", true);
                // animator.SetBool("isFalling", false);
            }
        }
        else
        {
            animator.SetBool("isGrounded", true);
            // animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", false);
            
            if (finalHorizontal != 0)
            {
                animator.SetBool("isRunning", true);
            }
            else
            {
                animator.SetBool("isRunning", false);
            }
        }

        if (finalHorizontal > 0f)
        {
            facingDirection = FacingDirection.Right;
            // spriteRenderer.flipX = false;

            spriteRenderer.GetPropertyBlock(propBlock);
            propBlock.SetVector(CustomFlipID, new Vector2(1f, 1f));
            spriteRenderer.SetPropertyBlock(propBlock);
        }
        else if (finalHorizontal < 0)
        {
            facingDirection = FacingDirection.Left;    
            // spriteRenderer.flipX = true;

            spriteRenderer.GetPropertyBlock(propBlock);
            propBlock.SetVector(CustomFlipID, new Vector2(-1f, 1f));
            spriteRenderer.SetPropertyBlock(propBlock);
        }

        
        if (Mathf.Approximately(CameraMoveValue, 0f))
            rb.linearVelocity = new Vector3(finalHorizontal * speed, verticalVelocity, 0);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State != GameState.Overworld) return;
        heldHorizontal = context.ReadValue<Vector2>().x;
        horizontal = GameManager.Instance.State == GameState.Overworld ? heldHorizontal : 0f;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State != GameState.Overworld) return;

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

    public void Dash()
    {
        if (!dashReady || isDashing)
            return;

        if (!grounded)
        {
            if (airDashesUsed >= MaxAirDashes)
                return; // out of air dashes: don't consume dashReady either
            airDashesUsed++;
        }

        dashReady = false;
        StartCoroutine(DoDash());
    }

    private System.Collections.IEnumerator DoDash()
    {
        isDashing = true;

        float dir = facingDirection == FacingDirection.Right ? 1f : -1f;
        dashVelocity = new Vector3(dir * dashForce, 0f, 0f);

        int enemyLayerIndex = LayerMaskToLayer(enemyLayer);
        Physics.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, true);

        bool dashedThroughEnemy = false;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, dashCheckSize / 2f, transform.rotation, enemyLayer);
            if (hits.Length > 0)
                dashedThroughEnemy = true;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Physics.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, false);
        isDashing = false;
        animator.SetBool("isDashing", false);

        if (dashedThroughEnemy)
            PlayerStats.Instance.AddDashMana(dashManaOnPassThrough);
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 1)
        {
            layer >>= 1;
            layerNumber++;
        }
        return layerNumber;
    }

    public void ResetDash()
    {
        dashReady = true;
    }

    public void SprintEnable() {
        speed = baseSpeed * sprintSpeedMultiplier;
    }

    public void SprintDisable() {
        speed = baseSpeed;
    }
}