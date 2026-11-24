using UnityEngine;

public class FlyingHoverMovement : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float hoverSpeed = 1.5f;
    [SerializeField] private float hoverHeight = 0.5f;

    private float t;
    private float lastOffset;
    private bool active = true;

    private void Update()
    {
        if (!active) return;

        t += Time.deltaTime;
        float offset = Mathf.Sin(t * hoverSpeed) * hoverHeight;

        // apply only the change since last frame, so nothing else's movement gets overwritten
        transform.position += Vector3.up * (offset - lastOffset);
        lastOffset = offset;
    }

    public void SetActive(bool isActive)
    {
        if (active == isActive) return; // safe to call every frame
        active = isActive;

        // hover around wherever the enemy is now, not where it started
        t = 0f;
        lastOffset = 0f;
    }
}