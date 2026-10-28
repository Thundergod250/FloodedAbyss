using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiFarm : UiModals
{
    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI laborCostText;
    [SerializeField] private TextMeshProUGUI notEnoughWorkersText;

    [Header("Buttons")]
    [SerializeField] private Button activateButton;
    [SerializeField] private TextMeshProUGUI activateButtonText;
    [SerializeField] private Button destroyButton;
    [SerializeField] private TextMeshProUGUI destroyButtonText;

    [Header("Progress Bar")]
    [SerializeField] private GameObject productionBarContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI progressDescriptionText;

    private StructureFarm currentFarm;

    protected override void Initialize()
    {
        base.Initialize();

        if (activateButtonText == null && activateButton != null)
            activateButtonText =
                activateButton.GetComponentInChildren<TextMeshProUGUI>();

        if (destroyButtonText == null && destroyButton != null)
            destroyButtonText =
                destroyButton.GetComponentInChildren<TextMeshProUGUI>();

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
        if (!IsOpen || currentFarm == null)
            return;

        bool showBar = currentFarm.IsWorking || currentFarm.IsDestroying;

        if (productionBarContainer != null &&
            productionBarContainer.activeSelf != showBar)
        {
            productionBarContainer.SetActive(showBar);
        }

        UpdateButtonStates();
        UpdateProgressBar();
    }

    public void Setup(StructureFarm farm)
    {
        if (farm == null)
            return;

        currentFarm = farm;

        if (titleText != null)
            titleText.text = farm.StructureTitle;

        if (laborCostText != null)
            laborCostText.text = $"Labor Cost: {farm.LaborCost}";

        if (notEnoughWorkersText != null)
            notEnoughWorkersText.gameObject.SetActive(false);

        UpdateButtonStates();
        UpdateProgressBar();
    }

    public void Hide()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    public bool IsShowingStructure(StructureFarm farm)
    {
        return IsOpen && currentFarm == farm;
    }

    public void ShowNotEnoughWorkersWarning()
    {
        if (notEnoughWorkersText != null)
        {
            notEnoughWorkersText.text = "Not enough workers!";
            notEnoughWorkersText.gameObject.SetActive(true);
        }
    }

    public void CompleteProgress()
    {
        if (fillImage != null)
            fillImage.fillAmount = 1f;
    }

    private void UpdateButtonStates()
    {
        if (currentFarm == null)
            return;

        if (activateButtonText != null)
        {
            if (currentFarm.IsDestroying)
                activateButtonText.text = "Destroying...";
            else if (currentFarm.IsWorking)
                activateButtonText.text = "Stop Working";
            else
                activateButtonText.text = "Start Working";
        }

        if (destroyButtonText != null)
        {
            destroyButtonText.text =
                currentFarm.IsDestroying ? "Cancel" : "Destroy";
        }

        if (activateButton != null)
            activateButton.interactable = !currentFarm.IsDestroying;

        if (destroyButton != null)
            destroyButton.interactable = currentFarm.CanBeDestroyed;
    }

    private void UpdateProgressBar()
    {
        if (currentFarm == null)
            return;

        if (fillImage != null)
            fillImage.fillAmount = currentFarm.ProgressPercentage;

        if (progressDescriptionText != null)
            progressDescriptionText.text = currentFarm.ProductionDescription;
    }

    private void OnActivateClicked()
    {
        currentFarm?.ToggleWorking();
    }

    private void OnDestroyClicked()
    {
        currentFarm?.ToggleDestroying();
    }
}