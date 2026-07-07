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

    private float horizontal;
    private bool grounded = false;
    private bool jumpCut = false; 

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

    private void Update()
    {
        // Timers only — no physics writes here
        if (grounded)
            cayoteTimeCounter = cayoteTime;
        else
            cayoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float verticalVelocity = rb.linearVelocity.y;

        if (jumpBufferCounter > 0f && cayoteTimeCounter > 0f)
        {
            verticalVelocity = jumpingPower;
            jumpBufferCounter = 0f;
            cayoteTimeCounter = 0f;
        }

        if (jumpCut && verticalVelocity > 0f)
        {
            verticalVelocity /= 2f;
            jumpCut = false;
        }

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
        if (context.performed)
            CameraMoveValue = context.ReadValue<float>();
        else if (context.canceled)
            CameraMoveValue = 0;
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                grounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            grounded = false;
    }
}