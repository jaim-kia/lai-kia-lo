using System.Collections;
using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider platformCollider;
    [SerializeField] private Renderer platformRenderer; 

    [Header("Break Settings")]
    [SerializeField] private float breakDelay = 1.5f;   
    [SerializeField] private float respawnTime = 3.0f; 

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float shakeSpeed = 50f;  

    [Header("One-Way Settings")]
    [SerializeField] private float topSurfaceTolerance = 0.15f;

    private Collider playerCollider;
    private Rigidbody playerRb;
    private Transform playerFeet;
    private bool playerNearby;

    private Coroutine breakCoroutine;
    private bool isBroken;
    private Vector3 originalLocalPosition;

    private void Start()
    {
        var pc = PlayerController.Instance;
        playerCollider = pc.GetComponent<Collider>();
        playerRb = pc.GetComponent<Rigidbody>();
        playerFeet = pc.GroundCheck;

        if (platformRenderer == null)
            platformRenderer = GetComponent<Renderer>();

        // Store standard position for resetting shake
        originalLocalPosition = transform.localPosition;
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
            StopBreakTimer();
            if (!isBroken)
                Physics.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    private void FixedUpdate()
    {
        if (isBroken || !playerNearby || playerCollider == null) return;

        float platformTop = platformCollider.bounds.max.y;
        
        // Check if player's feet are grounded directly on top of the platform
        bool playerGroundedOnTop = playerFeet.position.y >= platformTop - topSurfaceTolerance 
                                  && Mathf.Abs(playerRb.linearVelocity.y) <= 0.05f;

        // Standard one-way pass-through logic
        bool movingDownOrStill = playerRb.linearVelocity.y <= 0.05f;
        bool playerAboveTop = playerFeet.position.y >= platformTop - topSurfaceTolerance;
        bool shouldCollide = playerAboveTop && movingDownOrStill;

        Physics.IgnoreCollision(playerCollider, platformCollider, !shouldCollide);

        // Start breaking sequence only when standing firm on top
        if (playerGroundedOnTop)
        {
            if (breakCoroutine == null)
                breakCoroutine = StartCoroutine(BreakRoutine());
        }
        else
        {
            StopBreakTimer();
        }
    }

    private IEnumerator BreakRoutine()
    {
        float timer = 0f;

        // Shake while counting down
        while (timer < breakDelay)
        {
            timer += Time.deltaTime;

            // Simple noise-based jitter relative to start position
            float xOffset = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * 2f * shakeIntensity;
            float zOffset = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * 2f * shakeIntensity;
            transform.localPosition = originalLocalPosition + new Vector3(xOffset, 0f, zOffset);

            yield return null;
        }

        // Reset position before disappearing
        transform.localPosition = originalLocalPosition;

        // Disappear / "Break"
        isBroken = true;
        platformCollider.enabled = false;
        if (platformRenderer != null) platformRenderer.enabled = false;

        // Reset collision logic for player
        if (playerCollider != null)
            Physics.IgnoreCollision(playerCollider, platformCollider, false);

        // Optional Respawn Logic
        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);
            Respawn();
        }
    }

    private void StopBreakTimer()
    {
        if (breakCoroutine != null)
        {
            StopCoroutine(breakCoroutine);
            breakCoroutine = null;
        }

        // Return to resting position when timer is aborted
        transform.localPosition = originalLocalPosition;
    }

    public void Respawn()
    {
        StopBreakTimer();
        isBroken = false;
        transform.localPosition = originalLocalPosition;
        platformCollider.enabled = true;
        if (platformRenderer != null) platformRenderer.enabled = true;
    }
}