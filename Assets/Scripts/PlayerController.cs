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

    private float cayoteTimeCounter;
    private float jumpBufferCounter;

    public static PlayerController Instance;
    
    private void Awake()
    {
        Physics.gravity = new Vector3(0, -19.6f, 0);

        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (grounded)
            cayoteTimeCounter = cayoteTime;
        else
            cayoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && cayoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpingPower, 0);

            jumpBufferCounter = 0f;
            cayoteTimeCounter = 0f; 
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(horizontal * speed, rb.linearVelocity.y, 0);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (context.canceled && rb.linearVelocity.y > 0) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y/2, 0);
        }
    }

    // Ground Check Logic - uses bit comparison for layers
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
        {
            grounded = false;
        }
    }
}
