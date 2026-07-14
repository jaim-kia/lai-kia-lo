using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 2;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask targetLayer;

    private Vector3 direction;

    public void Init(Vector3 travelDirection, int? damageOverride = null)
    {
        direction = travelDirection.normalized;
        if (damageOverride.HasValue)
            damage = damageOverride.Value;

        Debug.Log("Proj Damage: " + damage);
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        if ((wallLayer.value & (1 << otherLayer)) != 0)
        {
            OnHit();
            Debug.Log("HitWall");
            return;
        }

        if ((targetLayer.value & (1 << otherLayer)) != 0)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(damage);

            OnHit();
        }
    }

    private void OnHit()
    {
        // add anim heh
        Destroy(gameObject);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

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