using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventorySlot : MonoBehaviour, ISelectHandler, ICancelHandler
{
    [SerializeField] private Image icon;            // child Image, not the button's own frame
    [SerializeField] private TMP_Text countText;    // small corner number for stacks
    [SerializeField] private GameObject equippedMarker;

    public ItemData Item { get; private set; }

    private Button button;
    private InventoryUI ui;

    // called once when the grid is built
    public void Init(InventoryUI owner)
    {
        ui = owner;
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (Item != null) ui.OnSlotSubmitted(this);
        });
        SetItem(null);
    }

    // data == null empties the cell
    public void SetItem(ItemData data)
    {
        Item = data;

        // empty cells aren't interactable, so stick/arrow navigation skips them
        button.interactable = data != null;

        icon.sprite = data != null ? data.icon : null;
        icon.enabled = data != null && data.icon != null;

        Refresh();
    }

    public void Refresh()
    {
        if (Item == null)
        {
            countText.text = "";
            equippedMarker.SetActive(false);
            return;
        }

        int count = Inventory.Instance.Count(Item);
        countText.text = count > 1 ? $"x{count}" : "";
        equippedMarker.SetActive(Item is CharmData charm && Inventory.Instance.IsEquipped(charm));
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (Item != null) ui.ShowDetails(Item);
    }

    public void OnCancel(BaseEventData eventData) => ui.Close();
}