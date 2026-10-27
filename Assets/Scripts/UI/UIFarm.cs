using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiFarm : UiModals
{
    [Header("Modal Reference")]
    [SerializeField] private GameObject ModalPrefab;

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

    private void Awake()
    {
        if (activateButton != null) activateButton.onClick.AddListener(OnActivateClicked);
        if (destroyButton != null) destroyButton.onClick.AddListener(OnDestroyClicked);
    }

    private void OnDestroy()
    {
        if (activateButton != null) activateButton.onClick.RemoveListener(OnActivateClicked);
        if (destroyButton != null) destroyButton.onClick.RemoveListener(OnDestroyClicked);
    }

    // ------------------------------------------------------------------
    // PUBLIC API (called by StructureFarm)
    // ------------------------------------------------------------------
    public void Setup(StructureFarm farm)
    {
        currentFarm = farm;

        if (titleText != null) titleText.text = farm.StructureTitle;
        if (laborCostText != null) laborCostText.text = $"Labor Cost: {farm.LaborCost}";
        if (notEnoughWorkersText != null) notEnoughWorkersText.gameObject.SetActive(false);

        UpdateButtonStates();
        UpdateProgressBar();
    }

    public void Show()
    {
        if (ModalPrefab != null) ModalPrefab.SetActive(true);
        if (notEnoughWorkersText != null) notEnoughWorkersText.gameObject.SetActive(false);
        UpdateButtonStates();
        UpdateProgressBar();
    }

    public void Hide()
    {
        if (ModalPrefab != null) ModalPrefab.SetActive(false);
    }

    public void ShowNotEnoughWorkersWarning()
    {
        if (notEnoughWorkersText != null)
        {
            notEnoughWorkersText.text = "Not enough workers!";
            notEnoughWorkersText.gameObject.SetActive(true);
        }
    }

    public bool IsShowingStructure(StructureFarm farm) => currentFarm == farm;

    public void CompleteProgress()
    {
        if (fillImage != null) fillImage.fillAmount = 1f;
    }

    // ------------------------------------------------------------------
    // UPDATE LOOP (live progress / button states)
    // ------------------------------------------------------------------
    private void Update()
    {
        if (currentFarm == null) return;
        if (ModalPrefab != null && !ModalPrefab.activeSelf) return;

        UpdateButtonStates();
        UpdateProgressBar();
    }

    private void UpdateButtonStates()
    {
        if (currentFarm == null) return;

        if (activateButtonText != null)
        {
            if (currentFarm.IsDestroying) activateButtonText.text = "Destroying...";
            else if (currentFarm.IsWorking) activateButtonText.text = "Stop Working";
            else activateButtonText.text = "Start Working";
        }

        if (destroyButtonText != null)
            destroyButtonText.text = currentFarm.IsDestroying ? "Cancel" : "Destroy";

        if (activateButton != null) activateButton.interactable = !currentFarm.IsDestroying;
        if (destroyButton != null) destroyButton.interactable = currentFarm.CanBeDestroyed;

        if (productionBarContainer != null)
            productionBarContainer.SetActive(currentFarm.IsWorking || currentFarm.IsDestroying);
    }

    private void UpdateProgressBar()
    {
        if (currentFarm == null) return;
        if (fillImage != null) fillImage.fillAmount = currentFarm.ProgressPercentage;
        if (progressDescriptionText != null)
            progressDescriptionText.text = currentFarm.ProductionDescription;
    }

    private void OnActivateClicked() => currentFarm?.ToggleWorking();
    private void OnDestroyClicked() => currentFarm?.ToggleDestroying();
}