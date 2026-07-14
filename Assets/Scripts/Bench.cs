using UnityEngine;
using UnityEngine.InputSystem;

public class Bench : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private InputActionReference interactAction; // assign the same Interact action asset here

    private bool playerInRange;
    private Transform player;

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void OnEnable()
    {
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactRange;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInRange) return;
        if (GameManager.Instance.State != GameState.Overworld) return;

        PlayerStats.Instance.SetSpawnPoint(transform.position);
        PlayerStats.Instance.Heal(PlayerStats.Instance.MaxHealth);

        Debug.Log("Rested at bench. Spawn point updated, health restored.");
    }
}