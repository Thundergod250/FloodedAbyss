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
    
    [Header("Destroy Button References")]
    [SerializeField] private Button destroyButton;
    [SerializeField] private TextMeshProUGUI destroyButtonText;

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
            activateButtonText = activateButton.GetComponentInChildren<TextMeshProUGUI>();

        if (destroyButtonText == null && destroyButton != null)
            destroyButtonText = destroyButton.GetComponentInChildren<TextMeshProUGUI>();

        if (activateButton != null)
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(OnActivateClicked);
        }

        if (destroyButton != null)
        {
            destroyButton.onClick.RemoveAllListeners();
            destroyButton.onClick.AddListener(OnDestroyClicked);
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
            bool showBar = currentStructure.IsWorking || currentStructure.IsDestroying;
            if (productionBarContainer != null && productionBarContainer.activeSelf != showBar)
            {
                productionBarContainer.SetActive(showBar);
            }

            if (showBar && fillImage != null)
            {
                fillImage.fillAmount = currentStructure.ProgressPercentage;
            }

            if (progressDescriptionText != null)
            {
                progressDescriptionText.text = currentStructure.ProductionDescription;
            }

            UpdateActivateButtonVisuals();
            UpdateDestroyButtonVisuals();
        }
    }

    public void Setup(StructureHarvestResource structure)
    {
        currentStructure = structure;

        if (titleText != null) titleText.text = structure.StructureTitle;
        if (laborCostText != null) laborCostText.text = $"Labor Cost: {structure.LaborCost}";
        if (notEnoughWorkersText != null) notEnoughWorkersText.gameObject.SetActive(false);

        // Show or hide destroy button based on structure configuration
        if (destroyButton != null)
        {
            destroyButton.gameObject.SetActive(structure.CanBeDestroyed);
        }

        UpdateActivateButtonVisuals();
        UpdateDestroyButtonVisuals();
    }

    public bool IsShowingStructure(StructureHarvestResource structure)
    {
        return IsOpen && currentStructure == structure;
    }

    private void UpdateActivateButtonVisuals()
    {
        if (activateButton != null && currentStructure != null)
        {
            bool isWorking = currentStructure.IsWorking;
            if (activateButtonText != null)
            {
                activateButtonText.text = isWorking ? "Deactivate" : "Activate";
            }

            ColorBlock colors = activateButton.colors;
            Color targetColor = isWorking ? new Color(1f, 0.6f, 0f) : Color.white;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.selectedColor = targetColor;
            activateButton.colors = colors;

            activateButton.interactable = !currentStructure.IsDestroying;
        }
    }

    private void UpdateDestroyButtonVisuals()
    {
        if (destroyButton != null && currentStructure != null && currentStructure.CanBeDestroyed)
        {
            bool isDestroying = currentStructure.IsDestroying;

            if (destroyButtonText != null)
            {
                destroyButtonText.text = isDestroying ? "Cancel" : "Destroy Structure";
            }

            ColorBlock colors = destroyButton.colors;
            Color targetColor = isDestroying ? Color.red : new Color(1f, 0.85f, 0f); 

            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.selectedColor = targetColor;
            destroyButton.colors = colors;
        }
    }

    private void OnActivateClicked()
    {
        if (currentStructure != null)
        {
            currentStructure.ToggleHarvesting();
        }
    }

    private void OnDestroyClicked()
    {
        if (currentStructure != null)
        {
            currentStructure.ToggleDestroying();
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
    }
}