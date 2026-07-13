using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{

    [Header("Attack Settings")]
    [SerializeField] Transform attackPoint;
    [SerializeField] GameObject hitboxPrefab;
    [SerializeField] float attackOffsetDistance = 1f;
    [SerializeField] float hitboxLifetime = 0.15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var facing = PlayerController.Instance.Facing;
        Vector3 attackDirection = facing == PlayerController.FacingDirection.Right
            ? Vector3.right
            : Vector3.left;

        SpawnHitbox(attackDirection);
    }

    private void SpawnHitbox(Vector3 direction)
    {
        Vector3 spawnPos = attackPoint.position + direction * attackOffsetDistance;

        GameObject hitbox = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity, attackPoint);

        Destroy(hitbox, hitboxLifetime);
    }
}
