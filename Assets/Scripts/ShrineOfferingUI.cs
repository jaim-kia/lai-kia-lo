using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class ShrineOfferingUI : MonoBehaviour
{
    public static ShrineOfferingUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject offeringPanel;
    [SerializeField] private Slider amountSlider;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text dialogue;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Selection Highlight")]
    [SerializeField] private RectTransform selectionHighlight; // Drag your SelectionHighlight Image here
    [SerializeField] private Vector2 highlightPadding = new Vector2(10f, 10f); // Extra breathing room around buttons

    [SerializeField] private CanvasGroup offeringCanvasGroup; 

    private ShrineController currentShrine;
    private string currentIncenseType;

    private void SetOfferingVisible(bool visible)
    {
        offeringCanvasGroup.alpha = visible ? 1f : 0f;
        offeringCanvasGroup.interactable = visible;
        offeringCanvasGroup.blocksRaycasts = visible;
    }

    private void Awake()
    {
        Instance = this;
        SetOfferingVisible(false);
    }

    private void Update()
    {
        if (offeringCanvasGroup.alpha <= 0f || selectionHighlight == null) return;

        GameObject currentSelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        // Check if the current selected object is part of this UI screen
        if (currentSelected != null && (currentSelected == amountSlider.gameObject || 
                                       currentSelected == confirmButton.gameObject || 
                                       currentSelected == cancelButton.gameObject))
        {
            if (!selectionHighlight.gameObject.activeSelf)
            {
                selectionHighlight.gameObject.SetActive(true);
            }

            // Snap the highlight position and size to match the selected UI element
            RectTransform targetRect = currentSelected.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                selectionHighlight.position = targetRect.position;
                selectionHighlight.sizeDelta = targetRect.sizeDelta + highlightPadding;
            }
        }
        else
        {
            // Hide selection frame if selection leaves these controls
            selectionHighlight.gameObject.SetActive(false);
        }
    }

    public void Open(ShrineController shrine, string incenseType)
    {
        currentShrine = shrine;
        currentIncenseType = incenseType;

        int owned = PlayerStats.Instance.IncenseSticks;

        amountSlider.wholeNumbers = true;
        amountSlider.minValue = 0;
        amountSlider.maxValue = owned;
        amountSlider.value = 0;

        UpdateAmountText();
        SetOfferingVisible(true);

        StartCoroutine(FocusSlider());
    }

    private void Close()
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(false);
        }

        SetOfferingVisible(false);
        currentShrine = null;
    }

    private IEnumerator FocusSlider()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(amountSlider.gameObject);
    }

    public void OnSliderChanged(float value)
    {   
        UpdateAmountText();
    }

    private void UpdateAmountText()
    {
        amountText.text = $"{(int)amountSlider.value}";
        dialogue.text = $"Offering {currentIncenseType} incense sticks";
    }

    public void OnConfirmPressed()
    {
        int amount = (int)amountSlider.value;
        currentShrine.OnAmountChosen(currentIncenseType, amount, false);
        Close();
    }

    public void OnCancelPressed()
    {
        currentShrine.OnAmountChosen(currentIncenseType, 0, true);
        Close();
    }
}