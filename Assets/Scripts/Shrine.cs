using UnityEngine;
using UnityEngine.InputSystem;

public class Shrine : MonoBehaviour
{
    [SerializeField] private int incenseCost = 3;
    [SerializeField] private float interactRange = 2f;

    public bool IsUnlocked { get; private set; } = false;

    private bool playerInRange;
    private Transform player;

    [SerializeField] private GameObject benchPrefab;
    [SerializeField] private Transform benchSpawnPoint;

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactRange;
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!playerInRange) return;
        if (IsUnlocked) return;

        if (GameManager.Instance.State == GameState.Overworld)
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        GameManager.Instance.UpdateGameState(GameState.Dialogue);
        DialogueUI.Instance.ShowShrinePrompt(this);
    }

    public void OnPlayerConfirm()
    {
        if (PlayerStats.Instance.TrySpendIncenseSticks(incenseCost))
        {
            IsUnlocked = true;
            DialogueUI.Instance.ShowMessage("The shrine accepts your offering...");

            Instantiate(benchPrefab, benchSpawnPoint.position, Quaternion.identity);

            EndDialogue();
        }
        else
        {
            DialogueUI.Instance.ShowMessage("You don't have enough incense sticks.");
        }
    }

    public void EndDialogue()
    {
        GameManager.Instance.UpdateGameState(GameState.Overworld);
        DialogueUI.Instance.Hide();
    }
}