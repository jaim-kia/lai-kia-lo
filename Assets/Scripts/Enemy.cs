using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] private int contactDamage = 1;
    private int currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackHorizontalForce = 5f;
    [SerializeField] private float knockbackVerticalForce = 4f;
    [SerializeField] private float knockbackDuration = 0.15f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerStats>(out var playerStats))
        {
            playerStats.TakeDamage(contactDamage);

            Vector3 knockDir = (collision.transform.position - transform.position).normalized;
            PlayerController.Instance.ApplyKnockback(knockDir, knockbackHorizontalForce, knockbackVerticalForce, knockbackDuration);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}