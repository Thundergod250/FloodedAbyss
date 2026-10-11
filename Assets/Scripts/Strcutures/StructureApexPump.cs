using UnityEngine;

public class StructureApexPump : Structures
{
    [Header("Pump Fuel & Resource Settings")]
    [SerializeField] private ResourceData[] acceptedResources; 
    [SerializeField] private int[] resourceFuelValues = { 1, 5, 10, 25, 50 }; 
    
    [Header("Switch Consumption Settings (Editable)")]
    [SerializeField] private int[] switchFuelCosts = { 1, 5, 10, 20, 50 }; 

    [Header("Debug Status")]
    [SerializeField] private int currentFuel = 0;
    [SerializeField] private bool[] switchStates = new bool[5]; 

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

        // Check if player has enough resource
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
    }

    public bool IsSwitchActive(int index) => index >= 0 && index < switchStates.Length && switchStates[index];
    public int CurrentFuel => currentFuel;
    public int GetSwitchCost(int index) => (index >= 0 && index < switchFuelCosts.Length) ? switchFuelCosts[index] : 0;
    public ResourceData[] AcceptedResources => acceptedResources;
}