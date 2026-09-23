using UnityEngine;

public class ShrineController : MonoBehaviour
{
    [SerializeField] private int incenseCost = 3;
    [SerializeField] private GameObject benchPrefab;
    [SerializeField] private Transform benchSpawnPoint;

    public bool IsUnlocked { get; private set; }

    private void OnEnable()
    {
        DialogueManager.OnDialogueTag += HandleDialogueTag;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueTag -= HandleDialogueTag;
    }

    private void HandleDialogueTag(GameObject speaker, string tag)
    {
        // Debug.Log($"HandleDialogueTag called. speaker={speaker?.name}, this={gameObject.name}, tag=[{tag}]");
        if (speaker != gameObject) return;
        // Debug.Log("Speaker matched, checking tag...");
        if (tag == "request_amount")
        {
            Debug.Log("Tag matched request_amount - pausing dialogue");
            string incenseType = (string)DialogueManager.GetInstance().GetStoryVariable("chosenType");

            DialogueManager.GetInstance().PauseDialogue();
            ShrineOfferingUI.Instance.Open(this, incenseType);
        }
        else if (tag == "unlock_shrine" && !IsUnlocked)
        {
            IsUnlocked = true;
            Instantiate(benchPrefab, benchSpawnPoint.position, Quaternion.identity);
        }
    }

    public void OnAmountChosen(string incenseType, int amount)
    {
        bool success = amount >= incenseCost && PlayerStats.Instance.TrySpendIncenseSticks(amount);

        var dm = DialogueManager.GetInstance();
        dm.SetStoryVariable("chosenAmount", amount);
        dm.SetStoryVariable("offeringSuccess", success);

        dm.ResumeDialogue();
    }
}