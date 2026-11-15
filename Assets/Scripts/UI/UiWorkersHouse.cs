using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UiWorkersHouse : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private TextMeshProUGUI actionMissingResourceText; // Red warning text for action button
    [SerializeField] private GameObject resourceRequirementContainer; // The orange container panel
    [SerializeField] private Transform resourceCostParent;            // Parent container inside the orange box where PanelResourcePair is spawned
    [SerializeField] private GameObject resourcePairPrefab;           // PanelResourcePair prefab reference

    [Header("Upgrade References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private TextMeshProUGUI upgradeMissingResourceText; // Red warning text for upgrade button
    [SerializeField] private GameObject upgradeRequirementContainer;
    [SerializeField] private Transform upgradeCostParent;

    [Header("Destroy Button References")]
    [SerializeField] private Button destroyButton;
    [SerializeField] private TextMeshProUGUI destroyButtonText;

    [Header("Progress Bar References")]
    [SerializeField] private GameObject productionBarContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI progressDescriptionText;

    [Header("Warning Settings")]
    [SerializeField] private float warningDisplayDuration = 2f;

    private StructureWorkersHouse currentHouse;
    private Coroutine actionWarningRoutine;
    private Coroutine upgradeWarningRoutine;

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

        if (actionMissingResourceText != null)
            actionMissingResourceText.gameObject.SetActive(false);

        if (upgradeMissingResourceText != null)
            upgradeMissingResourceText.gameObject.SetActive(false);
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
                productionBarContainer.SetActive(showBar);

            if (showBar && fillImage != null) 
                fillImage.fillAmount = currentHouse.ProgressPercentage;

            if (progressDescriptionText != null) 
                progressDescriptionText.text = currentHouse.ProgressDescription;

            UpdateActionButtonVisuals();
            UpdateDestroyButtonVisuals();
        }
    }

    public void Setup(StructureWorkersHouse house)
    {
        currentHouse = house;
        if (titleText != null) titleText.text = house.StructureTitle;

        if (actionMissingResourceText != null) actionMissingResourceText.gameObject.SetActive(false);
        if (upgradeMissingResourceText != null) upgradeMissingResourceText.gameObject.SetActive(false);

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
                Destroy(child.gameObject);
        }

        if (currentHouse.RecruitmentCostResource != null)
        {
            GameObject costGO = Instantiate(resourcePairPrefab, resourceCostParent);
            costGO.SetActive(true);

            PanelResourcePair panel = costGO.GetComponent<PanelResourcePair>();
            if (panel != null) 
                panel.Setup(currentHouse.RecruitmentCostResource, currentHouse.RecruitmentCostAmount);
        }
    }

    public bool IsShowingHouse(StructureWorkersHouse house) => IsOpen && currentHouse == house;

    private void UpdateActionButtonVisuals()
    {
        if (actionButton != null && currentHouse != null)
        {
            bool isHousingAction = currentHouse.IsReservedForHousing;

            if (resourceRequirementContainer != null) 
                resourceRequirementContainer.SetActive(!isHousingAction && !currentHouse.IsOccupied);

            actionButton.gameObject.SetActive(true);
            actionButton.interactable = !currentHouse.IsOccupied && !currentHouse.IsDestroying;

            if (actionButtonText != null)
            {
                if (currentHouse.IsOccupied)
                    actionButtonText.text = "Occupied";
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
                destroyButtonText.text = isDestroying ? "Cancel" : "Destroy Structure";

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
        if (upgradeCostParent == null || resourcePairPrefab == null || currentHouse == null || upgradeButton == null)
            return;

        StructureUpgrade upgrade = currentHouse.GetComponent<StructureUpgrade>();

        if (upgrade == null)
        {
            upgradeButton.gameObject.SetActive(false);
            return;
        }

        foreach (Transform child in upgradeCostParent) 
            Destroy(child.gameObject);

        int nextLevelIndex = upgrade.HouseLevelVal;

        if (nextLevelIndex >= upgrade.HouseLevels.Count)
        {
            upgradeButton.gameObject.SetActive(false);
            return;
        }

        StructureDataSO upgradeData = upgrade.HouseLevels[nextLevelIndex];

        if (upgradeData == null)
        {
            upgradeButton.gameObject.SetActive(false);
            return;
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
                panel.Setup(cost.resourceDataSo, cost.amount);
        }

        upgradeButton.gameObject.SetActive(true);
    }

    public void UpgradeHouseVisuals()
    {
        if (currentHouse == null)
            return;

        StructureUpgrade upgrade = currentHouse.GetComponent<StructureUpgrade>();
        if (upgrade == null) return;

        int nextLevelIndex = upgrade.HouseLevelVal;
        if (nextLevelIndex >= upgrade.HouseLevels.Count) return;
        StructureDataSO upgradeData = upgrade.HouseLevels[nextLevelIndex];

        // Check if player has enough resources for upgrade
        List<string> missingList = new List<string>();
        var resourceManager = GameManager.Instance.ResourceManager;

        if (upgradeData != null && resourceManager != null)
        {
            foreach (var cost in upgradeData.Costs)
            {
                if (cost.resourceDataSo == null) continue;
                int hasAmount = resourceManager.GetResourceAmount(cost.resourceDataSo);
                if (hasAmount < cost.amount)
                {
                    int needed = cost.amount - hasAmount;
                    missingList.Add($"Needs {needed} {cost.resourceDataSo.ResourceName}");
                }
            }
        }

        if (missingList.Count > 0)
        {
            string message = string.Join("\n", missingList);
            ShowUpgradeWarning(message);
            return;
        }

        bool upgraded = upgrade.TryUpgradeHouse();
        if (!upgraded) return;

        if (titleText != null)
            titleText.text = currentHouse.StructureTitle;

        BuildResourceCosts();
        UpdateUpgradeCostsVisuals();
        UpdateActionButtonVisuals();
        UpdateDestroyButtonVisuals();
    }

    private void OnActionClicked()
    {
        if (currentHouse != null)
        {
            // If already working, clicking toggles/stops it successfully without needing checks
            if (currentHouse.IsWorking)
            {
                currentHouse.ToggleHouseAction();
                return;
            }

            // If trying to start, check if requirements are met
            List<string> missingList = new List<string>();

            // Check recruitment resource cost if applicable
            if (!currentHouse.IsReservedForHousing && currentHouse.RecruitmentCostResource != null)
            {
                var resourceManager = GameManager.Instance.ResourceManager;
                if (resourceManager != null)
                {
                    int hasAmount = resourceManager.GetResourceAmount(currentHouse.RecruitmentCostResource);
                    if (hasAmount < currentHouse.RecruitmentCostAmount)
                    {
                        int needed = currentHouse.RecruitmentCostAmount - hasAmount;
                        missingList.Add($"Needs {needed} {currentHouse.RecruitmentCostResource.ResourceName}");
                    }
                }
            }

            if (missingList.Count > 0)
            {
                string message = string.Join("\n", missingList);
                ShowActionWarning(message);
                return;
            }

            currentHouse.ToggleHouseAction();
        }
    }

    private void ShowActionWarning(string message)
    {
        if (actionMissingResourceText != null)
        {
            if (actionWarningRoutine != null) StopCoroutine(actionWarningRoutine);
            actionWarningRoutine = StartCoroutine(FlashActionWarningRoutine(message));
        }
    }

    private IEnumerator FlashActionWarningRoutine(string message)
    {
        if (actionButtonText != null) actionButtonText.gameObject.SetActive(false);
        if (resourceRequirementContainer != null) resourceRequirementContainer.SetActive(false);

        actionMissingResourceText.gameObject.SetActive(true);
        actionMissingResourceText.text = message;

        yield return new WaitForSeconds(warningDisplayDuration);

        actionMissingResourceText.gameObject.SetActive(false);
        if (actionButtonText != null) actionButtonText.gameObject.SetActive(true);
        UpdateActionButtonVisuals(); // restores container correctly based on state
    }

    private void ShowUpgradeWarning(string message)
    {
        if (upgradeMissingResourceText != null)
        {
            if (upgradeWarningRoutine != null) StopCoroutine(upgradeWarningRoutine);
            upgradeWarningRoutine = StartCoroutine(FlashUpgradeWarningRoutine(message));
        }
    }

    private IEnumerator FlashUpgradeWarningRoutine(string message)
    {
        if (upgradeButtonText != null) upgradeButtonText.gameObject.SetActive(false);
        if (upgradeRequirementContainer != null) upgradeRequirementContainer.SetActive(false);

        upgradeMissingResourceText.gameObject.SetActive(true);
        upgradeMissingResourceText.text = message;

        yield return new WaitForSeconds(warningDisplayDuration);

        upgradeMissingResourceText.gameObject.SetActive(false);
        if (upgradeButtonText != null) upgradeButtonText.gameObject.SetActive(true);
        if (upgradeRequirementContainer != null) upgradeRequirementContainer.SetActive(true);
    }

    private void OnDestroyClicked()
    {
        if (currentHouse != null) 
            currentHouse.ToggleDestroying();
    }

    public void CompleteProgress()
    {
        if (productionBarContainer != null) productionBarContainer.SetActive(false);
    }
}