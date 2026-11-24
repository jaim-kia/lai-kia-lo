using UnityEngine;

public class FlyingChaseMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRadius = 5f;
    [SerializeField] private float loseRadius = 8f;
    [SerializeField] private float chaseSpeed = 3f;

    [Header("Return Settings")]
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float returnThreshold = 0.1f;

    [Header("Optional: hover while idle")]
    [SerializeField] private MonoBehaviour hoverComponent;

    private IEnemyMovement hover;
    private Transform player;
    private Vector3 startPos;
    private bool active = true;
    private bool isChasing;
    private bool isReturning;

    private void Awake()
    {
        hover = hoverComponent as IEnemyMovement;
    }

    private void Start()
    {
        player = PlayerController.Instance.transform;
        startPos = transform.position;
    }

    private void Update()
    {
        if (!active) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (!isChasing && distance <= chaseRadius)
        {
            isChasing = true;
            isReturning = false;
        }
        else if (isChasing && distance >= loseRadius)
        {
            isChasing = false;
            isReturning = true;
        }

        // enforced every frame: Enemy.cs turns every movement component back on after a hit,
        // which would otherwise leave hover running while we chase
        hover?.SetActive(!isChasing && !isReturning);

        if (isChasing)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
        }
        else if (isReturning)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPos) <= returnThreshold)
                isReturning = false; // hover takes over next frame, already at startPos
        }
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
        if (!isActive) hover?.SetActive(false);
        // chase/return state is kept, so after a stun it carries on instead of resetting
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}