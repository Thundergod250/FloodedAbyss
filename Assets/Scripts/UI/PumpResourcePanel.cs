using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PumpResourcePanel : MonoBehaviour
{
    [Header("Resource Config")]
    [SerializeField] private ResourceType resourceType;

    [Header("UI Controls")]
    [SerializeField] private Button btnDepositOne;
    [SerializeField] private Button btnDepositMinusOne;
    [SerializeField] private Button btnDepositTen;
    [SerializeField] private Button btnDepositMinusTen;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button thresholdUpgradeButton;

    [Header("UI Displays")]
    [SerializeField] private TMP_Text txtResourceDeposited;
    [SerializeField] private TMP_Text txtEnergyValue;
    [SerializeField] private TMP_Text txtUpgradeResourcesRequired;
    [SerializeField] private TMP_Text txtPumpLevel;

    [Header("Pump Settings")]
    [SerializeField] private float convertInterval = 2f;
    [SerializeField] private float energyDecayInterval = 5f;

    [Header("Upgrade Pump")]
    [SerializeField] private int resourcesRequired = 10;
    [SerializeField] private float convertIntervalReduction = 0.2f;
    [SerializeField] private float energyDecayIntervalIncrease = 0.5f;
    [SerializeField] private int upgradeCostIncrease = 5;
    [SerializeField] private int maxUpgradeLevel = 3;

    private int upgradeLevel = 0;
    private int depositedResource = 0;
    private int currentEnergy = 0;

    private float convertTimer = 0f;
    private float decayTimer = 0f;

    private PlayerResources playerResources;
    private UiApexPump parentUiApexPump;

    public event Action<PumpResourcePanel, bool> OnActiveStateChanged;

    public ResourceType ResourceType => resourceType;
    public int CurrentEnergy => currentEnergy;
    public bool IsActive => currentEnergy > 0;

    public void Initialize(PlayerResources resources)
    {
        playerResources = resources;
        parentUiApexPump = GetComponentInParent<UiApexPump>();

        if (btnDepositOne != null) btnDepositOne.onClick.AddListener(() => DepositResource(1));
        if (btnDepositMinusOne != null) btnDepositMinusOne.onClick.AddListener(() => DepositResource(-1));
        if (btnDepositTen != null) btnDepositTen.onClick.AddListener(() => DepositResource(10));
        if (btnDepositMinusTen != null) btnDepositMinusTen.onClick.AddListener(() => DepositResource(-10));
        if (upgradeButton != null) upgradeButton.onClick.AddListener(HandleUpgrade);
        if (thresholdUpgradeButton != null) thresholdUpgradeButton.onClick.AddListener(HandleThresholdUpgrade);

        UpdateUI();
    }

    private void Update()
    {
        HandleConversion();
        HandleEnergyDecay();
    }

    private void DepositResource(int amount)
    {
        if (playerResources == null) return;

        if (amount < 0)
        {
            int amountToReturn = Mathf.Min(-amount, depositedResource);

            if (amountToReturn > 0)
            {
                depositedResource -= amountToReturn;
                playerResources.AddResource(resourceType, amountToReturn);
            }

            UpdateUI();
            return;
        }

        if (playerResources.GetResource(resourceType) >= amount)
        {
            if (playerResources.SpendResource(resourceType, amount))
            {
                bool wasEmpty = depositedResource <= 0;

                depositedResource += amount;

                if (wasEmpty)
                {
                    StartWaterForecast();
                }

                UpdateUI();
            }
        }
        else
        {
            Debug.LogWarning($"Not enough {resourceType} to deposit.");
        }
    }

    private void HandleConversion()
    {
        if (depositedResource > 0)
        {
            convertTimer += Time.deltaTime;
            if (convertTimer >= convertInterval)
            {
                convertTimer = 0f;
                depositedResource--;

                bool wasActiveBefore = IsActive;
                currentEnergy++;

                if (!wasActiveBefore && IsActive)
                {
                    OnActiveStateChanged?.Invoke(this, true);
                }

                UpdateUI();
            }
        }
        else
        {
            convertTimer = 0f;
        }
    }

    private void HandleEnergyDecay()
    {
        if (currentEnergy > 0)
        {
            decayTimer += Time.deltaTime;
            if (decayTimer >= energyDecayInterval)
            {
                decayTimer = 0f;
                bool wasActiveBefore = IsActive;

                currentEnergy--;

                if (wasActiveBefore && !IsActive)
                {
                    OnActiveStateChanged?.Invoke(this, false);
                }

                UpdateUI();
            }
        }
        else
        {
            decayTimer = 0f;
        }
    }

    private void UpdateUI()
    {
        if (txtResourceDeposited != null) txtResourceDeposited.text = depositedResource.ToString();
        if (txtEnergyValue != null) txtEnergyValue.text = currentEnergy.ToString();
        if (txtUpgradeResourcesRequired != null) txtUpgradeResourcesRequired.text = resourcesRequired.ToString() + " " + resourceType.ToString();
        if (txtPumpLevel != null) txtPumpLevel.text = "Level: " + upgradeLevel.ToString();
    }

    private void HandleUpgrade()
    {
        if (playerResources == null) return;

        if (upgradeLevel >= maxUpgradeLevel)
        {
            if (upgradeButton != null)
            {
                var img = upgradeButton.GetComponent<Image>();
                if (img != null) img.color = Color.gray;
                upgradeButton.onClick.RemoveListener(HandleUpgrade);
                upgradeButton.enabled = false;
            }
            resourcesRequired = 0;
            UpdateUI();
            return;
        }

        if (playerResources.GetResource(resourceType) < resourcesRequired)
        {
            Debug.LogWarning($"Not enough {resourceType} to upgrade.");
            return;
        }

        if (!playerResources.SpendResource(resourceType, resourcesRequired)) return;

        upgradeLevel++;

        convertInterval = Mathf.Max(0.1f, convertInterval - convertIntervalReduction);
        energyDecayInterval += energyDecayIntervalIncrease;
        resourcesRequired += upgradeCostIncrease;

        UpdateUI();
    }

    public void HandleThresholdUpgrade()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerController == null) return;

        PlayerStats stats = GameManager.Instance.playerController.GetComponent<PlayerStats>();
        if (stats == null) return;

        // Route the threshold upgrade directly through PlayerStats so it integrates into your central hub
        switch (resourceType)
        {
            case ResourceType.Wood: stats.UpgradeWoodPumpThreshold(); break;
            case ResourceType.Stone: stats.UpgradeStonePumpThreshold(); break;
            case ResourceType.Copper: stats.UpgradeCopperPumpThreshold(); break;
            case ResourceType.Iron: stats.UpgradeIronPumpThreshold(); break;
            case ResourceType.Gold: stats.UpgradeGoldPumpThreshold(); break;
        }

        // Recalculate water levels using the active panel list
        if (GameManager.Instance.ApexPump != null && parentUiApexPump != null)
        {
            GameManager.Instance.ApexPump.RecalculateWaterLevelBasedOnPanels(parentUiApexPump.ResourcePanels);
        }
    }

    private void OnDestroy()
    {
        if (btnDepositOne != null) btnDepositOne.onClick.RemoveAllListeners();
        if (btnDepositTen != null) btnDepositTen.onClick.RemoveAllListeners();
        if (upgradeButton != null) upgradeButton.onClick.RemoveAllListeners();
        if (thresholdUpgradeButton != null) thresholdUpgradeButton.onClick.RemoveAllListeners();
    }

    private void StartWaterForecast()
    {
        if (GameManager.Instance == null)
            return;

        WaterForecast waterForecast = GameManager.Instance.uiController.GetComponentInChildren<WaterForecast>();

        if (waterForecast == null)
        {
            Debug.LogWarning("WaterForecast is not available.");
            return;
        }

        waterForecast.EnableTimeline();
    }
}