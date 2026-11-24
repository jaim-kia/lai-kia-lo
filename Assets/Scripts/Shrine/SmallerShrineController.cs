using UnityEngine;
using UnityEngine.SceneManagement;

public class SmallerShrineController : MonoBehaviour, IOfferingReceiver
{
    [SerializeField] private string shrineId = "small_01"; // unique within this room

    [Header("Required offering")]
    [SerializeField] private string incenseTypeRequired = "red";
    [SerializeField] private int incenseCost = 2;
    [SerializeField] private bool exactAmountRequired = true; // off = "at least this many"
    [SerializeField] private bool consumeOnFail = true;       // a wrong offering is still spent

    [Header("Reward")]
    [SerializeField] private ItemData rewardItem;
    [SerializeField] private int rewardAmount = 1;

    [Header("Punishment")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 1;
    [SerializeField] private Transform[] spawnPoints;          // empty children placed around the shrine
    [SerializeField] private float fallbackSpawnRadius = 3f;   // used only if no spawn points are set

    private bool claimed;
    private bool pendingSpawn;

    private string StateKey => $"smallshrine:{gameObject.scene.name}:{shrineId}";

    private void Start()
    {
        claimed = WorldState.Instance.Has(StateKey);
    }

    private void OnEnable()
    {
        DialogueManager.OnDialogueTag += HandleDialogueTag;
        DialogueManager.OnDialogueStarted += HandleDialogueStarted;
        DialogueManager.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueTag -= HandleDialogueTag;
        DialogueManager.OnDialogueStarted -= HandleDialogueStarted;
        DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueStarted(GameObject speaker)
    {
        if (speaker != gameObject) return;
        DialogueManager.GetInstance().SetStoryVariable("alreadyClaimed", claimed);
    }

    private void HandleDialogueTag(GameObject speaker, string tag)
    {
        if (speaker != gameObject) return;

        switch (tag)
        {
            case "request_amount":
                DialogueManager.GetInstance().PauseDialogue();
                ShrineOfferingUI.Instance.Open(this);
                break;

            case "give_reward":
                GiveReward();
                break;

            case "spawn_enemy":
                pendingSpawn = true; // spawns when the dialogue closes, not mid-sentence
                break;
        }
    }

    private void HandleDialogueEnded(GameObject speaker)
    {
        if (speaker != gameObject || !pendingSpawn) return;

        pendingSpawn = false;
        SpawnEnemies();
    }

    // called by ShrineOfferingUI when the player confirms or cancels
    public void OnAmountChosen(string incenseType, int amount, bool cancel)
    {
        var dm = DialogueManager.GetInstance();

        if (cancel)
        {
            dm.SetStoryVariable("offeringSuccess", false);
            dm.SetStoryVariable("failType", "other"); // no tag on this branch, so nothing spawns
            dm.ResumeDialogue();
            return;
        }

        bool amountOk = exactAmountRequired ? amount == incenseCost : amount >= incenseCost;
        bool typeOk = incenseType == incenseTypeRequired;
        bool correct = amountOk && typeOk;

        ItemData incense = Inventory.Instance.GetItem("incense_" + incenseType);
        if (incense == null)
            Debug.LogError($"No item 'incense_{incenseType}' in Inventory.allItems", this);

        if (correct)
            correct = incense != null && Inventory.Instance.TryRemove(incense, amount);
        else if (consumeOnFail && incense != null)
            Inventory.Instance.TryRemove(incense, amount);

        dm.SetStoryVariable("chosenType", incenseType);
        dm.SetStoryVariable("chosenAmount", amount);
        dm.SetStoryVariable("offeringSuccess", correct);

        if (!correct)
        {
            string fail = (!amountOk && !typeOk) ? "both" : (!typeOk ? "color" : "amount");
            dm.SetStoryVariable("failType", fail);
        }

        dm.ResumeDialogue();
    }

    private void GiveReward()
    {
        if (claimed) return;

        claimed = true;
        WorldState.Instance.Set(StateKey); // stays claimed after the room unloads

        if (!Inventory.Instance.Add(rewardItem, rewardAmount))
            Debug.LogWarning("Reward couldn't be added (item unassigned, already owned, or stack full)", this);
    }

    private void SpawnEnemies()
    {
        float z = PlayerController.Instance.transform.position.z; // stay on the gameplay plane

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 pos = spawnPoints != null && spawnPoints.Length > 0
                ? spawnPoints[Random.Range(0, spawnPoints.Length)].position
                : transform.position + new Vector3(Random.Range(-fallbackSpawnRadius, fallbackSpawnRadius), 1f, 0f);
            pos.z = z;

            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(enemy, gameObject.scene); // unloads with this room
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (var p in spawnPoints)
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.4f);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, fallbackSpawnRadius);
        }
    }
}