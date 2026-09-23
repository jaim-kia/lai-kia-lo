using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] Transform attackPoint;
    [SerializeField] GameObject hitboxPrefab;
    [SerializeField] float attackOffsetDistance = 1f;
    [SerializeField] float hitboxLifetime = 0.15f;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    private int comboIndex = 0;
    private int maxComboSteps = 3;

    private float lastClickTime;
    public float comboResetWindow = 1f;

    private bool isAttacking = false;
    private bool queuedNextAttack = false;
    private Vector3 currentAttackDirection;

    void Update()
    {
        if (Time.time - lastClickTime > comboResetWindow && comboIndex > 0)
        {
            ResetCombo();
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State != GameState.Overworld) return;
        if (!context.performed) return;

        if (isAttacking)
        {
            queuedNextAttack = true;
            return;
        }

        DoAttack();
    }

    private void DoAttack()
    {
        var facing = PlayerController.Instance.Facing;
        currentAttackDirection = facing == PlayerController.FacingDirection.Right
            ? Vector3.right
            : Vector3.left;

        lastClickTime = Time.time;
        comboIndex++;

        if (comboIndex > maxComboSteps)
            comboIndex = 1;

        anim.SetInteger("attackIndex", comboIndex);
        isAttacking = true;
        queuedNextAttack = false;
    }

    // Animation Event: place at the exact frame the blade crosses/swings through
    public void OnAttackHitFrame()
    {
        SpawnHitbox(currentAttackDirection);
    }

    // Animation Event: place at the very last frame of the clip
    public void EndOfAttackState()
    {
        isAttacking = false;

        if (queuedNextAttack)
        {
            DoAttack();
        }
        else if (Time.time - lastClickTime > 0.2f)
        {
            ResetCombo();
        }
    }

    public void ResetCombo()
    {
        comboIndex = 0;
        anim.SetInteger("attackIndex", 0);
        isAttacking = false;
        queuedNextAttack = false;
    }

    private void SpawnHitbox(Vector3 direction)
    {
        Vector3 spawnPos = attackPoint.position + direction * attackOffsetDistance;
        GameObject hitbox = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity, attackPoint);
        Destroy(hitbox, hitboxLifetime);
    }
}