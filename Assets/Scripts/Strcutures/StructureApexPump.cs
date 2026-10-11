using UnityEngine;

public class StructureApexPump : Structures
{
    [Header("Pump Fuel & Resource Settings")]
    [SerializeField] private ResourceData[] acceptedResources; // Match your 5 resources
    [SerializeField] private int[] resourceFuelValues = { 1, 5, 10, 25, 50 }; // Editable fuel value per material
    
    [Header("Switch Consumption Settings (Editable)")]
    [SerializeField] private int[] switchFuelCosts = { 1, 5, 10, 20, 50 }; // Costs per switch level

    [Header("Debug Status")]
    [SerializeField] private int currentFuel = 0;
    [SerializeField] private bool[] switchStates = new bool[5]; // 5 switches

    public override void Activate()
    {
        var apexUI = GameManager.Instance.uiController.UiApexPump;
        if (apexUI != null)
        {
            apexUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiApexPump>();
        }
    }

    public void ConvertResourceToFuel(int resourceIndex, int amount)
    {
        if (GameManager.Instance == null || GameManager.Instance.ResourceManager == null) return;
        if (acceptedResources == null || resourceIndex < 0 || resourceIndex >= acceptedResources.Length) return;

        ResourceData targetResource = acceptedResources[resourceIndex];
        if (targetResource == null) return;

        // Check and consume resource using your ResourceManager's ConsumeResource method
        bool success = GameManager.Instance.ResourceManager.ConsumeResource(targetResource, amount);
        if (!success)
        {
            Debug.LogWarning($"[StructureApexPump] Not enough {targetResource.ResourceName} to consume {amount}!");
            return;
        }

        // Add to fuel
        int fuelGained = amount * resourceFuelValues[resourceIndex];
        currentFuel += fuelGained;

        Debug.Log($"[StructureApexPump] Converted {amount} {targetResource.ResourceName} into {fuelGained} Fuel. Total Fuel: {currentFuel}");
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