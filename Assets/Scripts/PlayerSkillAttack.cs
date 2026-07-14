using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillAttack : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float spawnOffsetDistance = 1f;
    [SerializeField] private int manaCost = 30;

    public void SkillAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!PlayerStats.Instance.TrySpendMana(manaCost))
        {
            Debug.Log("Not enough mana");
            return;
        }

        var facing = PlayerController.Instance.Facing;
        Vector3 direction = facing == PlayerController.FacingDirection.Right
            ? Vector3.right
            : Vector3.left;

        SpawnProjectile(direction);
    }

    private void SpawnProjectile(Vector3 direction)
    {
        Vector3 spawnPos = firePoint.position + direction * spawnOffsetDistance;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.GetComponent<PlayerProjectile>().Init(direction);
    }
}