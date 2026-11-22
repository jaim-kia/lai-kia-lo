using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShrineOfferingUI : MonoBehaviour
{
    public static ShrineOfferingUI Instance;

    [Serializable]
    public class IncenseOption
    {
        public string type;       // "red" / "yellow": must match the asset name incense_<type>
        public Button button;
        public TMP_Text ownedText;
    }

    private enum Step { None, Type, Amount }

    [Header("Input")]
    [SerializeField] private InputActionReference cancelRef; // UI/Cancel

    [Header("Panel")]
    [SerializeField] private GameObject offeringPanel;
    [SerializeField] private CanvasGroup offeringCanvasGroup;
    [SerializeField] private GameObject WhiteBG;
    [SerializeField] private TMP_Text dialogue;

    [Header("Type step")]
    [SerializeField] private CanvasGroup typeGroup;
    [SerializeField] private IncenseOption[] options;

    [Header("Amount step")]
    [SerializeField] private CanvasGroup amountGroup;
    [SerializeField] private Slider amountSlider;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton; // now acts as "Back"

    [Header("Selection Highlight")]
    [SerializeField] private RectTransform selectionHighlight;
    [SerializeField] private Vector2 highlightPadding = new Vector2(10f, 10f);

    private ShrineController currentShrine;
    private string currentType;
    private int lastOptionIndex;
    private Step step = Step.None;

    private void Awake()
    {
        Instance = this;

        // type buttons are wired here because each needs to know its own index
        for (int i = 0; i < options.Length; i++)
        {
            int index = i; // local copy for the lambda
            options[i].button.onClick.AddListener(() => OnTypeChosen(index));
        }

        SetVisible(false);
        SetGroup(typeGroup, false);
        SetGroup(amountGroup, false);
        if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        cancelRef?.action.Enable(); // no Disable(): shared action asset
    }

    private void Update()
    {
        if (step == Step.None) return;

        if (cancelRef != null && cancelRef.action.WasPressedThisFrame())
        {
            if (step == Step.Amount) ShowTypeStep();   // back one step
            else Finish(null, 0, true);                // leave the offering
            return;
        }

        UpdateHighlight();
    }

    // ---------------- Open / close ----------------

    public void Open(ShrineController shrine)
    {
        if (options.Length == 0)
        {
            Debug.LogError("ShrineOfferingUI has no incense options assigned", this);
            shrine.OnAmountChosen(null, 0, true);
            return;
        }

        currentShrine = shrine;
        lastOptionIndex = 0;

        SetVisible(true);
        ShowTypeStep();
    }

    private void Finish(string type, int amount, bool cancelled)
    {
        StopAllCoroutines();
        EventSystem.current.SetSelectedGameObject(null); // deselect before hiding

        step = Step.None;
        SetGroup(typeGroup, false);
        SetGroup(amountGroup, false);
        if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(false);
        SetVisible(false);

        var shrine = currentShrine;
        currentShrine = null;
        shrine?.OnAmountChosen(type, amount, cancelled);
    }

    // ---------------- Step 1: choose the incense ----------------

    private void ShowTypeStep()
    {
        step = Step.Type;
        SetGroup(amountGroup, false);
        SetGroup(typeGroup, true);

        dialogue.text = "Choose an incense to offer";

        foreach (var o in options)
            if (o.ownedText != null) o.ownedText.text = $"x{OwnedOf(o.type)}";

        StopAllCoroutines();
        StartCoroutine(SelectNextFrame(options[Mathf.Clamp(lastOptionIndex, 0, options.Length - 1)].button.gameObject));
    }

    private void OnTypeChosen(int index)
    {
        lastOptionIndex = index;
        currentType = options[index].type;

        int owned = OwnedOf(currentType);
        if (owned < 1)
        {
            dialogue.text = $"You don't have any {currentType} incense";
            return;
        }

        amountSlider.wholeNumbers = true;
        amountSlider.minValue = 1;
        amountSlider.maxValue = owned;
        amountSlider.SetValueWithoutNotify(1);
        UpdateAmountText();

        step = Step.Amount;
        SetGroup(typeGroup, false);
        SetGroup(amountGroup, true);

        StopAllCoroutines();
        StartCoroutine(SelectNextFrame(amountSlider.gameObject));
    }

    // ---------------- Step 2: choose the amount ----------------
    // OnSliderChanged / OnConfirmPressed / OnCancelPressed stay wired in the Inspector as before

    public void OnSliderChanged(float value)
    {
        UpdateAmountText();
    }

    private void UpdateAmountText()
    {
        amountText.text = $"{(int)amountSlider.value}";
        dialogue.text = $"Offering {currentType} incense sticks";
    }

    public void OnConfirmPressed()
    {
        Finish(currentType, (int)amountSlider.value, false);
    }

    public void OnCancelPressed()
    {
        ShowTypeStep(); // "Back"
    }

    // ---------------- Helpers ----------------

    private int OwnedOf(string type)
    {
        ItemData item = Inventory.Instance.GetItem("incense_" + type);
        return item != null ? Inventory.Instance.Count(item) : 0;
    }

    private void UpdateHighlight()
    {
        if (selectionHighlight == null) return;

        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        if (selected != null
            && selected.transform.IsChildOf(offeringPanel.transform)
            && selected.TryGetComponent<RectTransform>(out var target))
        {
            selectionHighlight.gameObject.SetActive(true);
            selectionHighlight.position = target.position;
            selectionHighlight.sizeDelta = target.sizeDelta + highlightPadding;
        }
        else
        {
            selectionHighlight.gameObject.SetActive(false);
        }
    }

    private IEnumerator SelectNextFrame(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void SetVisible(bool visible)
    {
        offeringCanvasGroup.alpha = visible ? 1f : 0f;
        offeringCanvasGroup.interactable = visible;
        offeringCanvasGroup.blocksRaycasts = visible;
        WhiteBG.SetActive(visible);
    }

    private static void SetGroup(CanvasGroup group, bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}