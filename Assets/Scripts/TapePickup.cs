using UnityEngine;
using UnityEngine.InputSystem;

public class TapePickup : MonoBehaviour
{
    [SerializeField] private string tapeId = "tape_01"; // unique across the whole game
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private string speakerLabel = "Cassette Tape";

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactRef;
    [SerializeField] private GameObject visualCue; // optional "press interact" prompt
    [SerializeField] private bool requireGrounded = true;

    private bool playerInRange;
    private bool collected;

    private string StateKey => $"tape:{tapeId}";

    private void Start()
    {
        if (WorldState.Instance.Has(StateKey))
        {
            Destroy(gameObject);
            return;
        }

        if (visualCue != null) visualCue.SetActive(false);
    }

    private void OnEnable()
    {
        if (interactRef != null) interactRef.action.Enable();
        // deliberately no Disable() in OnDisable: the Interact action is shared
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        if (collected) return;

        bool canInteract = playerInRange
            && GameManager.Instance.State == GameState.Overworld
            && (!requireGrounded || PlayerController.Instance.IsGrounded);

        if (visualCue != null) visualCue.SetActive(canInteract);

        if (canInteract && interactRef != null && interactRef.action.WasPressedThisFrame())
            Collect();
    }

    private void Collect()
    {
        collected = true;
        WorldState.Instance.Set(StateKey);

        DialogueManager.GetInstance().EnterDialogueMode(inkJSON, gameObject, speakerLabel, freezeWorld: true);
        Destroy(gameObject);
    }
}