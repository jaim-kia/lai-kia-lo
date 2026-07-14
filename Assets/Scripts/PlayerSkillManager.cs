using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Attack Skill (Projectile)")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float spawnOffsetDistance = 1f;

    [Header("Auto Skill Attack Overrides")]
    [SerializeField] private int autoAttackDamage = 3;
    [SerializeField] private int autoAttackHealthCost = 1;

    [Header("Dash Skill (Shield)")]
    [SerializeField] private int shieldAmount = 1;

    [Header("Auto Shield Overrides")]
    [SerializeField] private int autoShieldAmount = 2;
    [SerializeField] private float autoShieldSlowMultiplier = 0.5f;
    [SerializeField] private float autoShieldSlowDuration = 3f;

    private void Start()
    {
        PlayerStats.Instance.OnAutoAttackSkill += AutoFireProjectile;
        PlayerStats.Instance.OnAutoDashSkill += AutoGrantShield;
    }

    private void OnDisable()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnAutoAttackSkill -= AutoFireProjectile;
            PlayerStats.Instance.OnAutoDashSkill -= AutoGrantShield;
        }
    }

    public void UseSkill(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (PlayerStats.Instance.TryManualAttackSkill())
        {
            ManualFireProjectile();
            return;
        }

        if (PlayerStats.Instance.TryManualDashSkill())
        {
            ManualGrantShield();
            return;
        }

        Debug.Log("No skill mana ready");
    }

    // ---------------- Manual ----------------

    private void ManualFireProjectile()
    {
        SpawnProjectile(GetFacingDirection(), null); // use projectile's default damage
    }

    private void ManualGrantShield()
    {
        PlayerStats.Instance.AddShield(shieldAmount);
    }

    // ---------------- Auto ----------------

    private void AutoFireProjectile()
    {
        SpawnProjectile(GetFacingDirection(), autoAttackDamage);
        PlayerStats.Instance.PayHealthCost(autoAttackHealthCost);
    }

    private void AutoGrantShield()
    {
        PlayerStats.Instance.AddShield(autoShieldAmount);
        PlayerController.Instance.ApplySlow(autoShieldSlowMultiplier, autoShieldSlowDuration);
    }

    // ---------------- Shared helper ----------------

    private Vector3 GetFacingDirection()
    {
        var facing = PlayerController.Instance.Facing;
        return facing == PlayerController.FacingDirection.Right ? Vector3.right : Vector3.left;
    }

    private void SpawnProjectile(Vector3 direction, int? damageOverride)
    {
        Vector3 spawnPos = firePoint.position + direction * spawnOffsetDistance;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.GetComponent<PlayerProjectile>().Init(direction, damageOverride);
    }
}