using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UiApexPump : UiModals
{
    [Header("UI Containers")]
    [SerializeField] private GameObject resourceButtonContainer;
    [SerializeField] private GameObject amountButtonContainer;
    [SerializeField] private Transform resourceRequirementContainer; // The orange bar container for upgrade costs[cite: 7, 8]
    [SerializeField] private GameObject resourcePairPrefabTemplate;   // Prefab with PanelResourcePair attached

    [Header("Display References")]
    [SerializeField] private TextMeshProUGUI currentFuelValueText;
    [SerializeField] private TextMeshProUGUI notEnoughResourceText;
    [SerializeField] private Button upgradeButton;                   // The upgrade button

    [Header("Interactive Buttons")]
    [SerializeField] private Button[] resourceButtons; 
    [SerializeField] private Button[] amountButtonInstances; 
    [SerializeField] private Button[] switchButtons; 

    private StructureApexPump currentPump;
    private int selectedResourceIndex = -1;
    private Coroutine warningRoutine;
    private List<GameObject> activeRequirementPanels = new();

    protected override void Initialize()
    {
        base.Initialize();

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);

        if (resourcePairPrefabTemplate != null)
            resourcePairPrefabTemplate.SetActive(false);

        // Setup Resource Buttons
        for (int i = 0; i < resourceButtons.Length; i++)
        {
            int index = i;
            if (resourceButtons[i] != null)
                resourceButtons[i].onClick.AddListener(() => OnResourceSelected(index));
        }

        // Setup Amount Buttons
        int[] amounts = { 10, 25, 50, 100 };
        for (int i = 0; i < amountButtonInstances.Length; i++)
        {
            int amount = amounts[i];
            if (amountButtonInstances[i] != null)
                amountButtonInstances[i].onClick.AddListener(() => OnAmountSelected(amount));
        }

        // Setup Switch Buttons
        for (int i = 0; i < switchButtons.Length; i++)
        {
            int index = i;
            if (switchButtons[i] != null)
                switchButtons[i].onClick.AddListener(() => OnSwitchClicked(index));
        }

        // Setup Upgrade Button
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
    }

    private void Update()
    {
        if (IsOpen && currentPump != null)
        {
            if (currentFuelValueText != null)
                currentFuelValueText.text = currentPump.CurrentFuel.ToString();

            // Refresh Switch Colors
            for (int i = 0; i < switchButtons.Length; i++)
            {
                if (switchButtons[i] != null)
                {
                    ColorBlock colors = switchButtons[i].colors;
                    Color targetColor = currentPump.IsSwitchActive(i) ? Color.green : Color.red;
                    
                    colors.normalColor = targetColor;
                    colors.highlightedColor = targetColor;
                    colors.pressedColor = targetColor;
                    colors.selectedColor = targetColor;
                    colors.disabledColor = targetColor;
                    
                    switchButtons[i].colors = colors;
                }
            }
        }
    }

    public void Setup(StructureApexPump pump)
    {
        currentPump = pump;
        selectedResourceIndex = -1;

        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(false);

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);

        RefreshResourceButtonVisuals();
        RefreshUpgradeRequirementsUI();
    }

    private void RefreshUpgradeRequirementsUI()
    {
        if (resourceRequirementContainer == null || resourcePairPrefabTemplate == null || currentPump == null) return;

        // Clear previous requirement panels
        foreach (var panel in activeRequirementPanels)
        {
            if (panel != null) Destroy(panel);
        }
        activeRequirementPanels.Clear();

        if (currentPump.IsMaxLevel)
        {
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }

        if (upgradeButton != null) upgradeButton.interactable = true;

        var nextUpgrade = currentPump.GetNextUpgradeData();
        if (nextUpgrade.upgradeCosts != null)
        {
            foreach (var cost in nextUpgrade.upgradeCosts)
            {
                if (cost.resourceDataSo == null) continue;

                GameObject itemGO = Instantiate(resourcePairPrefabTemplate, resourceRequirementContainer);
                itemGO.SetActive(true);

                PanelResourcePair panel = itemGO.GetComponent<PanelResourcePair>();
                if (panel != null)
                {
                    // Pass cost.amount so it displays the exact required upgrade cost (e.g., 2)
                    panel.Setup(cost.resourceDataSo, cost.amount);
                }

                activeRequirementPanels.Add(itemGO);
            }
        }
    }

    private void OnUpgradeClicked()
    {
        if (currentPump != null)
        {
            // Attempt the upgrade and capture the error message
            bool success = currentPump.TryUpgradePump(out string errorMsg);
            if (success)
            {
                Debug.Log("[UiApexPump] Upgrade successful! Refreshing UI.");
                RefreshUpgradeRequirementsUI();
            }
            else
            {
                // Debug log to console so you can inspect it immediately
                Debug.LogWarning($"[UiApexPump] Upgrade failed: {errorMsg}");
                
                // Fallback text if errorMsg is empty
                string displayMessage = string.IsNullOrEmpty(errorMsg) ? "Not Enough Resources for Upgrade!" : errorMsg;
                ShowNotEnoughResourceWarning(displayMessage);
            }
        }
    }

    private void OnResourceSelected(int resourceIndex)
    {
        selectedResourceIndex = resourceIndex;

        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(true);

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);

        RefreshResourceButtonVisuals();
    }

    private void RefreshResourceButtonVisuals()
    {
        for (int i = 0; i < resourceButtons.Length; i++)
        {
            if (resourceButtons[i] == null) continue;

            ColorBlock colors = resourceButtons[i].colors;

            if (selectedResourceIndex == -1)
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
                colors.selectedColor = Color.white;
            }
            else if (i == selectedResourceIndex)
            {
                colors.normalColor = Color.yellow;
                colors.highlightedColor = Color.yellow;
                colors.pressedColor = Color.yellow;
                colors.selectedColor = Color.yellow;
            }
            else
            {
                Color fadedColor = new Color(1f, 1f, 1f, 0.4f);
                colors.normalColor = fadedColor;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.6f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.3f);
                colors.selectedColor = fadedColor;
            }

            resourceButtons[i].colors = colors;
        }
    }

    private void OnAmountSelected(int amount)
    {
        if (currentPump != null && selectedResourceIndex != -1)
        {
            bool success = currentPump.ConvertResourceToFuel(selectedResourceIndex, amount);
            if (!success)
            {
                ShowNotEnoughResourceWarning("Not Enough Resource");
            }
        }
    }

    private void ShowNotEnoughResourceWarning(string message)
    {
        if (notEnoughResourceText != null)
        {
            notEnoughResourceText.text = message;
            if (warningRoutine != null) StopCoroutine(warningRoutine);
            warningRoutine = StartCoroutine(FlashWarningRoutine());
        }
    }

    private IEnumerator FlashWarningRoutine()
    {
        notEnoughResourceText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        notEnoughResourceText.gameObject.SetActive(false);
    }

    private void OnSwitchClicked(int switchIndex)
    {
        if (currentPump != null)
        {
            currentPump.ToggleSwitch(switchIndex);
        }
    }
}