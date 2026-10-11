using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UiResourceHarvest : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI laborCostText;
    [SerializeField] private TextMeshProUGUI notEnoughWorkersText;
    [SerializeField] private Button activateButton;
    [SerializeField] private TextMeshProUGUI activateButtonText; 
    
    [Header("Progress Bar References")]
    [SerializeField] private GameObject productionBarContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI progressDescriptionText;

    private StructureHarvestResource currentStructure;
    private Coroutine warningRoutine;

    protected override void Initialize()
    {
        base.Initialize();

        if (activateButtonText == null && activateButton != null)
        {
            activateButtonText = activateButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (activateButton != null)
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(OnActivateClicked);
        }

        if (notEnoughWorkersText != null)
            notEnoughWorkersText.gameObject.SetActive(false);

        if (productionBarContainer != null)
            productionBarContainer.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen && currentStructure != null)
        {
            if (productionBarContainer != null)
            {
                bool shouldShow = currentStructure.IsWorking;
                if (productionBarContainer.activeSelf != shouldShow)
                    productionBarContainer.SetActive(shouldShow);
            }

            if (currentStructure.IsWorking && fillImage != null)
            {
                fillImage.fillAmount = currentStructure.ProgressPercentage;
            }

            UpdateActivateButtonVisuals();
        }
    }

    public void Setup(StructureHarvestResource structure)
    {
        currentStructure = structure;

        if (titleText != null) titleText.text = structure.StructureTitle;
        if (laborCostText != null) laborCostText.text = $"Labor Cost: {structure.LaborCost}";
        if (progressDescriptionText != null) progressDescriptionText.text = structure.ProductionDescription;

        if (notEnoughWorkersText != null) notEnoughWorkersText.gameObject.SetActive(false);

        if (structure.IsWorking)
        {
            if (productionBarContainer != null) productionBarContainer.SetActive(true);
            if (fillImage != null) fillImage.fillAmount = structure.ProgressPercentage;
        }
        else
        {
            if (productionBarContainer != null) productionBarContainer.SetActive(false);
            if (fillImage != null) fillImage.fillAmount = 0f;
        }

        UpdateActivateButtonVisuals();
    }

    private void UpdateActivateButtonVisuals()
    {
        if (activateButton != null)
        {
            bool isWorking = currentStructure != null && currentStructure.IsWorking;

            // Set text label
            if (activateButtonText != null)
            {
                activateButtonText.text = isWorking ? "Deactivate" : "Activate";
            }

            // Override color states to Orange when active, White when default/inactive
            ColorBlock colors = activateButton.colors;
            Color targetColor = isWorking ? new Color(1f, 0.6f, 0f) : Color.white; // Vibrant Orange when active

            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.selectedColor = targetColor;
            colors.disabledColor = targetColor;

            activateButton.colors = colors;
        }
    }

    private void OnActivateClicked()
    {
        if (currentStructure != null)
        {
            currentStructure.ToggleHarvesting();
            UpdateActivateButtonVisuals();
        }
    }

    public void ShowNotEnoughWorkersWarning()
    {
        if (notEnoughWorkersText != null)
        {
            if (warningRoutine != null) StopCoroutine(warningRoutine);
            warningRoutine = StartCoroutine(FlashWarningRoutine());
        }
    }

    private IEnumerator FlashWarningRoutine()
    {
        notEnoughWorkersText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        notEnoughWorkersText.gameObject.SetActive(false);
    }

    public void CompleteProgress()
    {
        if (productionBarContainer != null) productionBarContainer.SetActive(false);
        UpdateActivateButtonVisuals();
    }
}