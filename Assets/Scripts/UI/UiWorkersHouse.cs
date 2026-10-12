using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UiWorkersHouse : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private GameObject resourceRequirementContainer; // The orange container panel
    [SerializeField] private Transform resourceCostParent;            // Parent container inside the orange box where PanelResourcePair is spawned
    [SerializeField] private GameObject resourcePairPrefab;           // PanelResourcePair prefab reference

    [Header("Destroy Button References")]
    [SerializeField] private Button destroyButton;
    [SerializeField] private TextMeshProUGUI destroyButtonText;

    [Header("Progress Bar References")]
    [SerializeField] private GameObject productionBarContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI progressDescriptionText;

    private StructureWorkersHouse currentHouse;

    protected override void Initialize()
    {
        base.Initialize();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        if (destroyButton != null)
        {
            destroyButton.onClick.RemoveAllListeners();
            destroyButton.onClick.AddListener(OnDestroyClicked);
        }

        if (productionBarContainer != null)
            productionBarContainer.SetActive(false);

        if (resourcePairPrefab != null)
            resourcePairPrefab.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen && currentHouse != null)
        {
            bool showBar = currentHouse.IsWorking || currentHouse.IsDestroying;
            if (productionBarContainer != null && productionBarContainer.activeSelf != showBar)
            {
                productionBarContainer.SetActive(showBar);
            }

            if (showBar && fillImage != null)
            {
                fillImage.fillAmount = currentHouse.ProgressPercentage;
            }

            if (progressDescriptionText != null)
            {
                progressDescriptionText.text = currentHouse.ProgressDescription;
            }

            UpdateActionButtonVisuals();
            UpdateDestroyButtonVisuals();
        }
    }

    public void Setup(StructureWorkersHouse house)
    {
        currentHouse = house;
        if (titleText != null) titleText.text = house.StructureTitle;

        BuildResourceCosts();

        UpdateActionButtonVisuals();
        UpdateDestroyButtonVisuals();
    }

    private void BuildResourceCosts()
    {
        if (resourceCostParent == null || resourcePairPrefab == null || currentHouse == null) return;

        // Clear existing generated cost pairs
        foreach (Transform child in resourceCostParent)
        {
            if (child.gameObject != resourcePairPrefab)
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate the required recruitment cost pair if valid
        if (currentHouse.RecruitmentCostResource != null)
        {
            GameObject costGO = Instantiate(resourcePairPrefab, resourceCostParent);
            costGO.SetActive(true);

            PanelResourcePair panel = costGO.GetComponent<PanelResourcePair>();
            if (panel != null)
            {
                int currentAmount = GameManager.Instance.ResourceManager != null 
                    ? GameManager.Instance.ResourceManager.GetResourceAmount(currentHouse.RecruitmentCostResource) 
                    : 0;
                panel.Setup(currentHouse.RecruitmentCostResource, currentHouse.RecruitmentCostAmount);
            }
        }
    }

    public bool IsShowingHouse(StructureWorkersHouse house)
    {
        return IsOpen && currentHouse == house;
    }

    private void UpdateActionButtonVisuals()
    {
        if (actionButton != null && currentHouse != null)
        {
            var workerMgr = GameManager.Instance.WorkerManager;
            bool isHousingAction = currentHouse.IsReservedForHousing || (workerMgr != null && workerMgr.HomelessWorker > 0);

            if (resourceRequirementContainer != null)
            {
                resourceRequirementContainer.SetActive(!isHousingAction && !currentHouse.IsOccupied);
            }

            actionButton.gameObject.SetActive(true);
            actionButton.interactable = !currentHouse.IsOccupied && !currentHouse.IsDestroying;

            if (actionButtonText != null)
            {
                if (currentHouse.IsOccupied)
                {
                    actionButtonText.text = "Occupied";
                }
                else
                {
                    actionButtonText.text = currentHouse.IsWorking 
                        ? (isHousingAction ? "Stop Housing" : "Stop Recruiting") 
                        : (isHousingAction ? "House Worker" : "Recruit Worker");
                }
            }
        }
    }

    private void UpdateDestroyButtonVisuals()
    {
        if (destroyButton != null && currentHouse != null)
        {
            bool isDestroying = currentHouse.IsDestroying;
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

    private void OnActionClicked()
    {
        if (currentHouse != null)
        {
            currentHouse.ToggleHouseAction();
        }
    }

    private void OnDestroyClicked()
    {
        if (currentHouse != null)
        {
            currentHouse.ToggleDestroying();
        }
    }

    public void CompleteProgress()
    {
        if (productionBarContainer != null) productionBarContainer.SetActive(false);
    }
}