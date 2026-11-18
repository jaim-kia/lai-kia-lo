using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private const int Capacity = 16;

    [Header("Input")]
    [SerializeField] private InputActionReference toggleRef;
    [SerializeField] private InputActionReference tabLeftRef;
    [SerializeField] private InputActionReference tabRightRef;

    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlot slotPrefab;
    [SerializeField] private GameObject emptyLabel;
    [SerializeField] private TMP_Text tabLabel;
    [SerializeField] private TMP_Text charmSlotsText;
    [SerializeField] private TMP_Text statusText;

    [Header("Details")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image detailIcon;

    private static readonly ItemCategory[] tabs = { ItemCategory.Object, ItemCategory.Tape, ItemCategory.Charm };
    private static readonly string[] tabNames = { "Objects", "Tapes", "Charms" };

    private readonly List<InventorySlot> slots = new List<InventorySlot>();
    private int currentTab;
    private bool isOpen;

    private void Awake()
    {
        // built once and reused: 16 cells, filled or emptied per tab
        for (int i = 0; i < Capacity; i++)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Init(this);
            slots.Add(slot);
        }

        SetVisible(false);
    }

    private void OnEnable()
    {
        // no action.Disable() anywhere: these actions live in a shared asset
        toggleRef?.action.Enable();
        tabLeftRef?.action.Enable();
        tabRightRef?.action.Enable();
    }

    private void OnDisable()
    {
        if (isOpen) // safety net so a disabled menu can't leave the game frozen
        {
            isOpen = false;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (toggleRef != null && toggleRef.action.WasPressedThisFrame())
        {
            if (isOpen) Close();
            else if (GameManager.Instance.State == GameState.Overworld) Open();
            return;
        }

        if (!isOpen) return;

        if (tabRightRef != null && tabRightRef.action.WasPressedThisFrame()) ChangeTab(1);
        else if (tabLeftRef != null && tabLeftRef.action.WasPressedThisFrame()) ChangeTab(-1);
    }

    public void Open()
    {
        isOpen = true;
        Time.timeScale = 0f;
        GameManager.Instance.UpdateGameState(GameState.Menu);

        Rebuild();
        SetVisible(true);
        StartCoroutine(SelectFirst());
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        EventSystem.current.SetSelectedGameObject(null);
        SetVisible(false);

        Time.timeScale = 1f;
        GameManager.Instance.UpdateGameState(GameState.Overworld);
    }

    private void ChangeTab(int delta)
    {
        currentTab = (currentTab + delta + tabs.Length) % tabs.Length;
        Rebuild();
        StartCoroutine(SelectFirst());
    }

    public void OnSlotSubmitted(InventorySlot slot)
    {
        switch (slot.Item)
        {
            case TapeData tape:
                if (tape.inkJSON == null) return;
                Close();
                DialogueManager.GetInstance().EnterDialogueMode(tape.inkJSON, null, tape.speakerLabel, freezeWorld: true);
                break;

            case CharmData charm:
                SetStatus(Inventory.Instance.TryToggleEquip(charm) ? "" : "No free charm slots");
                slot.Refresh();
                RefreshCharmSlots();
                break;

            // plain objects are definitions only
        }
    }

    public void ShowDetails(ItemData item)
    {
        titleText.text = item.displayName;
        descriptionText.text = item.description;
        detailIcon.sprite = item.icon;
        detailIcon.enabled = item.icon != null;
        SetStatus("");
    }

    private void Rebuild()
    {
        var items = Inventory.Instance.GetItems(tabs[currentTab]);

        if (items.Count > Capacity)
            Debug.LogWarning($"{tabNames[currentTab]} has {items.Count} items but the grid only shows {Capacity}");

        for (int i = 0; i < slots.Count; i++)
            slots[i].SetItem(i < items.Count ? items[i] : null);

        tabLabel.text = $"<  {tabNames[currentTab]}  >";
        if (emptyLabel != null) emptyLabel.SetActive(items.Count == 0);
        SetStatus("");
        RefreshCharmSlots();
    }

    private void RefreshCharmSlots()
    {
        if (charmSlotsText == null) return;

        bool onCharms = tabs[currentTab] == ItemCategory.Charm;
        charmSlotsText.gameObject.SetActive(onCharms);
        if (onCharms)
            charmSlotsText.text = $"Slots {Inventory.Instance.EquippedCount}/{Inventory.Instance.CharmSlots}";
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private IEnumerator SelectFirst()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame(); // frame-based, so it still runs at timeScale 0

        if (slots[0].Item != null)
        {
            EventSystem.current.SetSelectedGameObject(slots[0].gameObject);
        }
        else
        {
            titleText.text = "";
            descriptionText.text = "";
            detailIcon.enabled = false;
        }
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}