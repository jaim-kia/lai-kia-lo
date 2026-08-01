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

    [Header("Currency")]
    [SerializeField] private int incenseSticks = 3;

    [Header("Respawn")]
    [SerializeField] private Vector3 spawnPoint;

    [SerializeField] private UISkill uiSkill;

    public int IncenseSticks => incenseSticks;
    public int MaxHealth => maxHealth;

    private int attackMana;
    private int dashMana;
    private int pre_atkMana;
    private int pre_dashMana;

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

        spawnPoint = transform.position;
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

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        Debug.Log("Spawn point set: " + spawnPoint);
    }


    private void Die()
    {
        Debug.Log("Player died");

        currentHealth = maxHealth;
        currentShield = 0;
        transform.position = spawnPoint;

        if (TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = Vector3.zero;
    }

    // ---------------- Mana ----------------

    public void AddAttackMana(int amount)
    {
        int allowed = Mathf.Min(amount, maxTotalMana - (attackMana + dashMana));
        pre_atkMana = attackMana;
        attackMana += allowed;
        Debug.Log("Attack Mana: " + attackMana + " / Dash Mana: " + dashMana);

        uiSkill.UpdateSkill(dashMana, attackMana, dashMana, pre_atkMana);

        CheckAutoTrigger();
    }

    public void AddDashMana(int amount)
    {
        int allowed = Mathf.Min(amount, maxTotalMana - (attackMana + dashMana));
        pre_dashMana = dashMana;
        dashMana += allowed;
        Debug.Log("Attack Mana: " + attackMana + " / Dash Mana: " + dashMana);

        uiSkill.UpdateSkill(dashMana, attackMana, pre_dashMana, attackMana);

        CheckAutoTrigger();
    }

    private void CheckAutoTrigger()
    {
        if (attackMana >= autoTriggerThreshold)
        {
            pre_atkMana = attackMana;
            pre_dashMana = dashMana;
            attackMana = 0;
            dashMana = 0;
            OnAutoAttackSkill?.Invoke();
            uiSkill.UpdateSkill(dashMana, attackMana, pre_dashMana, pre_atkMana);
            return;
        }

        if (dashMana >= autoTriggerThreshold)
        {
            pre_atkMana = attackMana;
            pre_dashMana = dashMana;
            attackMana = 0;
            dashMana = 0;
            OnAutoDashSkill?.Invoke();
            uiSkill.UpdateSkill(dashMana, attackMana, pre_dashMana, pre_atkMana);
            return;
        }

        if (attackMana + dashMana >= maxTotalMana)
        {
            pre_atkMana = attackMana;
            pre_dashMana = dashMana;
            attackMana = 0;
            dashMana = 0;
            OnMaxManaSkill?.Invoke();
            uiSkill.UpdateSkill(dashMana, attackMana, pre_dashMana, pre_atkMana);
        }

    }

    public bool TryManualAttackSkill()
    {
        if (attackMana < manualTriggerThreshold) return false;

        pre_atkMana = attackMana;
        attackMana -= manualTriggerThreshold;
        uiSkill.UpdateSkill(dashMana, attackMana, dashMana, pre_atkMana);
        return true;
    }

    public bool TryManualDashSkill()
    {
        if (dashMana < manualTriggerThreshold) return false;

        pre_atkMana = attackMana;
        dashMana -= manualTriggerThreshold;
        uiSkill.UpdateSkill(dashMana, attackMana, pre_dashMana, attackMana);
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

    public void AddIncenseSticks(int amount)
    {
        incenseSticks += amount;
        Debug.Log("Incense Sticks: " + incenseSticks);
    }

    public bool TrySpendIncenseSticks(int amount)
    {
        if (incenseSticks < amount) return false;

        incenseSticks -= amount;
        Debug.Log("Incense Sticks: " + incenseSticks);
        return true;
    }
}