using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAutoMiner : UiModals
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

    private AutoMiner currentTargetMiner;

    protected override void Start()
    {
        //if (claimButton != null) claimButton.onClick.AddListener(OnClaimClicked);
        if (addWorkerButton != null) addWorkerButton.onClick.AddListener(OnAddWorkerClicked);
        if (removeWorkerButton != null) removeWorkerButton.onClick.AddListener(OnRemoveWorkerClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);
    }

    /*protected override void OnEnable()
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
    }*/

    public void OpenAutoMinerMenu(AutoMiner miner)
    {
        currentTargetMiner = miner;
        UpdateUI();

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.AutoMiner);
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
        if (currentTargetMiner == null) return;

        if (buildingNameText != null)
            buildingNameText.text = $"{currentTargetMiner.buildingName} (Lvl {currentTargetMiner.MinerLevel})";

        if (storageText != null)
        {
            string storageInfo = "Stored Resources:\n";
            foreach (var kvp in currentTargetMiner.StoredResources)
            {
                storageInfo += $"� {kvp.Key}: {kvp.Value} / {currentTargetMiner.MaxStoragePerResource}\n";
            }
            storageText.text = storageInfo;
        }

        /*
        if (workerCountText != null)
            workerCountText.text = $"Workers: {currentTargetMiner.AssignedWorkers} / {currentTargetMiner.MaxWorkers}";

        if (availablePopulationText != null && PopulationManager.Instance != null)
            availablePopulationText.text = $"Available Population: {PopulationManager.Instance.AvailablePopulation} / {PopulationManager.Instance.TotalPopulation}";

        if (efficiencyText != null)
            efficiencyText.text = $"Total Productivity: {currentTargetMiner.TotalProductivity * 100:F0}%";*/
    }

    /*
    private void OnClaimClicked()
    {
        if (currentTargetMiner != null)
        {
            currentTargetMiner.ClaimResources();
            UpdateUI();
        }
    }*/

    private void OnAddWorkerClicked()
    {
        if (currentTargetMiner != null)
        {
            currentTargetMiner.TryAddWorker();
            UpdateUI();
        }
    }

    private void OnRemoveWorkerClicked()
    {
        if (currentTargetMiner != null)
        {
            currentTargetMiner.TryRemoveWorker();
            UpdateUI();
        }
    }
}