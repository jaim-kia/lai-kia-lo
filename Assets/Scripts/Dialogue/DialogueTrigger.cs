using UnityEngine;
using UnityEngine.InputSystem; 

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;
    
    [Header("Ink Files")]
    [SerializeField] private TextAsset inkJSON;

    [SerializeField] private InputActionReference InteractRef;

    private bool playerInRange;

    private void Awake()
    {
        visualCue.SetActive(false);
        playerInRange = false;
    }

    private void OnEnable()
    {
        if (InteractRef != null)
        {
            InteractRef.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (InteractRef != null)
        {
            InteractRef.action.Disable();
        }
    }

    private void Update()
    {
        if (playerInRange
            && !DialogueManager.GetInstance().dialogueIsPlaying
            && GameManager.Instance.State == GameState.Overworld)
        {
            visualCue.SetActive(true);
            
            if (Interact())
            {
                DialogueManager.GetInstance().EnterDialogueMode(inkJSON, gameObject);
            } 
        }
        else
        {
            visualCue.SetActive(false); 
        }
    }

    // 3. Implement the Interact checking function
    private bool Interact()
    {
        if (InteractRef == null) return false;


        return InteractRef.action.WasPressedThisFrame();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tip: Standard Unity convention utilizes capitalized "Player" tag
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}