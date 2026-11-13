using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventorySlot : MonoBehaviour, ISelectHandler, ICancelHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject equippedMarker;

    public ItemData Item { get; private set; }
    private InventoryUI ui;

    public void Setup(ItemData data, InventoryUI owner)
    {
        Item = data;
        ui = owner;

        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = data.icon != null;
        }
        if (label != null) label.text = data.displayName;

        GetComponent<Button>().onClick.AddListener(() => ui.OnSlotSubmitted(this));
        Refresh();
    }

    public void Refresh()
    {
        int count = Inventory.Instance.Count(Item);
        if (countText != null) countText.text = count > 1 ? $"x{count}" : "";

        if (equippedMarker != null)
            equippedMarker.SetActive(Item is CharmData charm && Inventory.Instance.IsEquipped(charm));
    }

    public void OnSelect(BaseEventData eventData) => ui.ShowDetails(Item);
    public void OnCancel(BaseEventData eventData) => ui.Close();
}