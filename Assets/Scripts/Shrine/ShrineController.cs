using UnityEngine;
using UnityEngine.SceneManagement;

public class ShrineController : MonoBehaviour
{

    [SerializeField] private string shrineId = "main"; // unique within this room
    [SerializeField] private int incenseCost = 3;
    [SerializeField] private string incenseTypeRequired;
    [SerializeField] private GameObject benchPrefab;
    [SerializeField] private Transform benchSpawnPoint;

    public bool IsUnlocked { get; private set; }

    private string StateKey => $"shrine:{gameObject.scene.name}:{shrineId}";

    private void Start()
    {
        if (WorldState.Instance.Has(StateKey))
        {
            IsUnlocked = true;
            SpawnBench();
        }
    }

    private void OnEnable()
    {
        DialogueManager.OnDialogueTag += HandleDialogueTag;
        DialogueManager.OnDialogueStarted += HandleDialogueStarted;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueTag -= HandleDialogueTag;
        DialogueManager.OnDialogueStarted -= HandleDialogueStarted;
    }

    private void HandleDialogueTag(GameObject speaker, string tag)
    {
        Debug.Log($"[Shrine] tag=[{tag}] speaker={speaker?.name} this={name} unlocked={IsUnlocked}");
        if (speaker != gameObject) return;

        if (tag == "request_amount")
        {
            DialogueManager.GetInstance().PauseDialogue();
            ShrineOfferingUI.Instance.Open(this);
        }
        else if (tag == "unlock_shrine" && !IsUnlocked)
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAA");
            IsUnlocked = true;
            WorldState.Instance.Set(StateKey); 
            SpawnBench();
        }
    }


    private void HandleDialogueStarted(GameObject speaker)
    {
        if (speaker != gameObject) return;

        DialogueManager.GetInstance().SetStoryVariable("alreadyUnlocked", IsUnlocked);
    }

    private void SpawnBench()
    {
        GameObject bench = Instantiate(benchPrefab, benchSpawnPoint.position, Quaternion.identity);

        // Spawned bench is child of the added scene on top of persistent
        SceneManager.MoveGameObjectToScene(bench, gameObject.scene);
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
        ItemData incense = Inventory.Instance.GetItem("incense_" + incenseType);
        bool success = amountReq && typeReq && incense != null && Inventory.Instance.TryRemove(incense, amount);

        Debug.Log($"[Shrine] offered {amount} '{incenseType}': " +
                $"amountReq={amountReq} (needs {incenseCost}), " +
                $"typeReq={typeReq} (needs '{incenseTypeRequired}'), " +
                $"assetFound={Inventory.Instance.GetItem("incense_" + incenseType) != null}, " +
                $"success={success}");
                
        dm.SetStoryVariable("chosenType", incenseType);
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