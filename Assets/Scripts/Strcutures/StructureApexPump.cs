using UnityEngine;
using System.Collections;

public class StructureApexPump : Structures
{
    [Header("Pump Fuel & Resource Settings")]
    [SerializeField] private ResourceData[] acceptedResources; 
    [SerializeField] private int[] resourceFuelValues = { 1, 5, 10, 25, 50 }; 
    
    [Header("Switch Consumption Settings (Editable)")]
    [SerializeField] private int[] switchFuelCosts = { 1, 5, 10, 20, 50 }; 
    [SerializeField] private float consumptionInterval = 1f; // How often fuel is burned

    [Header("Water Drainage Settings")]
    [SerializeField] private float drainagePowerPerSwitch = 2f; // Drainage power added per active switch

    [Header("Debug Status")]
    [SerializeField] private int currentFuel = 0;
    [SerializeField] private bool[] switchStates = new bool[5]; 

    private Coroutine consumptionRoutine;

    private void Start()
    {
        consumptionRoutine = StartCoroutine(ConsumeFuelRoutine());
    }

    private void OnDisable()
    {
        if (consumptionRoutine != null)
        {
            StopCoroutine(consumptionRoutine);
        }
        
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

        ResourceData targetResource = acceptedResources[resourceIndex];
        if (targetResource == null) return false;

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

        Debug.Log($"[StructureApexPump] Converted {amount} {targetResource.ResourceName} into {fuelGained} Fuel. Total Fuel: {currentFuel}");
        return true;
    }

    public void ToggleSwitch(int switchIndex)
    {
        if (switchIndex < 0 || switchIndex >= switchStates.Length) return;

        switchStates[switchIndex] = !switchStates[switchIndex];
        Debug.Log($"[StructureApexPump] Switch {switchIndex + 1} is now {(switchStates[switchIndex] ? "ON (Green)" : "OFF (Red)")}");
        
        // Immediately recount active switches and update water drainage
        EvaluateAndApplyDrainage();
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
                    totalFuelCost += GetSwitchCost(i);
                    activeSwitchCount++;
                }
            }

            if (totalFuelCost > 0)
            {
                if (currentFuel >= totalFuelCost)
                {
                    currentFuel -= totalFuelCost;
                    Debug.Log($"[StructureApexPump] Consumed {totalFuelCost} fuel. Remaining Fuel: {currentFuel}");
                    
                    ApplyWaterDrainage(activeSwitchCount);
                }
                else
                {
                    Debug.LogWarning("[StructureApexPump] Out of fuel! Shutting down all switches and stopping water drainage.");
                    currentFuel = 0;
                    
                    for (int i = 0; i < switchStates.Length; i++)
                    {
                        switchStates[i] = false;
                    }
                    ClearDischargePower();
                }
            }
            else
            {
                ClearDischargePower();
            }
        }
    }

    private void EvaluateAndApplyDrainage()
    {
        int activeSwitchCount = 0;
        for (int i = 0; i < switchStates.Length; i++)
        {
            if (switchStates[i]) activeSwitchCount++;
        }

        if (currentFuel > 0 && activeSwitchCount > 0)
        {
            ApplyWaterDrainage(activeSwitchCount);
        }
        else
        {
            ClearDischargePower();
        }
    }

    private void ApplyWaterDrainage(int activeSwitchCount)
    {
        if (GameManager.Instance != null && GameManager.Instance.WaterLevel != null)
        {
            float totalDrainage = activeSwitchCount * drainagePowerPerSwitch;
            GameManager.Instance.WaterLevel.SetDischargePower(totalDrainage);
        }
    }

    private void ClearDischargePower()
    {
        if (GameManager.Instance != null && GameManager.Instance.WaterLevel != null)
        {
            GameManager.Instance.WaterLevel.SetDischargePower(0f);
        }
    }

    public bool IsSwitchActive(int index) => index >= 0 && index < switchStates.Length && switchStates[index];
    public int CurrentFuel => currentFuel;
    public int GetSwitchCost(int index) => (index >= 0 && index < switchFuelCosts.Length) ? switchFuelCosts[index] : 0;
    public ResourceData[] AcceptedResources => acceptedResources;
}