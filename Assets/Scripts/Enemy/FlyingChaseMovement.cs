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

    [Header("Optional: hand control back once returned home")]
    [SerializeField] private MonoBehaviour hoverComponent;

    private IEnemyMovement hover;
    private Transform player;
    private Vector3 startPos;
    private bool active = true;
    private bool isChasing;
    private bool isReturning;

    private void Start()
    {
        player = PlayerController.Instance.transform;
        hover = hoverComponent as IEnemyMovement;
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
            hover?.SetActive(false);
        }
        else if (isChasing && distance >= loseRadius)
        {
            isChasing = false;
            isReturning = true;
        }

        if (isChasing)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
        }
        else if (isReturning)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPos) <= returnThreshold)
            {
                isReturning = false;
                hover?.SetActive(true); 
            }
        }
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
        if (!isActive)
        {
            isChasing = false;
            isReturning = false;
            hover?.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}