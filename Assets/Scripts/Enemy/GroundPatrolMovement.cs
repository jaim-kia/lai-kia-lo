using UnityEngine;

public class GroundPatrolMovement : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float patrolDistance = 3f;

    private Vector3 startPos;
    private int direction = 1;
    private bool active = true;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!active) return;

        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x - startPos.x) >= patrolDistance)
            direction *= -1;
    }

    public void SetActive(bool isActive) => active = isActive;
}