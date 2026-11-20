using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    private enum Step { None, List, Quantity }

    [Header("Input")]
    [SerializeField] private InputActionReference cancelRef; // the UI/Cancel action

    [Header("Groups")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private CanvasGroup listGroup;
    [SerializeField] private CanvasGroup quantityGroup;

    [Header("List step")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private ShopRow rowPrefab;

    [Header("Quantity step")]
    [SerializeField] private TMP_Text quantityItemText;
    [SerializeField] private Slider amountSlider;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Shared")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text descriptionText;

    private readonly List<ShopRow> rows = new List<ShopRow>();
    private ShopController current;
    private ShopEntry selected;
    private Step step = Step.None;
    private int lastRowIndex;
    private bool boughtAnything;

    private int Coins => Inventory.Instance.Count(current.Currency);

    private void Awake()
    {
        Instance = this;

        // wired in code so there's nothing to forget in the Inspector
        amountSlider.wholeNumbers = true;
        amountSlider.onValueChanged.AddListener(_ => UpdateTotal());
        confirmButton.onClick.AddListener(OnConfirmPressed);
        backButton.onClick.AddListener(() => ShowList());

        SetGroup(rootGroup, false);
        SetGroup(listGroup, false);
        SetGroup(quantityGroup, false);
    }

    private void OnEnable()
    {
        cancelRef?.action.Enable(); // no Disable(): shared action asset
    }

    private void Update()
    {
        if (step == Step.None || cancelRef == null || !cancelRef.action.WasPressedThisFrame()) return;

        if (step == Step.Quantity) ShowList();
        else Close();
    }

    // ---------------- Open / close ----------------

    public void Open(ShopController shop)
    {
        current = shop;
        boughtAnything = false;
        lastRowIndex = 0;

        EventSystem.current.SetSelectedGameObject(null); // never destroy the selected object
        foreach (var r in rows) Destroy(r.gameObject);
        rows.Clear();

        foreach (var entry in shop.Stock)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            row.Init(entry, this);
            rows.Add(row);
        }

        if (rows.Count == 0)
        {
            Close();
            return;
        }

        SetGroup(rootGroup, true);
        ShowList();
    }

    private void Close()
    {
        StopAllCoroutines();
        EventSystem.current.SetSelectedGameObject(null);

        step = Step.None;
        SetGroup(listGroup, false);
        SetGroup(quantityGroup, false);
        SetGroup(rootGroup, false);

        var shop = current;
        current = null;
        shop?.OnShopClosed(boughtAnything);
    }

    // ---------------- Step 1: choose the item ----------------

    private void ShowList(string message = "")
    {
        step = Step.List;
        SetGroup(quantityGroup, false);
        SetGroup(listGroup, true);

        RefreshCoins();
        statusText.text = message;

        StopAllCoroutines();
        StartCoroutine(SelectNextFrame(rows[Mathf.Clamp(lastRowIndex, 0, rows.Count - 1)].gameObject));
    }

    public void ShowDetails(ShopEntry entry)
    {
        descriptionText.text = entry.item.description;
    }

    public void OnRowSubmitted(ShopRow row)
    {
        ShopEntry entry = row.Entry;
        lastRowIndex = rows.IndexOf(row);

        int space = entry.item.MaxStack - Inventory.Instance.Count(entry.item);
        int byMoney = entry.price <= 0 ? space : Coins / entry.price;
        int max = Mathf.Min(byMoney, space);

        if (max < 1)
        {
            statusText.text = space < 1 ? "You can't carry any more" : "Not enough coins";
            return;
        }

        selected = entry; // set before touching the slider, since its callback reads it

        amountSlider.minValue = 1;
        amountSlider.maxValue = max;
        amountSlider.SetValueWithoutNotify(1);
        UpdateTotal();

        step = Step.Quantity;
        SetGroup(listGroup, false);
        SetGroup(quantityGroup, true);
        statusText.text = "";

        StopAllCoroutines();
        StartCoroutine(SelectNextFrame(amountSlider.gameObject));
    }

    // ---------------- Step 2: choose the amount ----------------

    private void UpdateTotal()
    {
        if (selected == null) return;

        int qty = (int)amountSlider.value;
        int total = qty * selected.price;

        quantityItemText.text = $"{selected.item.displayName}  x{qty}";
        totalText.text = $"Total: {total} coins   (you have {Coins})";
    }

    private void OnConfirmPressed()
    {
        int qty = (int)amountSlider.value;
        int total = qty * selected.price;

        // re-check, in case something changed since the slider range was set
        if (!Inventory.Instance.Has(current.Currency, total))
        {
            ShowList("Not enough coins");
            return;
        }
        if (Inventory.Instance.Count(selected.item) + qty > selected.item.MaxStack)
        {
            ShowList("You can't carry that many");
            return;
        }

        Inventory.Instance.TryRemove(current.Currency, total);
        Inventory.Instance.Add(selected.item, qty);
        boughtAnything = true;

        ShowList($"Bought {qty} {selected.item.displayName}");
    }

    // ---------------- Helpers ----------------

    private void RefreshCoins()
    {
        coinsText.text = $"Coins: {Coins}";
        foreach (var row in rows) row.Refresh(Coins);
    }

    private IEnumerator SelectNextFrame(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(target);
    }

    private static void SetGroup(CanvasGroup group, bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}