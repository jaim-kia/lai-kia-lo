using UnityEngine;

public class EnemyGravity : MonoBehaviour, IEnemyMovement
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 20f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float raycastStartOffset = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    private float verticalVelocity;
    private bool active = true;

    private void Update()
    {
        if (!active) return;

        Vector3 origin = transform.position + Vector3.up * raycastStartOffset;
        float castDistance = raycastStartOffset + groundCheckDistance;

        bool grounded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castDistance, groundLayer);

        if (grounded && verticalVelocity <= 0f)
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y; 
            transform.position = pos;
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity -= gravityForce * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
        if (isActive) verticalVelocity = 0f;
    }
}