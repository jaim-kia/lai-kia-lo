using UnityEngine;
using UnityEngine.InputSystem;

public class OneWayPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider platformCollider;

    [Header("Settings")]
    [SerializeField] private float topSurfaceTolerance = 0.15f;

    private Collider playerCollider;
    private Rigidbody playerRb;
    private Transform playerFeet;
    private bool playerNearby;

    private void Start()
    {
        var pc = PlayerController.Instance;
        playerCollider = pc.GetComponent<Collider>();
        playerRb = pc.GetComponent<Rigidbody>();
        playerFeet = pc.GroundCheck;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerController.Instance.gameObject)
            playerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerController.Instance.gameObject)
        {
            playerNearby = false;
            Physics.IgnoreCollision(playerCollider, platformCollider, false); 
        }
    }

    private void FixedUpdate()
    {
        if (!playerNearby || playerCollider == null) return;

        bool holdingDown = PlayerController.CameraMoveValue < -0.5f;

        float platformTop = platformCollider.bounds.max.y;
        bool playerAboveTop = playerFeet.position.y >= platformTop - topSurfaceTolerance;
        bool movingDownOrStill = playerRb.linearVelocity.y <= 0.05f;

        bool shouldCollide = playerAboveTop && movingDownOrStill && !holdingDown;

        Physics.IgnoreCollision(playerCollider, platformCollider, !shouldCollide);
    }
}