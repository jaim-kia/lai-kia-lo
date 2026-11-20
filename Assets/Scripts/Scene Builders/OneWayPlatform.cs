using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider platformCollider;

    [Header("Settings")]
    [SerializeField] private float topSurfaceTolerance = 0.15f;
    [SerializeField] private float horizontalCheckDistance = 1.5f; // Prevents activating platforms too far away horizontally

    private Collider playerCollider;
    private Rigidbody playerRb;
    private Transform playerFeet;

    private void Start()
    {
        var pc = PlayerController.Instance;
        if (pc != null)
        {
            playerCollider = pc.GetComponent<Collider>();
            playerRb = pc.GetComponent<Rigidbody>();
            playerFeet = pc.GroundCheck;
        }
    }

    private void FixedUpdate()
    {
        if (playerCollider == null || playerFeet == null || playerRb == null) return;

        // 1. Check horizontal proximity so we only process platforms the player is actually near
        Vector3 playerPos = playerFeet.position;
        Vector3 closestPoint = platformCollider.ClosestPoint(new Vector3(playerPos.x, platformCollider.bounds.center.y, playerPos.z));
        float horizontalDist = Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(closestPoint.x, closestPoint.z));

        if (horizontalDist > horizontalCheckDistance)
        {
            // If too far horizontally, ensure collision is enabled and stop processing this platform
            Physics.IgnoreCollision(playerCollider, platformCollider, false);
            return;
        }

        // 2. Standard one-way logic
        bool holdingDown = PlayerController.CameraMoveValue < -0.5f;

        float platformTop = platformCollider.bounds.max.y;
        bool playerAboveTop = playerFeet.position.y >= platformTop - topSurfaceTolerance;
        bool movingDownOrStill = playerRb.linearVelocity.y <= 0.05f;

        bool shouldCollide = playerAboveTop && movingDownOrStill && !holdingDown;

        Physics.IgnoreCollision(playerCollider, platformCollider, !shouldCollide);
    }
}