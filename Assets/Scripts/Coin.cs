using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Coin : MonoBehaviour
{
    [SerializeField] private ItemData coinItem;   // your "coins" asset
    [SerializeField] private int value = 1;
    [SerializeField] private float pickupRadius = 0.8f;
    [SerializeField] private float pickupDelay = 0.4f; // lets the pop play out instead of vanishing instantly

    private Rigidbody rb;
    private Transform player;
    private float spawnTime;
    private Collider playerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }

    private void Start()
    {
        player = PlayerController.Instance.transform;
        playerCollider = PlayerController.Instance.GetComponent<Collider>();

        if (coinItem == null)
            Debug.LogError("Coin has no coinItem assigned", this);
    }

    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        if (Time.time - spawnTime < pickupDelay) return;

        // not during dialogue, menus or room transitions (the player gets teleported then)
        if (GameManager.Instance.State != GameState.Overworld) return;

        Bounds b = playerCollider.bounds;
        Vector3 p = transform.position;
        p.z = b.center.z; 

        if (b.SqrDistance(p) <= pickupRadius * pickupRadius)
            Collect();
    }

    private void Collect()
    {
        // returns false if the stack is full, so the coin stays on the ground instead of being wasted
        if (!Inventory.Instance.Add(coinItem, value)) return;

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}