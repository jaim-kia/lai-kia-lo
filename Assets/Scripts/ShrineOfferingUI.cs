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
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private CanvasGroup offeringCanvasGroup; // on the same object as offeringPanel

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
        SetOfferingVisible(false);
        currentShrine = null;
    }

    private IEnumerator FocusSlider()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(amountSlider.gameObject);
        Debug.Log("Selected: " + EventSystem.current.currentSelectedGameObject?.name);
    }

    public void OnSliderChanged(float value)
    {   
        Debug.Log("Slider changed to: " + value);
        UpdateAmountText();
    }

    private void UpdateAmountText()
    {
        amountText.text = $"Offer {(int)amountSlider.value} incense sticks";
    }

    public void OnConfirmPressed()
    {
        int amount = (int)amountSlider.value;
        currentShrine.OnAmountChosen(currentIncenseType, amount);
        Close();
    }

    public void OnCancelPressed()
    {
        // treat cancel as offering 0 — story still needs to resume either way
        currentShrine.OnAmountChosen(currentIncenseType, 0);
        Close();
    }

}