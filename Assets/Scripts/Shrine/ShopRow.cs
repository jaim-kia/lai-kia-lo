using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopRow : MonoBehaviour, ISelectHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text ownedText;

    public ShopEntry Entry { get; private set; }
    private ShopUI ui;

    public void Init(ShopEntry entry, ShopUI owner)
    {
        Entry = entry;
        ui = owner;

        GetComponent<Button>().onClick.AddListener(() => ui.OnRowSubmitted(this));

        icon.sprite = entry.item.icon;
        icon.enabled = entry.item.icon != null;
        nameText.text = entry.item.displayName;
    }

    public void Refresh(int coins)
    {
        ownedText.text = $"Owned: {Inventory.Instance.Count(Entry.item)}";
        priceText.text = $"{Entry.price} coins";
        priceText.color = coins >= Entry.price ? Color.white : new Color(1f, 0.4f, 0.4f); // red if you can't afford one
    }

    public void OnSelect(BaseEventData eventData) => ui.ShowDetails(Entry);
}