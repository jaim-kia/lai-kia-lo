using UnityEngine;

public class FlyingHoverMovement : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float hoverSpeed = 1.5f;
    [SerializeField] private float hoverHeight = 0.5f;

    private Vector3 startPos;
    private bool active = true;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!active) return;

        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }

    public void SetActive(bool isActive) => active = isActive;
}