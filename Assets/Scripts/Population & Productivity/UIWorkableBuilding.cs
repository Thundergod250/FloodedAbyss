using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWorkableBuilding : UiModals
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI workerCountText;
    [SerializeField] private TextMeshProUGUI availablePopulationText;
    [SerializeField] private TextMeshProUGUI localEfficiencyText;
    [SerializeField] private TextMeshProUGUI globalHappinessText;
    [SerializeField] private TextMeshProUGUI calculatedYieldText;

    [Header("Buttons")]
    [SerializeField] private Button addWorkerButton;
    [SerializeField] private Button removeWorkerButton;
    [SerializeField] private Button closeButton;

    private WorkableStructure currentTargetBuilding;

    private void Awake()
    {
        if (addWorkerButton != null) addWorkerButton.onClick.AddListener(OnAddWorkerClicked);
        if (removeWorkerButton != null) removeWorkerButton.onClick.AddListener(OnRemoveWorkerClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);
    }

    private void OnEnable()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.EvtOnPopulationChanged.AddListener(UpdateUI);
        }
    }

    private void OnDisable()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.EvtOnPopulationChanged.RemoveListener(UpdateUI);
        }
    }

    public void OpenBuildingMenu(WorkableStructure structure)
    {
        currentTargetBuilding = structure;
        UpdateUI();

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
        }
    }

    public void CloseMenu()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    public void UpdateUI()
    {
        if (currentTargetBuilding == null) return;

        if (buildingNameText != null)
            buildingNameText.text = currentTargetBuilding.buildingName;

        if (workerCountText != null)
            workerCountText.text = $"Workers: {currentTargetBuilding.AssignedWorkers} / {currentTargetBuilding.MaxWorkers}";

        if (availablePopulationText != null && PopulationManager.Instance != null)
            availablePopulationText.text = $"Available Population: {PopulationManager.Instance.AvailablePopulation} / {PopulationManager.Instance.TotalPopulation}";

        if (localEfficiencyText != null)
            localEfficiencyText.text = $"Staffing Efficiency: {currentTargetBuilding.LocalEfficiency * 100:F0}%";

        if (globalHappinessText != null)
            globalHappinessText.text = $"Global Happiness: {currentTargetBuilding.GlobalEfficiency * 100:F0}%";

        if (calculatedYieldText != null)
            calculatedYieldText.text = $"Est. Yield per Tick: {currentTargetBuilding.CalculatedYield}";
    }

    private void OnAddWorkerClicked()
    {
        if (currentTargetBuilding != null)
        {
            currentTargetBuilding.TryAddWorker();
            UpdateUI();
        }
    }

    private void OnRemoveWorkerClicked()
    {
        if (currentTargetBuilding != null)
        {
            currentTargetBuilding.TryRemoveWorker();
            UpdateUI();
        }
    }
}