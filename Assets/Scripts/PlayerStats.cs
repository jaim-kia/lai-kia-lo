using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Shield")]
    [SerializeField] private int maxShield = 5;
    private int currentShield;

    [Header("Combined Mana")]
    [SerializeField] private int maxTotalMana;
    [SerializeField] private int manualTriggerThreshold;
    [SerializeField] private int autoTriggerThreshold;

    private int attackMana;
    private int dashMana;

    public static PlayerStats Instance;

    public event Action OnAutoAttackSkill;
    public event Action OnAutoDashSkill;
    public event Action OnMaxManaSkill;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        currentHealth = maxHealth;
        currentShield = 0;
        attackMana = 0;
        dashMana = 0;
    }

    // ---------------- Health / Shield ----------------

    public void TakeDamage(int amount)
    {
        int remaining = amount;

        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, remaining);
            currentShield -= absorbed;
            remaining -= absorbed;
            Debug.Log("Shield absorbed: " + absorbed + ", Shield left: " + currentShield);
        }

        if (remaining > 0)
        {
            currentHealth -= remaining;
            Debug.Log("Health: " + currentHealth);

            if (currentHealth <= 0)
                Die();
        }
    }

    public void PayHealthCost(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Health (self cost): " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void AddShield(int amount)
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        Debug.Log("Shield: " + currentShield);
    }

    private void Die()
    {
        Debug.Log("Player died");
    }

    // ---------------- Mana ----------------

    public void AddAttackMana(int amount)
    {
        int allowed = Mathf.Min(amount, maxTotalMana - (attackMana + dashMana));
        attackMana += allowed;
        Debug.Log("Attack Mana: " + attackMana + " / Dash Mana: " + dashMana);

        CheckAutoTrigger();
    }

    public void AddDashMana(int amount)
    {
        int allowed = Mathf.Min(amount, maxTotalMana - (attackMana + dashMana));
        dashMana += allowed;
        Debug.Log("Attack Mana: " + attackMana + " / Dash Mana: " + dashMana);

        CheckAutoTrigger();
    }

    private void CheckAutoTrigger()
    {
        if (attackMana >= autoTriggerThreshold)
        {
            attackMana = 0;
            dashMana = 0;
            OnAutoAttackSkill?.Invoke();
            return;
        }

        if (dashMana >= autoTriggerThreshold)
        {
            attackMana = 0;
            dashMana = 0;
            OnAutoDashSkill?.Invoke();
            return;
        }

        if (attackMana + dashMana >= maxTotalMana)
        {
            attackMana = 0;
            dashMana = 0;
            OnMaxManaSkill?.Invoke();
        }
    }

    public bool TryManualAttackSkill()
    {
        if (attackMana < manualTriggerThreshold) return false;

        attackMana -= manualTriggerThreshold;
        return true;
    }

    public bool TryManualDashSkill()
    {
        if (dashMana < manualTriggerThreshold) return false;

        dashMana -= manualTriggerThreshold;
        return true;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Health: " + currentHealth);
    }

    public int returnHealth()
    {
        return currentHealth;
    }

    public int returnMaxHealth()
    {
        return maxHealth;
    }
}