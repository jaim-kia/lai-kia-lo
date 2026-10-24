using UnityEngine;
using UnityEngine.InputSystem;

public class RoomExit : MonoBehaviour
{
    public enum ExitType { Edge, Door }

    [SerializeField] private ExitType exitType = ExitType.Edge;
    [SerializeField] private string targetScene;
    [SerializeField] private string targetEntranceId;

    [Header("Edge only")]
    [SerializeField] private bool preserveVerticalOffset = true;

    [Header("Door only")]
    [SerializeField] private GameObject visualCue;
    [SerializeField] private InputActionReference interactRef;

    private bool playerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (exitType == ExitType.Edge)
            TryTransition();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        if (exitType != ExitType.Door) return;

        bool canUse = playerInRange && GameManager.Instance.State == GameState.Overworld;
        if (visualCue != null) visualCue.SetActive(canUse);

        if (canUse && interactRef.action.WasPressedThisFrame())
            TryTransition();
    }

    private void TryTransition()
    {
        if (GameManager.Instance.State != GameState.Overworld) return;

        float offsetY = 0f;
        if (exitType == ExitType.Edge && preserveVerticalOffset)
            offsetY = PlayerController.Instance.transform.position.y - transform.position.y;

        GameManager.Instance.TransitionToEntrance(targetScene, targetEntranceId, offsetY);
    }
}