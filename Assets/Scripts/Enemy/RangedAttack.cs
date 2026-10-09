using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackCooldown = 2f;

    private float cooldownTimer;
    private Transform player;

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange && cooldownTimer <= 0f)
        {
            Fire();
            cooldownTimer = attackCooldown;
        }
    }

    private void Fire()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(direction));
    }
}