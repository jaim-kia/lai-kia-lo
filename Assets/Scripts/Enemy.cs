using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 30;
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

    private void Die()
    {
        Destroy(gameObject);
    }
}