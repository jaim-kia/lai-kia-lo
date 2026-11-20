using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopEntry
{
    public ItemData item;
    public int price = 10;
}

public class ShopController : MonoBehaviour
{
    [SerializeField] private ItemData currency;     // your "coins" asset
    [SerializeField] private List<ShopEntry> stock; // per shopkeeper, so each can charge differently

    public ItemData Currency => currency;
    public IReadOnlyList<ShopEntry> Stock => stock;

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
        Debug.Log($"[Shop] tag=[{tag}] speaker={speaker?.name} this={name}");
        if (speaker != gameObject) return;

        if (tag == "open_shop")
        {
            Debug.Log($"[Shop] opening. ShopUI missing: {ShopUI.Instance == null}, stock entries: {stock.Count}");
            DialogueManager.GetInstance().PauseDialogue();
            ShopUI.Instance.Open(this);
        }
    }

    // called by the shop UI when it closes
    public void OnShopClosed(bool purchased)
    {
        var dm = DialogueManager.GetInstance();
        dm.SetStoryVariable("purchased", purchased);
        dm.ResumeDialogue();
    }
}