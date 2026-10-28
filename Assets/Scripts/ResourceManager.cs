using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Register All Game Resources Here")]
    [SerializeField] private List<ResourceDataSO> availableResources = new();

    private Dictionary<ResourceDataSO, int> resourceInventory = new();

    // Event fired whenever any resource amount changes (ResourceData, newAmount)
    public event Action<ResourceDataSO, int> OnResourceUpdated;

    public List<ResourceDataSO> AvailableResources => availableResources;

    private void Awake()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        resourceInventory.Clear();
        foreach (var data in availableResources)
        {
            if (data != null && !resourceInventory.ContainsKey(data))
            {
                resourceInventory.Add(data, data.StartingAmount);
            }
        }
    }

    public int GetResourceAmount(ResourceDataSO dataSo)
    {
        if (resourceInventory.TryGetValue(dataSo, out int amount)) return amount;
        return 0;
    }

    public void AddResource(ResourceDataSO dataSo, int amount)
    {
        if (resourceInventory.ContainsKey(dataSo))
        {
            resourceInventory[dataSo] += amount;
        }
        else
        {
            resourceInventory.Add(dataSo, amount);
        }
        Debug.Log($"[ResourceManager] Added {amount} of {dataSo.ResourceName}. Total: {resourceInventory[dataSo]}");
        
        // Notify HUD and any listeners
        OnResourceUpdated?.Invoke(dataSo, resourceInventory[dataSo]);

        // NEW: Notify the objective system of resources gathered.
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.NotifyResourceGathered(dataSo, amount);
        }
    }

    public bool ConsumeResource(ResourceDataSO dataSo, int amount)
    {
        if (resourceInventory.ContainsKey(dataSo) && resourceInventory[dataSo] >= amount)
        {
            resourceInventory[dataSo] -= amount;
            
            // Notify HUD and any listeners
            OnResourceUpdated?.Invoke(dataSo, resourceInventory[dataSo]);
            return true;
        }
        return false;
    }
}