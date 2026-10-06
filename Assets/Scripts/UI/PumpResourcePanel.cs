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
    [SerializeField] private Button btnDepositTen;
    [SerializeField] private Button upgradeButton;

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

    public event Action<PumpResourcePanel, bool> OnActiveStateChanged;

    public ResourceType ResourceType => resourceType;
    public int CurrentEnergy => currentEnergy;
    public bool IsActive => currentEnergy > 0;

    public void Initialize(PlayerResources resources)
    {
        playerResources = resources;

        if (btnDepositOne != null) btnDepositOne.onClick.AddListener(() => DepositResource(1));
        if (btnDepositTen != null) btnDepositTen.onClick.AddListener(() => DepositResource(10));
        if (upgradeButton != null) upgradeButton.onClick.AddListener(HandleUpgrade);

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

        if (playerResources.GetResource(resourceType) >= amount)
        {
            if (playerResources.SpendResource(resourceType, amount))
            {
                depositedResource += amount;
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
                    Debug.Log($"[{resourceType} Pump] is now ACTIVE!");
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
                    Debug.Log($"[{resourceType} Pump] is now INACTIVE!");
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
        if (txtPumpLevel != null) txtPumpLevel.text = "Pump Level: " + upgradeLevel.ToString();
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

        Debug.Log($"[{resourceType} Pump] upgraded to level {upgradeLevel}. Conversion: {convertInterval}s, Decay: {energyDecayInterval}s");
    }

    private void OnDestroy()
    {
        if (btnDepositOne != null) btnDepositOne.onClick.RemoveAllListeners();
        if (btnDepositTen != null) btnDepositTen.onClick.RemoveAllListeners();
        if (upgradeButton != null) upgradeButton.onClick.RemoveAllListeners();
    }
}