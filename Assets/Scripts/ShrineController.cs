using UnityEngine;
using UnityEngine.InputSystem;

public class ShrineController : MonoBehaviour
{
    [SerializeField] private int incenseCost = 3;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private GameObject benchPrefab;
    [SerializeField] private Transform benchSpawnPoint;

    public bool IsUnlocked { get; private set; }

    private bool playerInRange;
    private Transform player;

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void Update()
    {
        playerInRange = Vector3.Distance(transform.position, player.position) <= interactRange;
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!playerInRange) return;
        if (IsUnlocked) return;
        if (GameManager.Instance.State != GameState.Overworld) return;

        ShrineOfferingUI.Instance.Open(this, incenseCost);
    }

    public void TryOffer(int amount)
    {
        if (amount < incenseCost)
        {
            Debug.Log("Not enough offered.");
            return;
        }

        if (PlayerStats.Instance.TrySpendIncenseSticks(amount))
        {
            IsUnlocked = true;
            Instantiate(benchPrefab, benchSpawnPoint.position, Quaternion.identity);
        }
    }
}