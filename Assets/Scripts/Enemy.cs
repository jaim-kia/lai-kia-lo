using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] private int contactDamage = 1;
    private int currentHealth;

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
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}