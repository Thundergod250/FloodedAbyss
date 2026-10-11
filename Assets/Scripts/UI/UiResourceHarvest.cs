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
    
    [Header("Progress Bar References")]
    [SerializeField] private GameObject productionBarContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI progressDescriptionText;

    private StructureHarvestResource currentStructure;
    private Coroutine warningRoutine;

    protected override void Initialize()
    {
        base.Initialize();
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
        // Continuously read the progress percentage directly from the bound structure via Update
        if (IsOpen && currentStructure != null)
        {
            if (currentStructure.IsWorking)
            {
                if (productionBarContainer != null && !productionBarContainer.activeSelf)
                    productionBarContainer.SetActive(true);

                if (activateButton != null && activateButton.interactable)
                    activateButton.interactable = false;

                if (fillImage != null)
                {
                    fillImage.fillAmount = currentStructure.ProgressPercentage;
                }
            }
        }
    }

    public void Setup(StructureHarvestResource structure)
    {
        currentStructure = structure;

        if (titleText != null) titleText.text = structure.StructureTitle;
        if (laborCostText != null) laborCostText.text = $"Labor Cost: {structure.LaborCost}";
        if (progressDescriptionText != null) progressDescriptionText.text = structure.ProductionDescription;

        if (notEnoughWorkersText != null) notEnoughWorkersText.gameObject.SetActive(false);

        // If the structure is already working when opening the UI, sync immediately
        if (structure.IsWorking)
        {
            if (productionBarContainer != null) productionBarContainer.SetActive(true);
            if (activateButton != null) activateButton.interactable = false;
            if (fillImage != null) fillImage.fillAmount = structure.ProgressPercentage;
        }
        else
        {
            if (productionBarContainer != null) productionBarContainer.SetActive(false);
            if (activateButton != null) activateButton.interactable = true;
            if (fillImage != null) fillImage.fillAmount = 0f;
        }
    }

    private void OnActivateClicked()
    {
        if (currentStructure != null)
        {
            currentStructure.TryStartHarvest();
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

    public void BeginProgress(float duration)
    {
        if (activateButton != null) activateButton.interactable = false;
        if (productionBarContainer != null) productionBarContainer.SetActive(true);
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    public void UpdateProgress(float progress01)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = progress01;
        }
    }

    public void CompleteProgress()
    {
        if (productionBarContainer != null) productionBarContainer.SetActive(false);
    }
}