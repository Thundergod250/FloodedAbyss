using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Register All Game Resources Here")]
    [SerializeField] private List<ResourceData> availableResources = new();

    private Dictionary<ResourceData, int> resourceInventory = new();

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

    public int GetResourceAmount(ResourceData data)
    {
        if (resourceInventory.TryGetValue(data, out int amount)) return amount;
        return 0;
    }

    public void AddResource(ResourceData data, int amount)
    {
        if (resourceInventory.ContainsKey(data))
        {
            resourceInventory[data] += amount;
        }
        else
        {
            resourceInventory.Add(data, amount);
        }
        Debug.Log($"[ResourceManager] Added {amount} of {data.ResourceName}. Total: {resourceInventory[data]}");
    }

    public bool ConsumeResource(ResourceData data, int amount)
    {
        if (resourceInventory.ContainsKey(data) && resourceInventory[data] >= amount)
        {
            resourceInventory[data] -= amount;
            return true;
        }
        return false;
    }
}