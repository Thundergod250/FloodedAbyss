using UnityEngine;
using System.Collections.Generic;

public enum ResourceType
{
    Stone,
    Wood,
    Gold,
    Food
}

public class PlayerResources : MonoBehaviour
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        // Initialize all resources to 0
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
        Debug.Log($"{type} increased by {amount}. Total: {resources[type]}");
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (resources[type] >= amount)
        {
            resources[type] -= amount;
            Debug.Log($"{type} decreased by {amount}. Total: {resources[type]}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough {type}!");
            return false;
        }
    }

    public int GetResource(ResourceType type)
    {
        return resources[type];
    }
}