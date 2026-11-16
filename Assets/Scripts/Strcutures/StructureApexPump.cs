using UnityEngine;
using System.Collections;

[System.Serializable]
public struct PumpResourceCost
{
    public ResourceDataSO resourceDataSo;
    public int amount;
}

[System.Serializable]
public struct PumpUpgradeData
{
    [Header("Upgrade Cost")]
    public PumpResourceCost[] upgradeCosts;

    [Header("Power Boost")]
    public float drainagePowerPerSwitch;
}

public class StructureApexPump : Structures
{
    // PLACEHOLDER: Replace "SFXDestruction" with the real pump-start clip name
    // once the audio asset exists. Uses the same SFX key convention as
    // WaterForecast ("SFXRaining") so it resolves against the SoundManager child
    // GameObject of the same name.
    private const string PumpStartSFXName = "SFXDestruction";

    [Header("Pump Fuel & Resource Settings")]
    [SerializeField] private ResourceDataSO[] acceptedResources;
    [SerializeField] private int[] resourceFuelValues = { 1, 5, 10, 25, 50 };

    [Header("Switch Consumption Settings")]
    [SerializeField] private int[] switchFuelCosts = { 1, 5, 10, 20, 50 };
    [SerializeField] private float consumptionInterval = 1f;

    [Header("Switch Activation Requirements (Min Fuel to Open)")]
    [SerializeField] private int[] switchOpenThresholds = { 0, 50, 100, 500, 1000 };

    [Header("Switch Safety Shutdown Thresholds (Auto-Off Cutoff)")]
    [SerializeField] private int[] switchShutdownThresholds = { 0, 25, 50, 250, 500 };

    [Header("Discharge Pressure Upgrades")]
    [SerializeField] private PumpUpgradeData[] upgradeLevels;
    [SerializeField] private float defaultDrainagePowerPerSwitch = 50f;

    [Header("Great Flood Fuel Settings")]
    [SerializeField] private float greatFloodFuelMultiplier = 2f;

    [Header("Debug Status")]
    [SerializeField] private int currentFuel = 0;
    [SerializeField] private int currentUpgradeLevel = 0;
    [SerializeField] private bool[] switchStates = new bool[5];
    [SerializeField] private float estimatedRemainingTime = 0f;

    private Coroutine consumptionRoutine;

    private void Start() => consumptionRoutine = StartCoroutine(ConsumeFuelRoutine());

    private void Update()
    {
        estimatedRemainingTime = CalculatedEstimatedRemainingTime;
    }

    private void OnDisable()
    {
        if (consumptionRoutine != null)
            StopCoroutine(consumptionRoutine);
        ClearDischargePower();
    }

    public override void Activate()
    {
        var apexUI = GameManager.Instance.uiController.UiApexPump;
        if (apexUI != null)
        {
            apexUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiApexPump>();
        }
    }

    public bool ConvertResourceToFuel(int resourceIndex, int amount)
    {
        if (GameManager.Instance == null || GameManager.Instance.ResourceManager == null) return false;
        if (acceptedResources == null || resourceIndex < 0 || resourceIndex >= acceptedResources.Length) return false;

        ResourceDataSO targetResource = acceptedResources[resourceIndex];
        if (targetResource == null) return false;
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.NotifyResourceDeposited(
                targetResource,
                amount
            );
        }
        int availableAmount = GameManager.Instance.ResourceManager.GetResourceAmount(targetResource);
        if (availableAmount < amount)
        {
            Debug.LogWarning($"[StructureApexPump] Not enough {targetResource.ResourceName}! Player has {availableAmount}, tried to consume {amount}.");
            return false;
        }

        bool success = GameManager.Instance.ResourceManager.ConsumeResource(targetResource, amount);
        if (!success) return false;

        int fuelGained = amount * resourceFuelValues[resourceIndex];
        currentFuel += fuelGained;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.NotifyResourceDeposited(
                targetResource,
                amount
            );
        }

        Debug.Log($"[StructureApexPump] Converted {amount} {targetResource.ResourceName} into {fuelGained} Fuel. Total Fuel: {currentFuel}");
        return true;
    }

    public bool TryUpgradePump(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (upgradeLevels == null || currentUpgradeLevel >= upgradeLevels.Length)
        {
            errorMessage = "Pump is already at max upgrade level!";
            return false;
        }

        var nextUpgrade = upgradeLevels[currentUpgradeLevel];
        var resourceManager = GameManager.Instance.ResourceManager;

        if (resourceManager == null)
        {
            errorMessage = "Resource manager not found.";
            return false;
        }

        if (nextUpgrade.upgradeCosts != null)
        {
            foreach (var cost in nextUpgrade.upgradeCosts)
            {
                if (cost.resourceDataSo == null) continue;
                int currentAmount = resourceManager.GetResourceAmount(cost.resourceDataSo);
                if (currentAmount < cost.amount)
                {
                    errorMessage = $"Not enough {cost.resourceDataSo.ResourceName}! Need {cost.amount}, have {currentAmount}.";
                    return false;
                }
            }

            foreach (var cost in nextUpgrade.upgradeCosts)
            {
                if (cost.resourceDataSo == null) continue;
                resourceManager.ConsumeResource(cost.resourceDataSo, cost.amount);
            }
        }

        currentUpgradeLevel++;
        Debug.Log($"[StructureApexPump] Pump successfully upgraded to Tier {currentUpgradeLevel}!");

        EvaluateAndApplyDrainage();
        return true;
    }

    public bool ToggleSwitch(int switchIndex, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (switchIndex < 0 || switchIndex >= switchStates.Length) return false;

        if (!switchStates[switchIndex])
        {
            int requiredThreshold = GetSwitchOpenThreshold(switchIndex);
            if (currentFuel < requiredThreshold)
            {
                errorMessage = $"Cannot activate Switch {switchIndex + 1}! Requires at least {requiredThreshold} Fuel to open (Current: {currentFuel}).";
                Debug.LogWarning($"[StructureApexPump] {errorMessage}");
                return false;
            }
        }

        switchStates[switchIndex] = !switchStates[switchIndex];
        Debug.Log($"[StructureApexPump] Switch {switchIndex + 1} is now {(switchStates[switchIndex] ? "ON (Green)" : "OFF (Red)")}");

        if (switchStates[switchIndex])
            NotifyPumpActivated();

        EvaluateAndApplyDrainage();
        return true;
    }

    private IEnumerator ConsumeFuelRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(consumptionInterval);

            int totalFuelCost = 0;
            int activeSwitchCount = 0;

            for (int i = 0; i < switchStates.Length; i++)
            {
                if (switchStates[i])
                {
                    int shutdownLimit = GetSwitchShutdownThreshold(i);
                    if (currentFuel < shutdownLimit)
                    {
                        Debug.LogWarning($"[StructureApexPump] Fuel dropped below shutdown threshold ({shutdownLimit}) for Switch {i + 1}. Shutting it down.");
                        switchStates[i] = false;
                        continue;
                    }

                    totalFuelCost += GetAdjustedFuelCost(i);
                    activeSwitchCount++;
                }
            }

            if (totalFuelCost > 0)
            {
                if (currentFuel >= totalFuelCost)
                {
                    currentFuel -= totalFuelCost;
                    ApplyWaterDrainage(activeSwitchCount);
                }
                else
                {
                    Debug.LogWarning("[StructureApexPump] Out of fuel! Shutting down all switches.");
                    currentFuel = 0;
                    for (int i = 0; i < switchStates.Length; i++)
                        switchStates[i] = false;
                    ClearDischargePower();
                }
            }
            else
                ClearDischargePower();
        }
    }

    private void EvaluateAndApplyDrainage()
    {
        int activeSwitchCount = 0;
        for (int i = 0; i < switchStates.Length; i++)
        {
            if (switchStates[i])
            {
                int shutdownLimit = GetSwitchShutdownThreshold(i);
                if (currentFuel < shutdownLimit)
                {
                    switchStates[i] = false;
                    continue;
                }
                activeSwitchCount++;
            }
        }

        if (currentFuel > 0 && activeSwitchCount > 0)
            ApplyWaterDrainage(activeSwitchCount);
        else
            ClearDischargePower();
    }

    private void ApplyWaterDrainage(int activeSwitchCount)
    {
        if (GameManager.Instance != null && GameManager.Instance.WaterLevel != null)
        {
            float powerPerSwitch = defaultDrainagePowerPerSwitch;
            if (upgradeLevels != null && currentUpgradeLevel > 0 && currentUpgradeLevel <= upgradeLevels.Length)
                powerPerSwitch = upgradeLevels[currentUpgradeLevel - 1].drainagePowerPerSwitch;

            float totalDrainage = activeSwitchCount * powerPerSwitch;
            GameManager.Instance.WaterLevel.SetDischargePower(totalDrainage);
        }
    }

    private void ClearDischargePower()
    {
        if (GameManager.Instance != null && GameManager.Instance.WaterLevel != null)
            GameManager.Instance.WaterLevel.SetDischargePower(0f);
    }

    private int GetAdjustedFuelCost(int switchIndex)
    {
        int baseCost = GetSwitchCost(switchIndex);

        if (GameManager.Instance != null &&
            GameManager.Instance.WaterLevel != null &&
            GameManager.Instance.WaterLevel.GreatFloodStarted)
        {
            return Mathf.CeilToInt(baseCost * greatFloodFuelMultiplier);
        }

        return baseCost;
    }

    public PumpUpgradeData GetNextUpgradeData()
    {
        if (upgradeLevels != null && currentUpgradeLevel < upgradeLevels.Length)
            return upgradeLevels[currentUpgradeLevel];
        return default;
    }

    public float CalculatedEstimatedRemainingTime
    {
        get
        {
            int totalCostPerInterval = 0;
            for (int i = 0; i < switchStates.Length; i++)
            {
                if (switchStates[i])
                    totalCostPerInterval += GetAdjustedFuelCost(i);
            }

            if (totalCostPerInterval <= 0 || currentFuel <= 0 || consumptionInterval <= 0f)
                return 0f;

            float burnRatePerSecond = (float)totalCostPerInterval / consumptionInterval;
            return currentFuel / burnRatePerSecond;
        }
    }

    public bool IsMaxLevel => upgradeLevels == null || currentUpgradeLevel >= upgradeLevels.Length;
    public bool IsSwitchActive(int index) => index >= 0 && index < switchStates.Length && switchStates[index];
    public int CurrentFuel => currentFuel;
    public int CurrentUpgradeLevel => currentUpgradeLevel;
    public int GetSwitchCost(int index) => (index >= 0 && index < switchFuelCosts.Length) ? switchFuelCosts[index] : 0;

    public int GetSwitchOpenThreshold(int index) => (index >= 0 && index < switchOpenThresholds.Length) ? switchOpenThresholds[index] : 0;
    public int GetSwitchShutdownThreshold(int index) => (index >= 0 && index < switchShutdownThresholds.Length) ? switchShutdownThresholds[index] : 0;

    public ResourceDataSO[] AcceptedResources => acceptedResources;

    public int ActiveSwitchCount
    {
        get
        {
            int count = 0;

            foreach (bool isActive in switchStates)
            {
                if (isActive)
                    count++;
            }

            return count;
        }
    }

    public bool IsPumpActive
    {
        get
        {
            for (int i = 0; i < switchStates.Length; i++)
            {
                if (switchStates[i])
                    return true;
            }
            return false;
        }
    }

    public void NotifyPumpActivated()
    {
        // ── Pump start SFX (placeholder: destruction clip) ─────────────
        // Fires only when the pump goes from inactive (0 switches) to
        // active (1 switch). Turning on additional switches after that
        // does NOT re-trigger — the pump was already running.
        //
        // To play on EVERY switch activation instead, remove the
        // `ActiveSwitchCount == 1 &&` part of the guard below.
        if (ActiveSwitchCount == 1 &&
            GameManager.Instance != null &&
            GameManager.Instance.SoundManager != null)
        {
            GameManager.Instance.SoundManager.Play(PumpStartSFXName);
        }
        // ─────────────────────────────────────────────────────────────

        if (ObjectiveManager.Instance != null)
            ObjectiveManager.Instance.NotifyPumpActivated(this);
    }
}