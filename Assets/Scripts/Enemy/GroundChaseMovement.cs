using UnityEngine;

public class GroundChaseMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRadius = 5f;
    [SerializeField] private float chaseSpeed = 3f;

    [Header("Safety Checks (so it still won't walk off edges/into walls)")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float edgeCheckDistanceForward = 0.5f;
    [SerializeField] private float edgeCheckDepth = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Optional: hand control back when player leaves range")]
    [SerializeField] private MonoBehaviour patrolComponent;

    private IEnemyMovement patrol;
    private Transform player;
    private Rigidbody rb;
    private bool active = true;
    private bool isChasing;

        private void OnDrawGizmosSelected()
        {
            // chase radius
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);

            if (Application.isPlaying && player != null)
            {
                int dir = player.position.x > transform.position.x ? 1 : -1;
                DrawChecks(dir);
            }
            else
            {
                DrawChecks(1);
                DrawChecks(-1);
            }
        }

        private void DrawChecks(int dir)
        {
            // wall ray
            Vector3 wallStart = transform.position;
            Vector3 wallEnd = wallStart + Vector3.right * dir * wallCheckDistance;
            bool wallHit = Application.isPlaying && Physics.Raycast(wallStart, Vector3.right * dir, wallCheckDistance, wallLayer);

            Gizmos.color = Application.isPlaying ? (wallHit ? Color.red : Color.green) : Color.red;
            Gizmos.DrawLine(wallStart, wallEnd);
            Gizmos.DrawWireSphere(wallEnd, 0.05f);

            // edge ray
            Vector3 edgeStart = transform.position + Vector3.right * dir * edgeCheckDistanceForward;
            Vector3 edgeEnd = edgeStart + Vector3.down * edgeCheckDepth;
            bool groundHit = Application.isPlaying && Physics.Raycast(edgeStart, Vector3.down, edgeCheckDepth, groundLayer);

            Gizmos.color = Application.isPlaying ? (groundHit ? Color.green : Color.red) : Color.yellow;
            Gizmos.DrawWireSphere(edgeStart, 0.05f);
            Gizmos.DrawLine(edgeStart, edgeEnd);
            Gizmos.DrawWireSphere(edgeEnd, 0.05f);
        }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        patrol = patrolComponent as IEnemyMovement;
    }

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void FixedUpdate()
    {
        if (!active) return;

        float distance = Mathf.Abs(player.position.x - transform.position.x);
        isChasing = distance <= chaseRadius;

        patrol?.SetActive(!isChasing);

        if (!isChasing)
        {
            if (patrol == null) StopHorizontal();
            return;
        }

        int direction = player.position.x > transform.position.x ? 1 : -1;

        if (IsWallAhead(direction) || !IsGroundAhead(direction))
        {
            StopHorizontal();
            return;
        }

        rb.linearVelocity = new Vector3(direction * chaseSpeed, rb.linearVelocity.y, 0f);
    }

    private void StopHorizontal()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    private bool IsWallAhead(int direction)
    {
        return Physics.Raycast(transform.position, Vector3.right * direction, wallCheckDistance, wallLayer);
    }

    private bool IsGroundAhead(int direction)
    {
        Vector3 origin = transform.position + Vector3.right * direction * edgeCheckDistanceForward;
        return Physics.Raycast(origin, Vector3.down, edgeCheckDepth, groundLayer);
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
        if (!isActive) patrol?.SetActive(false);
    }

}