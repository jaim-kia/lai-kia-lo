using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText; // swap for TMP_Text if using TextMeshPro

    private Shrine currentShrine;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void ShowShrinePrompt(Shrine shrine)
    {
        currentShrine = shrine;
        dialoguePanel.SetActive(true);
        dialogueText.text = "Give 3 incense sticks to the shrine? (Confirm / Cancel)";
    }

    public void ShowMessage(string message)
    {
        dialogueText.text = message;
    }

    public void Hide()
    {
        dialoguePanel.SetActive(false);
        currentShrine = null;
    }

    public void Confirm(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (currentShrine == null) return;

        currentShrine.OnPlayerConfirm();
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (currentShrine == null) return;

        currentShrine.EndDialogue();
    }
}