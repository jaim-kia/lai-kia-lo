using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public string entranceId = "left";
    [Tooltip("How far an edge transition may shift the player's height. 0 for doors.")]
    public float verticalRange = 1.5f;

    public Vector3 GetSpawn(float offsetY)
    {
        return transform.position + Vector3.up * Mathf.Clamp(offsetY, -verticalRange, verticalRange);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position + Vector3.down * verticalRange,
                        transform.position + Vector3.up * verticalRange);
    }
}