using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ResourceType
{
    Stone,
    Wood,
    Gold,
    Food,
    Tin,
    Copper,
    Energy
}

public class PlayerResources : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent<ResourceType, int> EvtOnResourceChanged;

    [Header("Notification Settings")]
    [SerializeField] private Color gainColor = Color.green;
    [SerializeField] private Color spendColor = Color.red;
    
    private Dictionary<ResourceType, int> resources = new();

    private void Start() => InitializeResources();

    private void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType))) 
            resources[type] = 0;
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0) return;
        resources[type] += amount;
        Debug.Log($"{type} increased by {amount}. Total: {resources[type]}");
        EvtOnResourceChanged?.Invoke(type, resources[type]);
        Notification.Display($"+{amount} {type}", gainColor);
    }

    /// <summary>
    /// Spends a single resource type if available.
    /// </summary>
    public bool SpendResource(ResourceType type, int amount)
    {
        if (amount <= 0) return true;

        if (resources[type] >= amount)
        {
            resources[type] -= amount;
            Debug.Log($"{type} decreased by {amount}. Total: {resources[type]}");
            EvtOnResourceChanged?.Invoke(type, resources[type]);
            Notification.Display($"-{amount} {type}", spendColor);
            return true;
        }

        Debug.LogWarning($"Not enough {type}!");
        return false;
    }

    /// <summary>
    /// Alternate function to spend a single resource (alias for SpendResource).
    /// </summary>
    public bool TrySpendResource(ResourceType type, int amount)
    {
        return SpendResource(type, amount);
    }

    /// <summary>
    /// Safely attempts to get the current quantity of a resource.
    /// Returns true if the player holds at least 1 of the requested resource.
    /// </summary>
    public bool TryGetResource(ResourceType type, out int currentAmount)
    {
        currentAmount = GetResource(type);
        return currentAmount > 0;
    }

    /// <summary>
    /// Checks if the player has at least the required amount of a resource.
    /// </summary>
    public bool HasResource(ResourceType type, int requiredAmount)
    {
        return GetResource(type) >= requiredAmount;
    }

    public int GetResource(ResourceType type) => resources.TryGetValue(type, out int amount) ? amount : 0;

    public void AddTenToAllResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType))) 
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
            Notification.ShowWarning("Not enough resources!");
            return false;
        }

        if (requirements != null)
        {
            foreach (var req in requirements) 
                SpendResource(req.resourceType, req.amount);
        }

        return true;
    }
    
    public void ApplyResourceDeathPenalty(float penaltyRatio = 0.5f)
    {
        penaltyRatio = Mathf.Clamp01(penaltyRatio);
        bool lostAny = false;

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            int currentAmount = GetResource(type);
            if (currentAmount <= 0) 
                continue;
            int amountToLose = Mathf.FloorToInt(currentAmount * penaltyRatio);
            if (amountToLose > 0)
            {
                resources[type] -= amountToLose;
                EvtOnResourceChanged?.Invoke(type, resources[type]);
                lostAny = true;
            }
        }

        if (lostAny) 
            Notification.Display($"Lost {penaltyRatio * 100}% of resources on death!", spendColor);
    }
}