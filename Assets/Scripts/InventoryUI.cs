using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
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
    [SerializeField] private TMP_Text charmSlotsText; // "Slots 1/3", shown on the Charms tab
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

            // plain objects are definitions only: selecting shows the description, nothing to do on submit
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
        EventSystem.current.SetSelectedGameObject(null); // never destroy the selected object

        foreach (var s in slots) Destroy(s.gameObject);
        slots.Clear();

        foreach (var item in Inventory.Instance.GetItems(tabs[currentTab]))
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(item, this);
            slots.Add(slot);
        }

        tabLabel.text = $"<  {tabNames[currentTab]}  >";
        emptyLabel.SetActive(slots.Count == 0);
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

        if (slots.Count > 0)
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