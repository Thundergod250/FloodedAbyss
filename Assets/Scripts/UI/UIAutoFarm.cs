using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAutoFarm : UiModals
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI storageText;
    [SerializeField] private TextMeshProUGUI workerCountText;
    [SerializeField] private TextMeshProUGUI availablePopulationText;
    [SerializeField] private TextMeshProUGUI efficiencyText;

    [Header("Buttons")]
    [SerializeField] private Button claimButton;
    [SerializeField] private Button addWorkerButton;
    [SerializeField] private Button removeWorkerButton;
    [SerializeField] private Button closeButton;

    private AutoFarm currentTargetFarm;

    protected override void Start()
    {
        if (claimButton != null) claimButton.onClick.AddListener(OnClaimClicked);
        if (addWorkerButton != null) addWorkerButton.onClick.AddListener(OnAddWorkerClicked);
        if (removeWorkerButton != null) removeWorkerButton.onClick.AddListener(OnRemoveWorkerClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);
    }

    protected override void OnEnable()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.EvtOnPopulationChanged.AddListener(UpdateUI);
        }
    }

    protected override void OnDisable()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.EvtOnPopulationChanged.RemoveListener(UpdateUI);
        }
    }

    public void OpenAutoFarmMenu(AutoFarm farm)
    {
        currentTargetFarm = farm;
        UpdateUI();

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.AutoFarm);
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
        if (currentTargetFarm == null) return;

        if (buildingNameText != null)
            buildingNameText.text = $"{currentTargetFarm.buildingName} (Lvl {currentTargetFarm.FarmLevel})";

        if (storageText != null)
        {
            string storageInfo = "Stored Crops:\n";
            foreach (var kvp in currentTargetFarm.StoredResources)
            {
                storageInfo += $"• {kvp.Key}: {kvp.Value} / {currentTargetFarm.MaxStoragePerResource}\n";
            }
            storageText.text = storageInfo;
        }

        if (workerCountText != null)
            workerCountText.text = $"Farmers: {currentTargetFarm.AssignedWorkers} / {currentTargetFarm.MaxWorkers}";

        if (availablePopulationText != null && PopulationManager.Instance != null)
            availablePopulationText.text = $"Available Population: {PopulationManager.Instance.AvailablePopulation} / {PopulationManager.Instance.TotalPopulation}";

        if (efficiencyText != null)
            efficiencyText.text = $"Total Productivity: {currentTargetFarm.TotalProductivity * 100:F0}%";
    }

    private void OnClaimClicked()
    {
        if (currentTargetFarm != null)
        {
            currentTargetFarm.ClaimResources();
            UpdateUI();
        }
    }

    private void OnAddWorkerClicked()
    {
        if (currentTargetFarm != null)
        {
            currentTargetFarm.TryAddWorker();
            UpdateUI();
        }
    }

    private void OnRemoveWorkerClicked()
    {
        if (currentTargetFarm != null)
        {
            currentTargetFarm.TryRemoveWorker();
            UpdateUI();
        }
    }
}