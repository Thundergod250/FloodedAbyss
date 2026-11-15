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

    [SerializeField] private Button destroyStructureButton;

    private StructureWaterInfrastructure waterInfrastructure;

    protected override void Initialize()
    {
        base.Initialize();

        if (activateButtonText == null && activateButton != null)
            activateButtonText = activateButton.GetComponentInChildren<TextMeshProUGUI>();


        if (activateButton != null)
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(OnActivateClicked);
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

        bool isDestroying = waterInfrastructure.IsDestroying;
        bool isActivated = waterInfrastructure.IsActivated;

        if (activateButtonText != null)
        {
            activateButtonText.text = isActivated ? "Activated" : "Activate";
        }

        if (activateButton != null)
        {
            activateButton.interactable = !isActivated && !isDestroying;
        }

        if (destroyStructureButton != null)
        {
            destroyStructureButton.interactable = !isDestroying;
        }
    }

    private void OnActivateClicked()
    {
        if (waterInfrastructure == null || waterInfrastructure.IsDestroying)
            return;

        waterInfrastructure?.ToggleActivation();
    }

    private void OnDestroyStructureClicked()
    {
        if (waterInfrastructure == null || waterInfrastructure.IsDestroying)
            return;

        waterInfrastructure?.DestroyStructure();
    }
}
