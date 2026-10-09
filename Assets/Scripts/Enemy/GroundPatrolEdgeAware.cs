using UnityEngine;

public class GroundPatrolEdgeAware : MonoBehaviour, IEnemyMovement
{
    [Header("Movement")]
    [SerializeField] private float speed = 2f;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Edge Detection")]
    [SerializeField] private float edgeCheckDistanceForward = 0.5f;
    [SerializeField] private float edgeCheckDepth = 1f; // must reach below the enemy's feet
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private int direction = 1;
    private bool active = true;

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            DrawChecks(direction);
        }
        else
        {
            DrawChecks(1);
            DrawChecks(-1);
        }
    }

    private void DrawChecks(int dir)
    {
        Vector3 wallStart = transform.position;
        Vector3 wallEnd = wallStart + Vector3.right * dir * wallCheckDistance;
        bool wallHit = Application.isPlaying && Physics.Raycast(wallStart, Vector3.right * dir, wallCheckDistance, wallLayer);

        Gizmos.color = Application.isPlaying ? (wallHit ? Color.red : Color.green) : Color.red;
        Gizmos.DrawLine(wallStart, wallEnd);
        Gizmos.DrawWireSphere(wallEnd, 0.05f);

        Vector3 edgeStart = transform.position + Vector3.right * dir * edgeCheckDistanceForward;
        Vector3 edgeEnd = edgeStart + Vector3.down * edgeCheckDepth;
        bool groundHit = Application.isPlaying && Physics.Raycast(edgeStart, Vector3.down, edgeCheckDepth, groundLayer);

        Gizmos.color = Application.isPlaying ? (groundHit ? Color.green : Color.red) : Color.yellow;
        Gizmos.DrawWireSphere(edgeStart, 0.05f);   // where the edge ray begins
        Gizmos.DrawLine(edgeStart, edgeEnd);
        Gizmos.DrawWireSphere(edgeEnd, 0.05f);     // how deep it reaches
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!active) return;

        if (IsWallAhead() || !IsGroundAhead())
            direction *= -1;

        rb.linearVelocity = new Vector3(direction * speed, rb.linearVelocity.y, 0f);
    }

    private bool IsWallAhead()
    {
        return Physics.Raycast(transform.position, Vector3.right * direction, wallCheckDistance, wallLayer);
    }

    private bool IsGroundAhead()
    {
        Vector3 origin = transform.position + Vector3.right * direction * edgeCheckDistanceForward;
        return Physics.Raycast(origin, Vector3.down, edgeCheckDepth, groundLayer);
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
        if (!isActive)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }
}