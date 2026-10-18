using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Cinemachine;

public class UiWorkersHouse : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private GameObject resourceRequirementContainer; // The orange container panel
    [SerializeField] private Transform resourceCostParent;            // Parent container inside the orange box where PanelResourcePair is spawned
    [SerializeField] private GameObject resourcePairPrefab;           // PanelResourcePair prefab reference

    [Header("Upgrade References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private GameObject upgradeRequirementContainer;
    [SerializeField] private Transform upgradeCostParent;

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

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(UpgradeHouseVisuals);
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
            if (currentHouse.IsSubmerged)
            {
                Debug.Log("[UiWorkersHouse] Worker's House is submerged! Force closing UI.");
                GameManager.Instance.uiController.CloseAllModals();
                return;
            }

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

        UpdateUpgradeCostsVisuals();
        UpdateActionButtonVisuals();
        UpdateDestroyButtonVisuals();
    }

    private void BuildResourceCosts()
    {
        if (resourceCostParent == null || resourcePairPrefab == null || currentHouse == null) return;

        foreach (Transform child in resourceCostParent)
        {
            if (child.gameObject != resourcePairPrefab)
            {
                Destroy(child.gameObject);
            }
        }

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

    private void UpdateUpgradeCostsVisuals()
    {
        if (resourceCostParent == null || resourcePairPrefab == null || currentHouse == null) return;

        foreach (Transform child in upgradeCostParent)
        {
            if (child.gameObject != resourcePairPrefab)
            {
                Destroy(child.gameObject);
            }
        }
        int nextLevelIndex = currentHouse.HouseLevelVal;

        if (nextLevelIndex < currentHouse.HouseLevels.Count)
        {
            StructureDataSO upgradeData = currentHouse.HouseLevels[nextLevelIndex];

            if (upgradeData != null)
            {
                foreach (Transform child in upgradeCostParent)
                {
                    if (child.gameObject != resourcePairPrefab)
                        Destroy(child.gameObject);
                }

                foreach (ResourceCostEntry cost in upgradeData.Costs)
                {
                    if (cost.resourceDataSo == null)
                        continue;

                    GameObject costGO = Instantiate(
                        resourcePairPrefab,
                        upgradeCostParent
                    );

                    costGO.SetActive(true);

                    PanelResourcePair panel = costGO.GetComponent<PanelResourcePair>();

                    if (panel != null)
                    {
                        panel.Setup(cost.resourceDataSo, cost.amount);
                    }
                    else
                    {
                        Debug.LogWarning(
                            "[UiWorkersHouse] PanelResourcePair component missing on prefab!",
                            costGO
                        );
                    }
                }
                upgradeButton.gameObject.SetActive(true);
            }
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
        }
    }

    public void UpgradeHouseVisuals()
    {
        if (currentHouse == null)
            return;

        bool upgraded = currentHouse.TryUpgradeHouse();

        if (!upgraded)
            return;

        if (titleText != null)
            titleText.text = currentHouse.StructureTitle;

        BuildResourceCosts();
        UpdateActionButtonVisuals();
        UpdateDestroyButtonVisuals();
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