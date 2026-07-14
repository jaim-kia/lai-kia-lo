using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Mana")]
    [SerializeField] private int maxMana = 100;
    private int currentMana;

    public static PlayerStats Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        currentHealth = maxHealth;
        currentMana = 0;
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
        Debug.Log("Player died");
        // handle death go jaim lol
    }

    public void AddMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        Debug.Log("Mana: " + currentMana);
    }

    public bool TrySpendMana(int amount)
    {
        if (currentMana < amount) return false;

        currentMana -= amount;
        Debug.Log("Mana: " + currentMana);
        return true;
    }
}