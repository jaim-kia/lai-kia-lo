using UnityEngine;

public class ShrineController : MonoBehaviour
{
    [SerializeField] private int incenseCost = 3;
    [SerializeField] private string incenseTypeRequired;
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

    public void OnAmountChosen(string incenseType, int amount, bool cancel)
    {
        var dm = DialogueManager.GetInstance();
        if (cancel)
        {
            dm.SetStoryVariable("failType", "other");
            dm.ResumeDialogue();
            return;            
        }

        bool amountReq  = amount >= incenseCost;
        bool typeReq    = incenseTypeRequired == incenseType;
        bool success    = (amountReq && typeReq) ? PlayerStats.Instance.TrySpendIncenseSticks(amount, incenseType) : false;

        dm.SetStoryVariable("chosenAmount", amount);
        dm.SetStoryVariable("offeringSuccess", success);

        if (!success)
        {
            if (!amountReq && typeReq)          dm.SetStoryVariable("failType", "amount");
            else if (!typeReq && amountReq)     dm.SetStoryVariable("failType", "color");
            else if (!amountReq && !typeReq)    dm.SetStoryVariable("failType", "both");
        }


        dm.ResumeDialogue();
    }
}