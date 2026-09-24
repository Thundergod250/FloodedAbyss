using UnityEngine;

public class HousingStructure : Item
{
    /*[Header("Housing Settings")]
    [SerializeField] private int housingCapacity = 3;

    [Header("Hiring Settings")]
    [SerializeField] private ResourceType hiringCostType = ResourceType.Gold;
    [SerializeField] private int hiringCost = 10;

    private void OnEnable()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.RegisterHousingCapacity(housingCapacity);
        }
    }

    private void OnDisable()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UnregisterHousingCapacity(housingCapacity);
        }
    }*/

    public override void Activate()
    {
        // Interacting directly buys an employee if under housing limit
        /*if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.TryHireEmployee(hiringCostType, hiringCost);
        }*/
    }
}