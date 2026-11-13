using UnityEngine;
using UnityEngine.InputSystem;

public class TapePickup : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;
    [SerializeField] private string pickupId = "pickup_01"; // unique within this room
    [SerializeField] private bool requireInteract = true;    // turn off for coins you walk into

    [Header("Tape Info")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private string speakerLabel = "Cassette Tape";

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactRef;
    [SerializeField] private GameObject visualCue; // optional "press interact" prompt
    [SerializeField] private bool requireGrounded = true;

    private bool playerInRange;
    private bool collected;

    private string StateKey => $"pickup:{gameObject.scene.name}:{pickupId}";

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

        if (canInteract && (!requireInteract || (interactRef != null && interactRef.action.WasPressedThisFrame())))
            Collect();
    }

    private void Collect()
    {
        Debug.Log($"Collect: item={item?.name}, type={item?.GetType().Name}");

        if (!Inventory.Instance.Add(item, amount))
        {
            Debug.Log("Add returned false (already owned or stack full)");
            return;
        }

        collected = true;
        WorldState.Instance.Set(StateKey);

        Debug.Log($"Added. isTape={item is TapeData}, ink={(item as TapeData)?.inkJSON}");

        if (item is TapeData tape && tape.inkJSON != null)
            DialogueManager.GetInstance().EnterDialogueMode(tape.inkJSON, gameObject, tape.speakerLabel, freezeWorld: true);

        Destroy(gameObject);
    }
}