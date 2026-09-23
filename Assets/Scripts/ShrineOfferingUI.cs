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

    private ShrineController currentShrine;

    private void Awake()
    {
        Instance = this;
        offeringPanel.SetActive(false);
    }

    public void Open(ShrineController shrine, int incenseCost)
    {
        currentShrine = shrine;

        int owned = PlayerStats.Instance.IncenseSticks;

        amountSlider.wholeNumbers = true;
        // amountSlider.minValue = Mathf.Min(incenseCost, owned);
        amountSlider.minValue = 0;
        amountSlider.maxValue = Mathf.Max(owned, incenseCost);
        amountSlider.value = amountSlider.minValue;

        UpdateAmountText();

        offeringPanel.SetActive(true);
        GameManager.Instance.UpdateGameState(GameState.Dialogue);

        StartCoroutine(FocusSlider());
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
        amountText.text = $"Offer {(int)amountSlider.value} incense sticks";
    }

    public void OnConfirmPressed()
    {
        int amount = (int)amountSlider.value;
        currentShrine.TryOffer(amount);
        Close();
    }

    public void OnCancelPressed()
    {
        Close();
    }

    private void Close()
    {
        offeringPanel.SetActive(false);
        GameManager.Instance.UpdateGameState(GameState.Overworld);
        currentShrine = null;
    }
}