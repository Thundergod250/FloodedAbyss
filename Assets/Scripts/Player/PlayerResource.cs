using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ResourceType
{
    Stone,
    Wood,
    Gold,
    Food
}

public class PlayerResources : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent<ResourceType, int> EvtOnResourceChanged;
    
    private Dictionary<ResourceType, int> resources = new();

    private void Awake() => InitializeResources();

    private void InitializeResources()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType))) 
            resources[type] = 0;
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
        Debug.Log($"{type} increased by {amount}. Total: {resources[type]}");
        
        EvtOnResourceChanged?.Invoke(type, resources[type]);
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (resources[type] >= amount)
        {
            resources[type] -= amount;
            Debug.Log($"{type} decreased by {amount}. Total: {resources[type]}");
            
            EvtOnResourceChanged?.Invoke(type, resources[type]);
            return true;
        }

        Debug.LogWarning($"Not enough {type}!");
        return false;
    }

    public int GetResource(ResourceType type) => resources.TryGetValue(type, out int amount) ? amount : 0;

    public void AddTenToAllResources()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType))) 
            AddResource(type, 10);
    }
    
    public bool CanAfford(IReadOnlyList<StructureDataSO.ResourceRequirement> requirements)
    {
        if (requirements == null) return true;

        foreach (var req in requirements)
        {
            if (GetResource(req.resourceType) < req.amount)
                return false;
        }
        return true;
    }
    
    public bool TrySpendResources(IReadOnlyList<StructureDataSO.ResourceRequirement> requirements)
    {
        if (!CanAfford(requirements))
        {
            Debug.LogWarning("Transaction failed: Insufficient resources!");
            return false;
        }

        if (requirements != null)
        {
            foreach (var req in requirements) 
                SpendResource(req.resourceType, req.amount);
        }

        return true;
    }
}