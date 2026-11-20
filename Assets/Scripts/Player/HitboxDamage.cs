using UnityEngine;

public class HitboxDamage : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] int manaOnHit = 1;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask wallLayer;

    [Header("Recoil")]
    [SerializeField] private float recoilForce = 6f;
    [SerializeField] private float recoilDuration = 0.1f;

    private bool hasRecoiled = false;

    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        bool isTarget = (targetLayer.value & (1 << otherLayer)) != 0;
        bool isWall = (wallLayer.value & (1 << otherLayer)) != 0;

        if (!isTarget && !isWall) return;

        if (isTarget && other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            PlayerStats.Instance.AddAttackMana(manaOnHit);
        }

        if (!hasRecoiled)
        {
            hasRecoiled = true;
            ApplyRecoilToPlayer();
        }
    }

    private void ApplyRecoilToPlayer()
    {
        var facing = PlayerController.Instance.Facing;
        Vector3 recoilDirection = facing == PlayerController.FacingDirection.Right
            ? Vector3.left
            : Vector3.right;

        PlayerController.Instance.ApplyRecoil(recoilDirection, recoilForce, recoilDuration);
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
