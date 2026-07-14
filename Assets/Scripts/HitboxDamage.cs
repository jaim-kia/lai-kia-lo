using UnityEngine;

public class HitboxDamage : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] int manaOnHit = 1;
    [SerializeField] LayerMask targetLayer;

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            PlayerStats.Instance.AddAttackMana(manaOnHit);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (TryGetComponent<BoxCollider>(out var box))
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (TryGetComponent<SphereCollider>(out var sphere))
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}
