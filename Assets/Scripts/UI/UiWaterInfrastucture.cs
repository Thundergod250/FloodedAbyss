using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiWaterInfrastucture : UiModals
{
    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI laborCostText;
    [SerializeField] private TextMeshProUGUI notEnoughWorkersText;

    [Header("Buttons")]
    [SerializeField] private Button activateButton;
    [SerializeField] private TextMeshProUGUI activateButtonText;

    [SerializeField] private Button emptyStorageButton;
    [SerializeField] private TextMeshProUGUI emptyStorageButtonText;

    [SerializeField] private Button destroyStructureButton;

    private StructureWaterInfrastructure waterInfrastructure;

    protected override void Initialize()
    {
        base.Initialize();

        if (activateButtonText == null && activateButton != null)
            activateButtonText = activateButton.GetComponentInChildren<TextMeshProUGUI>();

        if (emptyStorageButtonText == null && emptyStorageButton != null)
            emptyStorageButtonText = emptyStorageButton.GetComponentInChildren<TextMeshProUGUI>();

        if (activateButton != null)
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(OnActivateClicked);
        }

        if (emptyStorageButton != null)
        {
            emptyStorageButton.onClick.RemoveAllListeners();
            emptyStorageButton.onClick.AddListener(OnEmptyStorageClicked);
        }

        if (destroyStructureButton != null)
        {
            destroyStructureButton.onClick.RemoveAllListeners();
            destroyStructureButton.onClick.AddListener(OnDestroyStructureClicked);
        }

        if (notEnoughWorkersText != null)
            notEnoughWorkersText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen || waterInfrastructure == null)
            return;

        UpdateButtonStates();
    }

    public void Setup(StructureWaterInfrastructure infrastructure)
    {
        if (infrastructure == null)
            return;

        waterInfrastructure = infrastructure;

        if (titleText != null)
            titleText.text = infrastructure.StructureName;

        if (laborCostText != null)
        {
            laborCostText.text = infrastructure.RequiresWorkers ? $"Labor Cost: {infrastructure.LaborCost}": "Labor Cost: None";
        }

        if (notEnoughWorkersText != null)
            notEnoughWorkersText.gameObject.SetActive(false);

        UpdateButtonStates();
    }

    public void ShowNotEnoughWorkersWarning()
    {
        if (notEnoughWorkersText != null)
        {
            notEnoughWorkersText.text = "Not enough workers!";
            notEnoughWorkersText.gameObject.SetActive(true);
        }
    }

    private void UpdateButtonStates()
    {
        if (waterInfrastructure == null)
            return;

        if (activateButtonText != null)
        {
            activateButtonText.text = waterInfrastructure.IsActivated ? "Deactivate": "Activate";
        }

        if (emptyStorageButtonText != null)
        {
            emptyStorageButtonText.text = waterInfrastructure.IsWorking ? "Emptying...": "Empty Storage";
        }

        if (activateButton != null)
            activateButton.interactable = !waterInfrastructure.IsWorking;

        if (emptyStorageButton != null)
            emptyStorageButton.interactable = waterInfrastructure.CanEmptyStorage && !waterInfrastructure.IsWorking;

        if (destroyStructureButton != null)
            destroyStructureButton.interactable = !waterInfrastructure.IsDestroying;

        if (emptyStorageButton != null)
        {
            emptyStorageButton.gameObject.SetActive(waterInfrastructure.CanEmptyStorage);

            emptyStorageButton.interactable = waterInfrastructure.CanEmptyStorage && !waterInfrastructure.IsWorking;
        }
    }

    private void OnActivateClicked()
    {
        waterInfrastructure?.ToggleActivation();
    }

    private void OnEmptyStorageClicked()
    {
        waterInfrastructure?.StartWaterRemoval();
    }

    private void OnDestroyStructureClicked()
    {
        waterInfrastructure?.DestroyStructure();
    }
}
