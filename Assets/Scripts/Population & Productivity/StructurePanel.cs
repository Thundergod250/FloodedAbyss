using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructurePanel : UiModals
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI buildingTitleText;
    [SerializeField] private TextMeshProUGUI workerCapacityText;
    [SerializeField] private TextMeshProUGUI productivityText;
    [SerializeField] private TextMeshProUGUI happinessMultiplierText;
    [SerializeField] private TextMeshProUGUI totalOutputText;

    [Header("Buttons")]
    [SerializeField] private Button addWorkerButton;
    [SerializeField] private Button removeWorkerButton;
    [SerializeField] private Button closeButton;

    private WorkableStructure targetStructure;

    private void Start()
    {
        if (addWorkerButton != null) addWorkerButton.onClick.AddListener(OnAddWorkerClicked);
        if (removeWorkerButton != null) removeWorkerButton.onClick.AddListener(OnRemoveWorkerClicked);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenStructurePanel(WorkableStructure structure)
    {
        targetStructure = structure;
        UpdateUI();

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Structure);
        }
    }

    public void ClosePanel()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    public void UpdateUI()
    {
        if (targetStructure == null) return;

        if (buildingTitleText != null)
            buildingTitleText.text = targetStructure.buildingName;

        if (workerCapacityText != null)
            workerCapacityText.text = $"Workers: {targetStructure.AssignedWorkers} / {targetStructure.MaxWorkers}";

        if (productivityText != null)
            productivityText.text = $"Staffing Efficiency: {targetStructure.LocalEfficiency * 100:F0}%";

        if (happinessMultiplierText != null)
            happinessMultiplierText.text = $"Happiness Multiplier: {targetStructure.GlobalEfficiency * 100:F0}%";

        if (totalOutputText != null)
            totalOutputText.text = $"Estimated Output: {targetStructure.CalculatedYield} {targetStructure.OutputResourceType}/cycle";
    }

    private void OnAddWorkerClicked()
    {
        if (targetStructure != null)
        {
            targetStructure.TryAddWorker();
            UpdateUI();
        }
    }

    private void OnRemoveWorkerClicked()
    {
        if (targetStructure != null)
        {
            targetStructure.TryRemoveWorker();
            UpdateUI();
        }
    }
}