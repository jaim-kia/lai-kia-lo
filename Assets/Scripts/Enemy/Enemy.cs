using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] private int contactDamage = 1;
    private int currentHealth;

    [Header("Knockback (dealt to player on contact)")]
    [SerializeField] private float knockbackHorizontalForce = 5f;
    [SerializeField] private float knockbackVerticalForce = 4f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Hit Reaction (when THIS enemy takes damage)")]
    [SerializeField] private float hitKnockbackHorizontalForce = 4f;
    [SerializeField] private float hitKnockbackVerticalForce = 2f;
    [SerializeField] private float hitKnockbackDuration = 0.15f;
    [SerializeField] private float movementFreezeDuration = 0.2f; // total time movement stays disabled; can exceed knockback duration for extra stun

    private IEnemyMovement[] movementBehaviors;
    private Coroutine hitstunCoroutine;
    private Rigidbody rb;

    private void Awake()
    {
        currentHealth = maxHealth;
        movementBehaviors = GetComponents<IEnemyMovement>();
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        ApplyHitReaction();
    }

    private void ApplyHitReaction()
    {
        if (hitstunCoroutine != null)
            StopCoroutine(hitstunCoroutine);

        hitstunCoroutine = StartCoroutine(HitstunRoutine());
    }

    private IEnumerator HitstunRoutine()
    {
        foreach (var movement in movementBehaviors)
            movement.SetActive(false);

        Vector3 knockDir = transform.position - PlayerController.Instance.transform.position;
        knockDir.y = 0f;
        knockDir = knockDir.sqrMagnitude > 0.0001f ? knockDir.normalized : Vector3.right;

        rb.linearVelocity = new Vector3(
            knockDir.x * hitKnockbackHorizontalForce,
            hitKnockbackVerticalForce,
            0f);

        yield return new WaitForSeconds(hitKnockbackDuration);

        rb.linearVelocity = new Vector3(0f, rb.useGravity ? rb.linearVelocity.y : 0f, 0f);

        float remaining = movementFreezeDuration - hitKnockbackDuration;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        foreach (var movement in movementBehaviors)
            movement.SetActive(true);

        hitstunCoroutine = null;
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